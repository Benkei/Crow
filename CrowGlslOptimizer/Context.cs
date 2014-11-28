using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CrowGlslOptimizer
{
	public class Context : IDisposable
	{
		IntPtr m_Handler;

		public Context ( Target target )
		{
			m_Handler = Native.glslopt_initialize ( target );
		}

		~Context ()
		{
			Native.glslopt_cleanup ( m_Handler );
		}

		public void SetMaxUnrollIterations ( uint iteration )
		{
			Native.glslopt_set_max_unroll_iterations ( m_Handler, iteration );
		}

		public Shader Optimize ( ShaderType type, string shaderSource, Options options )
		{
			var ptr = Marshal.StringToHGlobalAnsi ( shaderSource );
			try
			{
				var h = Native.glslopt_optimize ( m_Handler, type, ptr, options );
				return new Shader ( h );
			}
			finally
			{
				Marshal.FreeHGlobal ( ptr );
			}
		}

		public void Dispose ()
		{
			Native.glslopt_cleanup ( m_Handler );
			GC.SuppressFinalize ( this );
		}
	}
}
