namespace HoleIO.Engine.Gameplay.Actors
{
	public class Actor
	{
		public ActorTransform Transform { get; set; }

		private readonly List<ActorComponent> components = [];

		private readonly List<ActorComponent> pendingAddComponents = [];
		private readonly List<ActorComponent> pendingRemoveComponents = [];

		public Actor()
		{
			this.Transform = new ActorTransform(this);
		}

		public TComponent? GetComponent<TComponent>() where TComponent : ActorComponent
		{
			return this.components.OfType<TComponent>().Select(component => component).FirstOrDefault();
		}

		public TComponent[] GetComponents<TComponent>() where TComponent : ActorComponent
		{
			return this.components.OfType<TComponent>().Select(component => component).ToArray();
		}

		public TComponent[] GetComponentsInChildren<TComponent>() where TComponent : ActorComponent
		{
			List<TComponent> found = this.components.OfType<TComponent>().Select(component => component).ToList();

			foreach (ActorTransform child in this.Transform)
			{
				found.AddRange(child.owner.GetComponents<TComponent>());
			}
			
			return found.ToArray();
		}

		public TComponent[] GetComponentsInParent<TComponent>() where TComponent : ActorComponent
		{
			List<TComponent> found = GetComponents<TComponent>().ToList();

			found.AddRange(this.Transform.Parent?.owner.GetComponentsInParent<TComponent>() ?? []);
			
			return found.ToArray();
		}

		public TComponent AddComponent<TComponent>() where TComponent : ActorComponent, new()
		{
			TComponent component = new();

			this.pendingAddComponents.Add(component);

			return component;
		}

		public void RemoveComponent(ActorComponent component)
		{
			if (!this.components.Contains(component))
			{
				this.pendingAddComponents.Remove(component);
				this.pendingRemoveComponents.Remove(component);

				return;
			}

			this.pendingRemoveComponents.Add(component);
		}

		public virtual void BeginPlay()
		{
			foreach (ActorComponent component in this.components)
			{
				component.BeginPlay();
			}

			foreach (ActorTransform child in this.Transform)
			{
				child.owner.BeginPlay();
			}
		}

		public virtual void Tick()
		{
			foreach (ActorComponent component in this.components)
			{
				component.Tick();
			}

			foreach (ActorTransform child in this.Transform)
			{
				child.owner.Tick();
			}
		}

		public virtual void Render()
		{
			foreach (ActorComponent component in this.components)
			{
				component.Render();
			}

			foreach (ActorTransform child in this.Transform)
			{
				child.owner.Render();
			}
		}

		public virtual void EndPlay()
		{
			foreach (ActorComponent component in this.components)
			{
				component.EndPlay();
			}

			foreach (ActorTransform child in this.Transform)
			{
				child.owner.EndPlay();
			}
		}

		internal void ApplyChanges()
		{
			foreach (ActorComponent pendingComponent in this.pendingAddComponents)
			{
				pendingComponent.BeginPlay();
				this.components.Add(pendingComponent);
			}

			foreach (ActorComponent pendingRemoveComponent in this.pendingRemoveComponents)
			{
				pendingRemoveComponent.EndPlay();
				this.components.Remove(pendingRemoveComponent);
			}

			this.pendingAddComponents.Clear();
			this.pendingRemoveComponents.Clear();

			this.Transform.ApplyChanges();
			foreach (ActorTransform child in this.Transform)
			{
				child.ApplyChanges();
			}
			
			foreach (ActorTransform child in this.Transform)
			{
				child.owner.ApplyChanges();
			}
		}
	}
}