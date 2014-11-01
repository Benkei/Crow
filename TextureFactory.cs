using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowEngine
{
	class TextureFactory
	{
		public static Texture2D Load ( string filePath )
		{
			var bitmap = new Bitmap ( filePath );

			var bmp_data = bitmap.LockBits (
				new Rectangle ( 0, 0, bitmap.Width, bitmap.Height ),
				ImageLockMode.ReadOnly,
				PixelFormat.Format32bppArgb );

			Texture2D tex = new Texture2D ( bmp_data.Width, bmp_data.Height, bmp_data.Scan0 );

			bitmap.UnlockBits ( bmp_data );

			return tex;
		}
	}
}
