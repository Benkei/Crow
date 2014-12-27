using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowEngine.Components.UI
{
	public interface ICanvasElement
	{
		void Rebuild ( CanvasUpdate executing );
	}
}
