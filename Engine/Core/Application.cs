using HoleIO.Engine.Gameplay;
using HoleIO.Engine.Gameplay.Scenes;
using HoleIO.Engine.Utility;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace HoleIO.Engine.Core
{
    /// <summary>
    /// Main application entry point and manager for the game engine.
    /// Handles initialization, main loop coordination, and provides global access to core systems.
    /// Uses a singleton pattern to ensure only one application instance exists.
    /// </summary>
    public class Application
    {
       // Singleton instance of the running application
       private static Application? instance;

       /// <summary>
       /// Opens and runs the application with the specified game instance.
       /// This is a blocking call that runs until the window is closed.
       /// </summary>
       /// <typeparam name="TGameInstance">The game instance type to run (must have parameterless constructor)</typeparam>
       /// <exception cref="InvalidOperationException">Thrown if an application is already running</exception>
       public static void Open<TGameInstance>() where TGameInstance : GameInstance, new()
       {
          if (instance != null)
          {
             throw new InvalidOperationException("Application is already opened.");
          }

          // Create application instance with new game instance
          instance = new Application(new TGameInstance());

          // Run the main loop (blocks until window closes)
          instance.Run();

          // Clean up after window closes
          instance = null;
       }

       /// <summary>
       /// Requests the application to close gracefully.
       /// Triggers cleanup and shutdown procedures.
       /// </summary>
       /// <exception cref="InvalidOperationException">Thrown if no application is running</exception>
       public static void Quit()
       {
          if (instance == null)
          {
             throw new InvalidOperationException("Application is not open.");
          }

          instance.window!.Close();
       }

       /// <summary>
       /// Gets the global OpenGL context for rendering operations.
       /// Used throughout the engine for creating buffers, shaders, and rendering.
       /// </summary>
       /// <returns>The active OpenGL context</returns>
       /// <exception cref="InvalidOperationException">Thrown if no application is running</exception>
       public static GL OpenGlContext() => instance == null
          ? throw new InvalidOperationException("Application is not open.")
          : instance.window?.openGlContext!;

       /// <summary>
       /// Gets the global window instance.
       /// Provides access to window properties (size, title, etc.) and input context.
       /// </summary>
       /// <returns>The active window</returns>
       /// <exception cref="InvalidOperationException">Thrown if no application is running</exception>
       public static Window OpenGlWindow() => instance == null
          ? throw new InvalidOperationException("Application is not open.")
          : instance.window!;

       // Engine configuration loaded from Resources/Config/Engine.json
       private readonly Config<EngineConfigData> config = new("Engine");
       
       // Window and OpenGL context manager
       private Window? window;
       
       // User's game instance containing high-level game logic
       private readonly GameInstance gameInstance;

       /// <summary>
       /// Private constructor enforces singleton pattern.
       /// Use Application.Open() to create and run the application.
       /// </summary>
       /// <param name="gameInstance">The game instance to run</param>
       private Application(GameInstance gameInstance)
       {
          this.gameInstance = gameInstance;
       }

       /// <summary>
       /// Initializes the window and starts the main game loop.
       /// Catches and logs any exceptions that occur during execution.
       /// </summary>
       private void Run()
       {
          // Create window with engine configuration
          this.window = new Window(this.config.Get());

          try
          {
             // Open window and register lifecycle callbacks (blocks until window closes)
             this.window.Open(Load, Tick, Render, Unload);
          }
          catch (Exception e)
          {
             // Log any unhandled exceptions
             Console.WriteLine(e);
          }
       }

       /// <summary>
       /// Called once when the window is ready and OpenGL context is created.
       /// Initializes the game instance and applies any pending scene changes.
       /// </summary>
       private void Load()
       {
          // Initialize game instance (typically loads initial scenes)
          this.gameInstance.BeginPlay();
          
          // Apply any scene load operations that were queued during BeginPlay
          SceneManager.Instance.ApplyChanges();
       }

       /// <summary>
       /// Called every frame to update game logic.
       /// Updates time, game instance, scene manager, and all actors in order.
       /// </summary>
       /// <param name="deltaTime">Time in seconds since last frame</param>
       private void Tick(double deltaTime)
       {
          // Update global time values
          Time.Tick(deltaTime, this.window!.window!.Time, this.window.window.WindowState == WindowState.Minimized);
          
          // Update game-wide logic
          this.gameInstance.Tick();
          
          // Update all active scenes and their actors
          // (SceneManager.Tick internally calls Scene.Tick and Actor.Tick)
          SceneManager.Instance.Tick();
       }

       /// <summary>
       /// Called every frame to render visual elements.
       /// Renders game instance overlays, then all scenes and their actors.
       /// </summary>
       /// <param name="deltaTime">Time in seconds since last frame (unused but provided by Silk.NET)</param>
       private void Render(double deltaTime)
       {
          // Render game-wide overlays (HUD, menus, etc.)
          this.gameInstance.Render();
          
          // Render all active scenes and their actors
          // (SceneManager.Render internally calls Scene.Render and Actor.Render)
          SceneManager.Instance.Render();
       }

       /// <summary>
       /// Called when the window is closing.
       /// Performs cleanup in reverse order of initialization:
       /// scenes first, then game instance.
       /// </summary>
       private void Unload()
       {
          // Unload all active scenes (calls EndPlay on scenes and actors)
          SceneManager.Instance.UnloadAll();
          
          // Cleanup game instance
          this.gameInstance.EndPlay();
       }
    }
}