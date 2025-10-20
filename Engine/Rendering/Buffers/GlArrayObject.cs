using Silk.NET.OpenGL;

namespace HoleIO.Engine.Rendering.Buffers
{
    public class GlArrayObject<TVertexType, TIndexType> : IDisposable
        where TVertexType : unmanaged
        where TIndexType : unmanaged
    {
        private readonly uint handle;
        private readonly GL glContext;

        public GlArrayObject(GL glContext, GlBufferObject<TVertexType> vbo, GlBufferObject<TIndexType> ibo)
        {
            this.glContext = glContext;

            this.handle = this.glContext.GenVertexArray();
            Bind();
            vbo.Bind();
            ibo.Bind();
        }

        public unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, uint vertexSize,
            int offset, bool normalized = false)
        {
            this.glContext.VertexAttribPointer(index, count, type, normalized, vertexSize * (uint)sizeof(TVertexType),
                (void*)
                (offset * sizeof(TVertexType)));
            this.glContext.EnableVertexAttribArray(index);
        }

        public void Bind()
        {
            this.glContext.BindVertexArray(this.handle);
        }

        public void Dispose()
        {
            glContext.DeleteVertexArray(handle);
        }
    }
}