using System.Numerics;
using System.Runtime.InteropServices;
using HoleIO.Engine.Core;
using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using AssimpMesh = Silk.NET.Assimp.Mesh;
using AssimpScene = Silk.NET.Assimp.Scene;
using File = System.IO.File;

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

            AssimpScene* scene = assimp.ImportFile(modelPath, (uint)pp);
            if (scene == null)
            {
                throw new InvalidOperationException("Failed to import model: " + filename);
            }

            GL glContext = Application.OpenGlContext();
            StaticMesh mesh = new(glContext);
            return !mesh.CreateGlBuffers(scene->MMeshes[0], flipTextureV)
                ? throw new InvalidOperationException("Failed to create gl buffers.")
                : mesh;
        }

        private readonly GL? glContext;
        private VertexArrayObject<float, uint>? vao;
        private BufferObject<float>? vbo;
        private BufferObject<uint>? ibo;

        [StructLayout(LayoutKind.Sequential)]
        private struct Vertex
        {
            public static readonly uint SizeInBytes = (uint)Marshal.SizeOf<Vertex>();

            public Vector3 position;
            public Vector3 normal;
            public Vector3 tangent;
            public Vector3 biTangent;
            public Vector2 uv;
            public Vector4 color;
        }

        public void Dispose()
        {
            vao?.Dispose();
            vbo?.Dispose();
            ibo?.Dispose();
        }

        private StaticMesh(GL glContext)
        {
            this.glContext = glContext;
        }

        private unsafe bool CreateGlBuffers(AssimpMesh* mesh, bool flipTextureV)
        {
            if (this.glContext == null || mesh == null)
            {
                return false;
            }

            List<Vertex> vertices = [];
            List<uint> indices = [];

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
                    vertex.uv = new Vector2(texCoord.X, flipTextureV ? 1 - texCoord.Y : texCoord.Y);
                }

                if (mesh->MColors[0] != null)
                {
                    vertex.color = mesh->MColors[0][i];
                }

                vertices.Add(vertex);
            }

            // now walk through each of the mesh's faces (a face is a mesh its triangle) and retrieve the corresponding
            // vertex indices.
            for (uint i = 0; i < mesh->MNumFaces; i++)
            {
                Face face = mesh->MFaces[i];
                // retrieve all indices of the face and store them in the indices vector
                for (uint j = 0; j < face.MNumIndices; j++)
                {
                    indices.Add(face.MIndices[j]);
                }
            }

            ibo = new BufferObject<uint>(this.glContext, indices.ToArray(), BufferTargetARB.ElementArrayBuffer);
            vbo = new BufferObject<float>(this.glContext, BuildVertices(vertices), BufferTargetARB.ArrayBuffer);
            vao = new VertexArrayObject<float, uint>(this.glContext, vbo, ibo);

            uint index = 0;
            IntPtr offset = Marshal.OffsetOf<Vertex>(nameof(Vertex.position));
            vao.VertexAttributePointer(index++, 3, VertexAttribPointerType.Float, Vertex.SizeInBytes, offset.ToInt32());

            offset = Marshal.OffsetOf<Vertex>(nameof(Vertex.normal));
            vao.VertexAttributePointer(index++, 3, VertexAttribPointerType.Float, Vertex.SizeInBytes, offset.ToInt32(),
                true);

            offset = Marshal.OffsetOf<Vertex>(nameof(Vertex.tangent));
            vao.VertexAttributePointer(index++, 3, VertexAttribPointerType.Float, Vertex.SizeInBytes, offset.ToInt32(),
                true);

            offset = Marshal.OffsetOf<Vertex>(nameof(Vertex.biTangent));
            vao.VertexAttributePointer(index++, 3, VertexAttribPointerType.Float, Vertex.SizeInBytes, offset.ToInt32(),
                true);

            offset = Marshal.OffsetOf<Vertex>(nameof(Vertex.uv));
            vao.VertexAttributePointer(index++, 2, VertexAttribPointerType.Float, Vertex.SizeInBytes, offset.ToInt32());

            offset = Marshal.OffsetOf<Vertex>(nameof(Vertex.color));
            vao.VertexAttributePointer(index, 4, VertexAttribPointerType.Float, Vertex.SizeInBytes, offset.ToInt32());

            return true;
        }

        private float[] BuildVertices(List<Vertex> verts)
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
                vertices.Add(vert.color.X);
                vertices.Add(vert.color.Y);
                vertices.Add(vert.color.Z);
                vertices.Add(vert.color.W);
            }

            return vertices.ToArray();
        }
    }
}