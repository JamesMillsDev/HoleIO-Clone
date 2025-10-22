#version 330 core

struct DirectionalLight
{
    vec3 direction;
    vec3 color;
};

struct PointLight
{
    vec3 position;
    vec3 color;
    
    float constant;
    float linear;
    float quadratic;
};

out vec4 FragColor;

in VS_OUT
{
    vec4 position;
    mat3 tbn;
    vec2 texCoords;
} fs_in;

uniform vec3 cameraPosition;

uniform sampler2D baseColorMap;
uniform sampler2D normalMap;

const int DEFAULT_SPECULAR_POWER = 32;
const int MAX_LIGHTS = 8;

uniform int numDirectionalLights;
uniform DirectionalLight directionalLights[MAX_LIGHTS];

uniform int numPointLights;
uniform PointLight pointLights[MAX_LIGHTS];

uniform vec3 ambientColor;
uniform int specularPower;

void main()
{
    vec3 viewDir = normalize(cameraPosition - fs_in.position.xyz);
    
    float specFactor = specularPower;
    if(specFactor <= 0)
    {
        specFactor = DEFAULT_SPECULAR_POWER;
    }

    vec3 baseColor = texture(baseColorMap, fs_in.texCoords).rgb;
    vec3 normal = texture(normalMap, fs_in.texCoords).rgb;
    normal = normal * 2.0 - 1.0;
    normal = normalize(fs_in.tbn * normal);

    vec3 ambientTotal = ambientColor * baseColor;
    
    vec3 diffuseTotal = vec3(0);
    vec3 specularTotal = vec3(0);

    // Add directional lights
    for (int i = 0; i < numDirectionalLights; i++)
    {
        DirectionalLight dirL = directionalLights[i];
        vec3 lightDir = normalize(-dirL.direction);
        float diff = max(dot(normal, lightDir), 0.0);
        diffuseTotal += dirL.color * diff * baseColor;
        
        vec3 reflectDir = reflect(-lightDir, normal);
        float spec = pow(max(dot(viewDir, reflectDir), 0.0), specFactor);
        specularTotal += dirL.color * spec;
    }

    for (int i = 0; i < numPointLights; i++)
    {
        PointLight pl = pointLights[i];
        
        float distance = length(pl.position - fs_in.position.xyz);
        float atten = 1.0 / (pl.constant + pl.linear * distance + pl.quadratic * (distance * distance));
        
        vec3 lightDir = normalize(pl.position - fs_in.position.xyz);
        float diff = max(dot(normal, lightDir), 0.0);
        diffuseTotal += (pl.color * diff * baseColor) * atten;

        vec3 reflectDir = reflect(-lightDir, normal);
        float spec = pow(max(dot(viewDir, reflectDir), 0.0), specFactor);
        specularTotal += (pl.color * spec) * atten;
    }

    vec3 totalColor = ambientTotal + diffuseTotal + specularTotal;
    FragColor = vec4(totalColor, 1.0); // Should show the normal map colors
}