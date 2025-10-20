using System.Numerics;
using HoleIO.Engine.Core;
using Silk.NET.OpenGL;

namespace HoleIO.Engine.Rendering
{
    public class Shader
    {
        private readonly uint handle;
        private readonly GL glContext;

        public Shader(Dictionary<ShaderType, string> files)
        {
            this.glContext = Application.OpenGlContext();

            List<uint> stages = [];
            stages.AddRange(files.Select(file => LoadShader(file.Key, file.Value)));

            this.handle = this.glContext.CreateProgram();
            foreach (uint stage in stages)
            {
                this.glContext.AttachShader(this.handle, stage);
            }
            
            this.glContext.LinkProgram(this.handle);
            this.glContext.GetProgram(this.handle, GLEnum.LinkStatus, out int status);
            if (status == 0)
            {
                throw new Exception($"Program failed to link with error: {this.glContext.GetProgramInfoLog(this.handle)}");
            }

            foreach (uint stage in stages)
            {
                this.glContext.DetachShader(this.handle, stage);
            }

            foreach (uint stage in stages)
            {
                this.glContext.DeleteShader(stage);
            }
        }
        
        public void SetUniform(string name, int value)
        {
            int location = this.glContext.GetUniformLocation(this.handle, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader.");
            }
            this.glContext.Uniform1(location, value);
        }

        public unsafe void SetUniform(string name, Matrix4x4 value)
        {
            //A new overload has been created for setting a uniform so we can use the transform in our shader.
            int location = this.glContext.GetUniformLocation(this.handle, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader.");
            }
            this.glContext.UniformMatrix4(location, 1, false, (float*) &value);
        }

        public void SetUniform(string name, float value)
        {
            int location = this.glContext.GetUniformLocation(this.handle, name);
            if (location == -1)
            {
                throw new Exception($"{name} uniform not found on shader.");
            }
            this.glContext.Uniform1(location, value);
        }

        public void Bind()
        {
            this.glContext.UseProgram(this.handle);
        }

        public void Unbind()
        {
            this.glContext.UseProgram(0);
        }

        private uint LoadShader(ShaderType type, string file)
        {
            string src = File.ReadAllText(Path.Combine("Resources", "Shaders", $"{file}.{GetExtension(type)}.glsl"));
            uint stageHandle = this.glContext.CreateShader(type);

            this.glContext.ShaderSource(stageHandle, src);
            this.glContext.CompileShader(stageHandle);
            string infoLog = this.glContext.GetShaderInfoLog(stageHandle);
            return !string.IsNullOrWhiteSpace(infoLog)
                ? throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}")
                : stageHandle;
        }

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