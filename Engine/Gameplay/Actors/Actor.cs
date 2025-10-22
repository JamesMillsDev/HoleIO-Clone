using HoleIO.Engine.Gameplay.Scenes;

namespace HoleIO.Engine.Gameplay.Actors
{
	/// <summary>
	/// Base class for all game objects in the scene.
	/// Manages a transform, component collection, and lifecycle events.
	/// Similar to Unity's GameObject or Unreal's Actor pattern.
	/// </summary>
	public class Actor
	{
		/// <summary>
		/// Gets the transform component that defines this actor's position, rotation, and scale.
		/// Every actor has exactly one transform.
		/// </summary>
		public ActorTransform Transform { get; }

		/// <summary>
		/// The reference to the scene that owns this actor.
		/// </summary>
		public Scene Scene { get; internal init; } = null!;

		// Active components attached to this actor
		private readonly List<ActorComponent> components = [];

		// Components queued to be added (deferred until ApplyChanges)
		private readonly List<ActorComponent> pendingAddComponents = [];

		// Components queued to be removed (deferred until ApplyChanges)
		private readonly List<ActorComponent> pendingRemoveComponents = [];

		/// <summary>
		/// Creates a new actor with an identity transform.
		/// </summary>
		public Actor()
		{
			this.Transform = new ActorTransform(this);
		}

		/// <summary>
		/// Gets the first component of the specified type attached to this actor.
		/// </summary>
		/// <typeparam name="TComponent">The component type to search for</typeparam>
		/// <returns>The first matching component, or null if none found</returns>
		public TComponent? GetComponent<TComponent>() where TComponent : ActorComponent
		{
			return this.components.OfType<TComponent>().Select(component => component).FirstOrDefault();
		}

		/// <summary>
		/// Gets all components of the specified type attached to this actor.
		/// </summary>
		/// <typeparam name="TComponent">The component type to search for</typeparam>
		/// <returns>Array of all matching components (empty if none found)</returns>
		public TComponent[] GetComponents<TComponent>() where TComponent : ActorComponent
		{
			return this.components.OfType<TComponent>().Select(component => component).ToArray();
		}

		/// <summary>
		/// Gets all components of the specified type from this actor and all its children recursively.
		/// Only searches direct children (not grandchildren or deeper descendants).
		/// </summary>
		/// <typeparam name="TComponent">The component type to search for</typeparam>
		/// <returns>Array of all matching components in this actor and its immediate children</returns>
		public TComponent[] GetComponentsInChildren<TComponent>() where TComponent : ActorComponent
		{
			// Start with components on this actor
			List<TComponent> found = this.components.OfType<TComponent>().Select(component => component).ToList();

			// Add components from each direct child
			foreach (ActorTransform child in this.Transform)
			{
				found.AddRange(child.owner.GetComponents<TComponent>());
			}

			return found.ToArray();
		}

		/// <summary>
		/// Gets all components of the specified type from this actor and all its ancestors recursively.
		/// Searches up the parent hierarchy to the root.
		/// </summary>
		/// <typeparam name="TComponent">The component type to search for</typeparam>
		/// <returns>Array of all matching components in this actor and its parent chain</returns>
		public TComponent[] GetComponentsInParent<TComponent>() where TComponent : ActorComponent
		{
			// Start with components on this actor
			List<TComponent> found = GetComponents<TComponent>().ToList();

			// Recursively search parent hierarchy
			found.AddRange(this.Transform.Parent?.owner.GetComponentsInParent<TComponent>() ?? []);

			return found.ToArray();
		}

		/// <summary>
		/// Creates and attaches a new component of the specified type to this actor.
		/// The component is queued and won't be active until ApplyChanges() is called.
		/// </summary>
		/// <typeparam name="TComponent">The component type to create (must have parameterless constructor)</typeparam>
		/// <returns>The newly created component</returns>
		public TComponent AddComponent<TComponent>() where TComponent : ActorComponent, new()
		{
			// Create new component instance
			TComponent component = new()
			{
				Actor = this
			};

			// Queue for addition (deferred execution for safe modification during iteration)
			this.pendingAddComponents.Add(component);

			return component;
		}

		/// <summary>
		/// Removes a component from this actor.
		/// The component is queued and won't be removed until ApplyChanges() is called.
		/// </summary>
		/// <param name="component">The component to remove</param>
		public void RemoveComponent(ActorComponent component)
		{
			// If component isn't active yet, just remove it from pending lists
			if (!this.components.Contains(component))
			{
				this.pendingAddComponents.Remove(component);
				this.pendingRemoveComponents.Remove(component);

				return;
			}

			// Queue active component for removal
			this.pendingRemoveComponents.Add(component);
		}

		/// <summary>
		/// Called when the actor is spawned or the game starts.
		/// Initializes this actor and all its components and children recursively.
		/// Override to add custom initialization logic.
		/// </summary>
		public virtual void BeginPlay()
		{
			// Initialize all components on this actor
			foreach (ActorComponent component in this.components)
			{
				component.BeginPlay();
			}

			// Recursively initialize all child actors
			foreach (ActorTransform child in this.Transform)
			{
				child.owner.BeginPlay();
			}
		}

		/// <summary>
		/// Called every frame to update game logic.
		/// Updates this actor, all its components, and all children recursively.
		/// Override to add custom per-frame update logic.
		/// </summary>
		public virtual void Tick()
		{
			// Update all enabled components on this actor
			foreach (ActorComponent component in this.components.Where(comp => comp.Enabled))
			{
				component.Tick();
			}

			// Recursively update all child actors
			foreach (ActorTransform child in this.Transform)
			{
				child.owner.Tick();
			}
		}

		/// <summary>
		/// Called every frame to render visual elements.
		/// Renders this actor, all its components, and all children recursively.
		/// Override to add custom rendering logic.
		/// </summary>
		public virtual void Render()
		{
			// Render all enabled components on this actor
			foreach (ActorComponent component in this.components.Where(comp => comp.Enabled))
			{
				component.Render();
			}

			// Recursively render all child actors
			foreach (ActorTransform child in this.Transform)
			{
				child.owner.Render();
			}
		}

		/// <summary>
		/// Called when the actor is destroyed or the game ends.
		/// Cleans up this actor, all its components, and all children recursively.
		/// Override to add custom cleanup logic.
		/// </summary>
		public virtual void EndPlay()
		{
			// Clean up all components on this actor
			foreach (ActorComponent component in this.components)
			{
				component.EndPlay();
			}

			// Recursively clean up all child actors
			foreach (ActorTransform child in this.Transform)
			{
				child.owner.EndPlay();
			}
		}

		/// <summary>
		/// Applies all pending changes (component additions/removals, transform reparenting).
		/// This deferred execution pattern ensures safe modifications during iteration
		/// (e.g., adding/removing components during a Tick loop).
		/// Should be called after each frame's Tick phase.
		/// </summary>
		internal void ApplyChanges()
		{
			// Process all pending component additions
			foreach (ActorComponent pendingComponent in this.pendingAddComponents)
			{
				// Initialize the component
				pendingComponent.BeginPlay();

				// Add to active components list
				this.components.Add(pendingComponent);
			}

			// Process all pending component removals
			foreach (ActorComponent pendingRemoveComponent in this.pendingRemoveComponents)
			{
				// Clean up the component
				pendingRemoveComponent.EndPlay();

				// Remove from active components list
				this.components.Remove(pendingRemoveComponent);
			}

			// Clear pending queues
			this.pendingAddComponents.Clear();
			this.pendingRemoveComponents.Clear();

			// Apply pending transform changes (reparenting operations)
			this.Transform.ApplyChanges();

			// Apply pending transform changes for all child transforms
			foreach (ActorTransform child in this.Transform)
			{
				child.ApplyChanges();
			}

			// Recursively apply changes to all child actors
			foreach (ActorTransform child in this.Transform)
			{
				child.owner.ApplyChanges();
			}
		}
	}
}