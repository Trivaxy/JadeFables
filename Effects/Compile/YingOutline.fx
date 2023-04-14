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
float4 outlineColor;

float4 White(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
	float pixW = 2.0 / uImageSize0.x;
	float pixH = 2.0 / uImageSize0.y;
    coords.x = floor(coords.x / pixW) * pixW;
    coords.H = floor(coords.x / pixH) * pixH;
	float4 color = tex2D(uImage0, coords);
    float2 squareWidth = float2(pixW, pixH);
    float4 outlineColor2 = outlineColor * alpha;

    float4 clear = float4(0.0, 0.0, 0.0, 0.0);

    if (color.a != 0.0)
    {
        return color;
    }

    float2 opposite = squareWidth * float2(1.0, -1.0);

    if (tex2D(uImage0, coords + squareWidth).a != 0.0)
        return outlineColor2;
    if (tex2D(uImage0, coords - squareWidth).a != 0.0)
        return outlineColor2;
    if (tex2D(uImage0, coords + opposite).a != 0.0)
        return outlineColor2;
    if (tex2D(uImage0, coords - opposite).a != 0.0)
        return outlineColor2;

    return clear;
}

technique Technique1
{
    pass JadeOutline
    {
        PixelShader = compile ps_2_0 White();
    }
}