using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowEngine.Pooling
{
	public sealed class ObjectPool<T> : BaseObjectPool<T>
	{
		private readonly Func<T> m_OnCreate;
		private readonly Action<T> m_OnGet;
		private readonly Action<T> m_OnRecycle;

		public ObjectPool ( Func<T> onCreate, Action<T> onGet = null, Action<T> onRecycle = null )
		{
			if ( onCreate == null )
				throw new ArgumentNullException ( "onCreate" );
			m_OnCreate = onCreate;
			m_OnGet = onGet;
			m_OnRecycle = onRecycle;
		}
		
		protected override T OnCreate ()
		{
			return m_OnCreate ();
		}

		protected override void OnGet ( ref T obj )
		{
			if ( m_OnGet != null )
				m_OnGet ( obj );
		}

		protected override void OnRecycle ( ref T obj )
		{
			if ( m_OnGet != null )
				m_OnGet ( obj );
		}
	}
}