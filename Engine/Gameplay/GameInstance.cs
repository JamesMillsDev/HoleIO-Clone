namespace HoleIO.Engine.Gameplay
{
	public abstract class GameInstance
	{
		public Scene currentScene = new ();
		
		public abstract void BeginPlay();
		
		public abstract void Tick(double deltaTime);
		public abstract void Render(double deltaTime);
		
		public abstract void EndPlay();
	}
}