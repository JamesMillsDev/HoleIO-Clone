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
		/// Sets the uniform values for a point light at the specified index in the shader's point light array.
		/// </summary>
		/// <param name="light">The light component containing point light properties.</param>
		/// <param name="index">Reference to the current point light index, incremented after setting uniforms.</param>
		/// <param name="shader">The shader to set uniforms on.</param>
		public static void SetPointLightUniforms(LightComponent light, ref int index, Shader shader)
		{
			string uniformPrefix = $"pointLights[{index}]";
          
			// Set light position in world space
			shader.Set($"{uniformPrefix}.position", light.Transform.Position);
          
			// Set light color multiplied by intensity
			shader.Set($"{uniformPrefix}.color", light.color.ToVector3() * light.intensity);
          
			// Set attenuation parameters for distance-based falloff
			// Formula: 1.0 / (constant + linear * distance + quadratic * distance²)
			shader.Set($"{uniformPrefix}.constant", light.attenuation.constant);
			shader.Set($"{uniformPrefix}.linear", light.attenuation.linear);
			shader.Set($"{uniformPrefix}.quadratic", light.attenuation.quadratic);

			index++;
		}
	}
}