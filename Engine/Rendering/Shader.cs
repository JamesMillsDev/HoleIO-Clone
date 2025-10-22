using System.Numerics;
using HoleIO.Engine.Core;
using Silk.NET.OpenGL;

namespace HoleIO.Engine.Rendering
{
	/// <summary>
	/// Manages OpenGL shader programs for rendering.
	/// Handles loading, compiling, linking shader stages and setting uniform values.
	/// </summary>
	public class Shader
	{
		// OpenGL program handle
		private readonly uint handle;

		// OpenGL context for all GL operations
		private readonly GL glContext;

		/// <summary>
		/// Creates and links a shader program from multiple shader stages.
		/// </summary>
		/// <param name="files">Pairs of shader types and filenames (without path or extension)</param>
		/// <exception cref="Exception">Thrown if shader compilation or linking fails</exception>
		/// <example>
		/// new Shader(
		///     new(ShaderType.VertexShader, "basic"),
		///     new(ShaderType.FragmentShader, "basic")
		/// );
		/// // Loads Resources/Shaders/basic.vert.glsl and basic.frag.glsl
		/// </example>
		public Shader(params KeyValuePair<ShaderType, string>[] files)
		{
			this.glContext = Application.OpenGlContext();

			// Load and compile each shader stage
			List<uint> stages = [];
			stages.AddRange(files.Select(file => LoadShader(file.Key, file.Value)));

			// Create shader program
			this.handle = this.glContext.CreateProgram();

			// Attach all compiled shader stages
			foreach (uint stage in stages)
			{
				this.glContext.AttachShader(this.handle, stage);
			}

			// Link the program (combines all stages into executable)
			this.glContext.LinkProgram(this.handle);

			// Check for linking errors
			this.glContext.GetProgram(this.handle, GLEnum.LinkStatus, out int status);
			if (status == 0)
			{
				throw new Exception(
					$"Program failed to link with error: {this.glContext.GetProgramInfoLog(this.handle)}");
			}

			// Detach shaders (no longer needed after linking)
			foreach (uint stage in stages)
			{
				this.glContext.DetachShader(this.handle, stage);
			}

			// Delete shader objects to free memory
			foreach (uint stage in stages)
			{
				this.glContext.DeleteShader(stage);
			}
		}

		/// <summary>
		/// Sets a single integer uniform value.
		/// </summary>
		/// <param name="name">Name of the uniform in the shader</param>
		/// <param name="value">Integer value to set</param>
		/// <exception cref="Exception">Thrown if uniform not found</exception>
		public void Set(string name, int value)
		{
			int loc = this.glContext.GetUniformLocation(this.handle, name);
			if (loc == -1)
			{
				throw new Exception($"{name} uniform not found on shader.");
			}

			this.glContext.Uniform1(loc, value);
		}

		/// <summary>
		/// Sets a single float uniform value.
		/// </summary>
		/// <param name="name">Name of the uniform in the shader</param>
		/// <param name="value">Float value to set</param>
		/// <exception cref="Exception">Thrown if uniform not found</exception>
		public void Set(string name, float value)
		{
			int loc = this.glContext.GetUniformLocation(this.handle, name);
			if (loc == -1)
			{
				throw new Exception($"{name} uniform not found on shader.");
			}

			this.glContext.Uniform1(loc, value);
		}

		/// <summary>
		/// Sets a vec2 uniform value.
		/// </summary>
		/// <param name="name">Name of the uniform in the shader</param>
		/// <param name="value">2D vector value to set</param>
		/// <exception cref="Exception">Thrown if uniform not found</exception>
		public void Set(string name, Vector2 value)
		{
			int loc = this.glContext.GetUniformLocation(this.handle, name);
			if (loc == -1)
			{
				throw new Exception($"{name} uniform not found on shader.");
			}

			this.glContext.Uniform2(loc, value);
		}

		/// <summary>
		/// Sets a vec3 uniform value.
		/// </summary>
		/// <param name="name">Name of the uniform in the shader</param>
		/// <param name="value">3D vector value to set</param>
		/// <exception cref="Exception">Thrown if uniform not found</exception>
		public void Set(string name, Vector3 value)
		{
			int loc = this.glContext.GetUniformLocation(this.handle, name);
			if (loc == -1)
			{
				throw new Exception($"{name} uniform not found on shader.");
			}

			this.glContext.Uniform3(loc, value);
		}

		/// <summary>
		/// Sets a vec4 uniform value.
		/// </summary>
		/// <param name="name">Name of the uniform in the shader</param>
		/// <param name="value">4D vector value to set</param>
		/// <exception cref="Exception">Thrown if uniform not found</exception>
		public void Set(string name, Vector4 value)
		{
			int loc = this.glContext.GetUniformLocation(this.handle, name);
			if (loc == -1)
			{
				throw new Exception($"{name} uniform not found on shader.");
			}

			this.glContext.Uniform4(loc, value);
		}

		/// <summary>
		/// Sets a mat4 uniform value.
		/// Note: Uses transpose=false, assuming System.Numerics Matrix4x4 layout matches shader expectations.
		/// If matrices appear incorrect, try transposing before passing or set transpose=true.
		/// </summary>
		/// <param name="name">Name of the uniform in the shader</param>
		/// <param name="value">4x4 matrix value to set</param>
		/// <exception cref="Exception">Thrown if uniform not found</exception>
		public unsafe void Set(string name, Matrix4x4 value)
		{
			int loc = this.glContext.GetUniformLocation(this.handle, name);
			if (loc == -1)
			{
				throw new Exception($"{name} uniform not found on shader.");
			}

			this.glContext.UniformMatrix4(loc, 1, false, (float*)&value);
		}

		/// <summary>
		/// Sets an array of integer uniforms.
		/// WARNING: This uses Uniform4 which sets ivec4 values, not individual ints.
		/// For an int array, this should likely use Uniform1 instead.
		/// </summary>
		/// <param name="name">Name of the uniform array in the shader</param>
		/// <param name="value">Array of integers to set</param>
		/// <exception cref="Exception">Thrown if uniform not found</exception>
		public unsafe void Set(string name, int[] value)
		{
			int loc = this.glContext.GetUniformLocation(this.handle, name);
			if (loc == -1)
			{
				throw new Exception($"{name} uniform not found on shader.");
			}

			fixed (int* d = value)
			{
				// Uniform4 treats every 4 ints as an ivec4
				this.glContext.Uniform1(loc, (uint)value.Length, d);
			}
		}

		/// <summary>
		/// Sets an array of float uniforms.
		/// </summary>
		/// <param name="name">Name of the uniform array in the shader</param>
		/// <param name="value">Array of floats to set</param>
		/// <exception cref="Exception">Thrown if uniform not found</exception>
		public unsafe void Set(string name, float[] value)
		{
			int loc = this.glContext.GetUniformLocation(this.handle, name);
			if (loc == -1)
			{
				throw new Exception($"{name} uniform not found on shader.");
			}

			fixed (float* d = value)
			{
				this.glContext.Uniform1(loc, (uint)value.Length, d);
			}
		}

		/// <summary>
		/// Sets an array of vec2 uniforms.
		/// </summary>
		/// <param name="name">Name of the uniform array in the shader</param>
		/// <param name="value">Array of 2D vectors to set</param>
		/// <exception cref="Exception">Thrown if uniform not found</exception>
		public unsafe void Set(string name, Vector2[] value)
		{
			int loc = this.glContext.GetUniformLocation(this.handle, name);
			if (loc == -1)
			{
				throw new Exception($"{name} uniform not found on shader.");
			}

			fixed (Vector2* d = value)
			{
				this.glContext.Uniform2(loc, (uint)value.Length, (float*)d);
			}
		}

		/// <summary>
		/// Sets an array of vec3 uniforms.
		/// </summary>
		/// <param name="name">Name of the uniform array in the shader</param>
		/// <param name="value">Array of 3D vectors to set</param>
		/// <exception cref="Exception">Thrown if uniform not found</exception>
		public unsafe void Set(string name, Vector3[] value)
		{
			int loc = this.glContext.GetUniformLocation(this.handle, name);
			if (loc == -1)
			{
				throw new Exception($"{name} uniform not found on shader.");
			}

			fixed (Vector3* d = value)
			{
				this.glContext.Uniform3(loc, (uint)value.Length, (float*)d);
			}
		}

		/// <summary>
		/// Sets an array of vec4 uniforms.
		/// </summary>
		/// <param name="name">Name of the uniform array in the shader</param>
		/// <param name="value">Array of 4D vectors to set</param>
		/// <exception cref="Exception">Thrown if uniform not found</exception>
		public unsafe void Set(string name, Vector4[] value)
		{
			int loc = this.glContext.GetUniformLocation(this.handle, name);
			if (loc == -1)
			{
				throw new Exception($"{name} uniform not found on shader.");
			}

			fixed (Vector4* d = value)
			{
				this.glContext.Uniform4(loc, (uint)value.Length, (float*)d);
			}
		}

		/// <summary>
		/// Sets an array of mat4 uniforms.
		/// Useful for skeletal animation bone matrices or instanced rendering.
		/// </summary>
		/// <param name="name">Name of the uniform array in the shader</param>
		/// <param name="value">Array of 4x4 matrices to set</param>
		/// <exception cref="Exception">Thrown if uniform not found</exception>
		public unsafe void Set(string name, Matrix4x4[] value)
		{
			int loc = this.glContext.GetUniformLocation(this.handle, name);
			if (loc == -1)
			{
				throw new Exception($"{name} uniform not found on shader.");
			}

			fixed (Matrix4x4* d = value)
			{
				this.glContext.UniformMatrix4(loc, (uint)value.Length, false, (float*)d);
			}
		}

		/// <summary>
		/// Activates this shader program for rendering.
		/// All subsequent draw calls will use this shader until another is bound.
		/// </summary>
		public void Bind()
		{
			this.glContext.UseProgram(this.handle);
		}

		/// <summary>
		/// Deactivates the current shader program.
		/// Good practice but not strictly necessary if you always bind before drawing.
		/// </summary>
		public void Unbind()
		{
			this.glContext.UseProgram(0);
		}

		/// <summary>
		/// Loads and compiles a single shader stage from file.
		/// </summary>
		/// <param name="type">Type of shader (vertex, fragment, etc.)</param>
		/// <param name="file">Filename without path or extension</param>
		/// <returns>OpenGL handle to the compiled shader</returns>
		/// <exception cref="Exception">Thrown if compilation fails</exception>
		private uint LoadShader(ShaderType type, string file)
		{
			// Load shader source from file
			// Expected path: Resources/Shaders/{file}.{ext}.glsl
			string src = File.ReadAllText(Path.Combine("Resources", "Shaders", $"{file}.{GetExtension(type)}.glsl"));

			// Create shader object
			uint stageHandle = this.glContext.CreateShader(type);

			// Load source and compile
			this.glContext.ShaderSource(stageHandle, src);
			this.glContext.CompileShader(stageHandle);

			// Check for compilation errors
			string infoLog = this.glContext.GetShaderInfoLog(stageHandle);
			return !string.IsNullOrWhiteSpace(infoLog)
				? throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}")
				: stageHandle;
		}

		/// <summary>
		/// Maps shader types to file extensions.
		/// </summary>
		/// <param name="type">OpenGL shader type</param>
		/// <returns>File extension for that shader type</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown for unknown shader types</exception>
		private static string GetExtension(ShaderType type)
		{
			return type switch
			{
				ShaderType.FragmentShader => "frag",
				ShaderType.VertexShader => "vert",
				ShaderType.GeometryShader => "geom",
				ShaderType.TessEvaluationShader => "tesseval",
				ShaderType.TessControlShader => "tessctrl",
				ShaderType.ComputeShader => "comp",
				_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
			};
		}
	}
}