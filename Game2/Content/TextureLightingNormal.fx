uniform extern Texture2D gTex0;
uniform extern Texture2D gNormal0;

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

sampler samLinear = sampler_state
{
	Texture = <gTex0>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = WRAP;
	AddressV = WRAP;
};

sampler normalLinear = sampler_state
{
	Texture = <gNormal0>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = WRAP;
	AddressV = WRAP;
};

struct OutputVS
{
	float4 posH : POSITION0;
	float4 diffuse : COLOR0;
	float4 spec : COLOR1;
	float2 tex0 : TEXCOORD0;
};


OutputVS TransformVS(float3 posL : POSITION0, float3 normalL : NORMAL0, float2 tex0 : TEXCOORD0)
{
	OutputVS outVS = (OutputVS)0;

	float3 normalW = mul(float4(normalL, 0.0f), gWorldInverseTranspose).xyz;
	normalW = normalize(normalW);

	float3 posW = mul(float4(posL, 1.0f), World).xyz;

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
	float3 ambient = gAmbientMtrl.rgb * gAmbientLight.rgb;

	outVS.diffuse.rgb = ambient + diffuse;
	outVS.diffuse.a = gDiffuseMtrl.a;
	outVS.spec = float4(spec, 0.0f);

	// Transform to homogeneous clip space
	outVS.posH = mul(float4(posL, 1.0f), gWVP);
	outVS.tex0 = tex0;

	return outVS;
}

float4 TransformPS(float4 c : COLOR0, float4 spec : COLOR1, float2 tex0 : TEXCOORD0) : COLOR
{
	float3 texColor = tex2D(samLinear, tex0).rgb;
	float3 normalColor = tex2D(normalLinear, tex0).rgb;
	float3 lightVecW = normalize(gLightVecW);
	float s = saturate(max(dot(lightVecW, normalColor), 0.0f));

	float3 diffuse = s * (gDiffuseMtrl * gDiffuseLight).rgb * texColor;
	
	return float4(diffuse + spec.rgb, c.a);
}

technique TransformTech
{
	pass PO
	{
		vertexShader = compile vs_3_0 TransformVS();
		pixelShader = compile ps_3_0 TransformPS();
	}
};