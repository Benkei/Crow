using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowEngine.Pooling
{
	public struct PoolHandler<T> : IDisposable
	{
		private BaseObjectPool<T> m_Pool;
		private T m_Value;

		public PoolHandler ( BaseObjectPool<T> pool, T value )
		{
			m_Pool = pool;
			m_Value = value;
		}

		public T Value
		{
			get { return m_Value; }
		}

		public void Dispose ()
		{
			m_Pool.Recycle ( m_Value );
			m_Pool = null;
			m_Value = default ( T );
		}

		public static explicit operator T ( PoolHandler<T> handler )
		{
			return handler.m_Value;
		}
	}
}
