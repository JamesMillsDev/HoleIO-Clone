using System.Numerics;
using HoleIO.Engine.Core;
using HoleIO.Engine.Gameplay.Actors;
using HoleIO.Engine.Rendering;
using HoleIO.Engine.Rendering.Components;
using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using Material = HoleIO.Engine.Rendering.Material;
using Scene = HoleIO.Engine.Gameplay.Scenes.Scene;
using Shader = HoleIO.Engine.Rendering.Shader;
using Texture = HoleIO.Engine.Rendering.Texture;

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
			cameraActor.Transform.Position = new Vector3(0.0f, 0.0f, 10.0f);

			this.meshActor = Spawn<Actor>();
			StaticMeshComponent mesh = this.meshActor.AddComponent<StaticMeshComponent>();
			mesh.Material = new Material(standard);
			mesh.Mesh = spear;

			mesh.Material.Textures["diffuse"] = new Texture("T_Soulspear_B", ETextureFormat.Tga);
		}

		public override void Tick()
		{
			//Use elapsed time to convert to radians to allow our cube to rotate over time
			float difference = Time.ElapsedTime * 100;

			this.meshActor!.Transform.LocalEulerAngles = new Vector3(0, difference, 0);
		}
	}
}