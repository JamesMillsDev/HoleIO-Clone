using HoleIO.Engine.Gameplay;
using HoleIO.Engine.Gameplay.Scenes;
using HoleIO.Gameplay.Scenes;

namespace HoleIO.Gameplay
{
	public class HoleIoGameInstance : GameInstance
	{
		public override void BeginPlay()
		{
			SceneManager.Instance.AddScene(new TestScene());
			SceneManager.Instance.Load("Test");
		}

		public override void Tick()
		{
		}

		public override void Render()
		{
			
		}

		public override void EndPlay()
		{
		}
	}
}