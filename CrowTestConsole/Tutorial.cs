//#define Multythread

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

					int v = 0;
					m_Program.SetValue ( "diffuse", ref v );
					//m_Program.SetValue ( "modelview", ref matrix );

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
					m_Program.SetValue ( "modelview", ref matrix );

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

			var mesh = MeshPrimitive.CreateBox ();

			var rendere = m_Cube.GetComponent<MeshRenderer> ();
			rendere.Mesh = mesh;
		}
	}
}