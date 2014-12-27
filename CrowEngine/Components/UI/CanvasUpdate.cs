using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowEngine.Components.UI
{
	public enum CanvasUpdate : byte
	{
		PreLayout,
		Layout,
		PostLayout,
		PreRender,
		LatePreRender,
		//MaxUpdateValue
	}
}
