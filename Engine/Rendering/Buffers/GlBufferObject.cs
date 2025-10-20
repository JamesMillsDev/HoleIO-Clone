using Silk.NET.OpenGL;

namespace HoleIO.Engine.Rendering.Buffers
{
    public class GlBufferObject<TDataType> : IDisposable
        where TDataType : unmanaged
    {
        private readonly uint handle;
        private readonly BufferTargetARB bufferType;
        private readonly GL gl;

        public unsafe GlBufferObject(GL gl, Span<TDataType> data, BufferTargetARB bufferType)
        {
            this.gl = gl;
            this.bufferType = bufferType;
            
            this.handle = this.gl.GenBuffer();
            Bind();
            fixed (void* d = data)
            {
                this.gl.BufferData(this.bufferType, (nuint)(data.Length * sizeof(TDataType)), d, BufferUsageARB.DynamicDraw);
            }
        }

        public void Bind()
        {
            this.gl.BindBuffer(bufferType, this.handle);
        }

        public void Dispose()
        {
            this.gl.DeleteBuffer(this.handle);
        }
    }
}