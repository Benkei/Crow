using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowEngine
{
	enum VertexShaderSemanticInput
	{
		BINORMAL,		// Binormal; float4
		BLENDINDICES,	// Blend indices; uint
		BLENDWEIGHT,	// Blend weights; float
		COLOR,			// Diffuse and specular color; float4
		NORMAL,			// Normal vector; float4
		POSITION,		// Vertex position in object space; float4
		POSITIONT,		// Transformed vertex position; float4
		PSIZE,			// Point size; float
		TANGENT,			// Tangent; float4
		TEXCOORD0,		// Texture coordinates; float4
		TEXCOORD1,		// Texture coordinates; float4
	}
	enum VertexShaderSemanticOutput
	{
		COLOR,		// Diffuse or specular color; float4
		FOG,		// Vertex fog; float
		POSITION,	// Position of a vertex in homogenous space;	float4
		PSIZE,		// Point size; float
		TESSFACTOR,	// Tessellation factor; float
		TEXCOORD,	// Texture coordinates; float4
	}
}
