using HoleIO.Engine.Rendering.Components;
using HoleIO.Engine.Utility;

namespace HoleIO.Engine.Rendering.Lighting
{
	/// <summary>
	/// Helper class for setting light uniform data in shaders.
	/// </summary>
	public static partial class MaterialLightingHelper
	{
		/// <summary>
		/// Sets the uniform values for a directional light at the specified index in the shader's directional light array.
		/// Directional lights have no position and affect all objects equally (like sunlight).
		/// </summary>
		/// <param name="light">The light component containing directional light properties.</param>
		/// <param name="index">Reference to the current directional light index, incremented after setting uniforms.</param>
		/// <param name="shader">The shader to set uniforms on.</param>
		public static void SetDirectionalLightUniforms(LightComponent light, ref int index, Shader shader)
		{
			string uniformPrefix = $"directionalLights[{index}]";
          
			// Set light direction (direction the light is traveling)
			shader.Set($"{uniformPrefix}.direction", light.Transform.Forward);
          
			// Set light color multiplied by intensity
			shader.Set($"{uniformPrefix}.color", light.color.ToVector3() * light.intensity);

			index++;
		}
	}
}