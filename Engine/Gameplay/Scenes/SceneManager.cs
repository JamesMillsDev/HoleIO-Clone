using System.Numerics;
using System.Runtime.InteropServices;
using HoleIO.Engine.Core;
using HoleIO.Engine.Rendering.Components;
using Silk.NET.OpenGL;
using Shader = HoleIO.Engine.Rendering.Shader;

namespace HoleIO.Engine.Gameplay.Scenes
{
	/// <summary>
	/// Manages all game scenes, handling loading, unloading, and scene lifecycle.
	/// Supports multiple active scenes simultaneously (additive scene loading).
	/// Uses a singleton pattern for global access and deferred loading for safe operations.
	/// </summary>
	public class SceneManager
	{
		/// <summary>
		/// Gets the singleton instance of the SceneManager.
		/// </summary>
		public static SceneManager Instance { get; } = new();

		/// <summary>
		/// Gets the first active scene, typically used as the "main" scene.
		/// Returns null if no scenes are loaded.
		/// </summary>
		public Scene? Current => this.activeScenes.FirstOrDefault();

		// Dictionary of all registered scenes (both loaded and unloaded)
		// Key: scene name, Value: scene instance
		private readonly Dictionary<string, Scene> scenes = [];

		// List of currently active/loaded scenes
		// Scenes in this list have their Tick/Render methods called
		private readonly List<Scene> activeScenes = [];

		// Queue of pending load/unload operations
		// Item1: Scene to modify, Item2: true = load, false = unload
		private readonly List<Tuple<Scene, bool>> pendingChange = [];

		private GL? glContext;

		/// <summary>
		/// Registers a scene with the manager, making it available for loading.
		/// The scene is not loaded until Load() is called.
		/// </summary>
		/// <param name="scene">The scene to register</param>
		/// <exception cref="ArgumentException">Thrown if a scene with the same name already exists</exception>
		public void AddScene(Scene scene)
		{
			if (!this.scenes.TryAdd(scene.Name, scene))
			{
				throw new ArgumentException($"Scene with name {scene.Name} already exists");
			}
		}

		/// <summary>
		/// Queues a scene to be loaded and activated.
		/// The scene won't be active until ApplyChanges() is called.
		/// Multiple scenes can be loaded simultaneously (additive loading).
		/// </summary>
		/// <param name="name">Name of the scene to load</param>
		public void Load(string name)
		{
			if (!this.scenes.TryGetValue(name, out Scene? scene) || this.activeScenes.Contains(scene))
			{
				return;
			}

			// Queue load operation (deferred execution)
			this.pendingChange.Add(new Tuple<Scene, bool>(scene, true));
		}

		/// <summary>
		/// Queues a scene to be unloaded and deactivated.
		/// The scene won't be unloaded until ApplyChanges() is called.
		/// </summary>
		/// <param name="name">Name of the scene to unload</param>
		public void Unload(string name)
		{
			if (!this.scenes.TryGetValue(name, out Scene? scene) || !this.activeScenes.Contains(scene))
			{
				return;
			}

			// Queue unload operation (deferred execution)
			this.pendingChange.Add(new Tuple<Scene, bool>(scene, false));
		}

		/// <summary>
		/// Updates all active scenes and their actors.
		/// Called once per frame by the game loop.
		/// First applies pending load/unload operations, then updates scene logic and actors.
		/// </summary>
		internal void Tick()
		{
			this.glContext ??= Application.OpenGlContext();

			// Apply any pending scene load/unload operations
			ApplyChanges();

			// Update scene-specific logic for all active scenes
			foreach (Scene scene in this.activeScenes)
			{
				scene.Tick();
			}

			// Update all actors in all active scenes
			foreach (Scene scene in this.activeScenes)
			{
				scene.rootActor.Tick();
			}
		}

		/// <summary>
		/// Renders all active scenes and their actors.
		/// Called once per frame by the game loop after Tick().
		/// </summary>
		internal unsafe void Render()
		{
			// Get the active camera from the current scene
			CameraComponent? cam = this.Current?.MainCamera;

			// Only update matrices if we have a valid camera, UBO exists, and OpenGL context is available
			if (cam != null && Shader.prjViewUboHandle != 0 && this.glContext != null)
			{
				// Calculate size of a single Matrix4x4 in bytes (typically 64 bytes - 16 floats * 4 bytes each)
				uint matrixSize = (uint)Marshal.SizeOf<Matrix4x4>();

				// Get current camera matrices for rendering
				Matrix4x4 projection = cam.Projection;
				Matrix4x4 view = cam.View;

				// Bind the Uniform Buffer Object (UBO) that stores shared matrix data across all shaders
				this.glContext.BindBuffer(GLEnum.UniformBuffer, Shader.prjViewUboHandle);

				// Upload projection matrix to the first slot (offset 0) of the UBO
				this.glContext.BufferSubData(GLEnum.UniformBuffer, 0, matrixSize, (float*)&projection);

				// Upload view matrix to the second slot (offset = one matrix size) of the UBO
				this.glContext.BufferSubData(GLEnum.UniformBuffer, new IntPtr(matrixSize), matrixSize, (float*)&view);

				// Unbind the UBO to prevent accidental modifications
				this.glContext.BindBuffer(GLEnum.UniformBuffer, 0);
			}

			// Render scene-specific visuals for all active scenes
			foreach (Scene scene in this.activeScenes)
			{
				scene.Render();
			}

			// Render all actors in all active scenes
			foreach (Scene scene in this.activeScenes)
			{
				scene.rootActor.Render();
			}
		}

		/// <summary>
		/// Applies all pending scene load/unload operations.
		/// This deferred execution pattern ensures safe scene transitions during iteration.
		/// </summary>
		internal void ApplyChanges()
		{
			foreach (Tuple<Scene, bool> change in this.pendingChange)
			{
				if (change.Item2)
				{
					// Load scene: initialize and add to active list
					change.Item1.OnLoaded();
					change.Item1.rootActor.BeginPlay();
					this.activeScenes.Add(change.Item1);
				}
				else
				{
					// Unload scene: cleanup and remove from active list
					change.Item1.OnUnloaded();
					change.Item1.rootActor.EndPlay();
					this.activeScenes.Remove(change.Item1);
				}
			}

			// Clear the pending operations queue
			this.pendingChange.Clear();

			// Apply all scene hierarchy changes
			foreach (Scene scene in this.activeScenes)
			{
				scene.ApplyChanges();
			}
		}

		/// <summary>
		/// Unloads all currently active scenes.
		/// Useful for cleanup during application shutdown or returning to main menu.
		/// Applies changes immediately to ensure scenes are properly cleaned up.
		/// </summary>
		internal void UnloadAll()
		{
			// Queue all active scenes for unloading
			// Need to create a copy to avoid modification during iteration
			foreach (Scene scene in this.activeScenes.ToList())
			{
				Unload(scene.Name);
			}

			// Immediately apply the unload operations
			ApplyChanges();
		}
	}
}