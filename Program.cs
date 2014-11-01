using System;

namespace CrowEngine
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
