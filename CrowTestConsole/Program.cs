using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrowEngine;

namespace CrowTestConsole
{
	class Program
	{
		[STAThread]
		static void Main ( string[] args )
		{
			using ( var game = new Tutorial () )
			{
				game.Run ( 30, 30 );
			}
		}
	}
}
