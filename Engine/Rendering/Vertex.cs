using System.Numerics;
using System.Runtime.InteropServices;

namespace HoleIO.Engine.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Vertex
    {
        public static readonly uint SizeInBytes = (uint)Marshal.SizeOf<Vertex>();

        public Vector3 position;
        public Vector3 normal;
        public Vector3 tangent;
        public Vector3 biTangent;
        public Vector2 uv;
    }
}