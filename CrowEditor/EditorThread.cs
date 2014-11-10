using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrowEditor
{
	abstract class EditorThread
	{
		protected bool m_Running;
		protected Thread m_Thread;

		public void Start ( object obj = null )
		{
			m_Running = true;
			m_Thread = new Thread ( OnThreadUpdate );
			m_Thread.Name = GetType ().Name;
			m_Thread.IsBackground = true;
			m_Thread.Start ( obj );

			OnStart ( obj );
		}

		public void Stop ()
		{
			m_Running = false;
			if ( !m_Thread.Join ( 1000 * 15 ) )
			{
				m_Thread.Abort ();
			}

			OnStop ();
		}

		protected virtual void OnStart ( object obj )
		{
		}

		protected virtual void OnStop ()
		{
		}

		protected abstract void OnThreadUpdate ( object context );
	}
}
