using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowEngine
{
	enum PixelShaderSemanticInput
	{
		COLOR,		// Diffuse or specular color.	float4
		TEXCOORD,	// Texture coordinates	float4
	}
	enum PixelShaderSemanticOutput
	{
		COLOR, // Output color	float4
		DEPTH, // Output depth	float
	}
}
