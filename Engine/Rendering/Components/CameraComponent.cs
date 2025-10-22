using System.Numerics;
using HoleIO.Engine.Core;
using HoleIO.Engine.Gameplay.Actors;
using HoleIO.Engine.Utility;

namespace HoleIO.Engine.Rendering.Components
{
	/// <summary>
	/// Component that represents a camera viewpoint in the scene.
	/// Generates view and projection matrices for rendering from the camera's perspective.
	/// </summary>
	public class CameraComponent : ActorComponent
	{
		/// <summary>
		/// Gets the aspect ratio (width/height) of the current window.
		/// Dynamically queries the window size to handle window resizing.
		/// </summary>
		private static float Aspect
		{
			get
			{
				Vector2 size = Application.OpenGlWindow().Size;
				return size.X / size.Y;
			}
		}

		/// <summary>
		/// Gets the view matrix that transforms world space to camera space.
		/// Positions the camera at the actor's transform position, looking along the forward vector.
		/// Note: Forward Z is negated to convert from right-handed to left-handed coordinate system
		/// (or vice versa, depending on your engine's coordinate system convention).
		/// </summary>
		public Matrix4x4 View
		{
			get
			{
				ActorTransform transform = this.Transform;
				Vector3 forward = transform.Forward;

				// Negate Z component of forward vector for coordinate system conversion
				forward.Z *= -1;

				// Create look-at matrix: position, target (position + forward direction), up vector
				return Matrix4x4.CreateLookAt(transform.Position, transform.Position + forward, transform.Up);
			}
		}

		/// <summary>
		/// Gets the perspective projection matrix that transforms camera space to clip space.
		/// Creates a frustum based on field of view, aspect ratio, and near/far planes.
		/// </summary>
		public Matrix4x4 Projection => Matrix4x4.CreatePerspectiveFieldOfView(
			this.Fov * Maths.Deg2Rad, // Convert FOV from degrees to radians
			Aspect, // Width/height ratio
			this.NearPlane, // Near clipping plane distance
			this.FarPlane // Far clipping plane distance
		);

		/// <summary>
		/// Gets or sets the vertical field of view in degrees.
		/// Controls how "wide" the camera lens is (smaller = more zoomed in, larger = wider view).
		/// Default is 45 degrees (typical for games).
		/// </summary>
		public float Fov { get; set; } = 45f;

		/// <summary>
		/// Gets or sets the near clipping plane distance.
		/// Objects closer than this distance are not rendered.
		/// Default is 0.1 units. Too small values can cause z-fighting issues.
		/// </summary>
		public float NearPlane { get; set; } = .1f;

		/// <summary>
		/// Gets or sets the far clipping plane distance.
		/// Objects farther than this distance are not rendered.
		/// Default is 1000 units. Larger values reduce depth buffer precision.
		/// </summary>
		public float FarPlane { get; set; } = 1000f;
	}
}