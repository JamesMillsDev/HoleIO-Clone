using System.Drawing;
using System.Numerics;

namespace HoleIO.Engine.Utility
{
	public static class ColorExtensions
	{
		public static Vector3 ToVector3(this Color color)
		{
			return new Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
		}

		public static Vector4 ToVector4(this Color color)
		{
			return new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
		}
	}
}