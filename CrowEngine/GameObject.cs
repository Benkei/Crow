using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CrowEngine.Collections;
using CrowEngine.Components;
using CrowEngine.Pooling;
using CrowEngine.Reflection;

namespace CrowEngine
{
	public sealed class GameObject : Object, IDisposable
	{
		private List<Component> m_Components;
		private Transform m_Transform;
		private Scene m_Scene;

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
				if ( m_Transform == null )
					return Active;
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

		public GameObject Parent
		{
			get
			{
				return m_Transform != null && m_Transform.Parent != null
					? m_Transform.Parent.GameObject
					: this;
			}
		}

		public GameObject Root
		{
			get { return m_Transform != null ? m_Transform.Root.GameObject : this; }
		}

		public Transform Transform
		{
			get { CheckDestroyed (); return m_Transform; }
		}

		public Scene Scene
		{
			get { return m_Scene; }
			internal set { m_Scene = value; }
		}

		public override string Name
		{
			get;
			set;
		}

		public string FullName
		{
			get
			{
				var sb = ThreadSingleton<StringBuilder>.Instance;
				try
				{
					Transform current = m_Transform;
					while ( current != null )
					{
						sb.Insert ( 0, '/' );
						sb.Insert ( 1, current.Name );
						current = current.Parent;
					}
					return sb.ToString ( 1, sb.Length - 1 );
				}
				finally
				{
					sb.Clear ();
					sb.Capacity = 256;
				}
			}
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
			if ( !type.IsInterface && type.IsAssignableFrom ( typeof ( Component ) ) )
				throw new ArgumentException ();
			if ( m_Components == null )
				return null;
			Component comp;
			for ( int i = m_Components.Count - 1; i >= 0; i-- )
			{
				comp = m_Components[i];
				if ( type.IsInstanceOfType ( comp ) )
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
			if ( !type.IsInterface && type.IsAssignableFrom ( typeof ( Component ) ) )
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
			if ( !type.IsInterface && type.IsAssignableFrom ( typeof ( Component ) ) )
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
			if ( !type.IsInterface && type.IsAssignableFrom ( typeof ( Component ) ) )
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
			if ( !type.IsInterface && type.IsAssignableFrom ( typeof ( Component ) ) )
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
			if ( !type.IsInterface && type.IsAssignableFrom ( typeof ( Component ) ) )
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
			if ( !type.IsInterface && type.IsAssignableFrom ( typeof ( Component ) ) )
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
				if ( type.IsInstanceOfType ( comp ) )
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
			if ( !type.IsInterface && type.IsAssignableFrom ( typeof ( Component ) ) )
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
			if ( !type.IsInterface && type.IsAssignableFrom ( typeof ( Component ) ) )
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
			if ( type.IsAssignableFrom ( typeof ( Component ) ) )
				throw new ArgumentException ();
			if ( m_Components == null )
				m_Components = new List<Component> ();

			Component comp = (Component)InstanceFactory.Construct ( type );
			comp._Setup ( this );

			if ( comp is Transform )
			{
				m_Transform = comp as Transform;
			}

			m_Components.Add ( comp );
			if ( m_Scene != null )
			{
				m_Scene.AddComponent ( comp );
			}
			return comp;
		}


		public ComponentEnumerable IterateAllComponents ()
		{
			return new ComponentEnumerable ( m_Components );
		}

		public ParentComponentEnumerable IterateAllComponentsInParent ()
		{
			return new ParentComponentEnumerable ( this );
		}

		public ChildrenComponentEnumerable IterateAllComponentsInChildren ( bool includeInactive = false )
		{
			return new ChildrenComponentEnumerable ( this, includeInactive );
		}

		public ChildrenEnumerable IterateChildren ( bool deep, bool includeInactive = false )
		{
			return new ChildrenEnumerable ( this, deep, includeInactive );
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

		public struct ComponentEnumerable : IEnumerable<Component>
		{
			private List<Component> m_Root;

			internal ComponentEnumerable ( List<Component> root )
			{
				m_Root = root;
			}

			public ComponentEnumerator GetEnumerator ()
			{
				return new ComponentEnumerator ( m_Root );
			}

			IEnumerator<Component> IEnumerable<Component>.GetEnumerator ()
			{
				return new ComponentEnumerator ( m_Root );
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return new ComponentEnumerator ( m_Root );
			}
		}

		public struct ComponentEnumerator : IEnumerator<Component>
		{
			private int m_Index;
			private List<Component> m_List;
			private Component m_Current;

			public Component Current { get { return m_Current; } }

			object IEnumerator.Current { get { return m_Current; } }

			internal ComponentEnumerator ( List<Component> list )
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

		public struct ChildrenComponentEnumerable : IEnumerable<Component>
		{
			private GameObject m_Root;
			private bool m_IncludeInactive;

			internal ChildrenComponentEnumerable ( GameObject root, bool includeInactive )
			{
				m_Root = root;
				m_IncludeInactive = includeInactive;
			}

			public ChildrenComponentEnumerator GetEnumerator ()
			{
				return new ChildrenComponentEnumerator ( m_Root, m_IncludeInactive );
			}

			IEnumerator<Component> IEnumerable<Component>.GetEnumerator ()
			{
				return new ChildrenComponentEnumerator ( m_Root, m_IncludeInactive );
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return new ChildrenComponentEnumerator ( m_Root, m_IncludeInactive );
			}
		}

		public struct ChildrenComponentEnumerator : IEnumerator<Component>
		{
			private Transform.DeepEnumerator m_TreeIterator;
			private ComponentEnumerator m_CompIterator;
			private GameObject m_Root;
			private Component m_Current;
			private bool m_IncludeInactive;

			public Component Current { get { return m_Current; } }

			object IEnumerator.Current { get { return m_Current; } }

			internal ChildrenComponentEnumerator ( GameObject root, bool includeInactive )
			{
				if ( root.Transform != null )
					m_TreeIterator = root.Transform.GetDeepEnumerable ().GetEnumerator ();
				else
					m_TreeIterator = new Transform.DeepEnumerator ();
				if ( root.HasComponents )
					m_CompIterator = root.IterateAllComponents ().GetEnumerator ();
				else
					m_CompIterator = new ComponentEnumerator ();
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
				this = new ChildrenComponentEnumerator ( m_Root, m_IncludeInactive );
			}

			public void Dispose ()
			{
				m_TreeIterator.Dispose ();
				m_CompIterator.Dispose ();
				this = new ChildrenComponentEnumerator ();
			}
		}

		public struct ParentComponentEnumerable : IEnumerable<Component>
		{
			private GameObject m_Root;

			internal ParentComponentEnumerable ( GameObject root )
			{
				m_Root = root;
			}

			public ParentComponentEnumerator GetEnumerator ()
			{
				return new ParentComponentEnumerator ( m_Root );
			}

			IEnumerator<Component> IEnumerable<Component>.GetEnumerator ()
			{
				return new ParentComponentEnumerator ( m_Root );
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return new ParentComponentEnumerator ( m_Root );
			}
		}

		public struct ParentComponentEnumerator : IEnumerator<Component>
		{
			private ComponentEnumerator m_CompIterator;
			private GameObject m_Root;
			private GameObject m_Child;
			private Component m_Current;

			public Component Current { get { return m_Current; } }

			object IEnumerator.Current { get { return m_Current; } }

			internal ParentComponentEnumerator ( GameObject root )
			{
				if ( root.HasComponents )
					m_CompIterator = root.IterateAllComponents ().GetEnumerator ();
				else
					m_CompIterator = new ComponentEnumerator ();
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
				this = new ParentComponentEnumerator ( m_Root );
			}

			public void Dispose ()
			{
				m_CompIterator.Dispose ();
				this = new ParentComponentEnumerator ();
			}
		}

		public struct ChildrenEnumerable : IEnumerable<GameObject>
		{
			private GameObject m_Root;
			private bool m_Deep;
			private bool m_IncludeInactive;

			internal ChildrenEnumerable ( GameObject root, bool deep, bool includeInactive )
			{
				m_Root = root;
				m_Deep = deep;
				m_IncludeInactive = includeInactive;
			}

			public ChildrenEnumerator GetEnumerator ()
			{
				return new ChildrenEnumerator ( m_Root, m_Deep, m_IncludeInactive );
			}

			IEnumerator<GameObject> IEnumerable<GameObject>.GetEnumerator ()
			{
				return new ChildrenEnumerator ( m_Root, m_Deep, m_IncludeInactive );
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return new ChildrenEnumerator ( m_Root, m_Deep, m_IncludeInactive );
			}
		}

		public struct ChildrenEnumerator : IEnumerator<GameObject>
		{
			private Transform.Enumerator m_TreeIterator;
			private Transform.DeepEnumerator m_DeepTreeIterator;
			private GameObject m_Root;
			private GameObject m_Current;
			private bool m_Deep;
			private bool m_IncludeInactive;

			public GameObject Current { get { return m_Current; } }

			object IEnumerator.Current { get { return m_Current; } }

			internal ChildrenEnumerator ( GameObject root, bool deep, bool includeInactive )
			{
				m_TreeIterator = new Transform.Enumerator ();
				m_DeepTreeIterator = new Transform.DeepEnumerator ();
				if ( root.Transform != null )
				{
					if ( deep )
						m_TreeIterator = root.Transform.GetEnumerator ();
					else
						m_DeepTreeIterator = root.Transform.GetDeepEnumerable ().GetEnumerator ();
				}
				m_Root = root;
				m_Current = null;
				m_Deep = deep;
				m_IncludeInactive = includeInactive;
			}

			public bool MoveNext ()
			{
				if ( m_Deep && m_DeepTreeIterator.MoveNext () )
				{
					m_Current = m_DeepTreeIterator.Current.GameObject;
					return true;
				}
				else if ( m_TreeIterator.MoveNext () )
				{
					m_Current = m_TreeIterator.Current.GameObject;
					return true;
				}
				m_Current = null;
				return false;
			}

			public void Reset ()
			{
				m_TreeIterator.Dispose ();
				m_DeepTreeIterator.Dispose ();
				this = new ChildrenEnumerator ( m_Root, m_Deep, m_IncludeInactive );
			}

			public void Dispose ()
			{
				m_TreeIterator.Dispose ();
				m_DeepTreeIterator.Dispose ();
				this = new ChildrenEnumerator ();
			}
		}

		#endregion
	}
}