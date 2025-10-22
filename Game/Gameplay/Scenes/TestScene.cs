using System.Drawing;
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
			cameraActor.Transform.Position = new Vector3(0.0f, 4.0f, 10.0f);

			this.meshActor = Spawn<Actor>();
			StaticMeshComponent mesh = this.meshActor.AddComponent<StaticMeshComponent>();
			mesh.Material = new Material(standard);
			mesh.Mesh = spear;

			mesh.Material.SetTexture("baseColorMap", new Texture("T_Soulspear_B", ETextureFormat.Tga));
			mesh.Material.SetTexture("normalMap", new Texture("T_Soulspear_N", ETextureFormat.Tga));

			// Directional light - white light from above-right
			Actor directionalLightActor = Spawn<Actor>();
			directionalLightActor.Transform.EulerAngles = new Vector3(-45, 45, 0);
			LightComponent directionalLight = directionalLightActor.AddComponent<LightComponent>();
			directionalLight.lightType = ELightType.Directional;
			directionalLight.color = Color.White;
			directionalLight.intensity = 0.5f;

			// Point light 1 - Red light on the left
			Actor pointLight1Actor = Spawn<Actor>();
			pointLight1Actor.Transform.Position = new Vector3(-5.0f, 2.0f, 0f);
			LightComponent pointLight1 = pointLight1Actor.AddComponent<LightComponent>();
			pointLight1.lightType = ELightType.Point;
			pointLight1.color = Color.Red;
			pointLight1.intensity = 2.0f;

			// Point light 2 - Blue light on the right
			Actor pointLight2Actor = Spawn<Actor>();
			pointLight2Actor.Transform.Position = new Vector3(5.0f, 2.0f, 0f);
			LightComponent pointLight2 = pointLight2Actor.AddComponent<LightComponent>();
			pointLight2.lightType = ELightType.Point;
			pointLight2.color = Color.Blue;
			pointLight2.intensity = 2.0f;

			// Point light 3 - Green light from behind
			Actor pointLight3Actor = Spawn<Actor>();
			pointLight3Actor.Transform.Position = new Vector3(0.0f, 3.0f, -5.0f);
			LightComponent pointLight3 = pointLight3Actor.AddComponent<LightComponent>();
			pointLight3.lightType = ELightType.Point;
			pointLight3.color = Color.Lime;
			pointLight3.intensity = 1.5f;
		}

		public override void Tick()
		{
			//Use elapsed time to convert to radians to allow our cube to rotate over time
			float difference = Time.ElapsedTime * 100;

			this.meshActor!.Transform.LocalEulerAngles = new Vector3(0, difference, 0);
		}
	}
}