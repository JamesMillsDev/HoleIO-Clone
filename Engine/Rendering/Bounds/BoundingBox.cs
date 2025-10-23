using System.Numerics;

namespace HoleIO.Engine.Rendering.Bounds
{
	/// <summary>
	/// Axis-aligned bounding box for frustum culling tests.
	/// </summary>
	public struct BoundingBox(Vector3 min, Vector3 max)
	{
		public Vector3 min = min;
		public Vector3 max = max;

		/// <summary>
		/// Creates a bounding box from center point and extents (half-sizes).
		/// </summary>
		public static BoundingBox FromCenterAndExtents(Vector3 center, Vector3 extents)
		{
			return new BoundingBox(center - extents, center + extents);
		}

		/// <summary>
		/// Gets the center point of the bounding box.
		/// </summary>
		public Vector3 Center => (this.min + this.max) * 0.5f;

		/// <summary>
		/// Gets the extents (half-sizes) of the bounding box.
		/// </summary>
		public Vector3 Extents => (this.max - this.min) * 0.5f;

		/// <summary>
		/// Gets all 8 corners of the bounding box.
		/// </summary>
		public Vector3[] GetCorners()
		{
			return
			[
				new Vector3(this.min.X, this.min.Y, this.min.Z),
				new Vector3(this.max.X, this.min.Y, this.min.Z),
				new Vector3(this.max.X, this.max.Y, this.min.Z),
				new Vector3(this.min.X, this.max.Y, this.min.Z),
				new Vector3(this.min.X, this.min.Y, this.max.Z),
				new Vector3(this.max.X, this.min.Y, this.max.Z),
				new Vector3(this.max.X, this.max.Y, this.max.Z),
				new Vector3(this.min.X, this.max.Y, this.max.Z)
			];
		}

		/// <summary>
		/// Transforms the bounding box by a matrix.
		/// </summary>
		public BoundingBox Transform(Matrix4x4 matrix)
		{
			Vector3[] corners = GetCorners();
			Vector3 min = new Vector3(float.MaxValue);
			Vector3 max = new Vector3(float.MinValue);

			foreach (Vector3 corner in corners)
			{
				Vector3 transformed = Vector3.Transform(corner, matrix);
				min = Vector3.Min(min, transformed);
				max = Vector3.Max(max, transformed);
			}

			return new BoundingBox(min, max);
		}
	}
}