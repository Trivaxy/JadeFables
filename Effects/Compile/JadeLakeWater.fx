float time;
float2 screenSize;
float2 offset;
float alpha;

texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture sampleTexture2;
sampler2D samplerTex2 = sampler_state { texture = <sampleTexture2>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture bubbleTex;
sampler2D samplerBubble = sampler_state { texture = <bubbleTex>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture sampleTexture3;
sampler2D samplerTex3 = sampler_state { texture = <sampleTexture3>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float4 PixelShaderFunction(float4 screenSpace : TEXCOORD0) : COLOR0
{
    float2 st = screenSpace.xy;

    float4 color = tex2D(samplerTex, st);
    float power = tex2D(samplerTex2, (st + offset)).r;

    if (tex2D(samplerBubble, st + offset).r > 0)
        return float4(0,0,0,0);

    float factor = (0.4 + sin(power * 30.0 + time) * 0.6) * power;
    float bright = ((color.r + color.b + color.g) / 2.0);

    float4 color2 = float4(0.0, 0.0, 0.0, color.a);

    color2.r = lerp(color.r, 0.2 * bright, sqrt((power + factor) / 2.0));
    color2.g = lerp(color.g, 1.0 * bright, sqrt((power + factor) / 2.0));
    color2.b = lerp(color.b, 1.4 * bright, sqrt((power + factor) / 2.0));

    float4 color3 = tex2D(samplerTex3, st + float2(time * 0.002, time * 0.002) + offset) * 0.5;
    color3 += tex2D(samplerTex3, st + float2(time * -0.0015, time * 0.0012) + offset) * 0.35;

    bright = min(bright, 0.65);

    color2.g += color3.r * power * pow(bright, 2) * 60.0;
    color2.b += color3.r * power * pow(bright, 2) * 70.0;
    //color2.r += color3.r * power * pow(bright, 2) * 40.0;

    //return float4(offset.x * -1.0, offset.y * -1.0, 0, 1);

    return color2 * ((1.0 - power * 0.8)) * alpha;
}

technique Technique1
{
    pass PrimitivesPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
};