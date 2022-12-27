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

float alpha;

float4 White(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(uImage0, coords);
    if (color.a == 0)
        return float4(0,0,0,0);
    return float4(1,1,1,1) * alpha;
}

technique Technique1
{
    pass JadeKunaiFlash
    {
        PixelShader = compile ps_2_0 White();
    }
}