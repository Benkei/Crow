using System;
using System.Collections;
using System.Collections.Generic;
using CrowEngine.Components;
using CrowEngine.Pooling;
using CrowEngine.Reflection;

namespace CrowEngine
{
	public sealed class GameObject : Object, IDisposable
	{
		private List<Component> m_Components;
		private Transform m_Transform;

		public GameObject ()
		{
			Active = true;
			Layer = ~0;
		}

		public GameObject ( string name )
			: base ()
		{
			Name = name;
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
			get;
			set;
		}

		public bool HasComponents
		{
			get { return m_Components != null && m_Components.Count > 0; }
		}


		public T GetComponent<T> ()
			where T : class
		{
			if ( m_Components == null )
				return null;
			T comp;
			for ( int i = 0, len = m_Components.Count; i < len; i++ )
			{
				if ( (comp = m_Components[i] as T) != null )
					return comp;
			}
			return null;
		}

		public Component GetComponent ( Type type )
		{
			if ( type == null )
				throw new ArgumentException ();
			if ( !type.IsInterface && typeof ( Component ).IsAssignableFrom ( type ) )
				throw new ArgumentException ();
			if ( m_Components == null )
				return null;
			Component comp;
			for ( int i = m_Components.Count - 1; i >= 0; i-- )
			{
				comp = m_Components[i];
				if ( type.IsAssignableFrom ( comp.GetType () ) )
					return comp;
			}
			return null;
		}

		public T GetComponentInParent<T> ()
			where T : class
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

		public Component GetComponentInParent ( Type type )
		{
			if ( type == null )
				throw new ArgumentException ();
			if ( !type.IsInterface && typeof ( Component ).IsAssignableFrom ( type ) )
				throw new ArgumentException ();
			var trans = m_Transform;
			while ( trans != null )
			{
				var comp = trans.GameObject.GetComponent ( type );
				if ( comp != null )
					return comp;
				trans = trans.Parent;
			}
			return null;
		}

		public T GetComponentInChildren<T> ( bool includeInactive = false )
			where T : class
		{
			T comp;
			for ( int i = 0, len = m_Transform.Count; i < len; i++ )
			{
				comp = m_Transform[i].GameObject.GetComponentInChildrenRecurse<T> ( includeInactive );
				if ( comp != null ) return comp;
			}
			return null;
		}

		public Component GetComponentInChildren ( Type type, bool includeInactive = false )
		{
			if ( type == null )
				throw new ArgumentException ();
			if ( !type.IsInterface && typeof ( Component ).IsAssignableFrom ( type ) )
				throw new ArgumentException ();
			Component comp;
			for ( int i = 0, len = m_Transform.Count; i < len; i++ )
			{
				comp = m_Transform[i].GameObject.GetComponentInChildrenRecurse ( type, includeInactive );
				if ( comp != null ) return comp;
			}
			return null;
		}


		public T[] GetAllComponents<T> ()
			where T : class
		{
			if ( m_Components == null || m_Components.Count == 0 )
				return Arrays<T>.Empty;
			List<T> comps = new List<T> ();
			GetAllComponents ( comps );
			return comps.ToArray ();
		}

		public Component[] GetAllComponents ( Type type )
		{
			if ( type == null )
				throw new ArgumentException ();
			if ( !type.IsInterface && typeof ( Component ).IsAssignableFrom ( type ) )
				throw new ArgumentException ();
			if ( m_Components == null || m_Components.Count == 0 )
				return Arrays<Component>.Empty;
			var comps = new List<Component> ();
			GetAllComponents ( type, comps );
			return comps.ToArray ();
		}

		public T[] GetAllComponentsInParent<T> ()
			where T : class
		{
			List<T> comps = new List<T> ();
			GetAllComponentsInParent ( comps );
			return comps.Count == 0 ? Arrays<T>.Empty : comps.ToArray ();
		}

		public Component[] GetAllComponentsInParent ( Type type )
		{
			if ( type == null )
				throw new ArgumentException ();
			if ( !type.IsInterface && typeof ( Component ).IsAssignableFrom ( type ) )
				throw new ArgumentException ();
			var comps = new List<Component> ();
			GetAllComponentsInParent ( comps );
			return comps.Count == 0 ? Arrays<Component>.Empty : comps.ToArray ();
		}

		public T[] GetAllComponentsInChildren<T> ( bool includeInactive = false )
			where T : class
		{
			if ( m_Transform.Count == 0 )
				return Arrays<T>.Empty;
			List<T> comps = new List<T> ();
			GetAllComponentsInChildren ( comps, includeInactive );
			return comps.Count == 0 ? Arrays<T>.Empty : comps.ToArray ();
		}

		public Component[] GetAllComponentsInChildren ( Type type, bool includeInactive = false )
		{
			if ( type == null )
				throw new ArgumentException ();
			if ( !type.IsInterface && typeof ( Component ).IsAssignableFrom ( type ) )
				throw new ArgumentException ();
			if ( m_Transform.Count == 0 )
				return Arrays<Component>.Empty;
			var comps = new List<Component> ();
			GetAllComponentsInChildren ( comps, includeInactive );
			return comps.Count == 0 ? Arrays<Component>.Empty : comps.ToArray ();
		}


		public int GetAllComponents<T> ( IList<T> buffer )
			where T : class
		{
			if ( buffer == null )
				throw new ArgumentNullException ( "buffer" );
			if ( buffer.IsReadOnly )
				throw new ArgumentException ( "Given buffer is ReadOnly!", "buffer" );

			if ( m_Components == null || m_Components.Count == 0 )
				return 0;
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

		public int GetAllComponents ( Type type, IList<Component> buffer )
		{
			if ( type == null )
				throw new ArgumentNullException ();
			if ( !type.IsInterface && typeof ( Component ).IsAssignableFrom ( type ) )
				throw new ArgumentException ();
			if ( buffer == null )
				throw new ArgumentNullException ( "buffer" );
			if ( buffer.IsReadOnly )
				throw new ArgumentException ( "Given buffer is ReadOnly!", "buffer" );

			if ( m_Components == null || m_Components.Count == 0 )
				return 0;
			Component comp;
			int added = 0;
			for ( int i = m_Components.Count - 1; i >= 0; i-- )
			{
				comp = m_Components[i];
				if ( type.IsAssignableFrom ( comp.GetType () ) )
				{
					buffer.Add ( comp );
					added++;
				}
			}
			return added;
		}

		public int GetAllComponentsInParent<T> ( IList<T> buffer )
			where T : class
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

		public int GetAllComponentsInParent ( Type type, IList<Component> buffer )
		{
			if ( type == null )
				throw new ArgumentNullException ();
			if ( !type.IsInterface && typeof ( Component ).IsAssignableFrom ( type ) )
				throw new ArgumentException ();
			if ( buffer == null )
				throw new ArgumentNullException ( "buffer" );
			if ( buffer.IsReadOnly )
				throw new ArgumentException ( "Given buffer is ReadOnly!", "buffer" );

			int count = 0;
			var trans = m_Transform;
			while ( trans != null )
			{
				count += trans.GameObject.GetAllComponents ( type, buffer );
				trans = trans.Parent;
			}
			return count;
		}

		public int GetAllComponentsInChildren<T> ( IList<T> buffer, bool includeInactive = false )
			where T : class
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

		public int GetAllComponentsInChildren<T> ( Type type, IList<Component> buffer, bool includeInactive = false )
		{
			if ( type == null )
				throw new ArgumentNullException ();
			if ( !type.IsInterface && typeof ( Component ).IsAssignableFrom ( type ) )
				throw new ArgumentException ();
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


		public T AddComponent<T> ()
			where T : Component, new ()
		{
			return (T)AddComponent ( typeof ( T ) );
		}

		public Component AddComponent ( Type type )
		{
			if ( type == null )
				throw new ArgumentNullException ();
			if ( typeof ( Component ).IsAssignableFrom ( type ) )
				throw new ArgumentException ();
			if ( m_Components == null )
				m_Components = new List<Component> ();

			Component comp = (Component)InstanceFactory.Construct ( type );
			comp._Setup ( this );
			m_Components.Add ( comp );

			if ( comp is Transform )
			{
				m_Transform = comp as Transform;
			}

			//if ( component is Behavior )
			//{
			//	SceneManager.AddComponentBehavior ( (Behavior)component );
			//}
			return comp;
		}


		public Enumerable IterateAllComponents ()
		{
			return new Enumerable ( m_Components );
		}

		public ParentEnumerable IterateAllComponentsInParent ()
		{
			return new ParentEnumerable ( this );
		}

		public ChildrenEnumerable IterateAllComponentsInChildren ( bool includeInactive = false )
		{
			return new ChildrenEnumerable ( this, includeInactive );
		}


		public void Dispose ()
		{
			if ( m_Components != null )
			{
				for ( int i = m_Components.Count - 1; i >= 0; i-- )
				{
					m_Components[i]._Destroy ();
				}
				m_Components.Clear ();
				m_Components = null;
			}
			if ( m_Transform != null )
			{
				var tmp = m_Transform;
				m_Transform = null;
				tmp._Destroy ();
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
			where T : class
		{
			T comp;
			if ( includeInactive || Active )
			{
				comp = GetComponent<T> ();
				if ( comp != null )
					return comp;
			}
			for ( int i = 0, len = m_Transform.Count; i < len; i++ )
			{
				comp = m_Transform[i].GameObject.GetComponentInChildrenRecurse<T> ( includeInactive );
				if ( comp != null )
					return comp;
			}
			return null;
		}

		private Component GetComponentInChildrenRecurse ( Type type, bool includeInactive )
		{
			Component comp;
			if ( includeInactive || Active )
			{
				comp = GetComponent ( type );
				if ( comp != null )
					return comp;
			}
			for ( int i = 0, len = m_Transform.Count; i < len; i++ )
			{
				comp = m_Transform[i].GameObject.GetComponentInChildrenRecurse ( type, includeInactive );
				if ( comp != null )
					return comp;
			}
			return null;
		}

		private int GetAllComponentsInChildrenRecurse<T> ( IList<T> buffer, bool includeInactive )
			where T : class
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

		private int GetAllComponentsInChildrenRecurse ( Type type, IList<Component> buffer, bool includeInactive )
		{
			int added = 0;
			if ( includeInactive || Active )
			{
				added += GetAllComponents ( type, buffer );
			}
			for ( int i = 0, len = m_Transform.Count; i < len; i++ )
			{
				added += m_Transform[i].GameObject.GetAllComponentsInChildrenRecurse ( type, buffer, includeInactive );
			}
			return added;
		}

		#region Iterators

		public struct Enumerable : IEnumerable<Component>
		{
			private List<Component> m_Root;

			internal Enumerable ( List<Component> root )
			{
				m_Root = root;
			}

			public Enumerator GetEnumerator ()
			{
				return new Enumerator ( m_Root );
			}

			IEnumerator<Component> IEnumerable<Component>.GetEnumerator ()
			{
				return new Enumerator ( m_Root );
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return new Enumerator ( m_Root );
			}
		}

		public struct Enumerator : IEnumerator<Component>
		{
			private int m_Index;
			private List<Component> m_List;
			private Component m_Current;

			public Component Current { get { return m_Current; } }

			object IEnumerator.Current { get { return m_Current; } }

			internal Enumerator ( List<Component> list )
			{
				m_List = list;
				m_Index = 0;
				m_Current = null;
			}

			public bool MoveNext ()
			{
				if ( m_List != null && m_Index < m_List.Count )
				{
					m_Current = m_List[m_Index];
					m_Index++;
					return true;
				}
				m_Current = null;
				return false;
			}

			public void Reset ()
			{
				m_Index = 0;
				m_Current = null;
			}

			public void Dispose ()
			{
				m_List = null;
				Reset ();
			}
		}

		public struct ChildrenEnumerable : IEnumerable<Component>
		{
			private GameObject m_Root;
			private bool m_IncludeInactive;

			internal ChildrenEnumerable ( GameObject root, bool includeInactive )
			{
				m_Root = root;
				m_IncludeInactive = includeInactive;
			}

			public ChildrenEnumerator GetEnumerator ()
			{
				return new ChildrenEnumerator ( m_Root, m_IncludeInactive );
			}

			IEnumerator<Component> IEnumerable<Component>.GetEnumerator ()
			{
				return new ChildrenEnumerator ( m_Root, m_IncludeInactive );
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return new ChildrenEnumerator ( m_Root, m_IncludeInactive );
			}
		}

		public struct ChildrenEnumerator : IEnumerator<Component>
		{
			private Transform.DeepEnumerator m_TreeIterator;
			private Enumerator m_CompIterator;
			private GameObject m_Root;
			private Component m_Current;
			private bool m_IncludeInactive;

			public Component Current { get { return m_Current; } }

			object IEnumerator.Current { get { return m_Current; } }

			internal ChildrenEnumerator ( GameObject root, bool includeInactive )
			{
				if ( root.Transform != null )
					m_TreeIterator = root.Transform.GetDeepEnumerable ().GetEnumerator ();
				else
					m_TreeIterator = new Transform.DeepEnumerator ();
				if ( root.HasComponents )
					m_CompIterator = root.IterateAllComponents ().GetEnumerator ();
				else
					m_CompIterator = new Enumerator ();
				m_Root = root;
				m_Current = null;
				m_IncludeInactive = includeInactive;
			}

			public bool MoveNext ()
			{
			RESTART:
				if ( m_CompIterator.MoveNext () )
				{
					m_Current = m_CompIterator.Current;
					return true;
				}
				m_CompIterator.Dispose ();

				while ( m_TreeIterator.MoveNext () )
				{
					var node = m_TreeIterator.Current;
					if ( node.GameObject.HasComponents )
					{
						m_CompIterator = node.GameObject.IterateAllComponents ().GetEnumerator ();
						goto RESTART;
					}
				}
				m_Current = null;
				return false;
			}

			public void Reset ()
			{
				m_TreeIterator.Dispose ();
				m_CompIterator.Dispose ();
				this = new ChildrenEnumerator ( m_Root, m_IncludeInactive );
			}

			public void Dispose ()
			{
				m_TreeIterator.Dispose ();
				m_CompIterator.Dispose ();
				this = new ChildrenEnumerator ();
			}
		}

		public struct ParentEnumerable : IEnumerable<Component>
		{
			private GameObject m_Root;

			internal ParentEnumerable ( GameObject root )
			{
				m_Root = root;
			}

			public ParentEnumerator GetEnumerator ()
			{
				return new ParentEnumerator ( m_Root );
			}

			IEnumerator<Component> IEnumerable<Component>.GetEnumerator ()
			{
				return new ParentEnumerator ( m_Root );
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return new ParentEnumerator ( m_Root );
			}
		}

		public struct ParentEnumerator : IEnumerator<Component>
		{
			private Enumerator m_CompIterator;
			private GameObject m_Root;
			private GameObject m_Child;
			private Component m_Current;

			public Component Current { get { return m_Current; } }

			object IEnumerator.Current { get { return m_Current; } }

			internal ParentEnumerator ( GameObject root )
			{
				if ( root.HasComponents )
					m_CompIterator = root.IterateAllComponents ().GetEnumerator ();
				else
					m_CompIterator = new Enumerator ();
				m_Root = root;
				m_Child = root;
				m_Current = null;
			}

			public bool MoveNext ()
			{
			RESTART:
				if ( m_CompIterator.MoveNext () )
				{
					m_Current = m_CompIterator.Current;
					return true;
				}
				m_CompIterator.Dispose ();

				while ( m_Child.Transform.Parent != null )
				{
					m_Child = m_Child.Transform.Parent.GameObject;
					if ( m_Child.HasComponents )
					{
						m_CompIterator = m_Child.IterateAllComponents ().GetEnumerator ();
						goto RESTART;
					}
				}

				m_Current = null;
				return false;
			}

			public void Reset ()
			{
				m_CompIterator.Dispose ();
				this = new ParentEnumerator ( m_Root );
			}

			public void Dispose ()
			{
				m_CompIterator.Dispose ();
				this = new ParentEnumerator ();
			}
		}

		#endregion
	}
}