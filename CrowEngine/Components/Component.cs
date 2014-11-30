using System;

namespace CrowEngine.Components
{
	public abstract class Component : Object, IDisposable
	{
		private GameObject m_GameObject;

		public GameObject GameObject
		{
			get { return m_GameObject; }
		}

		public override string Name
		{
			get { return m_GameObject.Name; }
			set { m_GameObject.Name = value; }
		}

		public Transform Transform
		{
			get { return m_GameObject.Transform; }
		}

		public void Dispose ()
		{
			_Destroy ();
		}

		internal virtual void _Setup ( GameObject root )
		{
			m_GameObject = root;
			if ( this is IInitializable )
			{
				try
				{
					((IInitializable)this).OnInitialize ();
				}
				catch ( Exception ex )
				{
				}
			}
		}

		internal virtual void _Destroy ()
		{
			if ( m_GameObject != null )
			{
				m_GameObject.RemoveComponent ( this );
				m_GameObject = null;
			}
			if ( this is IInitializable )
			{
				try
				{
					((IInitializable)this).OnDestroy ();
				}
				catch ( Exception ex )
				{
				}
			}
		}
	}
}