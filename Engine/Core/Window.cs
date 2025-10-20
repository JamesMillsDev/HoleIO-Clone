using System.Drawing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SilkWindow = Silk.NET.Windowing.Window;

namespace HoleIO.Engine.Core
{
	public class Window(EngineConfigData config)
	{
		public int Width
		{
			get => this.width;
			set
			{
				this.width = value;
				this.window!.Size = new Vector2D<int>(this.width, this.height);
			}
		}

		public int Height
		{
			get => this.height;
			set
			{
				this.height = value;
				this.window!.Size = new Vector2D<int>(this.width, this.height);
			}
		}

		public string Title
		{
			get => this.title;
			set
			{
				this.title = value;
				this.window!.Title = this.title;
			}
		}

		public Color ClearColor
		{
			get => this.clearColor;
			set
			{
				this.clearColor = value;
				this.openGlContext!.ClearColor(this.clearColor);
			}
		}

		public bool Fullscreen { get; set; } = config.window.fullscreen;
		public bool Maximised { get; set; } = config.window.maximised;

		internal GL? openGlContext;
		
		private IWindow? window;

		private int width = config.window.width;
		private int height = config.window.height;
		private string title = config.window.title;
		private Color clearColor = config.clearColor;

		internal IInputContext CreateInputContext()
		{
			return this.window == null
				? throw new InvalidOperationException(
					"Couldn't create input context as the window was not initialized."
				)
				: this.window.CreateInput();
		}

		internal void Open(Action load, Action<double> tick, Action<double> render, Action close)
		{
			WindowOptions options = WindowOptions.Default with
			{
				Size = new Vector2D<int>(this.Width, this.Height),
				Title = this.Title,
				WindowState = this.Fullscreen ? WindowState.Fullscreen :
				this.Maximised ? WindowState.Maximized : WindowState.Normal,
			};

			this.window = SilkWindow.Create(options);

			this.window.Load += () =>
			{
				this.openGlContext = this.window.CreateOpenGL();
				if (this.openGlContext == null)
				{
					throw new InvalidOperationException("Failed to open GL context.");
				}

				this.openGlContext.ClearColor(this.ClearColor);
			};

			this.window.Load += load;
			this.window.Update += tick;
			this.window.Render += _ => NewFrame();
			this.window.Render += render;
			this.window.Closing += close;

			this.window.Run();
		}

		internal void Close()
		{
			this.window?.Close();
		}

		private void NewFrame()
		{
			this.openGlContext?.Clear(ClearBufferMask.ColorBufferBit);
		}
	}
}