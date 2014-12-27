using System;
using CrowEngine.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine.Graphics
{
	class DeviceContext
	{
		private static void SetEnable ( EnableCap cap, bool value )
		{
			if ( value ) GL.Enable ( cap );
			else GL.Disable ( cap );
		}

		public int ActiveTextureUnit
		{
			get { return GL.GetInteger ( GetPName.ActiveTexture ) - (int)TextureUnit.Texture0; }
			set { GL.ActiveTexture ( TextureUnit.Texture0 + value ); }
		}

		public int MaxActiveTextureUnit
		{
			get { return GL.GetInteger ( GetPName.MaxCombinedTextureImageUnits ); }
		}

		// samples setting
		public bool TextureCubeMapSeamless
		{
			get { return GL.IsEnabled ( EnableCap.TextureCubeMapSeamless ); }
			set { SetEnable ( EnableCap.TextureCubeMapSeamless, value ); }
		}

		public int CurrentProgram
		{
			get { return GL.GetInteger ( GetPName.CurrentProgram ); }
		}

		public bool PrimitiveRestart
		{
			get { return GL.IsEnabled ( EnableCap.PrimitiveRestart ); }
			set { SetEnable ( EnableCap.PrimitiveRestart, value ); }
		}

		public bool PrimitiveRestartFixedIndex
		{
			get { return GL.IsEnabled ( EnableCap.PrimitiveRestartFixedIndex ); }
			set { SetEnable ( EnableCap.PrimitiveRestartFixedIndex, value ); }
		}

		public void PrimitiveRestartIndex ( int index )
		{
			GL.PrimitiveRestartIndex ( index );
		}


		//GL_TEXTURE_BINDING_1D, 
		//GL_TEXTURE_BINDING_2D, 
		//GL_TEXTURE_BINDING_3D,
		//GL_TEXTURE_BINDING_1D_ARRAY, 
		//GL_TEXTURE_BINDING_2D_ARRAY, 
		//GL_TEXTURE_BINDING_RECTANGLE,
		//GL_TEXTURE_BINDING_BUFFER, 
		//GL_TEXTURE_BINDING_CUBE_MAP,
		//GL_TEXTURE_BINDING_CUBE_MAP, 
		//GL_TEXTURE_BINDING_CUBE_MAP_ARRAY, 
		//GL_TEXTURE_BINDING_2D_MULTISAMPLE, 
		//GL_TEXTURE_BINDING_2D_MULTISAMPLE_ARRAY.

		public DeviceContext ()
		{
			//GL.GetInteger(GetPName.MaxViewports)
		}

		public void Hint ()
		{
			GL.GetInteger ( GetPName.ArrayBufferBinding );
			GL.GetInteger ( GetPName.DrawIndirectBufferBinding );
			GL.GetInteger ( GetPName.ElementArrayBufferBinding );
			GL.GetInteger ( GetPName.PixelPackBufferBinding );
			GL.GetInteger ( GetPName.PixelUnpackBufferBinding );


			//GL_ARRAY_BUFFER_BINDING
			//GL_ATOMIC_COUNTER_BUFFER_BINDING
			//GL_COPY_READ_BUFFER_BINDING
			//GL_COPY_WRITE_BUFFER_BINDING
			//GL_DRAW_INDIRECT_BUFFER_BINDING
			//GL_DISPATCH_INDIRECT_BUFFER_BINDING
			//GL_ELEMENT_ARRAY_BUFFER_BINDING
			//GL_PIXEL_PACK_BUFFER_BINDING
			//GL_PIXEL_UNPACK_BUFFER_BINDING
			//GL_SHADER_STORAGE_BUFFER_BINDING
			//GL_TRANSFORM_FEEDBACK_BUFFER_BINDING
			//GL_UNIFORM_BUFFER_BINDING


			//GL.BindTexture( TextureTarget.)

			GL.GetInteger ( GetPName.TextureBinding1D );
			GL.GetInteger ( GetPName.TextureBinding2D );
			GL.GetInteger ( GetPName.TextureBinding3D );
			GL.GetInteger ( GetPName.TextureBinding1DArray );
			GL.GetInteger ( GetPName.TextureBinding2DArray );
			GL.GetInteger ( GetPName.TextureBindingRectangle );
			GL.GetInteger ( GetPName.TextureBindingBuffer );
			GL.GetInteger ( GetPName.TextureBindingCubeMap );
			GL.GetInteger ( GetPName.TextureBinding2DMultisample );
			GL.GetInteger ( GetPName.TextureBinding2DMultisampleArray );

			//GL_TEXTURE_BINDING_1D, 
			//GL_TEXTURE_BINDING_2D, 
			//GL_TEXTURE_BINDING_3D, 
			//GL_TEXTURE_BINDING_1D_ARRAY, 
			//GL_TEXTURE_BINDING_2D_ARRAY, 
			//GL_TEXTURE_BINDING_RECTANGLE, 
			//GL_TEXTURE_BINDING_BUFFER, 
			//GL_TEXTURE_BINDING_CUBE_MAP, 
			//GL_TEXTURE_BINDING_CUBE_MAP, 
			//GL_TEXTURE_BINDING_CUBE_MAP_ARRAY, 
			//GL_TEXTURE_BINDING_2D_MULTISAMPLE, 
			//GL_TEXTURE_BINDING_2D_MULTISAMPLE_ARRAY.

		}

		public void DrawArray ( PrimitiveType mode, int first, int count )
		{
			GL.DrawArrays ( mode, first, count );
		}
		public void DrawArraysInstancedBaseInstance ( PrimitiveType mode, int first, int count, int instanceCount, int baseInstance )
		{
			GL.DrawArraysInstancedBaseInstance ( mode, first, count, instanceCount, baseInstance );
		}
		public void DrawArraysInstanced ( PrimitiveType mode, int first, int count, int instanceCount )
		{
			GL.DrawArraysInstanced ( mode, first, count, instanceCount );
		}
		public void DrawArraysIndirect ( PrimitiveType mode, IntPtr indirect )
		{
			GL.DrawArraysIndirect ( mode, indirect );
		}
		public unsafe void MultiDrawArrays ( PrimitiveType mode, int* first, int* count, int drawcount )
		{
			GL.MultiDrawArrays ( mode, first, count, drawcount );
		}
		public void MultiDrawArrays ( PrimitiveType mode, int[] first, int[] count, int drawcount )
		{
			GL.MultiDrawArrays ( mode, first, count, drawcount );
		}
		public void MultiDrawArraysIndirect ( PrimitiveType mode, IntPtr indirect, int drawcount, int stride )
		{
			GL.MultiDrawArraysIndirect ( mode, indirect, drawcount, stride );
		}
		public void MultiDrawArraysIndirect<T> ( PrimitiveType mode, T[] indirect, int drawcount, int stride )
			where T : struct
		{
			GL.MultiDrawArraysIndirect ( mode, indirect, drawcount, stride );
		}
		public void DrawElements ( BeginMode mode, int count, DrawElementsType type, int offset )
		{
			GL.DrawElements ( mode, count, type, offset );
		}
		public void DrawElements ( PrimitiveType mode, int count, DrawElementsType type, IntPtr indices )
		{
			GL.DrawElements ( mode, count, type, indices );
		}
		public void DrawElements<T> ( PrimitiveType mode, int count, DrawElementsType type, T[] indices )
			where T : struct
		{
			GL.DrawElements ( mode, count, type, indices );
		}
		public void DrawElementsInstancedBaseInstance ( PrimitiveType mode, int count, DrawElementsType type, IntPtr indices, int instancecount, int baseinstance )
		{
			GL.DrawElementsInstancedBaseInstance ( mode, count, type, indices, instancecount, baseinstance );
		}
		public void DrawElementsInstancedBaseInstance<T> ( PrimitiveType mode, int count, DrawElementsType type, T[] indices, int instancecount, int baseinstance )
			where T : struct
		{
			GL.DrawElementsInstancedBaseInstance ( mode, count, type, indices, instancecount, baseinstance );
		}
		public void DrawElementsInstanced ( PrimitiveType mode, int count, DrawElementsType type, IntPtr indices, int instancecount )
		{
			GL.DrawElementsInstanced ( mode, count, type, indices, instancecount );
		}
		public void DrawElementsInstanced<T> ( PrimitiveType mode, int count, DrawElementsType type, T[] indices, int instancecount )
			where T : struct
		{
			GL.DrawElementsInstanced ( mode, count, type, indices, instancecount );
		}
		public unsafe void MultiDrawElements ( PrimitiveType mode, int* count, DrawElementsType type, IntPtr indices, int drawcount )
		{
			GL.MultiDrawElements ( mode, count, type, indices, drawcount );
		}
		public void MultiDrawElements ( PrimitiveType mode, int[] count, DrawElementsType type, IntPtr indices, int drawcount )
		{
			GL.MultiDrawElements ( mode, count, type, indices, drawcount );
		}
		public void MultiDrawElements<T> ( PrimitiveType mode, int[] count, DrawElementsType type, T[] indices, int drawcount )
			where T : struct
		{
			GL.MultiDrawElements ( mode, count, type, indices, drawcount );
		}
		public void DrawRangeElements ( PrimitiveType mode, int start, int end, int count, DrawElementsType type, IntPtr indices )
		{
			GL.DrawRangeElements ( mode, start, end, count, type, indices );
		}
		public void DrawRangeElements<T> ( PrimitiveType mode, int start, int end, int count, DrawElementsType type, T[] indices )
			where T : struct
		{
			GL.DrawRangeElements ( mode, start, end, count, type, indices );
		}
		public void DrawElementsBaseVertex ( PrimitiveType mode, int count, DrawElementsType type, IntPtr indices, int basevertex )
		{
			GL.DrawElementsBaseVertex ( mode, count, type, indices, basevertex );
		}
		public void DrawElementsBaseVertex<T> ( PrimitiveType mode, int count, DrawElementsType type, T[] indices, int basevertex )
			where T : struct
		{
			GL.DrawElementsBaseVertex ( mode, count, type, indices, basevertex );
		}
		public void DrawRangeElementsBaseVertex ( PrimitiveType mode, int start, int end, int count, DrawElementsType type, IntPtr indices, int basevertex )
		{
			GL.DrawRangeElementsBaseVertex ( mode, start, end, count, type, indices, basevertex );
		}
		public void DrawRangeElementsBaseVertex<T> ( PrimitiveType mode, int start, int end, int count, DrawElementsType type, T[] indices, int basevertex )
			where T : struct
		{
			GL.DrawRangeElementsBaseVertex ( mode, start, end, count, type, indices, basevertex );
		}
		public void DrawElementsInstancedBaseVertex ( PrimitiveType mode, int count, DrawElementsType type, IntPtr indices, int instancecount, int basevertex )
		{
			GL.DrawElementsInstancedBaseVertex ( mode, count, type, indices, instancecount, basevertex );
		}
		public void DrawElementsInstancedBaseVertex<T> ( PrimitiveType mode, int count, DrawElementsType type, T[] indices, int instancecount, int basevertex )
			where T : struct
		{
			GL.DrawElementsInstancedBaseVertex ( mode, count, type, indices, instancecount, basevertex );
		}
		public void DrawElementsInstancedBaseVertexBaseInstance ( PrimitiveType mode, int count, DrawElementsType type, IntPtr indices, int instancecount, int basevertex, int baseinstance )
		{
			GL.DrawElementsInstancedBaseVertexBaseInstance ( mode, count, type, indices, instancecount, basevertex, baseinstance );
		}
		public void DrawElementsInstancedBaseVertexBaseInstance<T> ( PrimitiveType mode, int count, DrawElementsType type, T[] indices, int instancecount, int basevertex, int baseinstance )
			where T : struct
		{
			GL.DrawElementsInstancedBaseVertexBaseInstance ( mode, count, type, indices, instancecount, basevertex, baseinstance );
		}
		public void DrawElementsIndirect ( PrimitiveType mode, All type, IntPtr indirect )
		{
			GL.DrawElementsIndirect ( mode, type, indirect );
		}
		public void DrawElementsIndirect<T> ( PrimitiveType mode, All type, T[] indirect )
			where T : struct
		{
			GL.DrawElementsIndirect ( mode, type, indirect );
		}
		public void MultiDrawElementsIndirect ( All mode, All type, IntPtr indirect, int drawcount, int stride )
		{
			GL.MultiDrawElementsIndirect ( mode, type, indirect, drawcount, stride );
		}
		public void MultiDrawElementsIndirect<T> ( All mode, All type, T[] indirect, int drawcount, int stride )
			where T : struct
		{
			GL.MultiDrawElementsIndirect ( mode, type, indirect, drawcount, stride );
		}
		public unsafe void MultiDrawElementsBaseVertex ( PrimitiveType mode, int* count, DrawElementsType type, IntPtr indices, int drawcount, int* basevertex )
		{
			GL.MultiDrawElementsBaseVertex ( mode, count, type, indices, drawcount, basevertex );
		}
		public void MultiDrawElementsBaseVertex<T> ( PrimitiveType mode, int[] count, DrawElementsType type, T[] indices, int drawcount, int[] basevertex )
			where T : struct
		{
			GL.MultiDrawElementsBaseVertex ( mode, count, type, indices, drawcount, basevertex );
		}


		internal struct FramebufferCtrl
		{
			public unsafe CrowEngine.Mathematics.Color4 ClearColor
			{
				get
				{
					CrowEngine.Mathematics.Color4 c;
					GL.GetFloat ( GetPName.ColorClearValue, (float*)&c );
					return c;
				}
				set { GL.ClearColor ( value.Red, value.Green, value.Blue, value.Alpha ); }
			}

			public double ClearDepth
			{
				get { return GL.GetDouble ( GetPName.DepthClearValue ); }
				set { GL.ClearDepth ( value ); }
			}

			public float ClearDepthF
			{
				get { return GL.GetFloat ( GetPName.DepthClearValue ); }
				set { GL.ClearDepth ( value ); }
			}

			public int ClearStencil
			{
				get { return GL.GetInteger ( GetPName.StencilClearValue ); }
				set { GL.ClearStencil ( value ); }
			}

			public bool DepthMask
			{
				get { return GL.GetBoolean ( GetPName.DepthWritemask ); }
				set { GL.DepthMask ( value ); }
			}

			public int StencilWritemask
			{
				get { return GL.GetInteger ( GetPName.StencilWritemask ); }
			}

			public int StencilBackWritemask
			{
				get { return GL.GetInteger ( GetPName.StencilBackWritemask ); }
			}


			public void Clear ( ClearBufferMask mask )
			{
				GL.Clear ( mask );
				//GL.ClearBuffer( ClearBuffer.Color, 0, )
			}

			public unsafe void ClearBufferColor ( int drawbuffer, CrowEngine.Mathematics.Color4 value )
			{
				GL.ClearBuffer ( OpenTK.Graphics.OpenGL4.ClearBuffer.Color, drawbuffer, (float*)&value );
			}

			public unsafe void ClearBufferDepth ( int drawbuffer, float value )
			{
				GL.ClearBuffer ( OpenTK.Graphics.OpenGL4.ClearBuffer.Depth, drawbuffer, &value );
			}

			public unsafe void ClearBufferStencil ( int drawbuffer, int value )
			{
				GL.ClearBuffer ( OpenTK.Graphics.OpenGL4.ClearBuffer.Stencil, drawbuffer, &value );
			}

			public unsafe void ClearBufferDepthStencil ( int drawbuffer, float depth, int stencil )
			{
				GL.ClearBuffer ( ClearBufferCombined.DepthStencil, drawbuffer, depth, stencil );
			}



			public void DrawBuffer ( DrawBufferMode mode )
			{
				GL.DrawBuffer ( mode );
			}

			public void DrawBuffers ( DrawBuffersEnum[] modes )
			{
				GL.DrawBuffers ( modes.Length, modes );
			}

			public void InvalidateFramebuffer ( FramebufferTarget target, FramebufferAttachment[] attachments )
			{
				GL.InvalidateFramebuffer ( target, attachments.Length, attachments );
			}

			public void InvalidateFramebuffer ( FramebufferTarget target, FramebufferAttachment[] attachments, RectangleF view )
			{
				GL.InvalidateSubFramebuffer ( target, attachments.Length, attachments, (int)view.X, (int)view.Y, (int)view.Width, (int)view.Height );
			}

			public void SetColorMask ( bool red, bool green, bool blue, bool alpha )
			{
				GL.ColorMask ( red, green, blue, alpha );
			}

			public void SetColorMask ( int index, bool red, bool green, bool blue, bool alpha )
			{
				GL.ColorMask ( index, red, green, blue, alpha );
			}

			public unsafe void GetColorMask ( out bool red, out bool green, out bool blue, out  bool alpha )
			{
				bool* m = stackalloc bool[4];
				GL.GetBoolean ( GetPName.ColorWritemask, m );
				red = m[0];
				green = m[1];
				blue = m[2];
				alpha = m[3];
			}

			public unsafe void GetColorMask ( int index, out bool red, out  bool green, out bool blue, out  bool alpha )
			{
				bool* m = stackalloc bool[4];
				GL.GetBoolean ( GetIndexedPName.ColorWritemask, index, m );
				red = m[0];
				green = m[1];
				blue = m[2];
				alpha = m[3];
			}

			public void SetStencilMask ( int mask )
			{
				GL.StencilMask ( mask );
			}

			public void SetStencilMask ( StencilFace face, int mask )
			{
				GL.StencilMaskSeparate ( face, mask );
			}


			public void ReadBuffer ( ReadBufferMode mode )
			{
				GL.ReadBuffer ( mode );
			}

			public void ReadPixels ( Rectangle view, PixelFormat format, PixelType type, IntPtr data )
			{
				GL.ReadPixels ( view.X, view.Y, view.Width, view.Height, format, type, data );
			}

			public void ClampColor ( ClampColorMode clamp )
			{
				GL.ClampColor ( ClampColorTarget.ClampReadColor, clamp );
			}


			public void BlitFramebuffer (
				int srcX0, int srcY0, int srcX1, int srcY1,
				int dstX0, int dstY0, int dstX1, int dstY1,
				ClearBufferMask mask, BlitFramebufferFilter filter )
			{
				GL.BlitFramebuffer (
					srcX0, srcY0, srcX1, srcY1,
					dstX0, dstY0, dstX1, dstY1,
					mask, filter );
			}

			public void CopyImageSubData (
				int srcName, ImageTarget srcTarget, int srcLevel, int srcX, int srcY, int srcZ,
				int dstName, ImageTarget dstTarget, int dstLevel, int dstX, int dstY, int dstZ,
				int srcWidth, int srcHeight, int srcDepth )
			{
				GL.CopyImageSubData (
					srcName, srcTarget, srcLevel, srcX, srcY, srcZ,
					dstName, dstTarget, dstLevel, dstX, dstY, dstZ,
					srcWidth, srcHeight, srcDepth );
			}
		}

		internal struct PrimitiveClipping
		{
			public bool Enable
			{
				get { return GL.IsEnabled ( EnableCap.DepthClamp ); }
				set { SetEnable ( EnableCap.DepthClamp, value ); }
			}

			public int MaxClipDistances
			{
				get { return GL.GetInteger ( GetPName.MaxClipDistances ); }
			}

			public void SetEnableClipDistance ( int index, bool enable )
			{
				if ( enable ) GL.Enable ( EnableCap.ClipDistance0 + index );
				else GL.Disable ( EnableCap.ClipDistance0 + index );
			}

			public bool GetEnableClipDistance ( int index )
			{
				return GL.IsEnabled ( EnableCap.ClipDistance0 + index );
			}
		}

		internal struct Viewports
		{
			public unsafe RectangleF Viewport
			{
				get
				{
					RectangleF view;
					GL.GetFloat ( GetPName.Viewport, (float*)&view );
					return view;
				}
				set { GL.Viewport ( (int)value.X, (int)value.Y, (int)value.Width, (int)value.Height ); }
			}

			public unsafe Vector2 ViewportBoundsRange
			{
				get
				{
					Vector2 size;
					GL.GetFloat ( GetPName.ViewportBoundsRange, (float*)&size );
					return size;
				}
			}

			public unsafe Vector2 MaxViewportDimension
			{
				get
				{
					Vector2 size;
					GL.GetFloat ( GetPName.MaxViewportDims, (float*)&size );
					return size;
				}
			}

			public int MaxViewports
			{
				get { return GL.GetInteger ( GetPName.MaxViewports ); }
			}

			public unsafe void SetViewports ( int first, int count, RectangleF[] views )
			{
				fixed ( RectangleF* ptr = views )
					GL.ViewportArray ( first, count, (float*)ptr );
			}

			public unsafe void SetViewport ( int index, RectangleF view )
			{
				GL.ViewportIndexed ( index, (float*)&view );
			}

			public unsafe RectangleF GetViewport ( int index )
			{
				int* r = stackalloc int[4];
				GL.GetInteger ( GetIndexedPName.Viewport, index, r );
				return new RectangleF () { X = r[0], Y = r[1], Width = r[2], Height = r[3] };
			}


			public unsafe void SetDepthRanges ( int first, int count, DepthRange[] depths )
			{
				fixed ( DepthRange* ptr = depths )
					GL.DepthRangeArray ( first, count, (double*)ptr );
			}

			public void SetDepthRange ( float near, float far )
			{
				GL.DepthRange ( near, far );
			}

			public void SetDepthRange ( DepthRange depth )
			{
				GL.DepthRange ( depth.Near, depth.Far );
			}

			public void SetDepthRange ( int index, DepthRange depth )
			{
				GL.DepthRangeIndexed ( index, depth.Near, depth.Far );
			}

			public unsafe void GetDepthRange ( out DepthRange depth )
			{
				fixed ( DepthRange* d = &depth )
					GL.GetDouble ( GetPName.DepthRange, (double*)d );
			}
		}

		internal struct Rasterization
		{
			public bool RasterizerDiscard
			{
				get { return GL.IsEnabled ( EnableCap.RasterizerDiscard ); }
				set { SetEnable ( EnableCap.RasterizerDiscard, value ); }
			}


			private struct Multisampling
			{
				public bool Multisample
				{
					get { return GL.IsEnabled ( EnableCap.Multisample ); }
					set { SetEnable ( EnableCap.Multisample, value ); }
				}

				public int Samples
				{
					get { return GL.GetInteger ( GetPName.Samples ); }
				}

				public float MinSampleShading
				{
					get { return GL.GetFloat ( GetPName.MinSampleShadingValue ); }
					set { GL.MinSampleShading ( value ); }
				}

				public unsafe Vector2 GetMultisample ( int index )
				{
					Vector2 v;
					GL.GetMultisample ( GetMultisamplePName.SamplePosition, index, (float*)&v );
					return v;
				}
			}

			private struct Point
			{
				public bool Enable
				{
					get { return GL.IsEnabled ( EnableCap.ProgramPointSize ); }
					set { SetEnable ( EnableCap.ProgramPointSize, value ); }
				}

				public float PointSize
				{
					get { return GL.GetFloat ( GetPName.PointSize ); }
					set { GL.PointSize ( value ); }
				}

				//public Point ( int x )
				//	: this ()
				//{
				//	GL.PointParameter ( PointParameterName.PointFadeThresholdSize, 0 );
				//	GL.PointParameter ( PointParameterName.PointSpriteCoordOrigin, 0 );
				//	GL.GetFloat ( GetPName.PointFadeThresholdSize );
				//}
			}

			private struct Line
			{
				public bool LineSmooth
				{
					get { return GL.IsEnabled ( EnableCap.LineSmooth ); }
					set { SetEnable ( EnableCap.LineSmooth, value ); }
				}

				public float LineWidth
				{
					get { return GL.GetFloat ( GetPName.LineWidth ); }
					set { GL.LineWidth ( value ); }
				}

				public Line ( int x )
					: this ()
				{
					//GL.Enable ( EnableCap.LineSmooth );
				}
			}

			private struct Polygon
			{
				public bool PolygonSmooth
				{
					get { return GL.IsEnabled ( EnableCap.PolygonSmooth ); }
					set { SetEnable ( EnableCap.PolygonSmooth, value ); }
				}

				public bool CullFace
				{
					get { return GL.IsEnabled ( EnableCap.CullFace ); }
					set { SetEnable ( EnableCap.CullFace, value ); }
				}

				public FrontFaceDirection FrontFace
				{
					get { return (FrontFaceDirection)GL.GetInteger ( GetPName.FrontFace ); }
					set { GL.FrontFace ( value ); }
				}

				public CullFaceMode CullFaceMode
				{
					get { return (CullFaceMode)GL.GetInteger ( GetPName.CullFaceMode ); }
					set { GL.CullFace ( value ); }
				}

				public bool PolygonOffsetFill
				{
					get { return GL.IsEnabled ( EnableCap.PolygonOffsetFill ); }
					set { SetEnable ( EnableCap.PolygonOffsetFill, value ); }
				}

				public bool PolygonOffsetLine
				{
					get { return GL.IsEnabled ( EnableCap.PolygonOffsetLine ); }
					set { SetEnable ( EnableCap.PolygonOffsetLine, value ); }
				}

				public bool PolygonOffsetPoint
				{
					get { return GL.IsEnabled ( EnableCap.PolygonOffsetPoint ); }
					set { SetEnable ( EnableCap.PolygonOffsetPoint, value ); }
				}

				public float PolygonOffsetFactor
				{
					get { return GL.GetFloat ( GetPName.PolygonOffsetFactor ); }
				}

				public float PolygonOffsetUnits
				{
					get { return GL.GetFloat ( GetPName.PolygonOffsetUnits ); }
				}


				public void PolygonOffset ( float factor, float units )
				{
					GL.PolygonOffset ( factor, units );
				}

				public void PolygonMode ( MaterialFace face, PolygonMode mode )
				{
					GL.PolygonMode ( face, mode );
				}
			}
		}

		internal struct Fragment
		{
			private struct ScissorTest
			{
				public bool Enable
				{
					get { return GL.IsEnabled ( EnableCap.ScissorTest ); }
					set { SetEnable ( EnableCap.ScissorTest, value ); }
				}

				public unsafe Rectangle Scissor
				{
					get
					{
						Rectangle v;
						GL.GetInteger ( GetPName.ScissorBox, (int*)&v );
						return v;
					}
					set { GL.Scissor ( value.X, value.Y, value.Width, value.Height ); }
				}

				public bool GetScissorTest ( int index )
				{
					return GL.IsEnabled ( IndexedEnableCap.ScissorTest, index );
				}

				public void SetScissorTest ( int index, bool value )
				{
					if ( value ) GL.Enable ( IndexedEnableCap.ScissorTest, index );
					else GL.Disable ( IndexedEnableCap.ScissorTest, index );
				}

				public unsafe void ScissorArray ( int first, int count, Rectangle[] v )
				{
					fixed ( Rectangle* ptr = v )
						GL.ScissorArray ( first, count, (int*)ptr );
				}

				public unsafe void ScissorIndexed ( int index, Rectangle box )
				{
					GL.ScissorIndexed ( index, (int*)&box );
				}
			}

			private struct Multisampling
			{
				//todo need GL.IsEnabled ( EnableCap.Multisample )

				public bool SampleAlphaToCoverage
				{
					get { return GL.IsEnabled ( EnableCap.SampleAlphaToCoverage ); }
					set { SetEnable ( EnableCap.SampleAlphaToCoverage, value ); }
				}
				public bool SampleAlphaToOne
				{
					get { return GL.IsEnabled ( EnableCap.SampleAlphaToOne ); }
					set { SetEnable ( EnableCap.SampleAlphaToOne, value ); }
				}
				public bool SampleCoverage
				{
					get { return GL.IsEnabled ( EnableCap.SampleCoverage ); }
					set { SetEnable ( EnableCap.SampleCoverage, value ); }
				}
				public bool SampleMask
				{
					get { return GL.IsEnabled ( EnableCap.SampleMask ); }
					set { SetEnable ( EnableCap.SampleMask, value ); }
				}

				public float SampleCoverageValue
				{
					get { return GL.GetFloat ( GetPName.SampleCoverageValue ); }
				}
				public bool SampleCoverageInvert
				{
					get { return GL.GetBoolean ( GetPName.SampleCoverageInvert ); }
				}

				public void SetSampleCoverage ( float value, bool invert )
				{
					GL.SampleCoverage ( value, invert );
				}

				public void SetSampleMask ( int maskNumber, int mask )
				{
					GL.SampleMask ( maskNumber, mask );
				}
			}

			private struct StencilTest
			{
				public bool Enable
				{
					get { return GL.IsEnabled ( EnableCap.StencilTest ); }
					set { SetEnable ( EnableCap.StencilTest, value ); }
				}

				public StencilFunction StencilFunc
				{
					get { return (StencilFunction)GL.GetInteger ( GetPName.StencilFunc ); }
				}
				public int StencilValueMask
				{
					get { return GL.GetInteger ( GetPName.StencilValueMask ); }
				}
				public int StencilReference
				{
					get { return GL.GetInteger ( GetPName.StencilRef ); }
				}
				public StencilFunction StencilBackFunc
				{
					get { return (StencilFunction)GL.GetInteger ( GetPName.StencilBackFunc ); }
				}
				public int StencilBackValueMask
				{
					get { return GL.GetInteger ( GetPName.StencilBackValueMask ); }
				}
				public int StencilBackReference
				{
					get { return GL.GetInteger ( GetPName.StencilBackRef ); }
				}

				public StencilOp StencilFail
				{
					get { return (StencilOp)GL.GetInteger ( GetPName.StencilFail ); }
				}
				public StencilOp StencilPassDepthPass
				{
					get { return (StencilOp)GL.GetInteger ( GetPName.StencilPassDepthPass ); }
				}
				public StencilOp StencilPassDepthFail
				{
					get { return (StencilOp)GL.GetInteger ( GetPName.StencilPassDepthFail ); }
				}
				public StencilOp StencilBackFail
				{
					get { return (StencilOp)GL.GetInteger ( GetPName.StencilBackFail ); }
				}
				public StencilOp StencilBackPassDepthPass
				{
					get { return (StencilOp)GL.GetInteger ( GetPName.StencilBackPassDepthPass ); }
				}
				public StencilOp StencilBackPassDepthFail
				{
					get { return (StencilOp)GL.GetInteger ( GetPName.StencilBackPassDepthFail ); }
				}


				public void StencilFunction ( StencilFunction func, int reference, int mask )
				{
					GL.StencilFunc ( func, reference, mask );
				}

				public void StencilFunction ( StencilFace face, StencilFunction func, int reference, int mask )
				{
					GL.StencilFuncSeparate ( face, func, reference, mask );
				}

				public void StencilOperation ( StencilOp fail, StencilOp zFail, StencilOp zPass )
				{
					GL.StencilOp ( fail, zFail, zPass );
				}

				public void StencilOperation ( StencilFace face, StencilOp fail, StencilOp zFail, StencilOp zPass )
				{
					GL.StencilOpSeparate ( face, fail, zFail, zPass );
				}
			}

			private struct DepthTest
			{
				public bool Enable
				{
					get { return GL.IsEnabled ( EnableCap.DepthTest ); }
					set { SetEnable ( EnableCap.DepthTest, value ); }
				}

				public DepthFunction DepthFunc
				{
					get { return (DepthFunction)GL.GetInteger ( GetPName.DepthFunc ); }
					set { GL.DepthFunc ( value ); }
				}
			}

			private struct Blending
			{
				public bool Enable
				{
					get { return GL.IsEnabled ( EnableCap.Blend ); }
					set { SetEnable ( EnableCap.Blend, value ); }
				}

				public BlendEquationMode BlendEquationRGB
				{
					get { return (BlendEquationMode)GL.GetInteger ( GetPName.BlendEquationRgb ); }
				}
				public BlendEquationMode BlendEquationAlpha
				{
					get { return (BlendEquationMode)GL.GetInteger ( GetPName.BlendEquationAlpha ); }
				}
				public BlendingFactorSrc BlendSrcRGB
				{
					get { return (BlendingFactorSrc)GL.GetInteger ( GetPName.BlendSrcRgb ); }
				}
				public BlendingFactorSrc BlendSrcAlpha
				{
					get { return (BlendingFactorSrc)GL.GetInteger ( GetPName.BlendSrcAlpha ); }
				}
				public BlendingFactorDest BlendDstRGB
				{
					get { return (BlendingFactorDest)GL.GetInteger ( GetPName.BlendDstRgb ); }
				}
				public BlendingFactorDest BlendDstAlpha
				{
					get { return (BlendingFactorDest)GL.GetInteger ( GetPName.BlendDstAlpha ); }
				}

				public unsafe CrowEngine.Mathematics.Color4 BlendColor
				{
					get
					{
						CrowEngine.Mathematics.Color4 c;
						GL.GetFloat ( GetPName.BlendColorExt, (float*)&c );
						return c;
					}
					set { GL.BlendColor ( value.Red, value.Green, value.Blue, value.Alpha ); }
				}

				public bool GetEnabled ( int index )
				{
					return GL.IsEnabled ( IndexedEnableCap.Blend, index );
				}

				public void SetEnabled ( int index, bool value )
				{
					if ( value ) GL.Enable ( IndexedEnableCap.Blend, index );
					else GL.Disable ( IndexedEnableCap.Blend, index );
				}

				public void BlendEquation ( BlendEquationMode mode )
				{
					GL.BlendEquation ( mode );
				}

				public void BlendEquation ( int index, BlendEquationMode mode )
				{
					GL.BlendEquation ( index, mode );
				}

				public void BlendEquation ( BlendEquationMode modeRGB, BlendEquationMode modeAlpha )
				{
					GL.BlendEquationSeparate ( modeRGB, modeAlpha );
				}

				public void BlendEquation ( int index, BlendEquationMode modeRGB, BlendEquationMode modeAlpha )
				{
					GL.BlendEquationSeparate ( index, modeRGB, modeAlpha );
				}

				public void BlendFunc ( BlendingFactorSrc src, BlendingFactorDest dst )
				{
					GL.BlendFunc ( src, dst );
				}

				public void BlendFunc ( int index, BlendingFactorSrc src, BlendingFactorDest dst )
				{
					GL.BlendFunc ( index, src, dst );
				}

				public void BlendFunc ( BlendingFactorSrc srcRGB, BlendingFactorDest dstRGB, BlendingFactorSrc srcAlpha, BlendingFactorDest dstAlpha )
				{
					GL.BlendFuncSeparate ( srcRGB, dstRGB, srcAlpha, dstAlpha );
				}

				public void BlendFunc ( int index, BlendingFactorSrc srcRGB, BlendingFactorDest dstRGB, BlendingFactorSrc srcAlpha, BlendingFactorDest dstAlpha )
				{
					GL.BlendFuncSeparate ( index, srcRGB, dstRGB, srcAlpha, dstAlpha );
				}
			}

			private struct Dithering
			{
				public bool Enable
				{
					get { return GL.IsEnabled ( EnableCap.Dither ); }
					set { SetEnable ( EnableCap.Dither, value ); }
				}
			}

			private struct ColorLogicalOperation
			{
				public bool Enable
				{
					get { return GL.IsEnabled ( EnableCap.ColorLogicOp ); }
					set { SetEnable ( EnableCap.ColorLogicOp, value ); }
				}

				public LogicOp LogicOperation
				{
					get { return (LogicOp)GL.GetInteger ( GetPName.LogicOpMode ); }
					set { GL.LogicOp ( value ); }
				}
			}
		}


		// todo create Rectangle with x, y, width and height
		public struct DepthRange
		{
			public double Near, Far;
		}
		public struct Rectangle
		{
			public int X, Y, Width, Height;
		}
		public struct RectangleF
		{
			public float X, Y, Width, Height;
		}
	}
}
