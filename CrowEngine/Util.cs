using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine
{
	static class Util
	{
		public static void CheckError ()
		{
			ErrorCode ec = GL.GetError ();
			if ( ec != 0 )
			{
				throw new System.Exception ( ec.ToString () );
			}
		}
	}
}
