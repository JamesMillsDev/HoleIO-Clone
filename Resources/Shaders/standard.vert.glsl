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
    mat3 tbn;
    vec2 texCoords;
} vs_out;

void main()
{
    //Multiplying our uniform with the vertex position, the multiplication order here does matter.
    gl_Position = projection * view * model * position;
    vs_out.position = model * position;
    vs_out.texCoords = texCoords;
    
    vec3 T = normalize(model * tangent).xyz;
    vec3 N = normalize(model * normal).xyz;
    T = normalize(T - dot(T, N) * N);
    vec3 B = cross(N, T);
    
    vs_out.tbn = mat3(T, B, N);
}