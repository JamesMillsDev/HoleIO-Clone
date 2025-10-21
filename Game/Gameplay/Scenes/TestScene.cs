using System.Numerics;
using HoleIO.Engine.Core;
using HoleIO.Engine.Gameplay.Actors;
using HoleIO.Engine.Rendering;
using HoleIO.Engine.Rendering.Components;
using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using Scene = HoleIO.Engine.Gameplay.Scenes.Scene;
using Shader = HoleIO.Engine.Rendering.Shader;

namespace HoleIO.Gameplay.Scenes
{
	public class TestScene() : Scene("Test")
	{
		private Actor? meshActor;
		
		public override void OnLoaded()
		{
			StaticMesh spear = StaticMesh.LoadFromAssimp("SM_Soulspear", PostProcessSteps.CalculateTangentSpace, true);
			Shader standard = new(
				new(ShaderType.FragmentShader, "standard"),
				new(ShaderType.VertexShader, "standard")
			);

			Actor cameraActor = Spawn<Actor>();
			cameraActor.AddComponent<CameraComponent>();
			cameraActor.Transform.Position = new Vector3(0.0f, 0.0f, 100.0f);

			this.meshActor = Spawn<Actor>();
			this.meshActor.Transform.LocalScale = Vector3.One * .1f;
			StaticMeshComponent mesh = this.meshActor.AddComponent<StaticMeshComponent>();
			mesh.Shader = standard;
			mesh.Mesh = spear;
		}

		public override void Tick()
		{
			//Use elapsed time to convert to radians to allow our cube to rotate over time
			float difference = Time.ElapsedTime * 100;

			this.meshActor!.Transform.LocalEulerAngles = new Vector3(0, difference, 0);
		}
	}
}