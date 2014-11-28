using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowStudio
{
	class Program
	{
		[STAThread]
		[LoaderOptimization ( LoaderOptimization.MultiDomain )]
		static void Main ( string[] args )
		{
			using ( var host = new EditorHost () )
			{
				host.Run ();
			}
		}
	}
}
