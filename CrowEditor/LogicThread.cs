using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CrowEditor.Jobs;

namespace CrowEditor
{
	class LogicThread : EditorThread
	{
		Stopwatch m_Watch;

		public readonly JobScheduler JobScheduler = new JobScheduler ();

		public LogicThread ()
		{
			m_Watch = new Stopwatch ();
		}

		public float DeltaTime { get; private set; }

		protected override void OnThreadUpdate ( object context )
		{
			m_Watch.Start ();
			while ( m_Running )
			{
				JobScheduler.Execute ();
				CrowEditorApp.OnUpdateLogicThread ();
				Thread.Sleep ( 1 );
				DeltaTime = (float)m_Watch.Elapsed.TotalSeconds;
				m_Watch.Restart ();
			}
		}
	}
}
