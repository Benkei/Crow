using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrowEditor.Jobs;
using CrowEditor.Serialization;
using CrowEngine;
using ImageMagick;
using OpenTK.Graphics.OpenGL4;

namespace CrowEditor.AssetProcessors
{
	class TextureProcessor : BaseProcessor, IJob
	{
		private string m_filePath;
		private Guid m_guid;

		//static TextureProcessor ()
		//{
		//	MagickNET.Log += ( a, b ) => { Console.WriteLine ( b.Message ); };
		//	MagickNET.SetLogEvents ( LogEvents.All );
		//}

		public override void Start ( string filePath, Guid guid )
		{
			m_filePath = filePath;
			m_guid = guid;

			CrowEditorApp.m_GLBackgroundThread.JobScheduler.AddJob ( this );
		}

		unsafe void IJob.Execute ()
		{
			var watch = System.Diagnostics.Stopwatch.StartNew ();
			var root = Path.Combine ( AssetDatabase.m_RootProjektFolder, m_filePath );
			using ( var img = new MagickImage ( root ) )
			{
				PixelInternalFormat internPixelFormat = PixelInternalFormat.CompressedRgbS3tcDxt1Ext;
				CrowSquish.ModusFlags compFlags = CrowSquish.ModusFlags.kDxt1;
				if ( img.HasAlpha )
				{
					internPixelFormat = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
					compFlags = CrowSquish.ModusFlags.kDxt5;
				}

				compFlags |= CrowSquish.ModusFlags.kColourIterativeClusterFit;

				img.Depth = 8;
				img.Format = MagickFormat.Rgba;

				var b = img.ToByteArray ( MagickFormat.Rgba );

				//var bitmap = img.ToBitmap ( System.Drawing.Imaging.ImageFormat.Bmp );
				//var lockdata = bitmap.LockBits (
				//	new System.Drawing.Rectangle ( 0, 0, img.Width, img.Height ),
				//	System.Drawing.Imaging.ImageLockMode.ReadOnly,
				//	System.Drawing.Imaging.PixelFormat.Format32bppArgb );

				int s = CrowSquish.Squish.GetStorageRequirements ( img.Width, img.Height, compFlags );
				IntPtr global = System.Runtime.InteropServices.Marshal.AllocHGlobal ( s );

				//CrowSquish.Squish.Compress ( lockdata.Scan0, global, CrowSquish.ModusFlags.kDxt5 );

				fixed ( byte* x = b )
				{
					CrowSquish.Squish.CompressImage ( (IntPtr)x, img.Width, img.Height, global, compFlags );
				}

				//bitmap.UnlockBits ( lockdata );
				//bitmap.Dispose ();


				var tex = new Texture2D ();
				tex.Bind ();

				tex.SetupCompressed ( img.Width, img.Height, 0, internPixelFormat, s, global );

				System.Runtime.InteropServices.Marshal.FreeHGlobal ( global );

				tex.GenerateMipmap ( HintMode.Nicest );

				int i;
				var length = tex.CountLevels;
				int size = 0;
				for ( i = 0; i < length; i++ )
				{
					size += tex.CompressedSize ( i );
				}

				TextureData metadata = new TextureData ();
				metadata.Width = tex.Width ( 0 );
				metadata.Height = tex.Height ( 0 );
				metadata.LevelCount = length;
				metadata.Format = internPixelFormat;
				metadata.DataSize = size;
				metadata.Data.Data = new byte[size];

				fixed ( byte* ptr2 = metadata.Data.Data )
				{
					byte* t = ptr2;
					for ( i = 0; i < length; i++ )
					{
						tex.GetCompressedData ( i, (IntPtr)t );
						t += tex.CompressedSize ( i );
					}
				}

				//tex.Delete ();

				var metadataFile = Path.Combine (
					Path.GetDirectoryName ( root ),
					Path.GetFileNameWithoutExtension ( root ) + ".metadata" );
				Factory.Save ( metadataFile, metadata );

				CrowEditorApp.m_GLRenderThread.List.Add ( tex );
			}
			Console.Write ( watch.Elapsed.TotalMilliseconds.ToString ( "0.0 ms " ) );
			Console.WriteLine ( root );
		}

		/*
		unsafe void IJob.Execute ()
		{
			var watch = System.Diagnostics.Stopwatch.StartNew ();
			var root = Path.Combine ( AssetDatabase.m_RootProjektFolder, m_filePath );
			using ( var img = new MagickImage ( root ) )
			{
				img.Format = MagickFormat.Dds;

				if ( img.HasAlpha )
					img.SetDefine ( MagickFormat.Dds, "compression", "dxt5" );
				else
					img.SetDefine ( MagickFormat.Dds, "compression", "dxt1" );

				img.SetDefine ( MagickFormat.Dds, "mipmaps", Util.CalculateMipmap ( img.Width, img.Height ).ToString () );

				var stream = new MemoryStream ();

				img.Write ( stream );

				//stream.Position = 0;

				//var xFile = Path.Combine (
				//	Path.GetDirectoryName ( root ),
				//	Path.GetFileNameWithoutExtension ( root ) + ".dds" );
				//StreamWriter f = new StreamWriter ( xFile, false );
				//stream.WriteTo ( f.BaseStream );
				//f.Dispose ();
				
				//var yFile = Path.Combine (
				//	Path.GetDirectoryName ( root ),
				//	Path.GetFileNameWithoutExtension ( root ) + ".blob" );
				//img.Format = MagickFormat.Rgba;
				//img.Write ( yFile );

				stream.Position = 0;

				var data = DirectDrawSurfaceLoader.Load ( stream );

				var desc = PixelformatConverter.PixelformatDesc ( data.Format );
				var pixelformat = (PixelInternalFormat)desc.Internal;


				var tex = new Texture2D ();
				tex.Bind ();

				int width = data.Width;
				int height = data.Height;
				fixed ( byte* ptr = data.Data )
				{
					byte* offset = ptr;
					for ( int level = 0; level < data.Levels; level++ )
					{
						int BlocksPerRow = (width + 3) >> 2;
						int BlocksPerColumn = (height + 3) >> 2;
						// DXTn stores Texels in 4x4 blocks, a Color block is 8 Bytes, an Alpha block is 8 Bytes for DXT3/5
						int SurfaceBlockCount = BlocksPerRow * BlocksPerColumn;
						int SurfaceSizeInBytes = SurfaceBlockCount * desc.BlockSize;

						tex.SetupCompressed (
							width, height, level,
							pixelformat, SurfaceSizeInBytes, (IntPtr)offset );
						//tex.SetCompressedData ( 
						//	0, 0, 
						//	width, height, level,
						//	(PixelFormat)pixelformat, SurfaceSizeInBytes, (IntPtr)offset );

						width /= 2;
						height /= 2;
						if ( width < 1 ) width = 1;
						if ( height < 1 ) height = 1;
						offset += SurfaceSizeInBytes;
					}
				}

				TextureData metadata = new TextureData ();
				metadata.Width = img.Width;
				metadata.Height = img.Height;
				metadata.LevelCount = data.Levels;
				metadata.Format = pixelformat;
				metadata.DataSize = data.Data.Length;
				metadata.Data.Data = data.Data;

				var metadataFile = Path.Combine (
					Path.GetDirectoryName ( root ),
					Path.GetFileNameWithoutExtension ( root ) + ".metadata" );

				Factory.Save ( metadataFile, metadata );

				CrowEditorApp.m_GLRenderThread.List.Add ( tex );
			}
			Console.Write ( watch.Elapsed.TotalMilliseconds.ToString ( "0.0 ms " ) );
			Console.WriteLine ( root );
		}
		*/
	}
}
