using System;

namespace CrowEngine.Components
{
	public abstract class Behavior : Component
	{
		private bool m_Enabled = true;

		public bool Enabled
		{
			get { return m_Enabled; }
			set
			{
				if ( m_Enabled != value )
				{
					m_Enabled = value;
					if ( this is IActivatable )
					{
						try
						{
							if ( value )
								((IActivatable)this).OnEnable ();
							else
								((IActivatable)this).OnDisable ();
						}
						catch ( Exception ) { }
					}
				}
			}
		}
	}
}