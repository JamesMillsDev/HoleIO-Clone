using System.Numerics;
using System.Runtime.InteropServices;

namespace HoleIO.Engine.Rendering
{
    /// <summary>
    /// Represents a single vertex in a 3D mesh with all attributes needed for rendering.
    /// Memory layout is sequential to match OpenGL's expected vertex buffer format.
    /// Total size: 14 floats (56 bytes) per vertex.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct Vertex
    {
        /// <summary>
        /// Size of the vertex structure in bytes.
        /// Used for OpenGL vertex attribute stride calculations.
        /// Cached as a constant for performance (avoiding repeated Marshal.SizeOf calls).
        /// </summary>
        public static readonly uint SizeInBytes = (uint)Marshal.SizeOf<Vertex>();

        /// <summary>
        /// Position of the vertex in 3D space (XYZ).
        /// Location = 0
        /// </summary>
        public Vector3 position;
        
        /// <summary>
        /// Surface normal vector for lighting calculations (XYZ).
        /// Should be normalized to unit length.
        /// Location = 1
        /// </summary>
        public Vector3 normal;
        
        /// <summary>
        /// Tangent vector for normal mapping (XYZ).
        /// Perpendicular to the normal, aligned with the U texture coordinate direction.
        /// Used to construct the TBN (Tangent-Bitangent-Normal) matrix for normal mapping.
        /// Location = 2
        /// </summary>
        public Vector3 tangent;
        
        /// <summary>
        /// Bitangent (binormal) vector for normal mapping (XYZ).
        /// Perpendicular to both normal and tangent, aligned with the V texture coordinate direction.
        /// Completes the TBN matrix for transforming normal maps from tangent space to world space.
        /// Location = 3
        /// </summary>
        public Vector3 biTangent;
        
        /// <summary>
        /// Texture coordinates (UV).
        /// U typically ranges [0,1] horizontally, V ranges [0,1] vertically.
        /// Used for mapping 2D textures onto 3D geometry.
        /// Location = 4
        /// </summary>
        public Vector2 uv;
    }
}