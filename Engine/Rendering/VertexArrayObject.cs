using HoleIO.Engine.Core;
using Silk.NET.OpenGL;

namespace HoleIO.Engine.Rendering
{
    public class VertexArrayObject<TVertexType, TIndexType> : IDisposable
        where TVertexType : unmanaged
        where TIndexType : unmanaged
    {
        private readonly uint handle;
        private readonly GL glContext;

        public VertexArrayObject(GL glContext, BufferObject<TVertexType> vbo, BufferObject<TIndexType> ibo)
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