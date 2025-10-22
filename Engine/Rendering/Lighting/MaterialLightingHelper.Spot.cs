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
		/// Sets the uniform values for a spotlight at the specified index in the shader's spotlight array.
		/// </summary>
		/// <param name="light">The light component containing spotlight properties.</param>
		/// <param name="index">Reference to the current spotlight index, incremented after setting uniforms.</param>
		/// <param name="shader">The shader to set uniforms on.</param>
		public static void SetSpotLightUniforms(LightComponent light, ref int index, Shader shader)
		{
			string uniformPrefix = $"spotLights[{index}]";
          
			// Set spotlight direction (where it's pointing)
			shader.Set($"{uniformPrefix}.direction", light.Transform.Forward);
          
			// Set spotlight position in world space
			shader.Set($"{uniformPrefix}.position", light.Transform.Position);
          
			// Set light color multiplied by intensity
			shader.Set($"{uniformPrefix}.color", light.color.ToVector3() * light.intensity);
          
			// Set attenuation parameters for distance-based falloff
			shader.Set($"{uniformPrefix}.constant", light.attenuation.constant);
			shader.Set($"{uniformPrefix}.linear", light.attenuation.linear);
			shader.Set($"{uniformPrefix}.quadratic", light.attenuation.quadratic);
          
			// Set cone angles (inner cone = full brightness, outer cone = edge falloff)
			shader.Set($"{uniformPrefix}.cutOff", light.innerCutoff);
			shader.Set($"{uniformPrefix}.outerCutOff", light.outerCutoff);

			index++;
		}
	}
}