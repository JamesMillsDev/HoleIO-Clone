namespace HoleIO.Engine.Rendering.Bounds
{
	/// <summary>
	/// Result of a frustum culling test.
	/// </summary>
	public enum EContainmentType
	{
		/// <summary>Object is completely outside the frustum.</summary>
		Outside,

		/// <summary>Object is partially inside the frustum (intersecting).</summary>
		Intersects,

		/// <summary>Object is completely inside the frustum.</summary>
		Inside
	}
}