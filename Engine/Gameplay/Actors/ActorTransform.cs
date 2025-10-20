using System.Collections;
using System.Numerics;

namespace HoleIO.Engine.Gameplay.Actors
{
	public class ActorTransform(Actor owner) : IEnumerable<ActorTransform>
	{
		#region Local Properties

		public Vector3 LocalPosition
		{
			get => this.localMatrix.Translation;
			set
			{
				this.localMatrix.M41 = value.X;
				this.localMatrix.M42 = value.Y;
				this.localMatrix.M43 = value.Z;
			}
		}

		public Quaternion LocalRotation
		{
			get => Quaternion.CreateFromRotationMatrix(GetRotationMatrix(this.localMatrix));
			set
			{
				Vector3 pos = this.LocalPosition;
				Vector3 scale = this.LocalScale;
				this.localMatrix = Matrix4x4.CreateScale(scale) *
				                   Matrix4x4.CreateFromQuaternion(value) *
				                   Matrix4x4.CreateTranslation(pos);
			}
		}

		public Vector3 LocalEulerAngles
		{
			get => QuaternionToEuler(this.LocalRotation);
			set => this.LocalRotation = EulerToQuaternion(value);
		}

		public Vector3 LocalScale
		{
			get => ExtractScale(this.localMatrix);
			set
			{
				Vector3 pos = this.LocalPosition;
				Quaternion rot = this.LocalRotation;
				this.localMatrix = Matrix4x4.CreateScale(value) *
				                   Matrix4x4.CreateFromQuaternion(rot) *
				                   Matrix4x4.CreateTranslation(pos);
			}
		}

		#endregion

		#region World Properties

		public Vector3 Position
		{
			get => GetWorldMatrix().Translation;
			set
			{
				if (this.Parent == null)
				{
					this.LocalPosition = value;
					return;
				}

				Matrix4x4 worldMatrix = GetWorldMatrix();
				Matrix4x4 parentWorldMatrix = this.Parent.GetWorldMatrix();

				if (!Matrix4x4.Invert(parentWorldMatrix, out Matrix4x4 invParent))
				{
					return;
				}

				worldMatrix.M41 = value.X;
				worldMatrix.M42 = value.Y;
				worldMatrix.M43 = value.Z;
				this.localMatrix = worldMatrix * invParent;
			}
		}

		public Quaternion Rotation
		{
			get => Quaternion.CreateFromRotationMatrix(GetRotationMatrix(GetWorldMatrix()));
			set
			{
				if (this.Parent == null)
				{
					this.LocalRotation = value;
					return;
				}

				Quaternion parentRot = this.Parent.Rotation;
				this.LocalRotation = Quaternion.Multiply(Quaternion.Inverse(parentRot), value);
			}
		}

		public Vector3 EulerAngles
		{
			get => QuaternionToEuler(this.Rotation);
			set => this.Rotation = EulerToQuaternion(value);
		}

		public Vector3 LossyScale => ExtractScale(GetWorldMatrix());

		#endregion

		#region Backing Fields

		private Matrix4x4 localMatrix = Matrix4x4.Identity;

		private readonly List<ActorTransform> children = new();

		// Pending reparent operation
		private ActorTransform? pendingParent;
		private bool hasPendingReparent;

		internal Actor owner = owner;

		#endregion

		#region Hierarchy

		public ActorTransform? Parent { get; private set; }

		public IReadOnlyList<ActorTransform> Children => this.children;

		/// <summary>
		/// Request a reparent operation. Call ApplyChanges() to execute.
		/// </summary>
		public void SetParent(ActorTransform? newParent, bool worldPositionStays = true)
		{
			this.pendingParent = newParent;
			this.hasPendingReparent = true;
		}

		/// <summary>
		/// Apply all pending changes (reparenting, etc.)
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

		private void ApplyReparent()
		{
			if (this.pendingParent == this.Parent)
			{
				return;
			}

			// Store current world matrix
			Matrix4x4 worldMatrix = GetWorldMatrix();

			// Remove from old parent
			this.Parent?.children.Remove(this);

			// Set new parent
			this.Parent = this.pendingParent;

			// Add to new parent
			this.Parent?.children.Add(this);

			// Recalculate local matrix to maintain world transform
			if (this.Parent != null)
			{
				Matrix4x4 parentWorldMatrix = this.Parent.GetWorldMatrix();
				if (Matrix4x4.Invert(parentWorldMatrix, out Matrix4x4 invParent))
				{
					this.localMatrix = worldMatrix * invParent;
				}
			}
			else
			{
				this.localMatrix = worldMatrix;
			}
		}

		#endregion

		#region Matrix Operations

		public Matrix4x4 GetLocalMatrix() => this.localMatrix;

		public Matrix4x4 GetWorldMatrix()
		{
			if (this.Parent == null)
			{
				return this.localMatrix;
			}

			return this.localMatrix * this.Parent.GetWorldMatrix();
		}

		public void SetLocalMatrix(Matrix4x4 matrix)
		{
			this.localMatrix = matrix;
		}

		public void SetWorldMatrix(Matrix4x4 worldMatrix)
		{
			if (this.Parent == null)
			{
				this.localMatrix = worldMatrix;
				return;
			}

			Matrix4x4 parentWorldMatrix = this.Parent.GetWorldMatrix();
			if (Matrix4x4.Invert(parentWorldMatrix, out Matrix4x4 invParent))
			{
				this.localMatrix = worldMatrix * invParent;
			}
		}

		#endregion

		#region Utility Methods

		private static Vector3 ExtractScale(Matrix4x4 matrix)
		{
			float scaleX = new Vector3(matrix.M11, matrix.M12, matrix.M13).Length();
			float scaleY = new Vector3(matrix.M21, matrix.M22, matrix.M23).Length();
			float scaleZ = new Vector3(matrix.M31, matrix.M32, matrix.M33).Length();
			return new Vector3(scaleX, scaleY, scaleZ);
		}

		private static Matrix4x4 GetRotationMatrix(Matrix4x4 matrix)
		{
			Vector3 scale = ExtractScale(matrix);

			// Avoid division by zero
			if (scale.X == 0 || scale.Y == 0 || scale.Z == 0)
			{
				return Matrix4x4.Identity;
			}

			return new Matrix4x4(
				matrix.M11 / scale.X, matrix.M12 / scale.X, matrix.M13 / scale.X, 0,
				matrix.M21 / scale.Y, matrix.M22 / scale.Y, matrix.M23 / scale.Y, 0,
				matrix.M31 / scale.Z, matrix.M32 / scale.Z, matrix.M33 / scale.Z, 0,
				0, 0, 0, 1
			);
		}

		#endregion

		#region Euler/Quaternion Conversion

		private static Vector3 QuaternionToEuler(Quaternion q)
		{
			Vector3 angles;

			// Roll (x-axis rotation)
			double sinrCosp = 2 * (q.W * q.X + q.Y * q.Z);
			double cosrCosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
			angles.X = (float)Math.Atan2(sinrCosp, cosrCosp);

			// Pitch (y-axis rotation)
			double sinp = 2 * (q.W * q.Y - q.Z * q.X);
			if (Math.Abs(sinp) >= 1)
			{
				angles.Y = (float)Math.CopySign(Math.PI / 2, sinp);
			}
			else
			{
				angles.Y = (float)Math.Asin(sinp);
			}

			// Yaw (z-axis rotation)
			double sinyCosp = 2 * (q.W * q.Z + q.X * q.Y);
			double cosyCosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
			angles.Z = (float)Math.Atan2(sinyCosp, cosyCosp);

			// Convert to degrees
			return new Vector3(
				angles.X * (180f / MathF.PI),
				angles.Y * (180f / MathF.PI),
				angles.Z * (180f / MathF.PI)
			);
		}

		private static Quaternion EulerToQuaternion(Vector3 euler)
		{
			// Convert to radians
			Vector3 rad = new(
				euler.X * (MathF.PI / 180f),
				euler.Y * (MathF.PI / 180f),
				euler.Z * (MathF.PI / 180f)
			);

			return Quaternion.CreateFromYawPitchRoll(rad.Z, rad.Y, rad.X);
		}

		#endregion
		
		#region IEnumerable Implementation

		/// <summary>
		/// Enumerates all children of this transform.
		/// </summary>
		public IEnumerator<ActorTransform> GetEnumerator()
		{
			return this.children.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		
		#endregion
	}
}