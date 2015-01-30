using System;
using System.IO;
using System.Text;
using CrowSerialization.LitJson;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine.GpuPrograms
{
	public class GpuProgramFactory
	{
		public static GLProgram Load ( string filePath )
		{
			GpuProgramAsset data;
			using ( var stream = new StreamReader ( filePath, Encoding.UTF8 ) )
			{
				var reader = new JsonReader ( stream );
				data = JsonMapper.ToObject<GpuProgramAsset> ( reader );
			}

			var folder = Path.GetDirectoryName ( filePath );

			string source = File.ReadAllText ( Path.Combine ( folder, data.Vertex.Path ), Encoding.UTF8 );

			//CrowGlslOptimizer.Context ctx = new CrowGlslOptimizer.Context ( CrowGlslOptimizer.Target.OpenGL );
			//var s = ctx.Optimize ( CrowGlslOptimizer.ShaderType.Vertex, source, CrowGlslOptimizer.Options.None );

			//Console.WriteLine ( "VertexShader" );
			//Console.WriteLine ( "Raw" );
			//Console.WriteLine ( s.RawOutput );
			//Console.WriteLine ( "Opt" );
			//Console.WriteLine ( s.Output );

			var vertex = new GLShader ( ShaderType.VertexShader, source );

			//s.Dispose ();

			source = File.ReadAllText ( Path.Combine ( folder, data.Fragment.Path ), Encoding.UTF8 );
			//s = ctx.Optimize ( CrowGlslOptimizer.ShaderType.Fragment, source, CrowGlslOptimizer.Options.None );

			//Console.WriteLine ( "FragmentShader" );
			//Console.WriteLine ( "Raw" );
			//Console.WriteLine ( s.RawOutput );
			//Console.WriteLine ( "Opt" );
			//Console.WriteLine ( s.Output );

			var fragment = new GLShader ( ShaderType.FragmentShader, source );

			//s.Dispose ();

			//ctx.Dispose ();

			var program = new GLProgram ();
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

			program.Reflection ();

			return program;
		}


		private static void ShaderParser ( string shaderRootPath, StringBuilder shaderSource )
		{
			//System.Text.RegularExpressions.Regex
		}



		//		std::string Shader::PreprocessIncludes( const std::string&amp; source, const boost::filesystem::path&amp; filename, int level /*= 0 */ )
		//{
		//	PrintIndent();
		//	if(level > 32)
		//		LogAndThrow(ShaderException,"header inclusion depth limit reached, might be caused by cyclic header inclusion");
		//	using namespace std;

		//	static const boost::regex re("^[ ]*#[ ]*include[ ]+[\"<](.*)[\">].*");
		//	stringstream input;
		//	stringstream output;
		//	input << source;

		//	size_t line_number = 1;
		//	boost::smatch matches;

		//	string line;
		//	while(std::getline(input,line))
		//	{
		//		if (boost::regex_search(line, matches, re))
		//		{
		//			std::string include_file = matches[1];
		//			std::string include_string;

		//			try
		//			{
		//				include_string = Core::FileIO::LoadTextFile(include_file);
		//			} 
		//			catch (Core::FileIO::FileNotFoundException&amp; e)
		//			{
		//				stringstream str;
		//				str <<  filename.file_string() <<"(" << line_number << ") : fatal error: cannot open include file " << e.File();
		//				LogAndThrow(ShaderException,str.str())
		//			}
		//			output << PreprocessIncludes(include_string, include_file, level + 1) << endl;
		//		}
		//		else
		//		{
		//			output << "#line "<< line_number << " \"" << filename << "\""  << endl;
		//			output <<  line << endl;
		//		}
		//		++line_number;
		//	}
		//	PrintUnindent();
		//	return output.str();
		//}
	}
}
