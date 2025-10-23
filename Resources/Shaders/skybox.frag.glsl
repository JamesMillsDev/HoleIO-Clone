#version 330 core

out vec4 fragColor;

in VS_OUT
{
    vec3 texCoords;
} fs_in;

uniform samplerCube skyTexture;

void main()
{
    fragColor = texture(skyTexture, fs_in.texCoords);
}