using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CrowGlslOptimizer
{
	public class Shader : IDisposable
	{
		IntPtr m_Handler;

		public Shader ( IntPtr handler )
		{
			m_Handler = handler;
		}

		~Shader ()
		{
			Native.glslopt_shader_delete ( m_Handler );
		}

		public bool Status
		{
			get { return Native.glslopt_get_status ( m_Handler ) == 1; }
		}

		public string Output
		{
			get { return Marshal.PtrToStringAnsi ( Native.glslopt_get_output ( m_Handler ) ); }
		}

		public string RawOutput
		{
			get { return Marshal.PtrToStringAnsi ( Native.glslopt_get_raw_output ( m_Handler ) ); }
		}

		public string Log
		{
			get { return Marshal.PtrToStringAnsi ( Native.glslopt_get_log ( m_Handler ) ); }
		}

		public int InputCount
		{
			get { return Native.glslopt_shader_get_input_count ( m_Handler ); }
		}

		public int UniformCount
		{
			get { return Native.glslopt_shader_get_uniform_count ( m_Handler ); }
		}

		public int TextureCount
		{
			get { return Native.glslopt_shader_get_texture_count ( m_Handler ); }
		}

		public int UniformTotalSize
		{
			get { return Native.glslopt_shader_get_uniform_total_size ( m_Handler ); }
		}

		public Description GetInput ( int index )
		{
			Description desc;
			IntPtr name;
			Native.glslopt_shader_get_input_desc ( m_Handler, index,
				out name, out desc.Type, out desc.Prec, out desc.VecSize,
				out desc.MatSize, out desc.ArraySize, out desc.Location );
			desc.Name = Marshal.PtrToStringAnsi ( name );
			return desc;
		}

		public Description GetUniform ( int index )
		{
			Description desc;
			IntPtr name;
			Native.glslopt_shader_get_uniform_desc ( m_Handler, index,
				out name, out desc.Type, out desc.Prec, out desc.VecSize,
				out desc.MatSize, out desc.ArraySize, out desc.Location );
			desc.Name = Marshal.PtrToStringAnsi ( name );
			return desc;
		}

		public Description GetTexture ( int index )
		{
			Description desc;
			IntPtr name;
			Native.glslopt_shader_get_texture_desc ( m_Handler, index,
				out name, out desc.Type, out desc.Prec, out desc.VecSize,
				out desc.MatSize, out desc.ArraySize, out desc.Location );
			desc.Name = Marshal.PtrToStringAnsi ( name );
			return desc;
		}

		public void GetStats ( out int approxMath, out int approxTex, out int approxFlow )
		{
			Native.glslopt_shader_get_stats ( m_Handler, out approxMath, out approxTex, out approxFlow );
		}

		public void Dispose ()
		{
			Native.glslopt_shader_delete ( m_Handler );
			GC.SuppressFinalize ( this );
		}
	}

	public struct Description
	{
		public string Name;
		public BasicType Type;
		public Precision Prec;
		public int VecSize;
		public int MatSize;
		public int ArraySize;
		public int Location;
	}
}
