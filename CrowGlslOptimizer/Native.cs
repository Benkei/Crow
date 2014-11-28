using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CrowGlslOptimizer
{
	public static class Native
	{
		//extern "C" __declspec(dllexport) glslopt_ctx* glslopt_initialize(glslopt_target target);
		//extern "C" __declspec(dllexport) void glslopt_cleanup(glslopt_ctx* ctx);

		//extern "C" __declspec(dllexport) void glslopt_set_max_unroll_iterations(glslopt_ctx* ctx, unsigned iterations);

		//extern "C" __declspec(dllexport) glslopt_shader* glslopt_optimize(glslopt_ctx* ctx, glslopt_shader_type type, const char* shaderSource, unsigned options);
		//extern "C" __declspec(dllexport) bool glslopt_get_status(glslopt_shader* shader);
		//extern "C" __declspec(dllexport) const char* glslopt_get_output (glslopt_shader* shader);
		//extern "C" __declspec(dllexport) const char* glslopt_get_raw_output (glslopt_shader* shader);
		//extern "C" __declspec(dllexport) const char* glslopt_get_log (glslopt_shader* shader);
		//extern "C" __declspec(dllexport) void glslopt_shader_delete (glslopt_shader* shader);

		//extern "C" __declspec(dllexport) int glslopt_shader_get_input_count (glslopt_shader* shader);
		//extern "C" __declspec(dllexport) void glslopt_shader_get_input_desc (glslopt_shader* shader, int index, const char** outName, glslopt_basic_type* outType, glslopt_precision* outPrec, int* outVecSize, int* outMatSize, int* outArraySize, int* outLocation);
		//extern "C" __declspec(dllexport) int glslopt_shader_get_uniform_count (glslopt_shader* shader);
		//extern "C" __declspec(dllexport) int glslopt_shader_get_uniform_total_size (glslopt_shader* shader);
		//extern "C" __declspec(dllexport) void glslopt_shader_get_uniform_desc (glslopt_shader* shader, int index, const char** outName, glslopt_basic_type* outType, glslopt_precision* outPrec, int* outVecSize, int* outMatSize, int* outArraySize, int* outLocation);
		//extern "C" __declspec(dllexport) int glslopt_shader_get_texture_count (glslopt_shader* shader);
		//extern "C" __declspec(dllexport) void glslopt_shader_get_texture_desc (glslopt_shader* shader, int index, const char** outName, glslopt_basic_type* outType, glslopt_precision* outPrec, int* outVecSize, int* outMatSize, int* outArraySize, int* outLocation);

		//// Get *very* approximate shader stats:
		//// Number of math, texture and flow control instructions.
		//extern "C" __declspec(dllexport) void glslopt_shader_get_stats(glslopt_shader* shader, int* approxMath, int* approxTex, int* approxFlow);

		const string DllPath = "lib/GlslOptimizer.dll";

		[DllImport ( DllPath, CallingConvention = CallingConvention.Cdecl )]
		public static extern IntPtr glslopt_initialize ( Target target );
		[DllImport ( DllPath, CallingConvention = CallingConvention.Cdecl )]
		public static extern void glslopt_cleanup ( IntPtr ctx );

		[DllImport ( DllPath, CallingConvention = CallingConvention.Cdecl )]
		public static extern void glslopt_set_max_unroll_iterations ( IntPtr ctx, uint iterations );

		[DllImport ( DllPath, CallingConvention = CallingConvention.Cdecl )]
		public static extern IntPtr glslopt_optimize ( IntPtr ctx, ShaderType type, IntPtr shaderSource, Options options );
		[DllImport ( DllPath, CallingConvention = CallingConvention.Cdecl )]
		public static extern int glslopt_get_status ( IntPtr shader );
		[DllImport ( DllPath, CallingConvention = CallingConvention.Cdecl )]
		public static extern IntPtr glslopt_get_output ( IntPtr shader );
		[DllImport ( DllPath, CallingConvention = CallingConvention.Cdecl )]
		public static extern IntPtr glslopt_get_raw_output ( IntPtr shader );
		[DllImport ( DllPath, CallingConvention = CallingConvention.Cdecl )]
		public static extern IntPtr glslopt_get_log ( IntPtr shader );
		[DllImport ( DllPath, CallingConvention = CallingConvention.Cdecl )]
		public static extern void glslopt_shader_delete ( IntPtr shader );

		[DllImport ( DllPath, CallingConvention = CallingConvention.Cdecl )]
		public static extern int glslopt_shader_get_input_count ( IntPtr shader );
		[DllImport ( DllPath, CallingConvention = CallingConvention.Cdecl )]
		public static extern void glslopt_shader_get_input_desc ( IntPtr shader, int index, out IntPtr outName, out BasicType outType, out Precision outPrec, out int outVecSize, out int outMatSize, out int outArraySize, out int outLocation );
		[DllImport ( DllPath, CallingConvention = CallingConvention.Cdecl )]
		public static extern int glslopt_shader_get_uniform_count ( IntPtr shader );
		[DllImport ( DllPath, CallingConvention = CallingConvention.Cdecl )]
		public static extern int glslopt_shader_get_uniform_total_size ( IntPtr shader );
		[DllImport ( DllPath, CallingConvention = CallingConvention.Cdecl )]
		public static extern void glslopt_shader_get_uniform_desc ( IntPtr shader, int index, out IntPtr outName, out BasicType outType, out Precision outPrec, out int outVecSize, out int outMatSize, out int outArraySize, out int outLocation );
		[DllImport ( DllPath, CallingConvention = CallingConvention.Cdecl )]
		public static extern int glslopt_shader_get_texture_count ( IntPtr shader );
		[DllImport ( DllPath, CallingConvention = CallingConvention.Cdecl )]
		public static extern void glslopt_shader_get_texture_desc ( IntPtr shader, int index, out IntPtr outName, out BasicType outType, out Precision outPrec, out int outVecSize, out int outMatSize, out int outArraySize, out int outLocation );

		[DllImport ( DllPath, CallingConvention = CallingConvention.Cdecl )]
		public static extern void glslopt_shader_get_stats ( IntPtr shader, out int approxMath, out int approxTex, out int approxFlow );
	}
}
