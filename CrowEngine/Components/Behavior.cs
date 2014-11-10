using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrowEngine.Components
{
	public abstract class Behavior : Component
	{
		bool m_Enabled = true;

		public bool Enabled
		{
			get { return m_Enabled; }
			set
			{
				if ( m_Enabled != value )
				{
					m_Enabled = value;
					OnChangeEnabled ();
				}
			}
		}
		
		protected virtual void OnChangeEnabled () { }
	}
}
