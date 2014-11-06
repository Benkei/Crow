using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine
{
	class TextureFactory
	{
		public static Texture2D Load ( string filePath )
		{
			var bitmap = new Bitmap ( filePath );

			var bmp_data = bitmap.LockBits (
				new Rectangle ( 0, 0, bitmap.Width, bitmap.Height ),
				System.Drawing.Imaging.ImageLockMode.ReadOnly,
				System.Drawing.Imaging.PixelFormat.Format32bppArgb );

			Texture2D tex = new Texture2D ();
			tex.Bind ();
			tex.Setup (
				bmp_data.Width, bmp_data.Height, 0,
				PixelInternalFormat.CompressedRgbaS3tcDxt5Ext, PixelFormat.Rgba, PixelType.UnsignedByte, bmp_data.Scan0 );

			bitmap.UnlockBits ( bmp_data );

			tex.GenerateMipmap ( HintMode.Nicest );

			//tex.MagFilter = TextureMagFilter.Nearest;
			//tex.MinFilter = TextureMinFilter.Nearest;

			Util.CheckGLError ();

			return tex;
		}
	}
}
