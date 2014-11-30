using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowEngine.Pooling
{
	public abstract class BaseObjectPool<T>
	{
		private readonly Stack<T> m_Stack = new Stack<T> ();

		public T Get ()
		{
			T obj;
			if ( m_Stack.Count == 0 )
				obj = OnCreate ();
			else
				obj = m_Stack.Pop ();
			OnGet ( ref obj );
			return obj;
		}

		public PoolHandler<T> GetHandler ()
		{
			return new PoolHandler<T> ( this, Get () );
		}

		public void Recycle ( T element )
		{
			if ( element == null || (m_Stack.Count > 0 && m_Stack.Contains ( element )) )
				throw new ArgumentException ();
			OnRecycle ( ref element );
			m_Stack.Push ( element );
		}

		public void Clear ()
		{
			m_Stack.Clear ();
			m_Stack.TrimExcess ();
		}

		protected abstract T OnCreate ();

		protected virtual void OnGet ( ref T obj )
		{ }

		protected virtual void OnRecycle ( ref T obj )
		{ }
	}
}
