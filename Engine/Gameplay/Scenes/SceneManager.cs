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
       /// <exception cref="ArgumentException">Thrown if the scene doesn't exist</exception>
       /// <exception cref="InvalidOperationException">Thrown if the scene is already loaded</exception>
       public void Load(string name)
       {
          if (!this.scenes.TryGetValue(name, out Scene? scene))
          {
             throw new ArgumentException($"Scene with name {name} does not exist");
          }

          if (this.activeScenes.Contains(scene))
          {
             throw new InvalidOperationException($"Scene with name {name} already loaded");
          }

          // Queue load operation (deferred execution)
          this.pendingChange.Add(new Tuple<Scene, bool>(scene, true));
       }

       /// <summary>
       /// Queues a scene to be unloaded and deactivated.
       /// The scene won't be unloaded until ApplyChanges() is called.
       /// </summary>
       /// <param name="name">Name of the scene to unload</param>
       /// <exception cref="ArgumentException">Thrown if the scene doesn't exist</exception>
       /// <exception cref="InvalidOperationException">Thrown if the scene is not loaded</exception>
       public void Unload(string name)
       {
          if (!this.scenes.TryGetValue(name, out Scene? scene))
          {
             throw new ArgumentException($"Scene with name {name} does not exist");
          }

          if (!this.activeScenes.Contains(scene))
          {
             throw new InvalidOperationException($"Scene with name {name} not loaded");
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
       internal void Render()
       {
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