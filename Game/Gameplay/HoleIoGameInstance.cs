using System.Numerics;
using HoleIO.Engine.Core;
using HoleIO.Engine.Gameplay;
using HoleIO.Engine.Rendering;
using HoleIO.Engine.Utility;
using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using Shader = HoleIO.Engine.Rendering.Shader;

namespace HoleIO.Gameplay
{
    public class HoleIoGameInstance : GameInstance
    {
        private StaticMesh spear = null!;
        private Shader standard = null!;
        
        //Setup the camera's location, directions, and movement speed
        private static Vector3 cameraPosition = new Vector3(0.0f, 0.0f, 100.0f);
        private static Vector3 cameraFront = new Vector3(0.0f, 0.0f, -1.0f);
        private static Vector3 cameraUp = Vector3.UnitY;
        private static Vector3 cameraDirection = Vector3.Zero;
        private static float cameraYaw = -90f;
        private static float cameraPitch = 0f;
        private static float cameraZoom = 45f;

        public override void BeginPlay()
        {
            this.spear = StaticMesh.LoadFromAssimp("SM_Soulspear", PostProcessSteps.CalculateTangentSpace, true);
            this.standard = new Shader(new Dictionary<ShaderType, string>
                {
                    { ShaderType.FragmentShader, "standard" },
                    { ShaderType.VertexShader, "standard" },
                }
            );
        }

        public override void Tick(double deltaTime)
        {
        }

        public override void Render(double deltaTime)
        {
            this.standard.Bind();
            
            //Use elapsed time to convert to radians to allow our cube to rotate over time
            float difference = Time.ElapsedTime * 100;

            Vector2 size = Application.OpenGlWindow().Size;
            
            Matrix4x4 model = Matrix4x4.CreateRotationY(MathHelper.DegreesToRadians(difference)) * Matrix4x4.CreateScale(new Vector3(0.1f, 0.1f, 0.1f));
            Matrix4x4 view = Matrix4x4.CreateLookAt(cameraPosition, cameraPosition + cameraFront, cameraUp);
            //Note that the apsect ratio calculation must be performed as a float, otherwise integer division will be performed (truncating the result).
            Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(cameraZoom), size.X / size.Y, 0.1f, 1000.0f);

            this.standard.SetUniform("uModel", model);
            this.standard.SetUniform("uView", view);
            this.standard.SetUniform("uProjection", projection);

            this.spear.Render();

            this.standard.Unbind();
        }

        public override void EndPlay()
        {
        }
    }
}