using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowGlslOptimizer
{
	// Options flags for glsl_optimize
	[Flags]
	public enum Options : byte
	{
		None,
		// Skip preprocessing shader source. Saves some time if you know you don't need it.
		SkipPreprocessor = (1 << 0),
		// Passed shader is not the full shader source. This makes some optimizations weaker.
		NotFullShader = (1 << 1),
	}
}
