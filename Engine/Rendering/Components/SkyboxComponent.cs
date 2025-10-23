using System.Numerics;
using HoleIO.Engine.Gameplay.Actors;
using HoleIO.Engine.Gameplay.Scenes;
using HoleIO.Engine.Rendering.Textures;
using Silk.NET.OpenGL;

namespace HoleIO.Engine.Rendering.Components
{
	public class SkyboxComponent : ActorComponent
	{
		private StaticMesh? mesh;
		private Material? material;

		public override void BeginPlay()
		{
			this.mesh = StaticMesh.Cube(true);
			this.material = new Material(
				new Shader(
					new KeyValuePair<ShaderType, string>(ShaderType.VertexShader, "skybox"),
					new KeyValuePair<ShaderType, string>(ShaderType.FragmentShader, "skybox")
				)
			);

			this.material.SetTexture("skyTexture", new Cubemap("skybox"));
			this.material.Shader.SetDepthFunc(DepthFunction.Lequal);
		}

		public override void Render()
		{
			// Skip rendering if mesh or shader aren't configured
			if (this.mesh == null || this.material == null)
			{
				return;
			}

			// Get the main camera from the current scene
			CameraComponent? cam = SceneManager.Instance.Current?.MainCamera;

			// Skip rendering if no active camera exists
			if (cam == null)
			{
				return;
			}

			// Activate the shader program for rendering
			this.material.Bind(this.Actor.Scene.LightData);

			try
			{
				// Set the model matrix (world transform)
				// Combines actor's transform with mesh's scale factor (from file metadata)
				// Scale factor converts from the mesh's original units to game units
				this.material.Shader.Set("model", this.Transform.GetLocalToWorldMatrix() *
				                                  Matrix4x4.CreateScale(Vector3.One * this.mesh.scaleFactor));
			}
			catch (Exception)
			{
				// Ignored... this can be ignored as not all shaders need this uniform
			}

			try
			{
				// Set the view matrix (camera position/orientation)
				this.material.Shader.Set("view", cam.View);
			}
			catch (Exception)
			{
				// Ignored... this can be ignored as not all shaders need this uniform
			}

			try
			{
				// Set the projection matrix (perspective/orthographic)
				this.material.Shader.Set("projection", cam.Projection);
			}
			catch (Exception)
			{
				// Ignored... this can be ignored as not all shaders need this uniform
			}

			try
			{
				// Set the camera position vector (view matrix with no orientation)
				this.material.Shader.Set("cameraPosition", cam.Transform.Position);
			}
			catch (Exception)
			{
				// Ignored... this can be ignored as not all shaders need this uniform
			}

			// Issue the actual draw call to OpenGL
			this.mesh.Render();

			// Deactivate the shader program
			this.material.Unbind();
		}
	}
}