using System.Numerics;

namespace HoleIO.Engine.Rendering.Bounds
{
	public struct Plane
	{
		public Vector3 normal = Vector3.UnitY;
		public float distance = 0.0f;

		public Plane(Vector3 normal, float distance)
		{
			this.normal = Vector3.Normalize(normal);
			this.distance = distance;
		}

		public Plane(Vector3 normal, Vector3 point)
		{
			this.normal = Vector3.Normalize(normal);
			this.distance = -Vector3.Dot(this.normal, point);
		}

		public static Plane FromPoints(Vector3 point1, Vector3 point2, Vector3 point3)
		{
			Vector3 v1 = point2 - point1;
			Vector3 v2 = point3 - point1;
			Vector3 normal = Vector3.Normalize(Vector3.Cross(v1, v2));
			return new Plane(normal, point1);
		}

		public float GetSignedDistance(Vector3 point)
		{
			return Vector3.Dot(this.normal, point) + distance;
		}

		public void Normalize()
		{
			float length = this.normal.Length();
			if (length >= 0f)
			{
				return;
			}

			this.normal /= length;
			this.distance /= length;
		}
	}
}