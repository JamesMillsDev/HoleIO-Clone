using System.Collections;
using System.Numerics;
using HoleIO.Engine.Utility;

namespace HoleIO.Engine.Gameplay.Actors
{
	/// <summary>
	/// Represents the spatial transformation of an Actor in 3D space.
	/// Manages position, rotation, and scale in both local (relative to parent) and world coordinates.
	/// Supports hierarchical parent-child relationships similar to Unity's Transform component.
	/// </summary>
	public class ActorTransform(Actor owner) : IEnumerable<ActorTransform>
	{
		#region Local Properties

		/// <summary>
		/// Gets or sets the position relative to the parent transform.
		/// If there's no parent, this is the same as world position.
		/// </summary>
		public Vector3 LocalPosition
		{
			get => this.localMatrix.Translation;
			set
			{
				// Directly modify translation components of the matrix
				this.localMatrix.M41 = value.X;
				this.localMatrix.M42 = value.Y;
				this.localMatrix.M43 = value.Z;
			}
		}

		/// <summary>
		/// Gets or sets the rotation as a quaternion relative to the parent transform.
		/// </summary>
		public Quaternion LocalRotation
		{
			get => Quaternion.CreateFromRotationMatrix(GetRotationMatrix(this.localMatrix));
			set
			{
				// Preserve position and scale while updating rotation
				Vector3 pos = this.LocalPosition;
				Vector3 scale = this.LocalScale;

				// Rebuild matrix with new rotation
				this.localMatrix = Matrix4x4.CreateTranslation(pos) *
				                   Matrix4x4.CreateFromQuaternion(value) *
				                   Matrix4x4.CreateScale(scale);
			}
		}

		/// <summary>
		/// Gets or sets the rotation as Euler angles (in degrees) relative to the parent transform.
		/// Uses XYZ rotation order (pitch, yaw, roll).
		/// </summary>
		public Vector3 LocalEulerAngles
		{
			get => QuaternionToEuler(this.LocalRotation);
			set => this.LocalRotation = EulerToQuaternion(value);
		}

		/// <summary>
		/// Gets or sets the scale relative to the parent transform.
		/// </summary>
		public Vector3 LocalScale
		{
			get => ExtractScale(this.localMatrix);
			set
			{
				// Preserve position and rotation while updating scale
				Vector3 pos = this.LocalPosition;
				Quaternion rot = this.LocalRotation;

				// Rebuild matrix with new scale
				this.localMatrix = Matrix4x4.CreateTranslation(pos) *
				                   Matrix4x4.CreateFromQuaternion(rot) *
				                   Matrix4x4.CreateScale(value);
			}
		}

		#endregion

		#region World Properties

		/// <summary>
		/// Gets or sets the position in world space.
		/// When setting, automatically adjusts local position to account for parent transforms.
		/// </summary>
		public Vector3 Position
		{
			get => GetLocalToWorldMatrix().Translation;
			set
			{
				// If no parent, world position equals local position
				if (this.Parent == null)
				{
					this.LocalPosition = value;
					return;
				}

				// Get current world and parent world matrices
				Matrix4x4 worldMatrix = GetLocalToWorldMatrix();
				Matrix4x4 parentWorldMatrix = this.Parent.GetLocalToWorldMatrix();

				// Invert parent matrix to convert from world to parent-local space
				if (!Matrix4x4.Invert(parentWorldMatrix, out Matrix4x4 invParent))
				{
					return; // Cannot invert, skip update
				}

				// Update world position and convert back to local space
				worldMatrix.M41 = value.X;
				worldMatrix.M42 = value.Y;
				worldMatrix.M43 = value.Z;
				this.localMatrix = worldMatrix * invParent;
			}
		}

		/// <summary>
		/// Gets or sets the rotation in world space.
		/// When setting, automatically adjusts local rotation to account for parent transforms.
		/// </summary>
		public Quaternion Rotation
		{
			get => Quaternion.CreateFromRotationMatrix(GetRotationMatrix(GetLocalToWorldMatrix()));
			set
			{
				// If no parent, world rotation equals local rotation
				if (this.Parent == null)
				{
					this.LocalRotation = value;
					return;
				}

				// Convert world rotation to local rotation by removing parent's rotation
				Quaternion parentRot = this.Parent.Rotation;
				this.LocalRotation = Quaternion.Multiply(Quaternion.Inverse(parentRot), value);
			}
		}

		/// <summary>
		/// Gets or sets the rotation as Euler angles (in degrees) in world space.
		/// </summary>
		public Vector3 EulerAngles
		{
			get => QuaternionToEuler(this.Rotation);
			set => this.Rotation = EulerToQuaternion(value);
		}

		/// <summary>
		/// Gets the global scale of the object in world space.
		/// "Lossy" because scale can't always be perfectly represented when 
		/// parent rotations are involved (e.g., non-uniform scale with rotation).
		/// This property is read-only.
		/// </summary>
		public Vector3 LossyScale => ExtractScale(GetLocalToWorldMatrix());

		#endregion

		#region Directional Vectors

		/// <summary>
		/// Gets the forward direction (positive Z axis) of the transform in world space.
		/// Normalized to unit length.
		/// </summary>
		public Vector3 Forward
		{
			get
			{
				Matrix4x4 worldMatrix = GetLocalToWorldMatrix();
				return Vector3.Normalize(new Vector3(worldMatrix.M31, worldMatrix.M32, worldMatrix.M33));
			}
		}

		/// <summary>
		/// Gets the up direction (positive Y axis) of the transform in world space.
		/// Normalized to unit length.
		/// </summary>
		public Vector3 Up
		{
			get
			{
				Matrix4x4 worldMatrix = GetLocalToWorldMatrix();
				return Vector3.Normalize(new Vector3(worldMatrix.M21, worldMatrix.M22, worldMatrix.M23));
			}
		}

		/// <summary>
		/// Gets the right direction (positive X axis) of the transform in world space.
		/// Normalized to unit length.
		/// </summary>
		public Vector3 Right
		{
			get
			{
				Matrix4x4 worldMatrix = GetLocalToWorldMatrix();
				return Vector3.Normalize(new Vector3(worldMatrix.M11, worldMatrix.M12, worldMatrix.M13));
			}
		}

		#endregion

		#region Backing Fields

		// Local transformation matrix (position, rotation, scale relative to parent)
		private Matrix4x4 localMatrix = Matrix4x4.Identity;

		// List of child transforms in the hierarchy
		private readonly List<ActorTransform> children = [];

		// Pending reparent operation (deferred execution pattern)
		private ActorTransform? pendingParent;
		private bool hasPendingReparent;

		// Reference to the Actor that owns this transform
		internal readonly Actor owner = owner;

		#endregion

		#region Hierarchy

		/// <summary>
		/// Gets the parent transform in the hierarchy, or null if this is a root transform.
		/// </summary>
		public ActorTransform? Parent { get; private set; }

		/// <summary>
		/// Gets a read-only list of all direct children of this transform.
		/// </summary>
		public IReadOnlyList<ActorTransform> Children => this.children;

		/// <summary>
		/// Requests a reparent operation to change this transform's parent.
		/// The operation is deferred until ApplyChanges() is called.
		/// </summary>
		/// <param name="newParent">The new parent transform, or null to become a root transform</param>
		/// <param name="worldPositionStays">If true, maintains world position/rotation/scale (currently not implemented)</param>
		public void SetParent(ActorTransform? newParent, bool worldPositionStays = true)
		{
			this.pendingParent = newParent;
			this.hasPendingReparent = true;
		}

		/// <summary>
		/// Applies all pending changes (such as reparenting operations).
		/// This deferred execution pattern allows safe hierarchy modifications during iteration.
		/// </summary>
		public void ApplyChanges()
		{
			if (!this.hasPendingReparent)
			{
				return;
			}

			ApplyReparent();
			this.hasPendingReparent = false;
		}

		/// <summary>
		/// Internal method that performs the actual reparenting operation.
		/// Maintains the world transform by recalculating the local matrix.
		/// </summary>
		private void ApplyReparent()
		{
			// No-op if parent hasn't actually changed
			if (this.pendingParent == this.Parent)
			{
				return;
			}

			// Store current world matrix to maintain world position/rotation/scale
			Matrix4x4 worldMatrix = GetLocalToWorldMatrix();

			// Remove from old parent's children list
			this.Parent?.children.Remove(this);

			// Update parent reference
			this.Parent = this.pendingParent;

			// Add to new parent's children list
			this.Parent?.children.Add(this);

			// Recalculate local matrix to maintain world transform
			if (this.Parent != null)
			{
				// Convert world matrix to local space relative to new parent
				Matrix4x4 parentWorldMatrix = this.Parent.GetLocalToWorldMatrix();
				if (Matrix4x4.Invert(parentWorldMatrix, out Matrix4x4 invParent))
				{
					this.localMatrix = worldMatrix * invParent;
				}
			}
			else
			{
				// No parent means local matrix equals world matrix
				this.localMatrix = worldMatrix;
			}
		}

		#endregion

		#region Matrix Operations

		/// <summary>
		/// Gets the local transformation matrix (relative to parent).
		/// </summary>
		/// <returns>4x4 transformation matrix containing position, rotation, and scale</returns>
		public Matrix4x4 GetLocalMatrix() => this.localMatrix;

		/// <summary>
		/// Gets the world transformation matrix by combining this transform with all parent transforms.
		/// Recursively multiplies local matrices up the hierarchy.
		/// </summary>
		/// <returns>4x4 transformation matrix in world space</returns>
		public Matrix4x4 GetLocalToWorldMatrix()
		{
			if (this.Parent == null)
			{
				return this.localMatrix;
			}

			// Multiply local matrix by parent's world matrix (right-to-left multiplication order)
			return this.localMatrix * this.Parent.GetLocalToWorldMatrix();
		}

		#endregion

		#region Utility Methods

		/// <summary>
		/// Extracts the scale components from a transformation matrix.
		/// Calculates scale by measuring the length of the basis vectors.
		/// </summary>
		/// <param name="matrix">The matrix to extract scale from</param>
		/// <returns>Scale vector (X, Y, Z)</returns>
		private static Vector3 ExtractScale(Matrix4x4 matrix)
		{
			// Scale is the length of each basis vector
			float scaleX = new Vector3(matrix.M11, matrix.M12, matrix.M13).Length();
			float scaleY = new Vector3(matrix.M21, matrix.M22, matrix.M23).Length();
			float scaleZ = new Vector3(matrix.M31, matrix.M32, matrix.M33).Length();
			return new Vector3(scaleX, scaleY, scaleZ);
		}

		/// <summary>
		/// Extracts the rotation component from a transformation matrix by removing scale.
		/// </summary>
		/// <param name="matrix">The matrix to extract rotation from</param>
		/// <returns>A matrix containing only rotation (no scale or translation)</returns>
		private static Matrix4x4 GetRotationMatrix(Matrix4x4 matrix)
		{
			Vector3 scale = ExtractScale(matrix);

			// Avoid division by zero if any scale component is zero
			if (scale.X == 0 || scale.Y == 0 || scale.Z == 0)
			{
				return Matrix4x4.Identity;
			}

			// Divide each basis vector by its scale to normalize and remove scale
			return new Matrix4x4(
				matrix.M11 / scale.X, matrix.M12 / scale.X, matrix.M13 / scale.X, 0,
				matrix.M21 / scale.Y, matrix.M22 / scale.Y, matrix.M23 / scale.Y, 0,
				matrix.M31 / scale.Z, matrix.M32 / scale.Z, matrix.M33 / scale.Z, 0,
				0, 0, 0, 1
			);
		}

		#endregion

		#region Euler/Quaternion Conversion

		/// <summary>
		/// Converts a quaternion to Euler angles (in degrees).
		/// Uses XYZ rotation order (pitch, yaw, roll).
		/// Handles gimbal lock edge cases.
		/// </summary>
		/// <param name="q">The quaternion to convert</param>
		/// <returns>Euler angles in degrees (X=pitch, Y=yaw, Z=roll)</returns>
		private static Vector3 QuaternionToEuler(Quaternion q)
		{
			Vector3 angles;

			// Pitch (x-axis rotation)
			double sinp = 2 * (q.W * q.Y - q.Z * q.X);
			if (Maths.Abs((float)sinp) >= 1)
			{
				// Gimbal lock case: use ±90 degrees
				angles.X = Maths.Sign((float)sinp) * Maths.Pi / 2f;
			}
			else
			{
				angles.X = Maths.Asin((float)sinp);
			}

			// Yaw (y-axis rotation)
			double sinyCosp = 2 * (q.W * q.Z + q.X * q.Y);
			double cosyCosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
			angles.Y = Maths.Atan2((float)sinyCosp, (float)cosyCosp);

			// Roll (z-axis rotation)
			double sinrCosp = 2 * (q.W * q.X + q.Y * q.Z);
			double cosrCosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
			angles.Z = Maths.Atan2((float)sinrCosp, (float)cosrCosp);

			// Convert radians to degrees
			return new Vector3(
				angles.X * Maths.Rad2Deg,
				angles.Y * Maths.Rad2Deg,
				angles.Z * Maths.Rad2Deg
			);
		}

		/// <summary>
		/// Converts Euler angles (in degrees) to a quaternion.
		/// Uses XYZ rotation order (pitch, yaw, roll).
		/// </summary>
		/// <param name="euler">Euler angles in degrees (X=pitch, Y=yaw, Z=roll)</param>
		/// <returns>Quaternion representing the same rotation</returns>
		private static Quaternion EulerToQuaternion(Vector3 euler)
		{
			// Convert degrees to radians
			Vector3 rad = new(
				euler.X * Maths.Deg2Rad,
				euler.Y * Maths.Deg2Rad,
				euler.Z * Maths.Deg2Rad
			);

			// Create quaternion using yaw-pitch-roll order
			return Quaternion.CreateFromYawPitchRoll(rad.Y, rad.X, rad.Z);
		}

		#endregion

		#region IEnumerable Implementation

		/// <summary>
		/// Enumerates all direct children of this transform.
		/// Allows using foreach loops over a transform's children.
		/// </summary>
		/// <returns>Enumerator for iterating through child transforms</returns>
		public IEnumerator<ActorTransform> GetEnumerator()
		{
			return this.children.GetEnumerator();
		}

		/// <summary>
		/// Non-generic IEnumerable implementation.
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}