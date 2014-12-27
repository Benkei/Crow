using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowEngine
{
	enum GameObjectState
	{
		/// <summary>
		/// The object is currently initializing.
		/// </summary>
		Initializing,
		/// <summary>
		/// The object has been fully initialized and is fully operational.
		/// </summary>
		Initialized,
		/// <summary>
		/// The object is currently disposing.
		/// </summary>
		Disposing,
		/// <summary>
		/// The object has been fully disposed and can be considered "dead".
		/// </summary>
		Disposed
	}
}
