matrix transformMatrix;

texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture noiseTexture;
sampler2D noiseTex = sampler_state { texture = <noiseTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float topIndex;
float bottomIndex;
float gradientStart;
float gradientEnd;

float multiplyNoiseScale;
float generalProgress = 0;

bool disabled;

float noiseRange;
float timeLeftSkeleton;

bool flip;


//Hlsl's % operator applies a modulo but conserves the sign of the dividend, hence the need for a custom modulo
float mod(float a, float n)
{
    return a - floor(a / n) * n;
}
float LinearLight(float bottom, float top)
{
    return bottom + top * 2 - 1;
}

struct VertexShaderInput
{
    float4 Position : POSITION;
    float2 TexCoords : TEXCOORD0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION;
    float2 TexCoords : TEXCOORD0;
    float4 Color : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    output.Color = input.Color;
    output.TexCoords = input.TexCoords;
    output.Position = mul(input.Position, transformMatrix);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 uv = input.TexCoords;

    float4 lightColor = input.Color;
    if (flip)
        uv.y = 1 - uv.y;
    float4 color = tex2D(samplerTex, uv);
    
    if (disabled)
        return float4(color.rgb * lightColor.rgb, color.a);
    
    if (color.a == 0)
        return color;
    
    float upwardsGradient = lerp(topIndex, bottomIndex, uv.y);
    //float correctedGradient = clamp((upwardsGradient - gradientStart) / (gradientEnd - gradientStart), 0, 1); //Squish our gradient to fit only into the range we need it.
    
    float progress = (1 + timeLeftSkeleton) - (1 + timeLeftSkeleton + noiseRange) * (1 - generalProgress);
    progress += pow((1 - upwardsGradient) * generalProgress, 3);
    
    //Multiply
    float multiplyValue = tex2D(noiseTex, float2(uv.x * multiplyNoiseScale, (uv.y + topIndex) * multiplyNoiseScale * 0.5));
    progress += (multiplyValue * noiseRange);
    
    float4 empty = float4(0, 0, 0, 0);
    if (progress <= 0.3)
    {
        color.xyz *= lightColor;
        return color;
    }
    else if (progress >= 1)
        return empty;
    
    progress -= 0.3;
    progress /= 0.7;
    
    color.r += progress * 2;
    color.g += pow(progress, 1.2) * 1.2;
    color.b += progress * 0.5;
    
    color.xyz *= lightColor;
    
    if (progress > 0.95)
        return lerp(color, empty, (progress - 0.95) / 0.05);
    
    //color.a *= (1 - correctedGradient);
    return color;
}

technique Technique1
{
    pass PrimitivesPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}