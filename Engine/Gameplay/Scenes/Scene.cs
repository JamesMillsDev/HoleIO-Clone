using HoleIO.Engine.Gameplay.Actors;
using HoleIO.Engine.Rendering;
using HoleIO.Engine.Rendering.Components;
using HoleIO.Engine.Rendering.Lighting;

namespace HoleIO.Engine.Gameplay.Scenes
{
	/// <summary>
	/// Represents a game scene containing actors and managing their lifecycle.
	/// Acts as a container for all game objects in a level/area.
	/// Uses a deferred spawn/destroy pattern for safe modification during iteration.
	/// </summary>
	/// <param name="name">Name identifier for this scene</param>
	public class Scene(string name)
	{
		/// <summary>
		/// Gets the first camera component found in the scene hierarchy.
		/// Used by renderers to determine the viewpoint for rendering.
		/// Returns null if no camera exists in the scene.
		/// </summary>
		public CameraComponent? MainCamera => FindFirstCamera(this.rootActor);

		/// <summary>
		/// Gets the name identifier for this scene.
		/// </summary>
		public string Name { get; } = name;
		
		/// <summary>
		/// Contains the lighting data for the current scene; i.e., ambient,
		/// all lights in the scene, skybox, etc.
		/// </summary>
		public LightData LightData { get; } = new();

		// Root actor that serves as the parent for all top-level actors in the scene
		// All spawned actors without an explicit parent become children of this root
		internal readonly Actor rootActor = new();

		// Actors queued to be spawned (deferred until ApplyChanges)
		private readonly List<Actor> pendingSpawn = [];

		// Actors queued to be destroyed (deferred until ApplyChanges)
		private readonly List<Actor> pendingDestroy = [];

		/// <summary>
		/// Queues an actor to be spawned in the scene.
		/// The actor won't be active until ApplyChanges() is called.
		/// If the actor has no parent, it will become a child of the scene root.
		/// </summary>
		/// <typeparam name="TActor">Type of actor to spawn (must have parameterless constructor)</typeparam>
		/// <returns>The newly created actor (not yet initialized)</returns>
		public TActor Spawn<TActor>() where TActor : Actor, new()
		{
			TActor actor = new()
			{
				Scene = this
			};

			this.pendingSpawn.Add(actor);

			return actor;
		}

		/// <summary>
		/// Queues an actor to be destroyed and removed from the scene.
		/// The actor won't be destroyed until ApplyChanges() is called.
		/// </summary>
		/// <param name="actor">The actor to destroy</param>
		public void Destroy(Actor actor)
		{
			this.pendingDestroy.Add(actor);
		}

		/// <summary>
		/// Called once when the scene is loaded or starts.
		/// Override to perform scene-specific initialization.
		/// </summary>
		public virtual void OnLoaded() { }

		/// <summary>
		/// Called every frame to update scene-specific logic.
		/// Override to add per-frame scene behavior (e.g., managing waves, objectives).
		/// </summary>
		public virtual void Tick() { }

		/// <summary>
		/// Called every frame for scene-specific rendering.
		/// Override to add custom rendering logic (e.g., debug visualizations, UI overlays).
		/// Note: Individual actors handle their own rendering through components.
		/// </summary>
		public virtual void Render() { }

		/// <summary>
		/// Called when the scene is unloaded or the game ends.
		/// Override to perform scene-specific cleanup.
		/// </summary>
		public virtual void OnUnloaded() { }

		/// <summary>
		/// Applies all pending spawn and destroy operations.
		/// This deferred execution pattern ensures safe modifications during iteration
		/// (e.g., spawning/destroying actors during a Tick loop).
		/// Should be called after each frame's Tick phase.
		/// </summary>
		internal void ApplyChanges()
		{
			// Process all pending spawns
			foreach (Actor actor in this.pendingSpawn)
			{
				// Apply any pending component additions/removals
				actor.ApplyChanges();

				// Initialize the actor
				actor.BeginPlay();

				// Skip reparenting if actor already has a parent
				if (actor.Transform.Parent != null)
				{
					continue;
				}

				// Make parentless actors children of the scene root
				// This ensures all actors are in the scene hierarchy
				actor.Transform.SetParent(this.rootActor.Transform);
				actor.ApplyChanges();
			}

			this.pendingSpawn.Clear();

			// Process all pending destroys
			foreach (Actor actor in this.pendingDestroy)
			{
				// Remove from scene hierarchy by clearing parent
				actor.Transform.SetParent(null);
				actor.Transform.ApplyChanges();
			}

			this.pendingDestroy.Clear();
		}

		/// <summary>
		/// Recursively searches the actor hierarchy for the first camera component.
		/// </summary>
		/// <param name="actor">The actor to start searching from</param>
		/// <returns>The first camera found, or null if none exists</returns>
		private static CameraComponent? FindFirstCamera(Actor actor)
		{
			// Check this actor and its immediate children
			CameraComponent? camera = actor.GetComponentsInChildren<CameraComponent>().FirstOrDefault();

			if (camera != null)
			{
				return camera;
			}

			// Recursively search deeper in the hierarchy
			foreach (ActorTransform child in actor.Transform)
			{
				camera = FindFirstCamera(child.owner);

				if (camera != null)
				{
					return camera;
				}
			}

			return null;
		}
	}
}