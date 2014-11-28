using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowGlslOptimizer
{
	// Optimizer target language
	public enum Target : byte
	{
		OpenGL = 0,
		OpenGLES20 = 1,
		OpenGLES30 = 2,
		Metal = 3,
	}
}
