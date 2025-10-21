namespace HoleIO.Engine.Gameplay.Actors
{
    /// <summary>
    /// Base class for all components that can be attached to an Actor.
    /// Components add specific functionality to actors (rendering, physics, game logic, etc.).
    /// Similar to Unity's MonoBehaviour or Unreal's ActorComponent pattern.
    /// </summary>
    public class ActorComponent
    {
       /// <summary>
       /// Gets the actor that owns this component.
       /// Set internally when the component is added to an actor via AddComponent().
       /// </summary>
       public Actor Actor { get; internal init; } = null!;

       /// <summary>
       /// Gets the transform of the owning actor.
       /// Convenience property to avoid repeatedly typing Actor.Transform.
       /// </summary>
       public ActorTransform Transform => this.Actor.Transform;

       /// <summary>
       /// Gets or sets whether this component is active.
       /// Note: Currently not enforced by the Actor class - derived classes should
       /// check this property in their lifecycle methods if they want to respect it.
       /// </summary>
       public bool Enabled { get; set; } = true;

       /// <summary>
       /// Protected constructor prevents direct instantiation.
       /// Components should be created via Actor.AddComponent() to ensure proper initialization.
       /// </summary>
       internal ActorComponent()
       {
       }

       /// <summary>
       /// Called when the component is first added to an actor or when the game starts.
       /// Override to perform initialization logic (e.g., caching references, setting up state).
       /// </summary>
       public virtual void BeginPlay()
       {
       }

       /// <summary>
       /// Called every frame to update game logic.
       /// Override to add per-frame behavior (e.g., movement, AI, input handling).
       /// </summary>
       public virtual void Tick()
       {
       }

       /// <summary>
       /// Called every frame to render visual elements.
       /// Override to draw meshes, particles, UI, or other visual content.
       /// </summary>
       public virtual void Render()
       {
       }

       /// <summary>
       /// Called when the component is removed from an actor or when the game ends.
       /// Override to perform cleanup logic (e.g., releasing resources, unsubscribing events).
       /// </summary>
       public virtual void EndPlay()
       {
       }
    }
}