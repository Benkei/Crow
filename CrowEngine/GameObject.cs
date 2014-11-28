﻿using System;
using System.Collections.Generic;
using CrowEngine.Components;

namespace CrowEngine
{
	public sealed class GameObject : Object
	{
		private List<Component> m_Components;
		private Transform m_Transform;

		public GameObject ()
		{
			m_Transform = new Transform ();

			Active = true;
			Layer = ~0;
		}

		public string Tag
		{
			get;
			set;
		}

		public bool Active
		{
			get;
			set;
		}

		/// <summary>
		/// Returns true if the GameObject and all parents in the hierarchy is active
		/// </summary>
		public bool IsActive
		{
			get
			{
				if ( !Active )
					return false;
				var current = m_Transform.Parent;
				while ( current != null )
				{
					if ( !current.GameObject.Active )
						return false;
					current = current.Parent;
				}
				return true;
			}
		}

		public int Layer
		{
			get;
			set;
		}

		public Transform Transform
		{
			get { CheckDestroyed (); return m_Transform; }
		}

		//public SceneManager SceneManager
		//{
		//	get { CheckDestroyed (); return m_Transform.SceneManager; }
		//}
		public override string Name
		{
			get { CheckDestroyed (); return m_Transform.Name; }
			set { CheckDestroyed (); m_Transform.Name = value; }
		}

		public T GetComponent<T> ()
			where T : Component
		{
			if ( m_Components == null ) { return null; }
			T comp = null;
			for ( int i = 0, len = m_Components.Count; i < len; i++ )
			{
				if ( (comp = m_Components[i] as T) != null ) { break; }
			}
			return comp;
		}

		public T GetComponentInChildren<T> ( bool includeInactive = false )
			where T : Component
		{
			if ( m_Components == null || m_Transform.Count == 0 ) { return null; }
			T comp;
			for ( int i = 0, len = m_Transform.Count; i < len; i++ )
			{
				comp = m_Transform[i].GameObject.GetComponentInChildrenRecurse<T> ( includeInactive );
				if ( comp != null ) { return comp; }
			}
			return null;
		}

		public T GetComponentInParent<T> ()
			where T : Component
		{
			var trans = m_Transform;
			while ( trans != null )
			{
				var comp = trans.GameObject.GetComponent<T> ();
				if ( comp != null )
					return comp;
				trans = trans.Parent;
			}
			return null;
		}


		public T[] GetAllComponents<T> ()
			where T : Component
		{
			if ( m_Components == null || m_Components.Count == 0 )
				return Arrays<T>.Empty;
			List<T> comps = new List<T> ();
			GetAllComponents ( comps );
			return comps.ToArray ();
		}

		public int GetAllComponents<T> ( IList<T> buffer )
			where T : Component
		{
			if ( buffer == null )
				throw new ArgumentNullException ( "buffer" );
			if ( buffer.IsReadOnly )
				throw new ArgumentException ( "Given buffer is ReadOnly!", "buffer" );

			if ( m_Components == null || m_Components.Count == 0 ) { return 0; }
			T comp;
			int added = 0;
			for ( int i = 0, len = m_Components.Count; i < len; i++ )
			{
				if ( (comp = m_Components[i] as T) != null )
				{
					buffer.Add ( comp );
					added++;
				}
			}
			return added;
		}


		public T[] GetAllComponentsInChildren<T> ( bool includeInactive = false )
			where T : Component
		{
			if ( m_Transform.Count == 0 )
				return Arrays<T>.Empty;
			List<T> comps = new List<T> ();
			GetAllComponentsInChildren ( comps, includeInactive );
			return comps.Count == 0 ? Arrays<T>.Empty : comps.ToArray ();
		}

		public int GetAllComponentsInChildren<T> ( IList<T> buffer, bool includeInactive = false )
			where T : Component
		{
			if ( buffer == null )
				throw new ArgumentNullException ( "buffer" );
			if ( buffer.IsReadOnly )
				throw new ArgumentException ( "Given buffer is ReadOnly!", "buffer" );

			if ( m_Transform.Count == 0 )
				return 0;
			int added = 0;
			for ( int i = 0, len = m_Transform.Count; i < len; i++ )
			{
				added += m_Transform[i].GameObject.GetAllComponentsInChildrenRecurse ( buffer, includeInactive );
			}
			return added;
		}

		public T[] GetAllComponentsInParent<T> ()
			where T : Component
		{
			List<T> comps = new List<T> ();
			GetAllComponentsInParent ( comps );
			return comps.Count == 0 ? Arrays<T>.Empty : comps.ToArray ();
		}

		public int GetAllComponentsInParent<T> ( IList<T> buffer )
			where T : Component
		{
			if ( buffer == null )
				throw new ArgumentNullException ( "buffer" );
			if ( buffer.IsReadOnly )
				throw new ArgumentException ( "Given buffer is ReadOnly!", "buffer" );

			int count = 0;
			var trans = m_Transform;
			while ( trans != null )
			{
				count += trans.GameObject.GetAllComponents<T> ( buffer );
				trans = trans.Parent;
			}
			return count;
		}


		public T AddComponent<T> ()
			where T : Component, new ()
		{
			if ( m_Components == null )
				m_Components = new List<Component> ();
			T comp = new T ();
			comp.Init ( this );
			m_Components.Add ( comp );

			//if ( component is Behavior )
			//{
			//	SceneManager.AddComponentBehavior ( (Behavior)component );
			//}
			return comp;
		}


		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			if ( m_Components != null )
			{
				for ( int i = m_Components.Count - 1; i >= 0; i-- )
				{
					m_Components[i].DestroyObject ();
				}
				m_Components.Clear ();
				m_Components = null;
			}
			if ( m_Transform != null )
			{
				var tmp = m_Transform;
				m_Transform = null;
				tmp.DestroyObject ();
			}
		}

		internal void RemoveComponent ( Component component )
		{
			m_Components.Remove ( component );

			//if ( component is Behavior )
			//{
			//	SceneManager.RemoveComponentBehavior ( (Behavior)component );
			//}
		}

		private T GetComponentInChildrenRecurse<T> ( bool includeInactive )
			where T : Component
		{
			T comp;
			if ( includeInactive || Active )
			{
				comp = GetComponent<T> ();
				if ( comp != null ) { return comp; }
			}
			for ( int i = 0, len = m_Transform.Count; i < len; i++ )
			{
				comp = m_Transform[i].GameObject.GetComponentInChildrenRecurse<T> ( includeInactive );
				if ( comp != null ) { return comp; }
			}
			return null;
		}

		private int GetAllComponentsInChildrenRecurse<T> ( IList<T> buffer, bool includeInactive )
			where T : Component
		{
			int added = 0;
			if ( includeInactive || Active )
			{
				added += GetAllComponents<T> ( buffer );
			}
			for ( int i = 0, len = m_Transform.Count; i < len; i++ )
			{
				added += m_Transform[i].GameObject.GetAllComponentsInChildrenRecurse ( buffer, includeInactive );
			}
			return added;
		}
	}
}