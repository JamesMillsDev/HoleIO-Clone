using System.Drawing;
using System.Numerics;
using HoleIO.Engine.Utility;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SilkWindow = Silk.NET.Windowing.Window;

namespace HoleIO.Engine.Core
{
    /// <summary>
    /// Manages the game window and OpenGL context using Silk.NET.
    /// Handles window creation, configuration, and the main render loop.
    /// </summary>
    /// <param name="config">Engine configuration containing initial window settings</param>
    public class Window(EngineConfigData config)
    {
       /// <summary>
       /// Gets or sets the window width in pixels.
       /// Setting this value resizes the window immediately.
       /// </summary>
       public int Width
       {
          get => this.width;
          set
          {
             this.width = value;
             this.window!.Size = new Vector2D<int>(this.width, this.height);
          }
       }

       /// <summary>
       /// Gets or sets the window height in pixels.
       /// Setting this value resizes the window immediately.
       /// </summary>
       public int Height
       {
          get => this.height;
          set
          {
             this.height = value;
             this.window!.Size = new Vector2D<int>(this.width, this.height);
          }
       }

       /// <summary>
       /// Gets the window dimensions as a Vector2 (width, height).
       /// Convenience property for camera aspect ratio calculations.
       /// </summary>
       public Vector2 Size => new(this.Width, this.Height);

       /// <summary>
       /// Gets or sets the window title text.
       /// Setting this value updates the window title bar immediately.
       /// </summary>
       public string Title
       {
          get => this.title;
          set
          {
             this.title = value;
             this.window!.Title = this.title;
          }
       }

       /// <summary>
       /// Gets or sets the color used to clear the screen each frame.
       /// Setting this value updates the OpenGL clear color immediately.
       /// </summary>
       public Color ClearColor
       {
          get => this.clearColor;
          set
          {
             this.clearColor = value;
             this.openGlContext!.ClearColor(this.clearColor);
          }
       }

       /// <summary>
       /// Gets or sets whether the window should be in fullscreen mode.
       /// Note: This is set during initialization and doesn't dynamically change the window state.
       /// </summary>
       public bool Fullscreen { get; set; } = config.window.fullscreen;
       
       /// <summary>
       /// Gets or sets whether the window should be maximized.
       /// Note: This is set during initialization and doesn't dynamically change the window state.
       /// </summary>
       public bool Maximised { get; set; } = config.window.maximised;

       // OpenGL context for rendering operations (null until window is created)
       internal GL? openGlContext;
       
       // Silk.NET window instance (null until Open() is called)
       internal IWindow? window;

       // Backing fields for window properties
       private int width = config.window.width;
       private int height = config.window.height;
       private string title = config.window.title;
       private Color clearColor = config.clearColor;

       /// <summary>
       /// Creates an input context for handling keyboard, mouse, and gamepad input.
       /// Must be called after the window is opened.
       /// </summary>
       /// <returns>Input context for polling input devices</returns>
       /// <exception cref="InvalidOperationException">Thrown if window hasn't been initialized</exception>
       internal IInputContext CreateInputContext()
       {
          return this.window == null
             ? throw new InvalidOperationException(
                "Couldn't create input context as the window was not initialized."
             )
             : this.window.CreateInput();
       }

       /// <summary>
       /// Opens the window and starts the main game loop.
       /// This is a blocking call that runs until the window is closed.
       /// </summary>
       /// <param name="load">Callback invoked once when the window is ready (use for initialization)</param>
       /// <param name="tick">Callback invoked each frame for game logic updates (receives delta time)</param>
       /// <param name="render">Callback invoked each frame for rendering (receives delta time)</param>
       /// <param name="close">Callback invoked when the window is closing (use for cleanup)</param>
       /// <exception cref="InvalidOperationException">Thrown if OpenGL context creation fails</exception>
       internal void Open(Action load, Action<double> tick, Action<double> render, Action close)
       {
          // Configure window options from current properties
          WindowOptions options = WindowOptions.Default with
          {
             Size = new Vector2D<int>(this.Width, this.Height),
             Title = this.Title,
             // Determine initial window state based on fullscreen/maximized flags
             WindowState = this.Fullscreen ? WindowState.Fullscreen :
             this.Maximised ? WindowState.Maximized : WindowState.Normal,
          };

          // Create the Silk.NET window
          this.window = SilkWindow.Create(options);

          // Setup OpenGL context when window is ready
          this.window.Load += () =>
          {
             this.openGlContext = this.window.CreateOpenGL();
             if (this.openGlContext == null)
             {
                throw new InvalidOperationException("Failed to open GL context.");
             }

             // Initialize clear color
             this.openGlContext.ClearColor(this.ClearColor);
          };

          // Register callbacks for the game loop
          this.window.Load += load;           // User initialization
          this.window.Update += tick;         // Game logic updates
          this.window.Render += _ => NewFrame(); // Pre-render setup (clear screen, enable depth test)
          this.window.Render += render;       // User rendering
          this.window.Closing += close;       // Cleanup

          // Start the main loop (blocks until window closes)
          this.window.Run();
       }

       /// <summary>
       /// Closes the window and exits the game loop.
       /// </summary>
       internal void Close()
       {
          this.window?.Close();
       }

       /// <summary>
       /// Prepares OpenGL for a new frame.
       /// Enables depth testing and clears the color and depth buffers.
       /// Called automatically before each render callback.
       /// </summary>
       private void NewFrame()
       {
          // Enable depth testing for proper 3D rendering (closer objects occlude farther ones)
          this.openGlContext?.Enable(EnableCap.DepthTest);
          
          // Clear both color buffer (to ClearColor) and depth buffer (to 1.0)
          this.openGlContext?.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
       }
    }
}