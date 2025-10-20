namespace HoleIO.Engine.Gameplay
{
	public abstract class GameInstance
	{
		public Scene currentScene = new ();
		
		public abstract void BeginPlay();
		
		public abstract void Tick();
		public abstract void Render();
		
		public abstract void EndPlay();
	}
}