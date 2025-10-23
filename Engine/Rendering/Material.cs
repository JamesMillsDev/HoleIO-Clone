using System.Numerics;
using HoleIO.Engine.Debugging;
using HoleIO.Engine.Rendering.Components;
using HoleIO.Engine.Rendering.Lighting;
using HoleIO.Engine.Utility;
using Silk.NET.OpenGL;
using Texture = HoleIO.Engine.Rendering.Textures.Texture;

namespace HoleIO.Engine.Rendering
{
	/// <summary>
	/// Represents a material that combines a shader with textures and uniform parameters.
	/// Materials define how surfaces should be rendered by encapsulating all rendering state
	/// (shader, textures, material properties) into a reusable object.
	/// </summary>
	/// <param name="shader">The shader program this material uses</param>
	public class Material(Shader shader)
	{
		/// <summary>
		/// Static lookup table mapping C# types to their corresponding shader uniform setter functions.
		/// Used to dynamically invoke the correct Shader.Set() overload based on uniform type.
		/// This enables type-safe uniform setting without runtime type checking on every call.
		/// </summary>
		private static readonly Dictionary<Type, Action<string, object, Shader>> Functions = new()
		{
			{ typeof(float), (uniform, value, shader) => shader.Set(uniform, (float)value) },
			{ typeof(int), (uniform, value, shader) => shader.Set(uniform, (int)value) },
			{ typeof(Vector2), (uniform, value, shader) => shader.Set(uniform, (Vector2)value) },
			{ typeof(Vector3), (uniform, value, shader) => shader.Set(uniform, (Vector3)value) },
			{ typeof(Vector4), (uniform, value, shader) => shader.Set(uniform, (Vector4)value) },
			{ typeof(Matrix4x4), (uniform, value, shader) => shader.Set(uniform, (Matrix4x4)value) },
			{ typeof(float[]), (uniform, value, shader) => shader.Set(uniform, (float[])value) },
			{ typeof(int[]), (uniform, value, shader) => shader.Set(uniform, (int[])value) },
			{ typeof(Vector2[]), (uniform, value, shader) => shader.Set(uniform, (Vector2[])value) },
			{ typeof(Vector3[]), (uniform, value, shader) => shader.Set(uniform, (Vector3[])value) },
			{ typeof(Vector4[]), (uniform, value, shader) => shader.Set(uniform, (Vector4[])value) },
			{ typeof(Matrix4x4[]), (uniform, value, shader) => shader.Set(uniform, (Matrix4x4[])value) },
		};

		/// <summary>
		/// Collection of textures used by this material.
		/// Key: uniform name in shader, Value: texture object.
		/// </summary>
		private Dictionary<string, Texture> Textures { get; } = [];

		/// <summary>
		/// Collection of uniform values used by this material.
		/// Key: uniform name in shader, Value: tuple of (value object, value type).
		/// Type is stored separately for efficient type-based dispatch in Bind().
		/// </summary>
		private Dictionary<string, Tuple<object, Type>> Uniforms { get; } = [];

		/// <summary>
		/// Gets the shader program used by this material.
		/// </summary>
		public Shader Shader { get; } = shader;

		/// <summary>
		/// Sets or updates a texture uniform for this material.
		/// The texture will be bound to sequential texture units starting from Texture0 when the material is bound.
		/// </summary>
		/// <param name="name">Name of the texture uniform in the shader (e.g., "albedoMap", "normalMap")</param>
		/// <param name="texture">Texture object to bind</param>
		public void SetTexture(string name, Texture texture)
		{
			if (!this.Textures.TryAdd(name, texture))
			{
				// If texture already exists, update it
				this.Textures[name] = texture;
			}
		}

		/// <summary>
		/// Sets or updates a uniform parameter for this material.
		/// Supports all types defined in the Functions lookup table.
		/// Values are cached and applied when Bind() is called.
		/// </summary>
		/// <typeparam name="T">Type of the uniform value (must match a type in Functions)</typeparam>
		/// <param name="name">Name of the uniform in the shader (e.g., "color", "roughness")</param>
		/// <param name="uniform">Value to set</param>
		/// <exception cref="InvalidOperationException">Thrown if uniform value is null</exception>
		public void SetUniform<T>(string name, T uniform)
		{
			if (uniform == null)
			{
				throw new InvalidOperationException("Cannot set null uniform");
			}

			// Store uniform value and its type for later dispatch
			Tuple<object, Type> uniformTuple = new(uniform, typeof(T));

			if (!this.Uniforms.TryAdd(name, uniformTuple))
			{
				// If uniform already exists, update it
				this.Uniforms[name] = uniformTuple;
			}
		}

		/// <summary>
		/// Activates this material by binding its shader and applying all textures and uniforms.
		/// Textures are bound to sequential texture units (Texture0, Texture1, etc.) and their
		/// corresponding sampler uniforms are set to the appropriate unit indices.
		/// All cached uniform values are uploaded to the shader.
		/// Errors are caught and logged rather than thrown to prevent rendering failures.
		/// </summary>
		/// <param name="lightData">The lighting data containing all scene lights and ambient color</param>
		public void Bind(LightData lightData)
		{
			// Activate the shader program
			this.Shader.Bind();

			// Bind all textures to sequential texture units
			string[] names = this.Textures.Keys.ToArray();
			Texture[] textures = this.Textures.Values.ToArray();
			for (int i = 0; i < this.Textures.Count; i++)
			{
				// Bind texture to unit i (Texture0 + i)
				textures[i].Bind((TextureUnit)((int)TextureUnit.Texture0 + i));

				try
				{
					// Set sampler uniform to point to this texture unit
					this.Shader.Set(names[i], i);
				}
				catch (Exception e)
				{
					if (Debug.Config.logShaderExceptions)
					{
						// Log error but continue rendering (graceful degradation)
						Debug.LogException(new Exception("Failed to set texture: '" + names[i] + "' - Error: " +
						                                 e.Message));
					}
				}
			}

			// Apply all cached uniform values using type-appropriate setters
			foreach (KeyValuePair<string, Tuple<object, Type>> uniformData in this.Uniforms)
			{
				try
				{
					// Lookup and invoke the appropriate setter function based on type
					Functions[uniformData.Value.Item2].Invoke(uniformData.Key, uniformData.Value.Item1, this.Shader);
				}
				catch (Exception e)
				{
					if (Debug.Config.logShaderExceptions)
					{
						// Log error but continue rendering (graceful degradation)
						Debug.LogException(new Exception("Failed to set uniform: '" + uniformData.Key + "' - Error: " +
						                                 e.Message));
					}
				}
			}

			// Set ambient lighting color (used as base illumination for all objects)
			try
			{
				this.Shader.Set("ambientColor", lightData.ambientLightColor.ToVector3());
			}
			catch (Exception)
			{
				// Silently ignore - unlit shaders may not have lighting uniforms
			}

			// Iterate through all scene lights and set their shader uniforms
			int pointIndex = 0, spotIndex = 0, directionalIndex = 0;
			foreach (LightComponent light in lightData.Lights)
			{
				try
				{
					// Route to appropriate helper based on light type
					switch (light.lightType)
					{
						case ELightType.Directional:
						{
							// Set directional light uniforms (infinite distance lights like sun)
							MaterialLightingHelper.SetDirectionalLightUniforms(light, ref directionalIndex,
								this.Shader);
							break;
						}
						case ELightType.Point:
						{
							// Set point light uniforms (omnidirectional lights with attenuation)
							MaterialLightingHelper.SetPointLightUniforms(light, ref pointIndex, this.Shader);
							break;
						}
						case ELightType.Spot:
						{
							// Set spotlight uniforms (cone-shaped directional lights with attenuation)
							MaterialLightingHelper.SetSpotLightUniforms(light, ref spotIndex, this.Shader);
							break;
						}
					}
				}
				catch (Exception)
				{
					// Silently ignore - unlit shaders may not have lighting uniforms
				}
			}

			// Set the count of each light type for shader array iteration
			try
			{
				this.Shader.Set("numDirectionalLights", directionalIndex);
				// Spotlight count currently commented out - possibly not implemented yet
				/*this.Shader.Set("numSpotLights", spotIndex);*/
				this.Shader.Set("numPointLights", pointIndex);
			}
			catch (Exception)
			{
				// Silently ignore - unlit shaders may not have lighting uniforms
			}
		}

		/// <summary>
		/// Deactivates this material by unbinding the shader.
		/// Good practice but not strictly necessary if another material is immediately bound.
		/// </summary>
		public void Unbind()
		{
			this.Shader.Unbind();
		}
	}
}