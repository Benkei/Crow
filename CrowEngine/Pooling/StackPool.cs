using System;
using System.Collections.Generic;

namespace CrowEngine.Pooling
{
	internal class StackPool<T> : BaseObjectPool<Stack<T>>
	{
		public readonly int InitCapacity;

		public StackPool ( int initCapacity = 0 )
		{
			InitCapacity = initCapacity;
		}

		public StackPool ()
		{ }

		protected override Stack<T> OnCreate ()
		{
			return new Stack<T> ( InitCapacity );
		}

		protected override void OnRecycle ( ref Stack<T> obj )
		{
			if ( obj.Count > 0 )
				obj.Clear ();
		}
	}
}
