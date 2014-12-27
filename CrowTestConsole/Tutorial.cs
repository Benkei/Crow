﻿//#define Multythread

using System;
using System.Diagnostics;
using System.Threading;
using CrowEngine.Components;
using CrowEngine.Components.UI;
using CrowEngine.Mathematics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Quaternion = CrowEngine.Mathematics.Quaternion;
using Vector4 = CrowEngine.Mathematics.Vector4;
using Vector3 = CrowEngine.Mathematics.Vector3;
using Vector2 = CrowEngine.Mathematics.Vector2;

namespace CrowEngine
{
	internal class Tutorial : GameWindow
	{
		private Thread m_RenderingThread;
		private Thread m_ResourceThread;
		private bool m_RunningThread = true;

		private float time;
		private int uniform_mview;
		private int uniform_diffuse;
		private GLProgram m_Program;
		private Texture2D m_Texture;

		private Camera m_MainCamera;

		private GameObject m_Camera;
		private GameObject m_Cube;

		private DebugProc m_DebugCallback;

		private Mesh m_QuadMesh;

		public Tutorial ()
			: base ( 512, 512,
			new GraphicsMode ( 32, 24, 0, 4 ),
			"Hello OpenTK!",
			GameWindowFlags.Default,
			DisplayDevice.Default,
			1, 0,
			GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug )
		{
			var version = "GL " + GL.GetInteger ( GetPName.MajorVersion )
				+ "." + GL.GetInteger ( GetPName.MinorVersion );
			Console.WriteLine ( version );

			Console.WriteLine ( GL.GetString ( StringName.Vendor ) );
			Console.WriteLine ( GL.GetString ( StringName.Version ) );
			Console.WriteLine ( GL.GetString ( StringName.Renderer ) );
			//Console.WriteLine ( GL.GetString ( StringName.Extensions ) );
			Console.WriteLine ( GL.GetString ( StringName.ShadingLanguageVersion ) );
			Console.WriteLine ();

			// cache callback
			m_DebugCallback = GLDebugCallback;

#if Multythread
			using ( var waitReady = new EventWaitHandle ( false, EventResetMode.ManualReset ) )
			{
				Context.MakeCurrent ( null );

				m_ResourceThread = new Thread ( ResourceThread );
				m_ResourceThread.Name = "Resource loader";
				m_ResourceThread.IsBackground = true;
				m_ResourceThread.Start ( waitReady );

				waitReady.WaitOne ();
				waitReady.Reset ();

				m_RenderingThread = new Thread ( RenderingThread );
				m_RenderingThread.Name = "Renderer";
				m_RenderingThread.IsBackground = true;
				m_RenderingThread.Start ( waitReady );

				waitReady.WaitOne ();
			}
			Debug.WriteLine ( "All thread started" );
#endif
		}

		private void GLDebugCallback ( DebugSource source, DebugType type,
			int id, DebugSeverity severity,
			int length, IntPtr message,
			IntPtr userParam )
		{
			var mes = System.Runtime.InteropServices.Marshal.PtrToStringAnsi ( message, length );
			Console.WriteLine ( "ID: {0};\nSource: {1};\nType: {2};\nSeverity: {3};\n  {4}", id, source, type, severity, mes );
		}

		protected unsafe override void OnLoad ( EventArgs e )
		{
			base.OnLoad ( e );

			m_Camera = new GameObject ();
			m_Camera.AddComponent<Transform> ();
			m_MainCamera = m_Camera.AddComponent<Camera> ();
			m_MainCamera.FieldOfView = 60;
			m_MainCamera.FarClipPlane = 40f;
			m_MainCamera.NearClipPlane = 1f;
			m_MainCamera.PixelScreenSize = new Rectangle ( 0, 0, ClientSize.Width, ClientSize.Height );

			m_Cube = new GameObject ();
			m_Cube.AddComponent<Transform> ();
			m_Cube.Transform.Position = new Vector3 ( 0.0f, 0.0f, 3.0f );
			m_Cube.AddComponent<MeshRenderer> ();

			// [0]---[1]
			//  |     |
			//  |     |
			// [3]---[2]
			Vertex* v = stackalloc Vertex[8];
			v[0] = Vertex.Default;
			v[1] = Vertex.Default;
			v[2] = Vertex.Default;
			v[3] = Vertex.Default;
			v[4] = Vertex.Default;
			v[5] = Vertex.Default;
			v[6] = Vertex.Default;
			v[7] = Vertex.Default;

			v[0].Position = new Vector3 ( 1.5f, 40, 0 );
			v[1].Position = new Vector3 ( 40, 20, 0 );
			v[2].Position = new Vector3 ( 40, 1.5f, 0 );
			v[3].Position = new Vector3 ( 1.5f, 1.5f, 0 );

			v[4].Position = new Vector3 ( 1.5f, 80f, 0 );
			v[5].Position = new Vector3 ( 40f, 80f, 0 );
			v[6].Position = new Vector3 ( 40f, 60f, 0 );
			v[7].Position = new Vector3 ( 1.5f, 60f, 0 );

			v[0].Color = new Color ( 255, 0, 0, 255 );
			v[1].Color = new Color ( 255, 0, 0, 255 );
			v[2].Color = new Color ( 0, 0, 0, 255 );
			v[3].Color = new Color ( 0, 0, 0, 255 );

			v[4].Color = new Color ( 0, 0, 0, 255 );
			v[5].Color = new Color ( 0, 0, 0, 255 );
			v[6].Color = new Color ( 0, 0, 0, 255 );
			v[7].Color = new Color ( 0, 0, 0, 255 );

			//ushort* i = stackalloc ushort[4];
			//i[0] = 0;
			//i[1] = 1;
			//i[2] = 2;
			//i[3] = 3;

			m_QuadMesh = new Mesh ();

			m_QuadMesh.m_Indices = 8;
			//m_QuadMesh.m_Ibo = new IndicesBuffer ( DrawElementsType.UnsignedShort );
			//m_QuadMesh.m_Ibo.Bind ();
			//m_QuadMesh.m_Ibo.Setup ( 4 * sizeof ( ushort ), (IntPtr)i, BufferUsageHint.StaticDraw );

			m_QuadMesh.m_Vbo = new GLBuffer ( BufferTarget.ArrayBuffer );
			m_QuadMesh.m_Vbo.Bind ();
			m_QuadMesh.m_Vbo.Setup ( 8 * SizeOf<Vertex>.Value, (IntPtr)v, BufferUsageHint.StaticDraw );

			// bind VBO to VAO
			#region VAO
			m_QuadMesh.m_Vao = VertexArrayObject.Create ();
			m_QuadMesh.m_Vao.Bind ();

			int offset = 0;
			// Position; 3 float
			GL.EnableVertexAttribArray ( (int)VertexShaderSemanticInput.POSITION );
			GL.VertexAttribPointer ( (int)VertexShaderSemanticInput.POSITION,
				3,
				VertexAttribPointerType.Float,
				false,
				SizeOf<Vertex>.Value, offset );
			offset += SizeOf<Vector3>.Value;
			// Normal; 3 float
			GL.EnableVertexAttribArray ( (int)VertexShaderSemanticInput.NORMAL );
			GL.VertexAttribPointer ( (int)VertexShaderSemanticInput.NORMAL,
				3,
				VertexAttribPointerType.Float,
				false,
				SizeOf<Vertex>.Value, offset );
			offset += SizeOf<Vector3>.Value;
			// Color; 4 byte
			GL.EnableVertexAttribArray ( (int)VertexShaderSemanticInput.COLOR );
			GL.VertexAttribPointer ( (int)VertexShaderSemanticInput.COLOR,
				4,
				VertexAttribPointerType.UnsignedByte,
				true,
				SizeOf<Vertex>.Value, offset );
			offset += SizeOf<Color>.Value;
			// UV0; 2 float
			GL.EnableVertexAttribArray ( (int)VertexShaderSemanticInput.TEXCOORD0 );
			GL.VertexAttribPointer ( (int)VertexShaderSemanticInput.TEXCOORD0,
				2,
				VertexAttribPointerType.Float,
				false,
				SizeOf<Vertex>.Value, offset );
			offset += SizeOf<Vector2>.Value;
			// UV1; 2 float
			GL.EnableVertexAttribArray ( (int)VertexShaderSemanticInput.TEXCOORD1 );
			GL.VertexAttribPointer ( (int)VertexShaderSemanticInput.TEXCOORD1,
				2,
				VertexAttribPointerType.Float,
				false,
				SizeOf<Vertex>.Value, offset );
			offset += SizeOf<Vector2>.Value;
			// Tangent; 4 float
			GL.EnableVertexAttribArray ( (int)VertexShaderSemanticInput.TANGENT );
			GL.VertexAttribPointer ( (int)VertexShaderSemanticInput.TANGENT,
				4,
				VertexAttribPointerType.Float,
				false,
				SizeOf<Vertex>.Value, offset );
			offset += SizeOf<Vector4>.Value;

			GL.BindVertexArray ( 0 );
			#endregion

#if !Multythread
			GL.Enable ( EnableCap.DebugOutput );
			GL.DebugMessageCallback ( m_DebugCallback, IntPtr.Zero );

			VSync = VSyncMode.Adaptive;

			GL.Enable ( EnableCap.DepthTest );
			GL.ClearColor ( OpenTK.Graphics.Color4.CornflowerBlue );

			LoadAssets ();
#endif
		}

		protected override void OnUnload ( EventArgs e )
		{
			m_RunningThread = false;

#if Multythread
			m_RenderingThread.Join ( 1000 * 15 );
			m_ResourceThread.Join ( 1000 * 15 );
#endif

			base.OnUnload ( e );
		}

		protected override void OnUpdateFrame ( FrameEventArgs e )
		{
			base.OnUpdateFrame ( e );
			time += (float)e.Time;

			m_Cube.Transform.Rotation = Quaternion.RotationAxis ( Vector3.UnitY, 0.55f * time )
				* Quaternion.RotationAxis ( Vector3.UnitX, 0.15f * time );
		}

		protected override void OnRenderFrame ( FrameEventArgs e )
		{
			base.OnRenderFrame ( e );
#if !Multythread
			Render ( e.Time );
			SwapBuffers ();
#endif
		}

		private void ResourceThread ( object obj )
		{
			var waitHandler = (EventWaitHandle)obj;

			Debug.WriteLine ( "Start resource loader thread" );
			Debug.WriteLine ( "Create shared context" );

			var window = new OpenTK.NativeWindow ();
			var context = new GraphicsContext (
				new GraphicsMode ( ColorFormat.Empty, 0, 0, 0, ColorFormat.Empty, 0, false ),// GraphicsMode.Default,
				window.WindowInfo,
				1, 0,
				GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug );
			context.MakeCurrent ( window.WindowInfo );

			waitHandler.Set ();
			waitHandler = null;
			obj = null;

			GL.Enable ( EnableCap.DebugOutput );
			GL.DebugMessageCallback ( m_DebugCallback, IntPtr.Zero );

			using ( window )
			using ( context )
			{
				LoadAssets ();

				while ( m_RunningThread )
				{
					window.ProcessEvents ();

					GL.Flush ();

					Thread.Sleep ( 5 );
				}
			}
		}

		private void RenderingThread ( object obj )
		{
			var waitHandler = (EventWaitHandle)obj;

			Debug.WriteLine ( "Start rendering thread" );

			MakeCurrent ();

			waitHandler.Set ();
			waitHandler = null;
			obj = null;

			var renderWatch = new Stopwatch ();
			renderWatch.Start ();

			VSync = VSyncMode.On;

			GL.Enable ( EnableCap.DebugOutput );
			GL.DebugMessageCallback ( m_DebugCallback, IntPtr.Zero );

			GL.Enable ( EnableCap.DepthTest );
			GL.ClearColor ( OpenTK.Graphics.Color4.CornflowerBlue );

			while ( m_RunningThread )
			{
				Render ( renderWatch.Elapsed.TotalSeconds );
				renderWatch.Restart ();

				SwapBuffers ();

				Thread.Yield ();
			}

			Context.MakeCurrent ( null );
		}

		private unsafe void Render ( double elapsed )
		{
			GL.Viewport ( 0, 0, Width, Height );
			GL.Clear ( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

			if ( m_Program != null && m_Cube != null && m_Texture != null )
			{
				var mesh = m_Cube.GetComponent<MeshRenderer> ().Mesh;
				if ( mesh != null )
				{
					m_Program.Use ();

					var matrix = m_Cube.Transform.WorldMatrix * m_MainCamera.ViewMatrix * m_MainCamera.ProjectionMatrix;

					//GL.UniformMatrix4 ( uniform_mview, 1, false, (float*)&matrix );
					GL.Uniform1 ( uniform_diffuse, 0 );
					//GL.Uniform1()

					GL.ActiveTexture ( TextureUnit.Texture0 + 0 );
					GL.BindTexture ( TextureTarget.Texture2D, m_Texture.Handler );

					//mesh.CheckVao ();
					//mesh.m_Vao.Bind ();

					//mesh.m_Ibo.Bind ();

					//GL.DrawElements ( BeginMode.Triangles, mesh.m_Indices, mesh.m_Ibo.ElementType, 0 );

					//GL.Disable ( EnableCap.DepthTest );
					//GL.Disable ( EnableCap.CullFace );
					//Matrix.OrthoLH ( Width, Height, 0, 100, out matrix );
					Matrix.OrthoOffCenterLH ( 0, Width, 0, Height, 0, 100, out matrix );
					//matrix = Matrix.Identity;
					GL.UniformMatrix4 ( uniform_mview, 1, false, (float*)&matrix );

					m_QuadMesh.m_Vao.Bind ();
					//m_QuadMesh.m_Ibo.Bind ();

					//GL.DrawElements ( BeginMode.Quads, m_QuadMesh.m_Indices, m_QuadMesh.m_Ibo.ElementType, 0 );
					GL.DrawArrays ( PrimitiveType.Quads, 0, m_QuadMesh.m_Indices );

					//var muh = new RenderContext ();
				}
			}
		}

		private void LoadAssets ()
		{
			var muh = new CrowEditor.AssetProcessors.TextureProcessor ();
			//muh.Setup ( "Assets/guid.jpg", "Assets/guid.meta" );
			//muh.Setup ( "Assets/that_girl.tif", "Assets/that_girl.meta" );
			//muh.Setup ( "Assets/test_0.jpg", "Assets/test_0.meta" );
			muh.Setup ( "Assets/lena.tiff", "Assets/lena.meta" );
			//muh.Setup ( "Assets/test_2.jpg", "Assets/test_2.meta" );
			muh.NearPowerOfTwo = true;
			muh.Execute ();
			m_Texture = muh.tex;

			//m_Texture = TextureFactory.Load ( "Assets/guid.JPG" );

			m_Program = GpuPrograms.GpuProgramFactory.Load ( "Assets/Simple.gfx" );

			uniform_mview = m_Program.GetUniformLocation ( "modelview" );
			uniform_diffuse = m_Program.GetUniformLocation ( "diffuse" );

			if ( uniform_mview == -1 )
			{
				Console.WriteLine ( "Error binding attributes" );
			}

			var mesh = MeshPrimitive.CreateBox ();

			var rendere = m_Cube.GetComponent<MeshRenderer> ();
			rendere.Mesh = mesh;
		}
	}

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