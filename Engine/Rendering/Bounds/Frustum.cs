using System.Numerics;

namespace HoleIO.Engine.Rendering.Bounds
{
    /// <summary>
    /// Represents a view frustum for culling objects outside the camera's view.
    /// </summary>
	public class Frustum
	{
		public Plane Near => this.planes[0];
		public Plane Far => this.planes[1];
		public Plane Left => this.planes[2];
		public Plane Right => this.planes[3];
		public Plane Top => this.planes[4];
		public Plane Bottom => this.planes[5];
		
        // The 6 planes that define the frustum
        // Order: Near, Far, Left, Right, Top, Bottom
		private readonly Plane[] planes = new Plane[6];

        /// <summary>
        /// Extracts frustum planes from a view-projection matrix.
        /// Uses the Gribb-Hartmann method.
        /// </summary>
		public void ExtractFromMatrix(Matrix4x4 viewProjection)
		{
			// Left plane
            this.planes[2] = new Plane(
                new Vector3(
                    viewProjection.M14 + viewProjection.M11,
                    viewProjection.M24 + viewProjection.M21,
                    viewProjection.M34 + viewProjection.M31
                ),
                viewProjection.M44 + viewProjection.M41
            );

            // Right plane
            this.planes[3] = new Plane(
                new Vector3(
                    viewProjection.M14 - viewProjection.M11,
                    viewProjection.M24 - viewProjection.M21,
                    viewProjection.M34 - viewProjection.M31
                ),
                viewProjection.M44 - viewProjection.M41
            );

            // Top plane
            this.planes[4] = new Plane(
                new Vector3(
                    viewProjection.M14 - viewProjection.M12,
                    viewProjection.M24 - viewProjection.M22,
                    viewProjection.M34 - viewProjection.M32
                ),
                viewProjection.M44 - viewProjection.M42
            );

            // Bottom plane
            this.planes[5] = new Plane(
                new Vector3(
                    viewProjection.M14 + viewProjection.M12,
                    viewProjection.M24 + viewProjection.M22,
                    viewProjection.M34 + viewProjection.M32
                ),
                viewProjection.M44 + viewProjection.M42
            );

            // Near plane
            this.planes[0] = new Plane(
                new Vector3(
                    viewProjection.M13,
                    viewProjection.M23,
                    viewProjection.M33
                ),
                viewProjection.M43
            );

            // Far plane
            this.planes[1] = new Plane(
                new Vector3(
                    viewProjection.M14 - viewProjection.M13,
                    viewProjection.M24 - viewProjection.M23,
                    viewProjection.M34 - viewProjection.M33
                ),
                viewProjection.M44 - viewProjection.M43
            );

            // Normalize all planes
            for (int i = 0; i < 6; i++)
            {
                this.planes[i].Normalize();
            }
		}

        /// <summary>
        /// Tests if a point is inside the frustum.
        /// </summary>
        public bool Contains(Vector3 point)
        {
            return this.planes.Any(plane => plane.GetSignedDistance(point) < 0);
        }

        /// <summary>
        /// Tests if a bounding sphere is inside the frustum.
        /// Returns Outside, Intersects, or Inside.
        /// </summary>
        public EContainmentType Contains(BoundingSphere sphere)
        {
            bool allInside = true;

            foreach (Plane plane in this.planes)
            {
                float distance = plane.GetSignedDistance(sphere.center);

                if (distance < -sphere.radius)
                {
                    // Sphere is completely outside this plane
                    return EContainmentType.Outside;
                }

                if (distance < sphere.radius)
                {
                    // Sphere intersects this plane
                    allInside = false;
                }
            }

            return allInside ? EContainmentType.Inside : EContainmentType.Intersects;
        }
        
        /// <summary>
        /// Tests if a bounding box is inside the frustum.
        /// Returns Outside, Intersects, or Inside.
        /// More accurate but slower than sphere test.
        /// </summary>
        public EContainmentType Contains(BoundingBox box)
        {
            bool allInside = true;

            for (int i = 0; i < 6; i++)
            {
                Plane plane = this.planes[i];
                
                // Get the positive vertex (vertex furthest along plane normal)
                Vector3 positiveVertex = new Vector3(
                    plane.normal.X >= 0 ? box.max.X : box.min.X,
                    plane.normal.Y >= 0 ? box.max.Y : box.min.Y,
                    plane.normal.Z >= 0 ? box.max.Z : box.min.Z
                );

                // If positive vertex is outside, box is completely outside
                if (plane.GetSignedDistance(positiveVertex) < 0)
                {
                    return EContainmentType.Outside;
                }

                // Get the negative vertex (vertex furthest against plane normal)
                Vector3 negativeVertex = new(
                    plane.normal.X >= 0 ? box.min.X : box.max.X,
                    plane.normal.Y >= 0 ? box.min.Y : box.max.Y,
                    plane.normal.Z >= 0 ? box.min.Z : box.max.Z
                );

                // If negative vertex is outside, box intersects
                if (plane.GetSignedDistance(negativeVertex) < 0)
                {
                    allInside = false;
                }
            }

            return allInside ? EContainmentType.Inside : EContainmentType.Intersects;
        }
        
        /// <summary>
        /// Fast check if a bounding sphere is visible (not completely outside).
        /// </summary>
        public bool IsVisible(BoundingSphere sphere)
        {
            return Contains(sphere) != EContainmentType.Outside;
        }

        /// <summary>
        /// Fast check if a bounding box is visible (not completely outside).
        /// </summary>
        public bool IsVisible(BoundingBox box)
        {
            return Contains(box) != EContainmentType.Outside;
        }
        
        /// <summary>
        /// Gets all 8 corner points of the frustum.
        /// Useful for debug visualization.
        /// </summary>
        public Vector3[] GetCorners()
        {
            Vector3[] corners = new Vector3[8];
            
            // This is a simplified version - you may want to compute actual intersections
            // between planes for more accurate corner positions
            
            // Near plane corners
            IntersectPlanes(this.Near, this.Left, this.Top, out corners[0]);
            IntersectPlanes(this.Near, this.Right, this.Top, out corners[1]);
            IntersectPlanes(this.Near, this.Right, this.Bottom, out corners[2]);
            IntersectPlanes(this.Near, this.Left, this.Bottom, out corners[3]);
            
            // Far plane corners
            IntersectPlanes(this.Far, this.Left, this.Top, out corners[4]);
            IntersectPlanes(this.Far, this.Right, this.Top, out corners[5]);
            IntersectPlanes(this.Far, this.Right, this.Bottom, out corners[6]);
            IntersectPlanes(this.Far, this.Left, this.Bottom, out corners[7]);
            
            return corners;
        }

        /// <summary>
        /// Finds the intersection point of three planes.
        /// </summary>
        private static bool IntersectPlanes(Plane p1, Plane p2, Plane p3, out Vector3 point)
        {
            Vector3 cross = Vector3.Cross(p2.normal, p3.normal);
            float det = Vector3.Dot(p1.normal, cross);

            if (Math.Abs(det) < 0.0001f)
            {
                point = Vector3.Zero;
                return false;
            }

            point = (
                Vector3.Cross(p2.normal, p3.normal) * -p1.distance +
                Vector3.Cross(p3.normal, p1.normal) * -p2.distance +
                Vector3.Cross(p1.normal, p2.normal) * -p3.distance
            ) / det;

            return true;
        }
	}
}