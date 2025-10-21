#version 330 core
layout (location = 0) in vec3 vPos;
layout (location = 1) in vec3 vNorm;
layout (location = 4) in vec2 vUv;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 fNorm;
out vec2 fUv;

void main()
{
    //Multiplying our uniform with the vertex position, the multiplication order here does matter.
    gl_Position = projection * view * model * vec4(vPos, 1.0);
    fUv = vUv;
    fNorm = vNorm;
}