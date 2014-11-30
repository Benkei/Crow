using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrowEngine.Collections;
using CrowEngine.Components;
using CrowEngine.Pooling;

namespace CrowEngine
{
	class Scene
	{
		internal List<Transform> m_RootNodes = new List<Transform> ();
		internal Dictionary<Type, Vector<Component>> m_ComponentsPerType = new Dictionary<Type, Vector<Component>> ();


		public IEnumerable<T> IterateComponents<T> ( Type compType )
			where T : class
		{
			var handler = ThreadSingleton<
				ListPool<Vector<KeyValuePair<Type, Vector<Component>>>, KeyValuePair<Type, Vector<Component>>>
			>.Instance.GetHandler ();
			using ( handler )
			{
				var list = handler.Value;
				list.AddRange ( m_ComponentsPerType );
				for ( int i = list.Count - 1; i >= 0; i-- )
				{
					var pair = list[i];
					if ( pair.Key.IsAssignableFrom ( compType ) )
					{
						var comps = pair.Value;
						for ( int j = comps.Count - 1; j >= 0; j-- )
						{
							yield return comps[j] as T;
						}
					}
				}
			}
		}

		public void AddGameObject ( GameObject go )
		{
			foreach ( var comp in go.IterateAllComponentsInChildren ( true ) )
			{
				AddComponent ( comp );
			}
		}

		public void RemoveGameObject ( GameObject go )
		{
			foreach ( var comp in go.IterateAllComponentsInChildren ( true ) )
			{
				RemoveComponent ( comp );
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
