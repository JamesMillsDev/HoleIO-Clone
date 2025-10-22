using System.Numerics;
using HoleIO.Engine.Core;
using HoleIO.Engine.Gameplay.Actors;
using HoleIO.Engine.Utility;
using Silk.NET.Input;

namespace HoleIO.Engine.Debugging.Components
{
    /// <summary>
    /// Component that provides simple FPS-style camera controls.
    /// WASD for horizontal movement, Q/E for vertical movement, right mouse drag to look around.
    /// </summary>
    public class DebugCameraControlsComponent : ActorComponent
    {
        /// <summary>
        /// Rotation speed in degrees per pixel of mouse movement.
        /// </summary>
        private float turnSpeed;

        /// <summary>
        /// Movement speed in units per second.
        /// </summary>
        private float moveSpeed;

        /// <summary>
        /// Current yaw angle in degrees (horizontal rotation).
        /// </summary>
        private float yaw;

        /// <summary>
        /// Current pitch angle in degrees (vertical rotation).
        /// </summary>
        private float pitch;

        /// <summary>
        /// Initializes camera control parameters and sets initial position.
        /// </summary>
        public override void BeginPlay()
        {
            this.turnSpeed = 0.1f; // Degrees per pixel
            this.moveSpeed = 5f;
            
            // Initialize rotation tracking
            this.yaw = 0f;
            this.pitch = 0f;
            
            // Set initial rotation
            this.Transform.EulerAngles = new Vector3(this.pitch, this.yaw, 0f);
        }

        /// <summary>
        /// Updates camera position and rotation based on input.
        /// </summary>
        public override void Tick()
        {
            float deltaTime = Time.DeltaTime;

            // Right mouse button to rotate camera (do this first so Forward/Right are updated)
            if (Input.IsMouseButtonDown(MouseButton.Right))
            {
                // Use the Input class's built-in mouse delta
                float mouseDeltaX = Input.GetMouseDeltaX();
                float mouseDeltaY = Input.GetMouseDeltaY();

                // Update our tracked rotation angles
                this.yaw += this.turnSpeed * mouseDeltaX;
                this.pitch += this.turnSpeed * mouseDeltaY;

                // Clamp pitch to prevent camera flipping
                this.pitch = Math.Clamp(this.pitch, -89f, 89f);

                // Normalize yaw to stay within 0-360 range (prevents overflow)
                this.yaw = this.yaw % 360f;

                // Apply the rotation to transform
                this.Transform.EulerAngles = new Vector3(this.pitch, this.yaw, 0f);
            }

            // Get direction vectors from the transform (after rotation is applied)
            Vector3 forward = this.Transform.Forward;
            Vector3 right = this.Transform.Right;
            Vector3 up = Vector3.UnitY; // World up for vertical movement

            // WASD movement (forward/backward/strafe)
            if (Input.IsKeyDown(Key.W))
            {
                this.Transform.Position += forward * deltaTime * this.moveSpeed;
            }
            
            if (Input.IsKeyDown(Key.S))
            {
                this.Transform.Position -= forward * deltaTime * this.moveSpeed;
            }
            
            if (Input.IsKeyDown(Key.A))
            {
                this.Transform.Position -= right * deltaTime * this.moveSpeed;
            }
            
            if (Input.IsKeyDown(Key.D))
            {
                this.Transform.Position += right * deltaTime * this.moveSpeed;
            }

            // Q/E for vertical movement (down/up)
            if (Input.IsKeyDown(Key.Q))
            {
                this.Transform.Position -= up * deltaTime * this.moveSpeed;
            }
            
            if (Input.IsKeyDown(Key.E))
            {
                this.Transform.Position += up * deltaTime * this.moveSpeed;
            }
        }
    }
}