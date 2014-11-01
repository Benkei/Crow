using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LitJson;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine.GpuPrograms
{
	class GpuProgramFactory
	{
		public static GpuProgram Load ( string filePath )
		{
			GpuProgramAsset data;
			using ( var reader = new StreamReader ( filePath, Encoding.UTF8 ) )
			{
				data = JsonMapper.ToObject<GpuProgramAsset> ( reader );
			}

			var folder = Path.GetDirectoryName ( filePath );

			var vertex = new GpuShader (
				ShaderType.VertexShader,
				File.ReadAllText ( Path.Combine ( folder, data.Vertex.Path ), Encoding.ASCII )
			);

			var fragment = new GpuShader (
				ShaderType.FragmentShader,
				File.ReadAllText ( Path.Combine ( folder, data.Fragment.Path ), Encoding.ASCII )
			);

			var program = new GpuProgram ();
			program.SetShader ( vertex );
			program.SetShader ( fragment );

			if ( data.Vertex.SemanticInput != null )
			{
				foreach ( var item in data.Vertex.SemanticInput )
				{
					program.BindAttributeLocation ( 
						item.Value,
						(int)(VertexShaderSemanticInput)Enum.Parse ( typeof ( VertexShaderSemanticInput ), item.Key ) 
					);
				}
			}

			program.Link ();

			return program;
		}
	}
}
