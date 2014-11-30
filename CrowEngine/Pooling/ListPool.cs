using System.Collections.Generic;
using CrowEngine.Reflection;

namespace CrowEngine.Pooling
{
	internal class ListPool<T, Tl> : BaseObjectPool<T>
		where T : IList<Tl>, new ()
	{
		public ListPool () { }

		protected override T OnCreate ()
		{
			return InstanceFactory<T>.Instance ();
		}

		protected override void OnRecycle ( ref T obj )
		{
			if ( obj.Count > 0 )
				obj.Clear ();
		}
	}
}
