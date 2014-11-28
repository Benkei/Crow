using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowGlslOptimizer
{
	// Type info
	public enum BasicType : byte
	{
		Float = 0,
		Int,
		Bool,
		Tex2D,
		Tex3D,
		TexCube,
		Other,
		
		Count
	}
}
