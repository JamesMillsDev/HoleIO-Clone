#version 330 core
in vec3 fNorm;
in vec2 fUv;

uniform sampler2D diffuse;

out vec4 FragColor;

void main()
{
    FragColor = texture(diffuse, fUv);
}