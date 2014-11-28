using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowEngine
{
	internal class ObjectPool<T>
	{
		private readonly Func<T> m_OnCreate;
		private readonly Action<T> m_OnGet;
		private readonly Action<T> m_OnRecycle;

		private readonly Stack<T> m_Stack = new Stack<T> ();

		public ObjectPool ( Func<T> onCreate, Action<T> onGet = null, Action<T> onRecycle = null )
		{
			if ( onCreate == null )
				throw new ArgumentNullException ( "onCreate" );
			m_OnCreate = onCreate;
			m_OnGet = onGet;
			m_OnRecycle = onRecycle;
		}

		public T Get ()
		{
			T t;
			if ( m_Stack.Count == 0 )
				t = m_OnCreate ();
			else
				t = m_Stack.Pop ();
			if ( m_OnGet != null )
				m_OnGet ( t );
			return t;
		}

		public void Recycle ( T element )
		{
			if ( m_Stack.Count > 0 && m_Stack.Contains ( element ) )
				throw new ArgumentException ();
			if ( m_OnRecycle != null )
				m_OnRecycle ( element );
			m_Stack.Push ( element );
		}

		public void Clear ()
		{
			m_Stack.Clear ();
			m_Stack.TrimExcess ();
		}
	}
}