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
	/// <summary>
	/// Represents a static 3D mesh loaded from file and prepared for OpenGL rendering.
	/// Handles loading models via Assimp, extracting geometry data, and managing GPU buffers.
	/// </summary>
	public class StaticMesh : IDisposable
	{
		// Singleton Assimp instance shared across all mesh loads
		private static Assimp? assimp;

		/// <summary>
		/// Loads a static mesh from an FBX file using Assimp.
		/// </summary>
		/// <param name="filename">The name of the model file (without extension) in Resources/Models/</param>
		/// <param name="pp">Post-processing steps to apply during import (triangulation, normals, etc.)</param>
		/// <param name="flipTextureV">Whether to flip texture V coordinates (useful for OpenGL vs DirectX conventions)</param>
		/// <returns>A loaded StaticMesh ready for rendering</returns>
		/// <exception cref="FileNotFoundException">Thrown when the model file doesn't exist</exception>
		/// <exception cref="InvalidOperationException">Thrown when import or buffer creation fails</exception>
		public static unsafe StaticMesh LoadFromAssimp(string filename, PostProcessSteps pp, bool flipTextureV = false)
		{
			// Initialize Assimp API if not already done
			assimp ??= Assimp.GetApi();

			// Construct full path to the FBX model file
			string modelPath = Path.Combine("Resources", "Models", $"{filename}.fbx");
			if (!File.Exists(modelPath))
			{
				throw new FileNotFoundException("Model file not found", filename);
			}

			// Add UV flipping to post-process steps if requested
			if (flipTextureV)
			{
				pp |= PostProcessSteps.FlipUVs;
			}

			// Import the scene from file with specified post-processing
			AssimpScene* scene = assimp.ImportFile(modelPath, (uint)pp);
			if (scene == null)
			{
				throw new InvalidOperationException("Failed to import model: " + filename);
			}

			// Create new mesh instance with current OpenGL context
			GL glContext = Application.OpenGlContext();
			StaticMesh mesh = new(glContext);

			// Extract scale factor from scene metadata if available
			if (scene->MMetaData != null)
			{
				Metadata* metadata = scene->MMetaData;

				// Iterate through all metadata properties
				for (uint i = 0; i < metadata->MNumProperties; i++)
				{
					AssimpString key = metadata->MKeys[i];
					MetadataEntry entry = metadata->MValues[i];

					// Convert metadata key to string
					string keyName = System.Text.Encoding.UTF8.GetString(
						key.Data,
						(int)key.Length
					);

					// Check if this is a scale factor property
					if (keyName != "UnitScaleFactor" && keyName != "OriginalUnitScaleFactor" && keyName != "FileScale")
					{
						continue;
					}

					// Extract scale factor based on data type
					mesh.scaleFactor = entry.MType switch
					{
						MetadataType.Float => *(float*)entry.MData,
						MetadataType.Double => (float)(*(double*)entry.MData),
						_ => mesh.scaleFactor // Keep default if unknown type
					};
				}
			}

			// Create OpenGL buffers from the first mesh in the scene
			bool flag = mesh.CreateGlBuffers(scene->MMeshes[0]);

			// Release Assimp scene memory
			assimp.ReleaseImport(scene);

			return flag ? mesh : throw new InvalidOperationException("Failed to create gl buffers.");
		}

		/// <summary>
		/// Extracts vertex and index data from an Assimp mesh structure.
		/// </summary>
		/// <param name="mesh">Pointer to the Assimp mesh</param>
		/// <param name="vertices">Output list of processed vertices</param>
		/// <param name="indices">Output list of triangle indices</param>
		private static unsafe void BuildVertices(AssimpMesh* mesh, out List<Vertex> vertices, out List<uint> indices)
		{
			vertices = [];
			indices = [];

			// Process each vertex in the mesh
			for (uint i = 0; i < mesh->MNumVertices; i++)
			{
				Vertex vertex = new()
				{
					position = new Vector4(mesh->MVertices[i], 1f)
				};

				// Extract normal if available
				if (mesh->MNormals != null)
				{
					vertex.normal = new Vector4(mesh->MNormals[i], 0f);
				}

				// Extract tangent if available (used for normal mapping)
				if (mesh->MTangents != null)
				{
					vertex.tangent = new Vector4(mesh->MTangents[i], 0f);
				}

				// Extract bitangent if available (used for normal mapping)
				if (mesh->MBitangents != null)
				{
					vertex.biTangent = new Vector4(mesh->MBitangents[i], 0f);
				}

				// Extract texture coordinates from first UV channel if available
				if (mesh->MTextureCoords[0] != null)
				{
					Vector3 texCoord = mesh->MTextureCoords[0][i];
					vertex.uv = new Vector2(texCoord.X, texCoord.Y);
				}

				vertices.Add(vertex);
			}

			// Process each face (triangle or quad) and extract indices
			for (uint i = 0; i < mesh->MNumFaces; i++)
			{
				Face face = mesh->MFaces[i];

				// Add first triangle with vertex order swapped (1, 2, 0)
				// Note: This changes winding order, possibly for left-handed to right-handed conversion
				indices.Add(face.MIndices[1]);
				indices.Add(face.MIndices[2]);
				indices.Add(face.MIndices[0]);

				// If face is a quad (4 vertices), split into second triangle
				if (face.MNumIndices != 4)
				{
					continue;
				}

				// Add second triangle of quad (2, 3, 0)
				indices.Add(face.MIndices[2]);
				indices.Add(face.MIndices[3]);
				indices.Add(face.MIndices[0]);
			}
		}

		/// <summary>
		/// Converts vertex list into a flat float array for OpenGL buffer upload.
		/// Layout: position(3), normal(3), tangent(3), bitangent(3), uv(2) per vertex.
		/// </summary>
		/// <param name="verts">List of vertices to convert</param>
		/// <returns>Flat array of floats ready for GPU upload</returns>
		private static float[] BuildVertexBuffer(List<Vertex> verts)
		{
			List<float> vertices = [];

			foreach (Vertex vert in verts)
			{
				// Position (XYZW)
				vertices.Add(vert.position.X);
				vertices.Add(vert.position.Y);
				vertices.Add(vert.position.Z);
				vertices.Add(vert.position.W);

				// Normal (XYZW)
				vertices.Add(vert.normal.X);
				vertices.Add(vert.normal.Y);
				vertices.Add(vert.normal.Z);
				vertices.Add(vert.normal.W);

				// Tangent (XYZW)
				vertices.Add(vert.tangent.X);
				vertices.Add(vert.tangent.Y);
				vertices.Add(vert.tangent.Z);
				vertices.Add(vert.tangent.W);

				// Bitangent (XYZW)
				vertices.Add(vert.biTangent.X);
				vertices.Add(vert.biTangent.Y);
				vertices.Add(vert.biTangent.Z);
				vertices.Add(vert.biTangent.W);

				// UV coordinates (XY)
				vertices.Add(vert.uv.X);
				vertices.Add(vert.uv.Y);
			}

			return vertices.ToArray();
		}

		/// <summary> Scale factor extracted from model metadata (default 1.0 = no scaling) </summary>
		public float scaleFactor = 1f;

		// OpenGL context for rendering operations
		private readonly GL? glContext;

		// OpenGL buffer handles
		private uint vao; // Vertex Array Object
		private uint vbo; // Vertex Buffer Object
		private uint ibo; // Index Buffer Object

		// Number of triangles to render
		private uint triCount;


		/// <summary>
		/// Renders the mesh using the bound shader program.
		/// Assumes shader and uniforms are already set up.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if mesh hasn't been initialized</exception>
		/// <exception cref="NullReferenceException">Thrown if GL context is null</exception>
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

			// Bind the vertex array containing all mesh data
			this.glContext.BindVertexArray(this.vao);

			// Use indexed or non-indexed drawing based on whether we have an index buffer
			if (this.ibo != 0)
			{
				// Indexed drawing (more efficient, reuses vertices)
				this.glContext.DrawElements(PrimitiveType.Triangles, 3 * this.triCount, GLEnum.UnsignedInt, null);
			}
			else
			{
				// Non-indexed drawing (less common)
				this.glContext.DrawArrays(PrimitiveType.Triangles, 0, 3 * this.triCount);
			}
		}

		/// <summary>
		/// Disposes of GPU resources.
		/// Note: Currently doesn't actually delete OpenGL buffers - should be implemented.
		/// </summary>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Private constructor - use LoadFromAssimp to create instances.
		/// </summary>
		/// <param name="glContext">OpenGL context for rendering</param>
		private StaticMesh(GL glContext)
		{
			this.glContext = glContext;
		}

		/// <summary>
		/// Creates OpenGL buffers from an Assimp mesh.
		/// </summary>
		/// <param name="mesh">Pointer to Assimp mesh data</param>
		/// <returns>True if successful, false otherwise</returns>
		private unsafe bool CreateGlBuffers(AssimpMesh* mesh)
		{
			if (this.glContext == null || mesh == null)
			{
				return false;
			}

			// Extract vertex and index data from Assimp mesh
			BuildVertices(mesh, out List<Vertex> vertices, out List<uint> indices);

			// Upload to GPU
			BindAttributes(vertices, indices);

			return true;
		}

		/// <summary>
		/// Creates and configures OpenGL buffers with vertex data.
		/// Sets up vertex attributes for position, normal, tangent, bitangent, and UV.
		/// </summary>
		/// <param name="vertices">List of vertices to upload</param>
		/// <param name="indices">List of indices to upload (empty for non-indexed)</param>
		/// <exception cref="InvalidOperationException">Thrown if GL context not initialized</exception>
		private unsafe void BindAttributes(List<Vertex> vertices, List<uint> indices)
		{
			if (this.glContext == null)
			{
				throw new InvalidOperationException("GL context not initialized.");
			}

			// Generate OpenGL objects
			this.vbo = this.glContext.GenBuffers(1);
			this.vao = this.glContext.GenVertexArrays(1);

			// Bind VAO to capture subsequent state
			this.glContext.BindVertexArray(this.vao);
			this.glContext.BindBuffer(GLEnum.ArrayBuffer, this.vbo);

			// Upload vertex data to GPU
			Span<float> data = BuildVertexBuffer(vertices);
			fixed (void* d = data)
			{
				this.glContext.BufferData(GLEnum.ArrayBuffer, (nuint)data.Length * sizeof(float), d, GLEnum.StaticDraw);
			}

			// Configure vertex attribute 0: Position (vec4)
			uint index = 0;
			IntPtr offset = Marshal.OffsetOf<Vertex>(nameof(Vertex.position));
			this.glContext.EnableVertexAttribArray(index);
			this.glContext.VertexAttribPointer(index++, 4, VertexAttribPointerType.Float, false, Vertex.SizeInBytes,
				offset);

			// Configure vertex attribute 1: Normal (vec4, normalized)
			offset = Marshal.OffsetOf<Vertex>(nameof(Vertex.normal));
			this.glContext.EnableVertexAttribArray(index);
			this.glContext.VertexAttribPointer(index++, 4, VertexAttribPointerType.Float, true, Vertex.SizeInBytes,
				offset);

			// Configure vertex attribute 2: Tangent (vec4, normalized)
			offset = Marshal.OffsetOf<Vertex>(nameof(Vertex.tangent));
			this.glContext.EnableVertexAttribArray(index);
			this.glContext.VertexAttribPointer(index++, 4, VertexAttribPointerType.Float, true, Vertex.SizeInBytes,
				offset);

			// Configure vertex attribute 3: Bitangent (vec4, normalized)
			offset = Marshal.OffsetOf<Vertex>(nameof(Vertex.biTangent));
			this.glContext.EnableVertexAttribArray(index);
			this.glContext.VertexAttribPointer(index++, 4, VertexAttribPointerType.Float, true, Vertex.SizeInBytes,
				offset);

			// Configure vertex attribute 4: UV (vec2)
			offset = Marshal.OffsetOf<Vertex>(nameof(Vertex.uv));
			this.glContext.EnableVertexAttribArray(index);
			this.glContext.VertexAttribPointer(index, 2, VertexAttribPointerType.Float, false, Vertex.SizeInBytes,
				offset);

			// Create and upload index buffer if indices exist
			if (indices.Count != 0)
			{
				this.ibo = this.glContext.GenBuffers(1);
				this.glContext.BindBuffer(GLEnum.ElementArrayBuffer, this.ibo);

				uint[] ind = indices.ToArray();
				fixed (void* d = ind)
				{
					this.glContext.BufferData(GLEnum.ElementArrayBuffer, (nuint)indices.Count * sizeof(uint), d,
						GLEnum.StaticDraw);
				}

				// Calculate triangle count from indices (3 indices per triangle)
				this.triCount = (uint)indices.Count / 3;
			}
			else
			{
				// Calculate triangle count from vertices (3 vertices per triangle)
				this.triCount = (uint)vertices.Count / 3;
			}

			// Unbind to prevent accidental modification
			this.glContext.BindVertexArray(0);
			this.glContext.BindBuffer(GLEnum.ElementArrayBuffer, 0);
			this.glContext.BindBuffer(GLEnum.ArrayBuffer, 0);
		}
	}
}