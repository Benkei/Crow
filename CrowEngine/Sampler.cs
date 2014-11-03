using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using SharpDX;

namespace CrowEngine
{
	/*
	struct Parameters
	{
		public Color4 TextureBorderColor;
		public TextureMagFilter TextureMagFilter;
		public TextureMinFilter TextureMinFilter;
		public TextureWrapMode TextureWrapS;
		public TextureWrapMode TextureWrapT;
		public TextureWrapMode TextureWrapR;
		public float TextureMinLod;
		public float TextureMaxLod;
		//public float TextureMaxAnisotropyExt;
		public float TextureLodBias;
		public TextureCompareMode TextureCompareMode;
		public TextureCompareFunc TextureCompareFunc;
	}
	*/

	class Sampler : GLHandler, ISamplerValues
	{
		public Sampler ()
		{
			m_Handler = GL.GenSampler ();
		}

		#region Sampler values
		public unsafe Color4 BorderColor
		{
			get
			{
				Color4 value;
				GL.GetSamplerParameter ( m_Handler, SamplerParameterName.TextureBorderColor, (float*)&value );
				return value;
			}
			set { GL.SamplerParameter ( m_Handler, SamplerParameterName.TextureBorderColor, (float*)&value ); }
		}

		public TextureMagFilter MagFilter
		{
			get
			{
				int value;
				GL.GetSamplerParameter ( m_Handler, SamplerParameterName.TextureMagFilter, out value );
				return (TextureMagFilter)value;
			}
			set { GL.SamplerParameter ( m_Handler, SamplerParameterName.TextureMagFilter, (int)value ); }
		}

		public TextureMinFilter MinFilter
		{
			get
			{
				int value;
				GL.GetSamplerParameter ( m_Handler, SamplerParameterName.TextureMinFilter, out value );
				return (TextureMinFilter)value;
			}
			set { GL.SamplerParameter ( m_Handler, SamplerParameterName.TextureMinFilter, (int)value ); }
		}

		public TextureWrapMode WrapS
		{
			get
			{
				int value;
				GL.GetSamplerParameter ( m_Handler, SamplerParameterName.TextureWrapS, out value );
				return (TextureWrapMode)value;
			}
			set { GL.SamplerParameter ( m_Handler, SamplerParameterName.TextureWrapS, (int)value ); }
		}

		public TextureWrapMode WrapT
		{
			get
			{
				int value;
				GL.GetSamplerParameter ( m_Handler, SamplerParameterName.TextureWrapT, out value );
				return (TextureWrapMode)value;
			}
			set { GL.SamplerParameter ( m_Handler, SamplerParameterName.TextureWrapT, (int)value ); }
		}

		public TextureWrapMode WrapR
		{
			get
			{
				int value;
				GL.GetSamplerParameter ( m_Handler, SamplerParameterName.TextureWrapR, out value );
				return (TextureWrapMode)value;
			}
			set { GL.SamplerParameter ( m_Handler, SamplerParameterName.TextureWrapR, (int)value ); }
		}

		public float MinLod
		{
			get
			{
				float value;
				GL.GetSamplerParameter ( m_Handler, SamplerParameterName.TextureMinLod, out value );
				return value;
			}
			set { GL.SamplerParameter ( m_Handler, SamplerParameterName.TextureMinLod, (int)value ); }
		}

		public float MaxLod
		{
			get
			{
				float value;
				GL.GetSamplerParameter ( m_Handler, SamplerParameterName.TextureMaxLod, out value );
				return value;
			}
			set { GL.SamplerParameter ( m_Handler, SamplerParameterName.TextureMaxLod, (int)value ); }
		}

		public float LodBias
		{
			get
			{
				float value;
				GL.GetSamplerParameter ( m_Handler, SamplerParameterName.TextureLodBias, out value );
				return value;
			}
			set { GL.SamplerParameter ( m_Handler, SamplerParameterName.TextureLodBias, (int)value ); }
		}

		public TextureCompareMode CompareMode
		{
			get
			{
				int value;
				GL.GetSamplerParameter ( m_Handler, SamplerParameterName.TextureCompareMode, out value );
				return (TextureCompareMode)value;
			}
			set { GL.SamplerParameter ( m_Handler, SamplerParameterName.TextureCompareMode, (int)value ); }
		}

		public TextureCompareFunc CompareFunc
		{
			get
			{
				int value;
				GL.GetSamplerParameter ( m_Handler, SamplerParameterName.TextureCompareFunc, out value );
				return (TextureCompareFunc)value;
			}
			set { GL.SamplerParameter ( m_Handler, SamplerParameterName.TextureCompareFunc, (int)value ); }
		}
		#endregion

		public void Bind ( int index )
		{
			GL.BindSampler ( index, m_Handler );
		}

		public void Delete ()
		{
			GL.DeleteSampler ( m_Handler );
		}
	}

	public enum TextureCompareFunc
	{
		// Zusammenfassung:
		//     Original was GL_NEVER = 0x0200
		Never = 512,
		//
		// Zusammenfassung:
		//     Original was GL_LESS = 0x0201
		Less = 513,
		//
		// Zusammenfassung:
		//     Original was GL_EQUAL = 0x0202
		Equal = 514,
		//
		// Zusammenfassung:
		//     Original was GL_LEQUAL = 0x0203
		Lequal = 515,
		//
		// Zusammenfassung:
		//     Original was GL_GREATER = 0x0204
		Greater = 516,
		//
		// Zusammenfassung:
		//     Original was GL_NOTEQUAL = 0x0205
		Notequal = 517,
		//
		// Zusammenfassung:
		//     Original was GL_GEQUAL = 0x0206
		Gequal = 518,
		//
		// Zusammenfassung:
		//     Original was GL_ALWAYS = 0x0207
		Always = 519,
	}

	public interface ISamplerValues
	{
		Color4 BorderColor { get; set; }

		TextureMagFilter MagFilter { get; set; }

		TextureMinFilter MinFilter { get; set; }

		TextureWrapMode WrapS { get; set; }

		TextureWrapMode WrapT { get; set; }

		TextureWrapMode WrapR { get; set; }

		float MinLod { get; set; }

		float MaxLod { get; set; }

		float LodBias { get; set; }

		TextureCompareMode CompareMode { get; set; }

		TextureCompareFunc CompareFunc { get; set; }
	}
}
