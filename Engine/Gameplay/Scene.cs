using HoleIO.Engine.Gameplay.Actors;

namespace HoleIO.Engine.Gameplay
{
	public class Scene
	{
		internal readonly Actor rootActor = new();
		private readonly List<Actor> pendingSpawn = [];
		private readonly List<Actor> pendingDestroy = [];

		public TActor Spawn<TActor>() where TActor : Actor, new()
		{
			TActor actor = new();

			this.pendingSpawn.Add(actor);
			
			return actor;
		}

		public void Destroy(Actor actor)
		{
			this.pendingDestroy.Add(actor);
		}

		internal void ApplyChanges()
		{
			foreach (Actor actor in this.pendingSpawn)
			{
				actor.ApplyChanges();
				actor.BeginPlay();
				
				if (actor.Transform.Parent != null)
				{
					continue;
				}

				// We need to make this a child of the root scene
				actor.Transform.SetParent(this.rootActor.Transform);
				actor.ApplyChanges();
			}
			
			this.pendingSpawn.Clear();

			foreach (Actor actor in this.pendingDestroy)
			{
				actor.Transform.SetParent(null);
				actor.Transform.ApplyChanges();
			}
			
			this.pendingDestroy.Clear();
		}
	}
}