using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine
{
	public class TextureFactory
	{
		public static Texture2D Load ( string filePath )
		{
			var bitmap = new Bitmap ( filePath );

			var bmp_data = bitmap.LockBits (
				new Rectangle ( 0, 0, bitmap.Width, bitmap.Height ),
				System.Drawing.Imaging.ImageLockMode.ReadOnly,
				System.Drawing.Imaging.PixelFormat.Format24bppRgb );

			Texture2D tex = new Texture2D ();
			tex.Bind ();
			tex.SetData (
				0, bmp_data.Width, bmp_data.Height,
				PixelInternalFormat.CompressedRgbS3tcDxt1Ext, PixelFormat.Rgb, PixelType.UnsignedByte, bmp_data.Scan0 );

			bitmap.UnlockBits ( bmp_data );

			tex.GenerateMipmap ( HintMode.Nicest );

			//tex.MagFilter = TextureMagFilter.Nearest;
			//tex.MinFilter = TextureMinFilter.Nearest;

			Util.CheckGLError ();

			return tex;
		}
		public static Texture2D Load2 ( string filePath )
		{
			var bitmap = new Bitmap ( filePath );

			var bmp_data = bitmap.LockBits (
				new Rectangle ( 0, 0, bitmap.Width, bitmap.Height ),
				System.Drawing.Imaging.ImageLockMode.ReadOnly,
				System.Drawing.Imaging.PixelFormat.Format24bppRgb );

			//int s = CrowSquish.Squish.GetStorageRequirements ( bitmap.Width, bitmap.Height, CrowSquish.ModusFlags.kDxt1 );
			//IntPtr global = System.Runtime.InteropServices.Marshal.AllocHGlobal ( s );

			//CrowSquish.Squish.Compress ( lockdata.Scan0, global, CrowSquish.ModusFlags.kDxt5 );



			Texture2D tex = new Texture2D ();
			tex.Bind ();
			tex.SetData (
				0, bmp_data.Width, bmp_data.Height,
				PixelInternalFormat.CompressedRgbS3tcDxt1Ext, PixelFormat.Rgb, PixelType.UnsignedByte, bmp_data.Scan0 );

			bitmap.UnlockBits ( bmp_data );

			tex.GenerateMipmap ( HintMode.Nicest );

			//tex.MagFilter = TextureMagFilter.Nearest;
			//tex.MinFilter = TextureMinFilter.Nearest;

			Util.CheckGLError ();

			return tex;
		}
	}
}
