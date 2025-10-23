using System.Numerics;

namespace HoleIO.Engine.Rendering.Bounds
{
	public static class Culling
	{
		/// <summary>
		/// Creates a frustum from camera view and projection matrices.
		/// </summary>
		public static Frustum CreateFrustum(Matrix4x4 view, Matrix4x4 projection)
		{
			Frustum frustum = new();
			frustum.ExtractFromMatrix(projection * view);
			return frustum;
		}

		/// <summary>
		/// Culls a list of objects based on their bounding spheres.
		/// Returns only visible objects.
		/// </summary>
		public static List<T> CullBySphere<T>(Frustum frustum, IEnumerable<T> objects,
			Func<T, BoundingSphere> getBounds) =>
			(from obj in objects let bounds = getBounds(obj) where frustum.IsVisible(bounds) select obj).ToList();

		/// <summary>
		/// Culls a list of objects based on their bounding boxes.
		/// Returns only visible objects.
		/// </summary>
		public static List<T> CullByBox<T>(Frustum frustum, IEnumerable<T> objects, Func<T, BoundingBox> getBounds) =>
			(from obj in objects let bounds = getBounds(obj) where frustum.IsVisible(bounds) select obj).ToList();
	}
}