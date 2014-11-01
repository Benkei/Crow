using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowEngine.GpuPrograms
{
	class GpuProgramAsset
	{
		public ShaderAsset Fragment;
		public VertexShaderAsset Vertex;
		public ShaderAsset Geometry;
		public ShaderAsset TessEvaluation;
		public ShaderAsset TessControl;
		public ShaderAsset Compute;
	}

	class ShaderAsset
	{
		public string Path;
		public string Source;
	}

	class VertexShaderAsset : ShaderAsset
	{
		public Dictionary<string, string> SemanticInput;
	}
}
