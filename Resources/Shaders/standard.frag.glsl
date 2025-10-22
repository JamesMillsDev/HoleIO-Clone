#version 330 core

out vec4 FragColor;

in VS_OUT
{
    vec4 position;
    vec3 normal;
    vec3 tangent;
    vec3 biTangent;
    vec2 texCoords;
} fs_in;

uniform sampler2D diffuse;

void main()
{
    FragColor = texture(diffuse, fs_in.texCoords);
}