namespace HoleIO.Engine.Gameplay
{
	/// <summary>
	/// Base class for the main game logic controller.
	/// Provides lifecycle hooks for game-wide initialization, updates, rendering, and cleanup.
	/// Similar to Unreal's GameInstance - persists across scene changes and manages high-level game flow.
	/// 
	/// Inherit from this class to implement your game's main logic, such as:
	/// - Scene loading/transitions
	/// - Global game state management
	/// - Persistent data handling
	/// - Main menu and UI flow
	/// - Save/load systems
	/// </summary>
	/// <example>
	/// public class MyGameInstance : GameInstance
	/// {
	///     public override void BeginPlay()
	///     {
	///         // Load initial scene, initialize systems
	///         SceneManager.Instance.Load("MainMenu");
	///     }
	///     
	///     public override void Tick()
	///     {
	///         // Update game-wide logic
	///     }
	///     
	///     public override void Render()
	///     {
	///         // Render game-wide UI overlays
	///     }
	///     
	///     public override void EndPlay()
	///     {
	///         // Save game state, cleanup
	///     }
	/// }
	/// </example>
	public abstract class GameInstance
	{
		/// <summary>
		/// Called once when the game starts, before any scenes are loaded.
		/// Use this to initialize game systems, load initial scenes, and set up persistent state.
		/// </summary>
		public abstract void BeginPlay();

		/// <summary>
		/// Called every frame to update game-wide logic.
		/// Runs before scene and actor Tick methods.
		/// Use this for high-level game logic that transcends individual scenes
		/// (e.g., global timers, achievement tracking, network synchronization).
		/// </summary>
		public abstract void Tick();

		/// <summary>
		/// Called every frame to render game-wide visual elements.
		/// Runs before scene and actor Render methods.
		/// Use this for persistent UI overlays (e.g., HUD, pause menus, loading screens).
		/// </summary>
		public abstract void Render();

		/// <summary>
		/// Called once when the game is shutting down.
		/// Use this to save data, cleanup resources, and perform final shutdown procedures.
		/// </summary>
		public abstract void EndPlay();
	}
}