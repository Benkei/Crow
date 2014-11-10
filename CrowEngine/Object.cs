using System;

namespace CrowEngine
{
	[Flags]
	public enum HideFlags
	{
		HideInHierarchy = 1 >> 0,
		HideInInspector = 1 >> 1,
		DontSave = 1 >> 2,
		NotEditable = 1 >> 3,

		HideAndDontSave = HideInHierarchy | DontSave | NotEditable,
	}

	public abstract class Object
	{
		public abstract string Name { get; set; }
		public bool IsDestroyed { get; private set; }
		public HideFlags HideFlags { get; set; }
		

		public override string ToString ()
		{
			return string.Format ( "Name: {0}", Name );
		}


		protected virtual void OnDestroy () { }


		protected internal void CheckDestroyed ()
		{
			if ( IsDestroyed )
				throw new ObjectDisposedException ( "Object already destoryed! " + ToString () );
		}

		internal virtual void DestroyObject ()
		{
			if ( !IsDestroyed )
			{
				IsDestroyed = true;
				OnDestroy ();
			}
		}
	}
}
