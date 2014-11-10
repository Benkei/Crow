using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CrowEditor.Jobs;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace CrowEditor
{
	class GLBackgroundThread : GLThread
	{
		public readonly JobScheduler JobScheduler = new JobScheduler ();

		public GLBackgroundThread ( GraphicsMode mode, GraphicsContextFlags flags )
			: base ( mode, flags )
		{
		}

		protected override void OnStart ( object obj )
		{
			base.OnStart ( obj );
			m_Thread.Priority = ThreadPriority.Lowest;
		}

		protected override void GLBeginUpdate ()
		{
		}

		protected override void GLUpdate ()
		{
			if ( JobScheduler.Execute () )
			{
				GL.Flush ();
			}
		}

		protected override void GLEndUpdate ()
		{
		}
	}
}
