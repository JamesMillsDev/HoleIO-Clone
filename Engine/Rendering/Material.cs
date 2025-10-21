using Silk.NET.OpenGL;

namespace HoleIO.Engine.Rendering
{
	public class Material(Shader shader)
	{
		public Dictionary<string, Texture> Textures { get; } = [];
		public Shader Shader { get; } = shader;

		public void Bind()
		{
			this.Shader.Bind();

			string[] names = this.Textures.Keys.ToArray();
			Texture[] textures = this.Textures.Values.ToArray();
			for (int i = 0; i < this.Textures.Count; i++)
			{
				textures[i].Bind((TextureUnit)((int)TextureUnit.Texture0 + i));

				try
				{
					this.Shader.Set(names[i], i);
				}
				catch (Exception e)
				{
					// ignored
				}
			}
		}

		public void Unbind()
		{
			this.Shader.Unbind();
		}
	}
}