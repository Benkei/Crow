using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using SharpDX;

namespace CrowEngine
{
	public enum TextureSwizzle
	{
		Zero = All.Zero,
		One = All.One,
		Red = All.Red,
		Green = All.Green,
		Blue = All.Blue,
		Alpha = All.Alpha,
	}

	public enum ColorType
	{
		None = All.None,
		SignedNormalized = All.SignedNormalized,
		UnsignedNormalized = All.UnsignedNormalized,
		Float = All.Float,
		Int = All.Int,
		UnsignedInt = All.UnsignedInt,
	}

	struct MyStruct
	{
		public Color4 TextureBorderColor;
		public TextureMagFilter TextureMagFilter;
		public TextureMinFilter TextureMinFilter;
		public TextureWrapMode TextureWrapS;
		public TextureWrapMode TextureWrapT;
		public TextureWrapMode TextureWrapR;
		public float TextureMinLod;
		public float TextureMaxLod;
		public float TextureLodBias;
		public TextureCompareMode TextureCompareMode;
		public TextureCompareFunc TextureCompareFunc;

		public int TextureBaseLevel;
		public int TextureMaxLevel;
		public int DepthTextureMode; // GL_DEPTH_COMPONENT, GL_STENCIL_COMPONENT
		public TextureSwizzle TextureSwizzleR;
		public TextureSwizzle TextureSwizzleG;
		public TextureSwizzle TextureSwizzleB;
		public TextureSwizzle TextureSwizzleA;
		public TextureSwizzle[] TextureSwizzleRgba;
		//public  TexturePriority ;
		//public  TexturePriorityExt ;
		//public  TextureDepth ;
		//public  TextureWrapRExt ;
		//public  TextureWrapROes ;
		//public  DetailTextureLevelSgis ;
		//public  DetailTextureModeSgis ;
		//public  TextureCompareFailValue ;
		//public  ShadowAmbientSgix;
		//public  DualTextureSelectSgis ;
		//public  QuadTextureSelectSgis ;
		//public  ClampToBorder ;
		//public  ClampToEdge ;
		//public  TextureWrapQSgis;
		//public  TextureClipmapCenterSgix;
		//public  TextureClipmapFrameSgix ;
		//public  TextureClipmapOffsetSgix;
		//public  TextureClipmapVirtualDepthSgix ;
		//public  TextureClipmapLodOffsetSgix ;
		//public  TextureClipmapDepthSgix   ;
		//public  PostTextureFilterBiasSgix ;
		//public  PostTextureFilterScaleSgix;
		//public  TextureLodBiasSSgix ;
		//public  TextureLodBiasTSgix ;
		//public  TextureLodBiasRSgix ;
		//public  GenerateMipmap      ;
		//public  GenerateMipmapSgis  ;
		//public  TextureCompareSgix  ;
		//public  TextureMaxClampSSgix;
		//public  TextureMaxClampTSgix;
		//public  TextureMaxClampRSgix;
	}


	public class Texture2D : Texture, ISamplerValues
	{
		public Texture2D ()
		{
		}

		public int BaseLevel
		{
			get
			{
				int value;
				GL.GetTexParameter ( TextureTarget.Texture2D, GetTextureParameter.TextureBaseLevel, out value );
				return value;
			}
			set { GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, value ); }
		}

		public int MaxLevel
		{
			get
			{
				int value;
				GL.GetTexParameter ( TextureTarget.Texture2D, GetTextureParameter.TextureMaxLevel, out value );
				return value;
			}
			set { GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, value ); }
		}

		public bool NeedGenerateMipmap
		{
			get
			{
				int value;
				GL.GetTexParameter ( TextureTarget.Texture2D, GetTextureParameter.GenerateMipmap, out value );
				return value == 1;
			}
			set { GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, value ? 1 : 0 ); }
		}

		public int CountLevels
		{
			get { return Util.CalculateMipmap ( Width ( 0 ), Height ( 0 ) ); }
		}

		public TextureSwizzle SwizzleR
		{
			get
			{
				int value;
				GL.GetTexParameter ( TextureTarget.Texture2D, GetTextureParameter.TextureSwizzleR, out value );
				return (TextureSwizzle)value;
			}
			set { GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureSwizzleR, (int)value ); }
		}

		public TextureSwizzle SwizzleG
		{
			get
			{
				int value;
				GL.GetTexParameter ( TextureTarget.Texture2D, GetTextureParameter.TextureSwizzleG, out value );
				return (TextureSwizzle)value;
			}
			set { GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureSwizzleG, (int)value ); }
		}

		public TextureSwizzle SwizzleB
		{
			get
			{
				int value;
				GL.GetTexParameter ( TextureTarget.Texture2D, GetTextureParameter.TextureSwizzleB, out value );
				return (TextureSwizzle)value;
			}
			set { GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureSwizzleB, (int)value ); }
		}

		public TextureSwizzle SwizzleA
		{
			get
			{
				int value;
				GL.GetTexParameter ( TextureTarget.Texture2D, GetTextureParameter.TextureSwizzleA, out value );
				return (TextureSwizzle)value;
			}
			set { GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureSwizzleA, (int)value ); }
		}

		#region Sampler values
		public unsafe Color4 BorderColor
		{
			get
			{
				Color4 value;
				GL.GetTexParameter ( TextureTarget.Texture2D, GetTextureParameter.TextureBorderColor, (float*)&value );
				return value;
			}
			set { GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, (float*)&value ); }
		}

		public TextureMagFilter MagFilter
		{
			get
			{
				int value;
				GL.GetTexParameter ( TextureTarget.Texture2D, GetTextureParameter.TextureMagFilter, out value );
				return (TextureMagFilter)value;
			}
			set { GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)value ); }
		}

		public TextureMinFilter MinFilter
		{
			get
			{
				int value;
				GL.GetTexParameter ( TextureTarget.Texture2D, GetTextureParameter.TextureMinFilter, out value );
				return (TextureMinFilter)value;
			}
			set { GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)value ); }
		}

		public TextureWrapMode WrapS
		{
			get
			{
				int value;
				GL.GetTexParameter ( TextureTarget.Texture2D, GetTextureParameter.TextureWrapS, out value );
				return (TextureWrapMode)value;
			}
			set { GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)value ); }
		}

		public TextureWrapMode WrapT
		{
			get
			{
				int value;
				GL.GetTexParameter ( TextureTarget.Texture2D, GetTextureParameter.TextureWrapT, out value );
				return (TextureWrapMode)value;
			}
			set { GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)value ); }
		}

		public TextureWrapMode WrapR
		{
			get
			{
				int value;
				GL.GetTexParameter ( TextureTarget.Texture2D, GetTextureParameter.TextureWrapR, out value );
				return (TextureWrapMode)value;
			}
			set { GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int)value ); }
		}

		public float MinLod
		{
			get
			{
				float value;
				GL.GetTexParameter ( TextureTarget.Texture2D, GetTextureParameter.TextureMinLod, out value );
				return value;
			}
			set { GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureMinLod, (int)value ); }
		}

		public float MaxLod
		{
			get
			{
				float value;
				GL.GetTexParameter ( TextureTarget.Texture2D, GetTextureParameter.TextureMaxLod, out value );
				return value;
			}
			set { GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureMaxLod, (int)value ); }
		}

		public float LodBias
		{
			get { throw new NotSupportedException (); }
			set { GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureLodBias, (int)value ); }
		}

		public TextureCompareMode CompareMode
		{
			get
			{
				int value;
				GL.GetTexParameter ( TextureTarget.Texture2D, GetTextureParameter.TextureCompareMode, out value );
				return (TextureCompareMode)value;
			}
			set { GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)value ); }
		}

		public TextureCompareFunc CompareFunc
		{
			get
			{
				int value;
				GL.GetTexParameter ( TextureTarget.Texture2D, GetTextureParameter.TextureCompareFunc, out value );
				return (TextureCompareFunc)value;
			}
			set { GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureCompareFunc, (int)value ); }
		}
		#endregion

		#region Texture level values
		public int Width ( int level )
		{
			GL.GetTexLevelParameter ( TextureTarget.Texture2D, level, GetTextureParameter.TextureWidth, out level );
			return level;
		}

		public int Height ( int level )
		{
			GL.GetTexLevelParameter ( TextureTarget.Texture2D, level, GetTextureParameter.TextureHeight, out level );
			return level;
		}

		public int Depth ( int level )
		{
			GL.GetTexLevelParameter ( TextureTarget.Texture2D, level, GetTextureParameter.TextureDepth, out level );
			return level;
		}

		public PixelInternalFormat PixelInternalFormat ( int level )
		{
			GL.GetTexLevelParameter ( TextureTarget.Texture2D, level, GetTextureParameter.TextureInternalFormat, out level );
			return (PixelInternalFormat)level;
		}

		public ColorType RedType ( int level )
		{
			GL.GetTexLevelParameter ( TextureTarget.Texture2D, level, GetTextureParameter.TextureRedType, out level );
			return (ColorType)level;
		}

		public ColorType GreenType ( int level )
		{
			GL.GetTexLevelParameter ( TextureTarget.Texture2D, level, GetTextureParameter.TextureGreenType, out level );
			return (ColorType)level;
		}

		public ColorType BlueType ( int level )
		{
			GL.GetTexLevelParameter ( TextureTarget.Texture2D, level, GetTextureParameter.TextureBlueType, out level );
			return (ColorType)level;
		}

		public ColorType AlphaType ( int level )
		{
			GL.GetTexLevelParameter ( TextureTarget.Texture2D, level, GetTextureParameter.TextureAlphaType, out level );
			return (ColorType)level;
		}

		public ColorType DepthType ( int level )
		{
			GL.GetTexLevelParameter ( TextureTarget.Texture2D, level, GetTextureParameter.TextureDepthType, out level );
			return (ColorType)level;
		}

		public int RedSize ( int level )
		{
			GL.GetTexLevelParameter ( TextureTarget.Texture2D, level, GetTextureParameter.TextureRedSize, out level );
			return level;
		}

		public int GreenSize ( int level )
		{
			GL.GetTexLevelParameter ( TextureTarget.Texture2D, level, GetTextureParameter.TextureGreenSize, out level );
			return level;
		}

		public int BlueSize ( int level )
		{
			GL.GetTexLevelParameter ( TextureTarget.Texture2D, level, GetTextureParameter.TextureBlueSize, out level );
			return level;
		}

		public int AlphaSize ( int level )
		{
			GL.GetTexLevelParameter ( TextureTarget.Texture2D, level, GetTextureParameter.TextureAlphaSize, out level );
			return level;
		}

		public int DepthSize ( int level )
		{
			GL.GetTexLevelParameter ( TextureTarget.Texture2D, level, GetTextureParameter.TextureDepthSize, out level );
			return level;
		}

		public bool Compressed ( int level )
		{
			int value;
			GL.GetTexLevelParameter ( TextureTarget.Texture2D, level, GetTextureParameter.TextureCompressed, out value );
			return value == 1;
		}

		public int CompressedSize ( int level )
		{
			GL.GetTexLevelParameter ( TextureTarget.Texture2D, level, GetTextureParameter.TextureCompressedImageSize, out level );
			return level;
		}

		public int BufferOffset ( int level )
		{
			GL.GetTexLevelParameter ( TextureTarget.Texture2D, level, (GetTextureParameter)All.TextureBufferOffset, out level );
			return level;
		}

		public int BufferSize ( int level )
		{
			GL.GetTexLevelParameter ( TextureTarget.Texture2D, level, (GetTextureParameter)All.TextureBufferSize, out level );
			return level;
		}
		#endregion

		public void SetSwizzle ( TextureSwizzle r, TextureSwizzle g, TextureSwizzle b, TextureSwizzle a )
		{
			unsafe
			{
				int* rgba = stackalloc int[4];
				rgba[0] = (int)r;
				rgba[1] = (int)g;
				rgba[2] = (int)b;
				rgba[3] = (int)a;
				GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureSwizzleRgba, rgba );
			}
		}

		public void GetSwizzle ( out TextureSwizzle r, out TextureSwizzle g, out TextureSwizzle b, out TextureSwizzle a )
		{
			unsafe
			{
				int* rgba = stackalloc int[4];
				GL.GetTexParameter ( TextureTarget.Texture2D, GetTextureParameter.TextureSwizzleRgba, rgba );
				r = (TextureSwizzle)rgba[0];
				g = (TextureSwizzle)rgba[1];
				b = (TextureSwizzle)rgba[2];
				a = (TextureSwizzle)rgba[3];
			}
		}

		public void Bind ()
		{
			GL.BindTexture ( TextureTarget.Texture2D, m_Handler );
		}

		public void SetDebugName ( string name )
		{
			if ( name == null )
				GL.ObjectLabel ( ObjectLabelIdentifier.Texture, m_Handler, 0, null );
			else
				GL.ObjectLabel ( ObjectLabelIdentifier.Texture, m_Handler, name.Length, name );
		}

		public void SetData ( int level,
			int width, int height,
			PixelInternalFormat internalformat, PixelFormat format )
		{
			GL.TexImage2D ( TextureTarget.Texture2D, level, internalformat, width, height, 0, format, PixelType.UnsignedByte, IntPtr.Zero );
		}

		public void SetData ( int level,
			int width, int height,
			PixelInternalFormat internalformat, PixelFormat format,
			PixelType type, IntPtr data )
		{
			GL.TexImage2D ( TextureTarget.Texture2D, level, internalformat, width, height, 0, format, type, data );
		}

		public void SetData ( int level,
			int xoffset, int yoffset,
			int width, int height, PixelFormat format,
			PixelType type, IntPtr data )
		{
			GL.TexSubImage2D ( TextureTarget.Texture2D, level, xoffset, yoffset, width, height, format, type, data );
		}

		public void SetDataBuffer ( SizedInternalFormat internalformat, GLBuffer buffer )
		{
			GL.TexBuffer ( TextureBufferTarget.TextureBuffer, internalformat, buffer.Handler );
		}

		public void GetData ( int level, PixelFormat format,
			PixelType type, IntPtr data )
		{
			GL.GetTexImage ( TextureTarget.Texture2D, level, format, type, data );
		}


		public void SetCompressedData ( int level,
			int width, int height, PixelInternalFormat internalformat,
			int dataSize, IntPtr data )
		{
			GL.CompressedTexImage2D ( TextureTarget.Texture2D, level, internalformat, width, height, 0, dataSize, data );
		}

		public void SetCompressedData ( int level,
			int xoffset, int yoffset,
			int width, int height, PixelFormat format,
			int dataSize, IntPtr data )
		{
			GL.CompressedTexSubImage2D ( TextureTarget.Texture2D, level, xoffset, yoffset, width, height, format, dataSize, data );
		}

		public void GetCompressedData ( int level, IntPtr data )
		{
			GL.GetCompressedTexImage ( TextureTarget.Texture2D, level, data );
		}

		// copy data from screen
		public void Copy ( int level,
			int xoffset, int yoffset,
			int x, int y,
			int width, int height )
		{
			GL.CopyTexSubImage2D ( TextureTarget.Texture2D, level, xoffset, yoffset, x, y, width, height );
		}

		public void GenerateMipmap ( HintMode hint )
		{
			//GL.Hint ( HintTarget.GenerateMipmapHint, hint );
			GL.GenerateMipmap ( GenerateMipmapTarget.Texture2D );
		}

		//public void Invalidate ( int level )
		//{
		//	GL.InvalidateTexImage ( m_Handler, level );
		//}

		//public void Clear ()
		//{
		//	GL.ClearTexImage(m_Handler,)
		//}
	}
}
