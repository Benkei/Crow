using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowEngine
{
	public abstract class GLObject
	{
		protected int m_Handler;

		public int Handler
		{
			get { return m_Handler; }
			set { m_Handler = value; }
		}
	}
}
