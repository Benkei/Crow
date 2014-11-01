using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

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
