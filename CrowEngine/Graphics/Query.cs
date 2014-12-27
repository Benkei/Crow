using OpenTK.Graphics.OpenGL4;

namespace CrowEngine.Graphics
{
	class Query : GLObject
	{
		public Query ()
		{
			m_Handler = GL.GenQuery ();

			//GL.GetQuery ( QueryTarget.Timestamp, GetQueryParam.CurrentQuery, null );
			//GL.GetQueryObject ( m_Handler, GetQueryObjectParam.QueryResultAvailable, null );

			//GL.QueryCounter ( m_Handler, QueryCounterTarget.Timestamp );
		}
		
		public bool Available
		{
			get
			{
				int b;
				GL.GetQueryObject ( m_Handler, GetQueryObjectParam.QueryResultAvailable, out b );
				return b == 1;
			}
		}

		public long Result
		{
			get
			{
				long r;
				GL.GetQueryObject ( m_Handler, GetQueryObjectParam.QueryResult, out r );
				return r;
			}
		}

		public long ResultNoWait
		{
			get
			{
				long r;
				GL.GetQueryObject ( m_Handler, GetQueryObjectParam.QueryResultNoWait, out r );
				return r;
			}
		}


		public void Delete ()
		{
			GL.DeleteQuery ( m_Handler );
			m_Handler = 0;
		}

		public void Begin ( QueryTarget target )
		{
			GL.BeginQuery ( target, m_Handler );
			//GL.BeginQueryIndexed ( target, 0, m_Handler );
		}

		public void End ( QueryTarget target )
		{
			GL.EndQuery ( target );
		}
	}
}
