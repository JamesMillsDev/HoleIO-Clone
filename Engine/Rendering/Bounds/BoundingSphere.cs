using System.Numerics;

namespace HoleIO.Engine.Rendering.Bounds
{
	/// <summary>
	/// Bounding sphere for frustum culling tests.
	/// Generally faster than AABB tests but less accurate.
	/// </summary>
	public struct BoundingSphere(Vector3 center, float radius)
	{
		public Vector3 center = center;
		public float radius = radius;

		/// <summary>
		/// Creates a bounding sphere from a bounding box.
		/// </summary>
		public static BoundingSphere FromBoundingBox(BoundingBox box)
		{
			Vector3 center = box.Center;
			float radius = Vector3.Distance(center, box.max);
			return new BoundingSphere(center, radius);
		}

		/// <summary>
		/// Transforms the bounding sphere by a matrix.
		/// Note: Only handles uniform scaling correctly.
		/// </summary>
		public BoundingSphere Transform(Matrix4x4 matrix)
		{
			Vector3 transformedCenter = Vector3.Transform(this.center, matrix);

			// Extract scale from matrix (assumes uniform scale)
			Vector3 scale = new Vector3(
				new Vector3(matrix.M11, matrix.M12, matrix.M13).Length(),
				new Vector3(matrix.M21, matrix.M22, matrix.M23).Length(),
				new Vector3(matrix.M31, matrix.M32, matrix.M33).Length()
			);

			float maxScale = Math.Max(Math.Max(scale.X, scale.Y), scale.Z);
			return new BoundingSphere(transformedCenter, this.radius * maxScale);
		}
	}
}