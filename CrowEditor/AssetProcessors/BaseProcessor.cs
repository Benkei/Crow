using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowEditor.AssetProcessors
{
	public abstract class BaseProcessor
	{
		public abstract void Start ( string filePath, Guid guid );
	}
}
