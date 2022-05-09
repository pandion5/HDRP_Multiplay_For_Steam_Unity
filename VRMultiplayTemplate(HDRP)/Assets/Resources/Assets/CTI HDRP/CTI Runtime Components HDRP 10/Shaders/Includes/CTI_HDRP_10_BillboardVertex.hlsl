uniform float4 unity_BillboardCameraParams;
#define unity_BillboardCameraPosition (unity_BillboardCameraParams.xyz)

float3 unity_BillboardSize;
float4 _CTI_SRP_Wind;

float4 SmoothCurve(float4 x) {
	return x * x * (3.0 - 2.0 * x);
}

float4 TriangleWave(float4 x) {
	return abs(frac(x + 0.5) * 2.0 - 1.0);
}

float4 AfsSmoothTriangleWave(float4 x) {
	return (SmoothCurve(TriangleWave(x)) - 0.5) * 2.0;
}

// This function always return the absolute position in WS
float3 CTI_GetAbsolutePositionWS(float3 positionRWS)
{
#if (SHADEROPTIONS_CAMERA_RELATIVE_RENDERING != 0)
    positionRWS += _WorldSpaceCameraPos;
#endif
    return positionRWS;
}
// Return absolute world position of current object
float3 CTI_GetObjectAbsolutePositionWS()
{
    float4x4 modelMatrix = UNITY_MATRIX_M;
    return CTI_GetAbsolutePositionWS(modelMatrix._m03_m13_m23); // Translation object to world
}

//	Billboard Vertex Function

//	IMPORTANT: Only use original mesh attributes
//	colorVAriation has to be written to uv1!? otherwise it gets evaluated twice?!

void CTIBillboard_float (
	in float3  positionOS,
	in float2  uv,
	in float3  uv1,

	in float   windStrength,
	in float   windPower,

	out float3 positionOut,
	out float3 normalOut,
	out float3 tangentOut,
	out float2 uvOut,
	out float cv
) {


// we are in? ApplyMeshModification

// See: VertMesh.hlsl: positionRWS gets set up AFTER calling ApplyMeshModification!?
// This return the camera relative position (if enable)
// float3 positionRWS = TransformObjectToWorld(input.positionOS);

// ApplyVertexModification happens later...


	float3 position = positionOS;
// 	Using sum to handle dynamic batching!? - well: it gets shifted to the pixel shader anyway.
	float3 worldPos = positionOS + CTI_GetObjectAbsolutePositionWS();
	float3 TreeWorldPos = worldPos;

//	Get Color Variation
//	This seams to be calculated in a 2nd evaluation? and thus operates on the extruded vertices
	float colorVariation = saturate ( ( frac(TreeWorldPos.x + TreeWorldPos.y + TreeWorldPos.z) + frac( (TreeWorldPos.x + TreeWorldPos.y + TreeWorldPos.z) * 3.3 ) ) * 0.5 );


// 	////////////////////////////////////
//	Set vertex position

	float3 positionRWS = TransformObjectToWorld(positionOS);
	float3 eyeVec = GetWorldSpaceNormalizeViewDir(positionRWS);
	
	float3 billboardTangent = normalize(float3(-eyeVec.z, 0, eyeVec.x));
	float3 billboardNormal = float3(billboardTangent.z, 0, -billboardTangent.x);	// cross({0,1,0},billboardTangent)

	float2 percent = uv.xy;
	float3 billboardPos = 0;
	billboardPos.xz = (percent.x - 0.5) * unity_BillboardSize.x * uv1.x * billboardTangent.xz;
	billboardPos.y += (percent.y * unity_BillboardSize.y * 2.0 + unity_BillboardSize.z) * uv1.y;
	position.xyz += billboardPos;

	positionOut = position.xyz;

// 	////////////////////////////////////
//	Wind

	if (windStrength > 0) {
		//worldPos.xyz = abs(worldPos.xyz * 0.125f);
		float sinuswave = _SinTime.z;
		float shiftedsinuswave = (_SinTime.z + _SinTime.w) * 0.5;
		float4 vOscillations = AfsSmoothTriangleWave(float4(worldPos.x + sinuswave, worldPos.z + sinuswave * 0.7, worldPos.x + shiftedsinuswave, worldPos.z + shiftedsinuswave * 0.8));
		float fOsc = vOscillations.x + (vOscillations.y * vOscillations.y);
		fOsc = 0.75 + (fOsc + 3.33) * 0.33;
		positionOut += windStrength * _CTI_SRP_Wind.w * _CTI_SRP_Wind.xyz * fOsc * pow(percent.y, windPower);	// pow(y,1.5) matches the wind baked to the mesh trees
	}

// 	////////////////////////////////////
//	Get billboard texture coords

// 	Here we need the billboard Tangent and Normal in absolute WS
	float3 viewVecAbsWS;
//	We have to distinguish between shadow caster and regular passes
	#if(SHADERPASS == SHADERPASS_SHADOWS)
//	From ShaderVariablesFunctions.hlsl
		float3 camPos = GetCurrentViewPosition();
		viewVecAbsWS = normalize(camPos - ( worldPos) );
	#else
		viewVecAbsWS = normalize(_WorldSpaceCameraPos - worldPos);
	#endif
	float3 billboardTangentAbsWS = normalize(float3(-viewVecAbsWS.z, 0, viewVecAbsWS.x));
	float3 billboardNormalAbsWS = float3(billboardTangentAbsWS.z, 0, -billboardTangentAbsWS.x);
	
	float angle = atan2(billboardNormalAbsWS.z, billboardNormalAbsWS.x); // signed angle between billboardNormal to {0,0,1}
	angle += angle < 0 ? 2 * PI : 0;										
//	Set Rotation
	angle += uv1.z;
//	Write final billboard texture coords
	const float invDelta = 1.0 / (45.0 * ((PI * 2.0) / 360.0));
	float imageIndex = fmod(floor(angle * invDelta + 0.5f), 8);
	float2 column_row;
// 	We do not care about the horizontal coord that much as our billboard texture tiles
	column_row.x = imageIndex * 0.25; 
	column_row.y = saturate(4 - imageIndex) * 0.5;
	uvOut.xy = column_row + uv.xy * float2(0.25, 0.5);

// 	////////////////////////////////////
//	Set Normal and Tangent

//  Using no flip mode
	normalOut = normalize(billboardNormal.xyz);
	tangentOut = normalize(billboardTangent.xyz);
//	This leaves up down being flipped - so we fix this in the pixel shader -->

//	Store Color Variation
	cv = colorVariation;
}