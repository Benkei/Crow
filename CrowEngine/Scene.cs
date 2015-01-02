using System;
using System.Collections.Generic;
using CrowEngine.Collections;
using CrowEngine.Components;
using CrowEngine.Pooling;

namespace CrowEngine
{
	using KeyPair = KeyValuePair<Type, Vector<Component>>;

	public class Scene
	{
		internal List<Transform> m_RootNodes = new List<Transform> ();
		internal Dictionary<Type, Vector<Component>> m_ComponentsPerType = new Dictionary<Type, Vector<Component>> ();


		public IEnumerable<Component> IterateComponents ( Type compType )
		{
			using ( var handler = ThreadSingleton<ListPool<Vector<KeyPair>, KeyPair>>.Instance.GetHandler () )
			{
				var list = handler.Value;
				list.AddRange ( m_ComponentsPerType );
				for ( int i = list.Count - 1; i >= 0; i-- )
				{
					var pair = list[i];
					if ( pair.Key.IsCastableTo ( compType ) )
					{
						var comps = pair.Value;
						for ( int j = comps.Count - 1; j >= 0; j-- )
						{
							yield return comps[j];
						}
					}
				}
			}
		}

		public void AddGameObject ( GameObject go )
		{
			if ( go == null )
				throw new ArgumentNullException ();

			go = go.Root;

			if ( go.Scene != this )
			{
				if ( go.Scene != null )
				{
					go.Scene.RemoveGameObject ( go );
				}

				go.Scene = this;
				foreach ( var comp in go.IterateAllComponents () )
				{
					AddComponent ( comp );
				}
				foreach ( var node in go.IterateChildren ( true, true ) )
				{
					node.Scene = this;
					foreach ( var comp in node.IterateAllComponents () )
					{
						AddComponent ( comp );
					}
				}
			}
		}

		internal void RemoveGameObject ( GameObject go )
		{
			if ( go == null )
				throw new ArgumentNullException ();
			if ( go.Scene != this )
				throw new ArithmeticException ();

			go = go.Root;

			if ( go.Scene == this )
			{
				go.Scene = null;
				foreach ( var comp in go.IterateAllComponents () )
				{
					RemoveComponent ( comp );
				}
				foreach ( var node in go.IterateChildren ( true, true ) )
				{
					node.Scene = null;
					foreach ( var comp in node.IterateAllComponents () )
					{
						RemoveComponent ( comp );
					}
				}
			}
		}
		
		internal void AddComponent ( Component comp )
		{
			var compType = comp.GetType ();
			Vector<Component> compList;
			if ( !m_ComponentsPerType.TryGetValue ( compType, out compList ) )
			{
				compList = new Vector<Component> ();
				m_ComponentsPerType.Add ( compType, compList );
			}
			compList.Add ( comp );
		}

		internal void RemoveComponent ( Component comp )
		{
			var compType = comp.GetType ();
			Vector<Component> compList;
			if ( m_ComponentsPerType.TryGetValue ( compType, out compList ) )
			{
				compList.Remove ( comp );
			}
		}

	}
}
