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
		public HideFlags HideFlags { get; set; }

		public bool IsDestroyed { get; internal set; }

		public abstract string Name { get; set; }

		public override string ToString ()
		{
			return string.Format ( GetType () + "; Name: {0};", Name );
		}

		protected internal void CheckDestroyed ()
		{
			if ( IsDestroyed )
				throw new ObjectDisposedException ( "Object already destoryed! " + ToString () );
		}
	}
}