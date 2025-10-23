using HoleIO.Engine.Core;
using Silk.NET.OpenGL;

namespace HoleIO.Engine.Rendering.Textures
{
	public abstract class Texture(string fileName, TextureTarget textureTarget) : IDisposable
	{
		/// <summary>
		/// Gets the base filename of the texture (without extension or path).
		/// </summary>
		public string FileName { get; } = fileName;

		/// <summary>
		/// OpenGL texture handle identifier.
		/// </summary>
		protected uint handle;

		/// <summary>
		/// Reference to the OpenGL context for rendering operations.
		/// </summary>
		protected readonly GL? glContext = Application.OpenGlContext();
		
		protected readonly TextureTarget textureTarget = textureTarget;

		public void Dispose()
		{
			if (this.glContext != null && this.handle != 0)
			{
				this.glContext.DeleteTextures(1, this.handle);
			}

			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Binds this texture to the specified texture unit for rendering.
		/// </summary>
		/// <param name="slot">The texture unit to bind to (defaults to Texture0).</param>
		/// <exception cref="InvalidOperationException">Thrown when OpenGL context is null.</exception>
		public void Bind(TextureUnit slot = TextureUnit.Texture0)
		{
			if (this.glContext == null)
			{
				throw new InvalidOperationException("OpenGL context is null.");
			}

			this.glContext.ActiveTexture(slot);
			this.glContext.BindTexture(this.textureTarget, this.handle);
		}
		
		protected abstract unsafe void Load();
	}
}