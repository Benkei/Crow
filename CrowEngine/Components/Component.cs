using System;

namespace CrowEngine.Components
{
	public abstract class Component : Object
	{
		GameObject m_GameObject;


		public GameObject GameObject
		{
			get { return m_GameObject; }
		}
		public Transform Transform
		{
			get { return m_GameObject.Transform; }
		}
		public override string Name
		{
			get { return m_GameObject.Name; }
			set { m_GameObject.Name = value; }
		}


		internal void Init ( GameObject root )
		{
			m_GameObject = root;
			OnAwake ();
		}

		protected virtual void OnAwake ()
		{
		}

		internal override void DestroyObject ()
		{
			base.DestroyObject ();
			if ( m_GameObject != null )
			{
				m_GameObject.RemoveComponent ( this );
				m_GameObject = null;
			}
		}
	}
}
