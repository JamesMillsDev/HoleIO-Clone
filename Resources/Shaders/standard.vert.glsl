#version 330 core
layout (location = 0) in vec4 position;
layout (location = 1) in vec4 normal;
layout (location = 2) in vec4 tangent;
layout (location = 4) in vec2 texCoords;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out VS_OUT
{
    vec4 position;
    vec3 normal;
    vec3 tangent;
    vec3 biTangent; 
    vec2 texCoords;
} vs_out;

void main()
{
    //Multiplying our uniform with the vertex position, the multiplication order here does matter.
    gl_Position = projection * view * model * position;
    vs_out.position = model * position;
    vs_out.texCoords = texCoords;
    vs_out.normal = (model * normal).xyz;
    vs_out.tangent = (model * tangent).xyz;
    vs_out.biTangent = cross(vs_out.normal, vs_out.tangent) * tangent.w;
}