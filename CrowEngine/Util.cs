using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine
{
	public static class Util
	{
		public static int CalculateMipmap ( int width, int height )
		{
			var count = Math.Max ( width, height );
			if ( count > 0 )
			{
				count = 1 + (int)Math.Floor ( (float)Math.Log10 ( count ) / (float)Math.Log10 ( 2.0 ) );
			}
			return count;
		}

		public static void CheckGLError ()
		{
			ErrorCode ec = GL.GetError ();
			if ( ec != 0 )
			{
				throw new System.Exception ( ec.ToString () );
			}
		}

		public static void Swap<T> ( ref T a, ref T b )
		{
			T tmp = a;
			a = b;
			b = tmp;
		}
	}
}
