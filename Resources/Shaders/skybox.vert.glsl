#version 330 core
layout(location = 0) in vec4 position;

out VS_OUT
{
    vec3 texCoords;
} vs_out;

void main()
{
    vs_out.texCoords = position.xyz;
    mat4 viewNoTranslation = mat4(mat3(view));
    vec4 pos = projection * viewNoTranslation * position;
    gl_Position = pos.xyww;
}