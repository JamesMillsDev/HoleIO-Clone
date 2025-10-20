using HoleIO.Engine.Gameplay;
using HoleIO.Engine.Rendering;
using Silk.NET.Assimp;

namespace HoleIO.Gameplay
{
	public class HoleIoGameInstance : GameInstance
	{
		private StaticMesh spear;
		
		public override void BeginPlay()
		{
			spear = StaticMesh.LoadFromAssimp("SM_Soulspear", PostProcessSteps.CalculateTangentSpace, true);
		}

		public override void Tick(double deltaTime)
		{
		}

		public override void Render(double deltaTime)
		{
		}

		public override void EndPlay()
		{
		}
	}
}