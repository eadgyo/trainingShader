uniform extern float4x4 gWorldInverseTranspose;
uniform extern float4x4 World;
uniform extern float4x4 View;
uniform extern float4x4 Projection;
uniform extern float4x4 gWVP;

uniform extern float4 gAmbientMtrl;
uniform extern float4 gAmbientLight;
uniform extern float4 gDiffuseMtrl;
uniform extern float4 gDiffuseLight;
uniform extern float3 gLightVecW;

uniform extern float4 gSpecularMtrl;
uniform extern float4 gSpecularLight;
uniform extern float gSpecularPower;

uniform extern float3 gEyePos;

uniform extern float4x4 gFinalXForms[35];

struct OutputVS {
    float4 posH : POSITION0;
    float4 color : COLOR0;
};

OutputVS VBlend2VS(float3 posL : POSITION0,
    float3 normalL : NORMAL0,
    float2 tex0 : TEXCOORD0,
    float4 weight : BLENDWEIGHT0,
    int4 boneIndex : BLENDINDICES0,
    float4 color : COLOR0)
{
    OutputVS outVS = (OutputVS)0;
    
    // Equation 16.3
    float4 p = weight[0] * mul(float4(posL, 1.0f),
        gFinalXForms[boneIndex[0]]);
    
    if (weight[1] != 0)
    {
        p += weight[1] * mul(float4(posL, 1.0f),
            gFinalXForms[boneIndex[1]]);
    }

    if (weight[2] != 0)
    {
        p += weight[2] * mul(float4(posL, 1.0f),
            gFinalXForms[boneIndex[2]]);
    }
    
    if (weight[3] != 0)
    {
        p += weight[3] * mul(float4(posL, 1.0f),
            gFinalXForms[boneIndex[3]]);
    }
    p.w = 1.0f;



    // Equation 16.4
    float4 n = weight[0] * mul(float4(normalL, 0.0f),
        gFinalXForms[boneIndex[0]]);
    if (weight[1] != 0)
    {
        n += weight[1] * mul(float4(normalL, 0.0f),
            gFinalXForms[boneIndex[1]]);
    }
    if (weight[2] != 0)
    {
        n += weight[2] * mul(float4(normalL, 0.0f),
            gFinalXForms[boneIndex[2]]);
    }
    if (weight[3] != 0)
    {
        n += weight[3] * mul(float4(normalL, 0.0f),
            gFinalXForms[boneIndex[3]]);
    }
    n.w = 0.0f;

    float3 normalW = mul(n, gWorldInverseTranspose).xyz;
	normalW = normalize(normalW);

	// Transform vertex position to world space.
	float3 posW = mul(p, World).xyz;

	// ======================================================
	// Compute the color: Equation 10.3.
	float3 toEye = normalize(gEyePos - posW);
	
	// Normalize the light vec
	float3 lightVecW = normalize(gLightVecW);

	// Compute the reflection vector.
	float3 r = reflect(-lightVecW, normalW);

	// Determine how much (if any) specular light makes it into eye.
	float t = pow(max(dot(r, toEye), 0.0f), gSpecularPower);

	// Determine the diffuse light intensity, and specular terms seperately.
	float s = saturate(max(dot(lightVecW, normalW), 0.0f));

	// Compute the ambient, diffuse, and specular terms separately.
	float3 spec = t * (gSpecularMtrl * gSpecularLight).rgb;
	float3 diffuse = s * (gDiffuseMtrl * gDiffuseLight).rgb;
	float3 ambient = (gAmbientMtrl * gAmbientLight).rgb;

	// Sum all the terms together and copy over the diffuse alpha.
	outVS.color.rgb = ambient + diffuse + spec;
	outVS.color.a = gDiffuseMtrl.a;

	// Transform to homogeneous clip space
	outVS.posH = mul(p, gWVP);
    return outVS;
}

float4 TransformPS(float4 color : COLOR0) : COLOR
{
    return color;
}

technique TransformTech
{
    pass PO
    {
        vertexShader = compile vs_2_0 VBlend2VS();
        pixelShader = compile ps_2_0 TransformPS();
    }
};