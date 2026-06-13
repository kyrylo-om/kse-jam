void GetMainLight_float(float3 WorldPos, out float3 Direction, out float3 Color, out float ShadowAtten)
{
#if defined(SHADERGRAPH_PREVIEW)
    // Значення для прев'ю всередині самого Shader Graph
    Direction = normalize(float3(0.5, 0.5, -0.25));
    Color = float3(1, 1, 1);
    ShadowAtten = 1.0;
#else
    // Отримуємо дані про світло в URP
    #if defined(UNIVERSAL_LIGHTING_INCLUDED)

        // Отримуємо координати для тіней
        float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);

        // Отримуємо головне світло
        Light mainLight = GetMainLight(shadowCoord);

        Direction = mainLight.direction;
        Color = mainLight.color;

        // Shadow attenuation відповідає за тіні, що відкидаються іншими об'єктами
        ShadowAtten = mainLight.shadowAttenuation;
    #else
        // Фолбек, якщо освітлення не підключено
        Direction = normalize(float3(0.5, 0.5, -0.25));
        Color = float3(1, 1, 1);
        ShadowAtten = 1.0;
    #endif
#endif
}
