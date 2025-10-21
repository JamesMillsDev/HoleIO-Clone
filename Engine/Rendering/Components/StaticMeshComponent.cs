using System.Numerics;
using HoleIO.Engine.Gameplay;
using HoleIO.Engine.Gameplay.Actors;
using HoleIO.Engine.Gameplay.Scenes;

namespace HoleIO.Engine.Rendering.Components
{
    /// <summary>
    /// Component that renders a static 3D mesh at the actor's transform location.
    /// Combines a mesh, shader, and transform to draw geometry in the scene.
    /// Similar to Unity's MeshRenderer + MeshFilter combination.
    /// </summary>
    public class StaticMeshComponent : ActorComponent
    {
       /// <summary>
       /// Gets or sets the static mesh to render.
       /// Contains the vertex data, indices, and OpenGL buffer handles.
       /// </summary>
       public StaticMesh? Mesh { get; set; }
       
       /// <summary>
       /// Gets or sets the shader program to use for rendering this mesh.
       /// The shader defines how the mesh is drawn (lighting, materials, etc.).
       /// </summary>
       public Shader? Shader { get; set; }

       /// <summary>
       /// Renders the static mesh using the configured shader and camera.
       /// Called every frame during the render phase.
       /// </summary>
       public override void Render()
       {
          // Skip rendering if mesh or shader aren't configured
          if (this.Mesh == null || this.Shader == null)
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
          this.Shader.Bind();

          // Set the model matrix (world transform)
          // Combines actor's transform with mesh's scale factor (from file metadata)
          // Scale factor converts from the mesh's original units to game units
          this.Shader.Set("model", this.Transform.GetLocalToWorldMatrix() *
                                          Matrix4x4.CreateScale(Vector3.One * this.Mesh.scaleFactor));
          
          // Set the view matrix (camera position/orientation)
          this.Shader.Set("view", cam.View);
          
          // Set the projection matrix (perspective/orthographic)
          this.Shader.Set("projection", cam.Projection);

          // Issue the actual draw call to OpenGL
          this.Mesh.Render();

          // Deactivate the shader program
          this.Shader.Unbind();
       }
    }
}