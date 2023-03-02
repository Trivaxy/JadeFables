sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;

float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;
float2 uTargetPosition;

float time;
float4 inputColor;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 bottomCoords = float2(coords.x, time % 1.0);
    
    float color = tex2D(uImage0, bottomCoords).r;
    
    color = pow(color, (coords.y * 4.0));
    color *= (0.5 - abs(coords.x - 0.5)) * 2.0;
    color *= 1.0 - coords.y;
    return float4(color,color,color,color) * inputColor;
}

technique Technique1
{
	pass FishingSpotPass
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}