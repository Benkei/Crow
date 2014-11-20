using System;
using System.IO;
using CrowEditor.Jobs;
using CrowEditor.Serialization;
using CrowEngine;
using ImageMagick;
using OpenTK.Graphics.OpenGL4;

namespace CrowEditor.AssetProcessors
{
	internal class TextureProcessor : BaseProcessor, IJob
	{
		private string m_filePath;
		private Guid m_guid;

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

				MemoryStream imgStream = new MemoryStream ( img.Width * img.Height * 4 );

				img.Write ( imgStream );

				int cSize = CrowSquish.Squish.GetStorageRequirements ( img.Width, img.Height, compFlags );

				// 8(DXT1) or 16(DXT2-5)
				int totalSize = Math.Max ( 1, ((img.Width + 3) / 4) ) * Math.Max ( 1, ((img.Height + 3) / 4) );
				totalSize *= (compFlags & CrowSquish.ModusFlags.kDxt1) > 0 ? 8 : 16;

				MemoryStream bcStream = new MemoryStream ( totalSize );

				fixed ( byte* imgPtr = imgStream.GetBuffer () )
				fixed ( byte* bcPtr = bcStream.GetBuffer () )
				{
					CrowSquish.Squish.CompressImage ( (IntPtr)imgPtr, img.Width, img.Height, (IntPtr)bcPtr, compFlags );
				}

				var tex = new Texture2D ();
				tex.Bind ();
				fixed ( byte* bcPtr = bcStream.GetBuffer () )
				{
					tex.SetupCompressed ( img.Width, img.Height, 0, internPixelFormat, cSize, (IntPtr)bcPtr );
				}
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
				metadata.Data = new byte[size];

				fixed ( byte* ptr2 = metadata.Data )
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
	}
}