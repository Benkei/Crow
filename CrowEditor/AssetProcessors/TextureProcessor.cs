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
		private string m_SourceFile;
		private string m_MetaFile;
		//private Guid m_guid;

		public bool NearPowerOfTwo;
		public Texture2D tex;

		public override void Setup ( string sourceFilePath, string metaFilePath )
		{
			m_SourceFile = sourceFilePath;
			m_MetaFile = metaFilePath;
		}

		public override void Run ()
		{
			CrowEditorApp.m_GLBackgroundThread.JobScheduler.AddJob ( this );
		}

		public unsafe void Execute ()
		{
			var watch = System.Diagnostics.Stopwatch.StartNew ();
			var root = m_SourceFile;
			Console.Write ( root );
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


				int baseWidth = img.BaseWidth;
				int baseHeight = img.BaseHeight;
				if ( NearPowerOfTwo )
				{
					baseWidth = OpenTK.MathHelper.NextPowerOfTwo ( img.BaseWidth );
					baseHeight = OpenTK.MathHelper.NextPowerOfTwo ( img.BaseHeight );
				}

				int i;
				int width = baseWidth;
				int height = baseHeight;
				int mipmaps = Util.CalculateMipmap ( baseWidth, baseHeight );

				int cSize = 0;
				for ( i = 0; i < mipmaps; ++i )
				{
					cSize += CrowSquish.Squish.GetStorageRequirements ( width, height, compFlags );
					width = Math.Max ( 1, width / 2 );
					height = Math.Max ( 1, height / 2 );
				}

				MemoryStream imgStream = new MemoryStream ( baseWidth * baseHeight * 4 );
				MemoryStream bcStream = new MemoryStream ( cSize );

				var w = System.Diagnostics.Stopwatch.StartNew ();

				//int bytesPerBlock = (compFlags & CrowSquish.ModusFlags.kDxt1) > 0 ? 8 : 16;
				width = baseWidth;
				height = baseHeight;
				int offset = 0;
				fixed ( byte* imgPtr = imgStream.GetBuffer () )
				fixed ( byte* bcPtr = bcStream.GetBuffer () )
				{
					MagickGeometry geo = new MagickGeometry ( 0, 0 );
					geo.IgnoreAspectRatio = true;
					for ( i = 0; i < mipmaps; ++i )
					{
						int surfaceSizeInBytes = CrowSquish.Squish.GetStorageRequirements ( width, height, compFlags );

						#region MyRegion
						imgStream.Position = 0;

						geo.Width = width;
						geo.Height = height;
						//img.Resize ( geo );
						img.Scale ( geo );
						img.Write ( imgStream, MagickFormat.Rgba );

						//var file = Path.Combine ( Path.GetDirectoryName ( root ), Path.GetFileNameWithoutExtension ( root ) + "_" + i + ".png" );
						//img.Write ( file );

						CrowSquish.Squish.CompressImageParallel ( (IntPtr)imgPtr, width, height, (IntPtr)(bcPtr + offset), compFlags );
						#endregion

						width = Math.Max ( 1, width / 2 );
						height = Math.Max ( 1, height / 2 );
						offset += surfaceSizeInBytes;
					}
				}

				Console.Write ( w.Elapsed.TotalMilliseconds.ToString ( " 0.0ms" ) );


				TextureData metadata = new TextureData ();
				metadata.Width = baseWidth;
				metadata.Height = baseHeight;
				metadata.LevelCount = mipmaps;
				metadata.Format = internPixelFormat;
				metadata.DataSize = bcStream.GetBuffer ().Length;
				metadata.Data = bcStream.GetBuffer ();

				var metadataFile = Path.Combine ( Path.GetDirectoryName ( root ), Path.GetFileNameWithoutExtension ( root ) + ".metadata" );
				Factory.Save ( metadataFile, metadata );


				// load texture

				tex = new Texture2D ();
				tex.Bind ();
				tex.SetDebugName ( Path.GetFileNameWithoutExtension ( root ) );

				width = metadata.Width;
				height = metadata.Height;
				mipmaps = metadata.LevelCount;
				offset = 0;
				fixed ( byte* bcPtr = metadata.Data )
				{
					for ( i = 0; i < mipmaps; ++i )
					{
						int surfaceSizeInBytes = CrowSquish.Squish.GetStorageRequirements ( width, height, compFlags );

						tex.SetCompressedData ( i, width, height, metadata.Format, surfaceSizeInBytes, (IntPtr)(bcPtr + offset) );

						width = Math.Max ( 1, width / 2 );
						height = Math.Max ( 1, height / 2 );
						offset += surfaceSizeInBytes;
					}
				}

				tex.BaseLevel = 0;
				tex.MaxLevel = metadata.LevelCount;
				tex.MagFilter = TextureMagFilter.Nearest;
				tex.MinFilter = TextureMinFilter.Nearest;

				//tex.Delete ();
				//CrowEditorApp.m_GLRenderThread.List.Add ( tex );
			}
			Console.Write ( watch.Elapsed.TotalMilliseconds.ToString ( " 0.0ms" ) );
			Console.WriteLine ();

		}
	}
}
