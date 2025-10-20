using System.Numerics;
using System.Runtime.InteropServices;
using HoleIO.Engine.Core;
using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using AssimpMesh = Silk.NET.Assimp.Mesh;
using AssimpScene = Silk.NET.Assimp.Scene;
using File = System.IO.File;
using PrimitiveType = Silk.NET.OpenGL.PrimitiveType;

namespace HoleIO.Engine.Rendering
{
    public class StaticMesh : IDisposable
    {
        private static Assimp? assimp;

        public static unsafe StaticMesh LoadFromAssimp(string filename, PostProcessSteps pp, bool flipTextureV = false)
        {
            assimp ??= Assimp.GetApi();

            string modelPath = Path.Combine("Resources", "Models", $"{filename}.fbx");
            if (!File.Exists(modelPath))
            {
                throw new FileNotFoundException("Model file not found", filename);
            }

            if (flipTextureV)
            {
                pp |= PostProcessSteps.FlipUVs;
            }
            AssimpScene* scene = assimp.ImportFile(modelPath, (uint)pp);
            if (scene == null)
            {
                throw new InvalidOperationException("Failed to import model: " + filename);
            }

            GL glContext = Application.OpenGlContext();
            StaticMesh mesh = new(glContext);
            return !mesh.CreateGlBuffers(scene->MMeshes[0])
                ? throw new InvalidOperationException("Failed to create gl buffers.")
                : mesh;
        }
        
        private static unsafe void BuildVertices(AssimpMesh* mesh, out List<Vertex> vertices, out List<uint> indices)
        {
            vertices = [];
            indices = [];
            
            for (uint i = 0; i < mesh->MNumVertices; i++)
            {
                Vertex vertex = new()
                {
                    position = mesh->MVertices[i]
                };

                if (mesh->MNormals != null)
                {
                    vertex.normal = mesh->MNormals[i];
                }

                if (mesh->MTangents != null)
                {
                    vertex.tangent = mesh->MTangents[i];
                }

                if (mesh->MBitangents != null)
                {
                    vertex.biTangent = mesh->MBitangents[i];
                }

                if (mesh->MTextureCoords[0] != null)
                {
                    Vector3 texCoord = mesh->MTextureCoords[0][i];
                    vertex.uv = new Vector2(texCoord.X, texCoord.Y);
                }

                vertices.Add(vertex);
            }

            // now walk through each of the mesh's faces (a face is a mesh its triangle) and retrieve the corresponding
            // vertex indices.
            for (uint i = 0; i < mesh->MNumFaces; i++)
            {
                Face face = mesh->MFaces[i];
                
                indices.Add(face.MIndices[1]);
                indices.Add(face.MIndices[2]);
                indices.Add(face.MIndices[0]);

                if (face.MNumIndices != 4)
                {
                    continue;
                }

                indices.Add(face.MIndices[2]);
                indices.Add(face.MIndices[3]);
                indices.Add(face.MIndices[0]);
            }
        }

        private static float[] BuildVertexBuffer(List<Vertex> verts)
        {
            List<float> vertices = [];

            foreach (Vertex vert in verts)
            {
                vertices.Add(vert.position.X);
                vertices.Add(vert.position.Y);
                vertices.Add(vert.position.Z);
                vertices.Add(vert.normal.X);
                vertices.Add(vert.normal.Y);
                vertices.Add(vert.normal.Z);
                vertices.Add(vert.tangent.X);
                vertices.Add(vert.tangent.Y);
                vertices.Add(vert.tangent.Z);
                vertices.Add(vert.biTangent.X);
                vertices.Add(vert.biTangent.Y);
                vertices.Add(vert.biTangent.Z);
                vertices.Add(vert.uv.X);
                vertices.Add(vert.uv.Y);
            }

            return vertices.ToArray();
        }

        private readonly GL? glContext;
        private uint vao;
        private uint vbo;
        private uint ibo;
        private uint triCount;

        public unsafe void Render()
        {
            if (this.vao == 0)
            {
                throw new InvalidOperationException("Cannot render uninitialized mesh.");
            }

            if (this.glContext == null)
            {
                throw new NullReferenceException("glContext is null!");
            }

            this.glContext.BindVertexArray(this.vao);
            if (this.ibo != 0)
            {
                this.glContext.DrawElements(PrimitiveType.Triangles, 3 * this.triCount, GLEnum.UnsignedInt, null);
            }
            else
            {
                this.glContext.DrawArrays(PrimitiveType.Triangles, 0, 3 * this.triCount);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private StaticMesh(GL glContext)
        {
            this.glContext = glContext;
        }

        private unsafe bool CreateGlBuffers(AssimpMesh* mesh)
        {
            if (this.glContext == null || mesh == null)
            {
                return false;
            }

            BuildVertices(mesh, out List<Vertex> vertices, out List<uint> indices);
            BindAttributes(vertices, indices);

            return true;
        }

        private unsafe void BindAttributes(List<Vertex> vertices, List<uint> indices)
        {
            if (this.glContext == null)
            {
                throw new InvalidOperationException("GL context not initialized.");
            }

            this.vbo = this.glContext.GenBuffers(1);
            this.vao = this.glContext.GenVertexArrays(1);
            
            this.glContext.BindVertexArray(this.vao);
            this.glContext.BindBuffer(GLEnum.ArrayBuffer, this.vbo);

            Span<float> data = BuildVertexBuffer(vertices);
            fixed (void* d = data)
            {
                this.glContext.BufferData(GLEnum.ArrayBuffer, (nuint)data.Length * sizeof(float), d, GLEnum.StaticDraw);
            }
            
            uint index = 0;
            IntPtr offset = Marshal.OffsetOf<Vertex>(nameof(Vertex.position));
            this.glContext.EnableVertexAttribArray(index);
            this.glContext.VertexAttribPointer(index++, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, offset);

            offset = Marshal.OffsetOf<Vertex>(nameof(Vertex.normal));
            this.glContext.EnableVertexAttribArray(index);
            this.glContext.VertexAttribPointer(index++, 3, VertexAttribPointerType.Float, true, Vertex.SizeInBytes, offset);

            offset = Marshal.OffsetOf<Vertex>(nameof(Vertex.tangent));
            this.glContext.EnableVertexAttribArray(index);
            this.glContext.VertexAttribPointer(index++, 3, VertexAttribPointerType.Float, true, Vertex.SizeInBytes, offset);

            offset = Marshal.OffsetOf<Vertex>(nameof(Vertex.biTangent));
            this.glContext.EnableVertexAttribArray(index);
            this.glContext.VertexAttribPointer(index++, 3, VertexAttribPointerType.Float, true, Vertex.SizeInBytes, offset);

            offset = Marshal.OffsetOf<Vertex>(nameof(Vertex.uv));
            this.glContext.EnableVertexAttribArray(index);
            this.glContext.VertexAttribPointer(index, 2, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, offset);

            if (indices.Count != 0)
            {
                this.ibo = this.glContext.GenBuffers(1);
                
                this.glContext.BindBuffer(GLEnum.ElementArrayBuffer, this.ibo);
                
                uint[] ind = indices.ToArray();
                fixed (void* d = ind)
                {
                    this.glContext.BufferData(GLEnum.ElementArrayBuffer, (nuint)indices.Count * sizeof(uint), d, GLEnum.StaticDraw);
                }

                this.triCount = (uint)indices.Count / 3;
            }
            else
            {
                this.triCount = (uint)vertices.Count / 3;
            }
            
            this.glContext.BindVertexArray(0);
            this.glContext.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            this.glContext.BindBuffer(GLEnum.ArrayBuffer, 0);
        }
    }
}