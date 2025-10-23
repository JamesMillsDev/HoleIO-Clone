using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace HoleIO.Engine.Rendering.Textures
{
	public class Cubemap : Texture
	{
		private static readonly List<string> TextureFaces = ["px", "nx", "py", "ny", "pz", "nz"];


		public Cubemap(string filename) : base(filename, TextureTarget.TextureCubeMap)
		{
			Load();
		}

		protected override unsafe void Load()
		{
			if (this.glContext == null)
			{
				throw new InvalidOperationException("OpenGL context is null.");
			}

			this.handle = this.glContext.GenTextures(1);
			this.glContext.BindTexture(TextureTarget.TextureCubeMap, this.handle);

			for (int index = 0; index < TextureFaces.Count; index++)
			{
				string face = TextureFaces[index];
				string path = Path.Combine("Resources", "Skyboxes", $"{this.FileName}_{face}.png");

				using (Image<Rgb24> img = Image.Load<Rgb24>(path))
				{
					Span<Rgb24> data = new Rgb24[img.Width * img.Height];
					img.CopyPixelDataTo(data);
					fixed (void* d = data)
					{
						TextureTarget target = TextureTarget.TextureCubeMapPositiveX + index;

						this.glContext.TexImage2D(target, 0, InternalFormat.Rgb, (uint)img.Width, (uint)img.Height, 0,
							PixelFormat.Rgb, PixelType.UnsignedByte, d);
					}
				}

				SetParameters();
			}
		}

		private void SetParameters()
		{
			if (this.glContext == null)
			{
				throw new InvalidOperationException("OpenGL context is null, cannot set texture parameters.");
			}

			// Set filtering modes for minification (with mipmaps) and magnification
			this.glContext.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter,
				(int)GLEnum.Linear);
			this.glContext.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter,
				(int)GLEnum.Linear);

			// Set mipmap level range
			this.glContext.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureBaseLevel, 0);
			this.glContext.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMaxLevel, 8);

			// Generate mipmaps automatically
			this.glContext.GenerateMipmap(TextureTarget.TextureCubeMap);
		}
	}
}