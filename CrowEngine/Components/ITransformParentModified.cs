﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowEngine.Components
{
	public interface ITransformParentModified
	{
		void OnTransformParentChanged ( Transform oldParent );
	}
}
