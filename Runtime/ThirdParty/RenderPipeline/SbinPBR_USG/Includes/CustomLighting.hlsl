#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED


void InitBRDFData_float(float3 albedo_in, float3 diffuse_in, float3 specular_in, float reflectivity_in, float oneMinusReflectivity, float smoothness, float alpha_in,
    out float3 albedo, out float3 diffuse, out float3 specular,
    out float reflectivity, out float perceptualRoughness, out float roughness, out float roughness2, out float grazingTerm, out float normalizationTerm, out float roughness2MinusOne, out float alpha
)
{
    albedo = albedo_in;
    diffuse = diffuse_in;
    specular = specular_in;
    reflectivity = reflectivity_in;
    perceptualRoughness = 1.0 - smoothness;
    roughness = max(perceptualRoughness * perceptualRoughness, 0.0001);
    roughness2 = max(roughness * roughness, 0.0001);
    grazingTerm = saturate(smoothness + reflectivity);
    normalizationTerm = roughness * float(4.0) + float(2.0);
    roughness2MinusOne = roughness2 - float(1.0);

    alpha = alpha_in;
#ifdef _ALPHAPREMULTIPLY_ON
    diffuse *= alpha;
    alpha = alpha * oneMinusReflectivity + reflectivity;
#endif

}

void InitBRDFData_half(half3 albedo_in, half3 diffuse_in, half3 specular_in, half reflectivity_in, half oneMinusReflectivity, half smoothness, half alpha_in,
                        out half3 albedo, out half3 diffuse, out half3 specular,
                        out half reflectivity, out half perceptualRoughness, out half roughness, out half roughness2, out half grazingTerm, out half normalizationTerm, out half roughness2MinusOne,out half alpha
                    )
{
    albedo = albedo_in;
    diffuse = diffuse_in;
    specular = specular_in;
    reflectivity = reflectivity_in;
    perceptualRoughness = 1.0 - smoothness;
    roughness = max(perceptualRoughness * perceptualRoughness, 0.0001);
    roughness2 = max(roughness * roughness, 0.0001);
    grazingTerm = saturate(smoothness + reflectivity);
    normalizationTerm = roughness * half(4.0) + half(2.0);
    roughness2MinusOne = roughness2 - half(1.0);

    alpha = alpha_in;
#ifdef _ALPHAPREMULTIPLY_ON
    diffuse *= alpha;
    alpha = alpha * oneMinusReflectivity + reflectivity;
#endif

}


void MainLight_float(float3 WorldPos, out float3 Direction, out float3 Color, out float DistanceAtten, out float ShadowAtten)
{
#if SHADERGRAPH_PREVIEW
    Direction = float3(0.5, 0.5, 0);
    Color = 1;
    DistanceAtten = 1;
    ShadowAtten = 1;
#else
#if SHADOWS_SCREEN
    float4 clipPos = TransformWorldToHClip(WorldPos);
    float4 shadowCoord = ComputeScreenPos(clipPos);
#else
    float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
#endif
    
    Light mainLight = GetMainLight(shadowCoord);
    Direction = mainLight.direction;
    Color =  mainLight.color;
    DistanceAtten = mainLight.distanceAttenuation;  
    ShadowAtten = mainLight.shadowAttenuation;
    //  Ambient = unity_AmbientSky.rgb;
#endif
}

void MainLight_half(float3 WorldPos, out half3 Direction, out half3 Color, out half DistanceAtten, out half ShadowAtten)
{
#if SHADERGRAPH_PREVIEW
    Direction = half3(0.5, 0.5, 0);
    Color = 1;
    DistanceAtten = 1;
    ShadowAtten = 1;
#else
#if SHADOWS_SCREEN
    half4 clipPos = TransformWorldToHClip(WorldPos);
    half4 shadowCoord = ComputeScreenPos(clipPos);
#else
    half4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
#endif
    Light mainLight = GetMainLight(shadowCoord);
    Direction = mainLight.direction;
    Color = mainLight.color;
    DistanceAtten = mainLight.distanceAttenuation;
    ShadowAtten = mainLight.shadowAttenuation;
  //  Ambient = unity_AmbientSky.rgb;
#endif
}


void DirectSpecular_float(float3 Specular, float Smoothness, float3 Direction, float3 Color, float3 WorldNormal, float3 WorldView, out float3 Out)
{
#if SHADERGRAPH_PREVIEW
    Out = 0;
#else
    Smoothness = exp2(10 * Smoothness + 1);
    WorldNormal = normalize(WorldNormal);
    WorldView = SafeNormalize(WorldView);
    Out = LightingSpecular(Color, Direction, WorldNormal, WorldView, float4(Specular, 0), Smoothness);
#endif
}

void DirectSpecular_half(half3 Specular, half Smoothness, half3 Direction, half3 Color, half3 WorldNormal, half3 WorldView, out half3 Out)
{
#if SHADERGRAPH_PREVIEW
    Out = 0;
#else
    Smoothness = exp2(10 * Smoothness + 1);
    WorldNormal = normalize(WorldNormal);
    WorldView = SafeNormalize(WorldView);
    Out = LightingSpecular(Color, Direction, WorldNormal, WorldView,half4(Specular, 0), Smoothness);
#endif
}

void AdditionalLights_float(float3 SpecColor, float Smoothness, float3 WorldPosition, float3 WorldNormal, float3 WorldView, out float3 Diffuse, out float3 Specular)
{
    float3 diffuseColor = 0;
    float3 specularColor = 0;

#ifndef SHADERGRAPH_PREVIEW
    Smoothness = exp2(10 * Smoothness + 1);
    WorldNormal = normalize(WorldNormal);
    WorldView = SafeNormalize(WorldView);
    int pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; ++i)
    {
        Light light = GetAdditionalLight(i, WorldPosition);
        half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
        diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal);
        specularColor += LightingSpecular(attenuatedLightColor, light.direction, WorldNormal, WorldView, float4(SpecColor, 0), Smoothness);
    }
#endif

    Diffuse = diffuseColor;
    Specular = specularColor;
}

void AdditionalLights_half(half3 SpecColor, half Smoothness, half3 WorldPosition, half3 WorldNormal, half3 WorldView, out half3 Diffuse, out half3 Specular)
{
    half3 diffuseColor = 0;
    half3 specularColor = 0;

#ifndef SHADERGRAPH_PREVIEW
    Smoothness = exp2(10 * Smoothness + 1);
    WorldNormal = normalize(WorldNormal);
    WorldView = SafeNormalize(WorldView);
    int pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; ++i)
    {
        Light light = GetAdditionalLight(i, WorldPosition);
        half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
        diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal);
        specularColor += LightingSpecular(attenuatedLightColor, light.direction, WorldNormal, WorldView, half4(SpecColor, 0), Smoothness);
    }
#endif

    Diffuse = diffuseColor;
    Specular = specularColor;
}

#endif
