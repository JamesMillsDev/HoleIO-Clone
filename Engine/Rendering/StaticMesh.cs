using System.Drawing;
using System.Numerics;
using Assimp.Unmanaged;

namespace HoleIO.Engine.Rendering
{
	public class StaticMesh
	{
		public struct Vertex
		{
			public Vector4 position;
			public Vector4 normal;
			public Vector4 tangent;
			public Vector4 biTangent;

			public Vector2[] uvs;
			public Color[] colors;
		}

		private uint triCount;
		private uint vao;
		private uint vbo;
		private uint ibo;

		public void LoadFromAssimp(string filename, bool flipTextureV = false)
		{
			
		}
	}
}