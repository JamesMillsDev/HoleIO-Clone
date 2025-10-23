using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace HoleIO.Engine.Rendering.Textures
{
	/// <summary>
	/// Supported texture file formats.
	/// </summary>
	public enum ETextureFormat
	{
		Png,
		Tga,
		Tiff
	}

	/// <summary>
	/// Represents an OpenGL texture that can be loaded from various image formats.
	/// Handles texture creation, binding, and OpenGL resource management.
	/// </summary>
	public class Texture2D : Texture
	{
		/// <summary>
		/// Converts a texture format enum to its corresponding file extension.
		/// </summary>
		/// <param name="format">The texture format to convert.</param>
		/// <returns>The file extension string without the leading dot.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when an unsupported format is provided.</exception>
		private static string ExtForFormat(ETextureFormat format)
		{
			return format switch
			{
				ETextureFormat.Png => "png",
				ETextureFormat.Tga => "tga",
				ETextureFormat.Tiff => "tiff",
				_ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
			};
		}

		/// <summary>
		/// Gets the Assimp texture type (currently unused, defaults to None).
		/// </summary>
		public TextureType TextureType { get; private set; }

		/// <summary>
		/// Gets the file format of the texture.
		/// </summary>
		public ETextureFormat Format { get; }

		/// <summary>
		/// Gets the width of the texture in pixels.
		/// </summary>
		public uint Width { get; private set; }

		/// <summary>
		/// Gets the height of the texture in pixels.
		/// </summary>
		public uint Height { get; private set; }

		/// <summary>
		/// Initializes a new texture from a file.
		/// </summary>
		/// <param name="fileName">The base filename without extension.</param>
		/// <param name="format">The format of the texture file (defaults to PNG).</param>
		public Texture2D(string fileName, ETextureFormat format = ETextureFormat.Png)
			: base(fileName, TextureTarget.Texture2D)
		{
			this.TextureType = TextureType.None;
			this.Format = format;
			Load();
		}

		/// <summary>
		/// Loads the texture from disk and uploads it to the GPU.
		/// Creates mipmaps and sets texture parameters.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when OpenGL context is null.</exception>
		protected override unsafe void Load()
		{
			if (this.glContext == null)
			{
				throw new InvalidOperationException("OpenGL context is null.");
			}

			// Generate a new OpenGL texture handle
			this.handle = this.glContext.GenTextures(1);
			Bind();

			// Construct the full file path from Resources/Textures directory
			string path = Path.Combine("Resources", "Textures", $"{this.FileName}.{ExtForFormat(this.Format)}");

			// Load the image using ImageSharp
			using (Image<Rgba32> img = Image.Load<Rgba32>(path))
			{
				this.Width = (uint)img.Width;
				this.Height = (uint)img.Height;

				// Allocate GPU memory for the texture
				this.glContext.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)img.Width,
					(uint)img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);

				// Upload pixel data row by row to the GPU
				img.ProcessPixelRows(accessor =>
				{
					for (int y = 0; y < accessor.Height; y++)
					{
						fixed (void* data = accessor.GetRowSpan(y))
						{
							this.glContext.TexSubImage2D(TextureTarget.Texture2D, 0, 0, y, (uint)accessor.Width, 1,
								PixelFormat.Rgba, PixelType.UnsignedByte, data);
						}
					}
				});
			}

			// Configure texture parameters and generate mipmaps
			SetParameters();
		}

		/// <summary>
		/// Configures OpenGL texture parameters including wrapping, filtering, and mipmap generation.
		/// Sets the texture to clamp to edge, use linear filtering with mipmaps, and generates 8 mipmap levels.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when OpenGL context is null.</exception>
		private void SetParameters()
		{
			if (this.glContext == null)
			{
				throw new InvalidOperationException("OpenGL context is null, cannot set texture parameters.");
			}

			// Set filtering modes for minification (with mipmaps) and magnification
			this.glContext.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
				(int)GLEnum.LinearMipmapLinear);
			this.glContext.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
				(int)GLEnum.Linear);

			// Set mipmap level range
			this.glContext.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
			this.glContext.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);

			// Generate mipmaps automatically
			this.glContext.GenerateMipmap(TextureTarget.Texture2D);
		}
	}
}