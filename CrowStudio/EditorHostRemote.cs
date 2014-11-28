using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowStudio
{
	class EditorHostRemote : MarshalByRefObject
	{
		public void TestHost ()
		{
			Console.WriteLine ( AppDomain.CurrentDomain.FriendlyName );
			Console.WriteLine ( "Hallo host" );
		}
	}
}
