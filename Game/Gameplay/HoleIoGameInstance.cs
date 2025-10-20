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
        private StaticMesh spear;
        private Shader standard;
        
        //Setup the camera's location, directions, and movement speed
        private static Vector3 CameraPosition = new Vector3(0.0f, 0.0f, 3.0f);
        private static Vector3 CameraFront = new Vector3(0.0f, 0.0f, -1.0f);
        private static Vector3 CameraUp = Vector3.UnitY;
        private static Vector3 CameraDirection = Vector3.Zero;
        private static float CameraYaw = -90f;
        private static float CameraPitch = 0f;
        private static float CameraZoom = 45f;

        public override void BeginPlay()
        {
            spear = StaticMesh.LoadFromAssimp("SM_Soulspear", PostProcessSteps.CalculateTangentSpace, true);
            standard = new Shader(new Dictionary<ShaderType, string>
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
            standard.Bind();
            
            //Use elapsed time to convert to radians to allow our cube to rotate over time
            var difference = (float) (Application.Time * 100);

            var size = Application.FramebufferSize;
            
            var model = Matrix4x4.CreateRotationY(MathHelper.DegreesToRadians(difference)) * Matrix4x4.CreateRotationX(MathHelper.DegreesToRadians(difference));
            var view = Matrix4x4.CreateLookAt(CameraPosition, CameraPosition + CameraFront, CameraUp);
            //Note that the apsect ratio calculation must be performed as a float, otherwise integer division will be performed (truncating the result).
            var projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(CameraZoom), (float)size.X / size.Y, 0.1f, 100.0f);
            
            standard.SetUniform("uModel", model);
            standard.SetUniform("uView", view);
            standard.SetUniform("uProjection", projection);
            
            spear.Render();
            
            standard.Unbind();
        }

        public override void EndPlay()
        {
        }
    }
}