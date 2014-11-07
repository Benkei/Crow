using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine
{
	class Tutorial : GameWindow
	{
		Thread m_RenderingThread;
		Thread m_ResourceThread;
		bool m_RunningThread = true;

		float time;
		int uniform_mview;
		int uniform_diffuse;
		GLProgram m_Program;
		Mesh m_Mesh;
		Texture2D m_Texture;

		Model m_Cube;

		DebugProc m_DebugCallback;

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
		}

		void GLDebugCallback ( DebugSource source, DebugType type,
			int id, DebugSeverity severity,
			int length, IntPtr message,
			IntPtr userParam )
		{
			var mes = System.Runtime.InteropServices.Marshal.PtrToStringAnsi ( message, length );
			Console.WriteLine ( "ID: {0};\nSource: {1};\nType: {2};\nSeverity: {3};\n  {4}", id, source, type, severity, mes );
		}

		protected override void OnUnload ( EventArgs e )
		{
			m_RunningThread = false;

			m_RenderingThread.Join ( 1000 * 15 );
			m_ResourceThread.Join ( 1000 * 15 );

			base.OnUnload ( e );
		}

		protected override void OnUpdateFrame ( FrameEventArgs e )
		{
			base.OnUpdateFrame ( e );
			time += (float)e.Time;

			if ( m_Cube != null )
			{
				m_Cube.Rotation = SharpDX.Quaternion.RotationAxis ( SharpDX.Vector3.UnitY, 0.55f * time )
					* SharpDX.Quaternion.RotationAxis ( SharpDX.Vector3.UnitX, 0.15f * time );
			}
		}

		void ResourceThread ( object obj )
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

		void RenderingThread ( object obj )
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
			GL.ClearColor ( Color4.CornflowerBlue );

			while ( m_RunningThread )
			{
				Render ( renderWatch.Elapsed.TotalSeconds );
				renderWatch.Restart ();

				SwapBuffers ();

				Thread.Yield ();
			}

			Context.MakeCurrent ( null );
		}

		unsafe void Render ( double elapsed )
		{
			GL.Viewport ( 0, 0, Width, Height );
			GL.Clear ( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

			if ( m_Program != null && m_Cube != null && m_Texture != null && m_Mesh != null )
			{
				m_Program.Use ();

				var matrix = m_Cube.WorldMatrix;
				matrix *= SharpDX.Matrix.PerspectiveFovRH ( 1.3f, ClientSize.Width / (float)ClientSize.Height, 1.0f, 40.0f );

				GL.UniformMatrix4 ( uniform_mview, 1, false, (float*)&matrix );
				GL.Uniform1 ( uniform_diffuse, 0 );

				GL.ActiveTexture ( TextureUnit.Texture0 + 0 );
				GL.BindTexture ( TextureTarget.Texture2D, m_Texture.Handler );

				m_Mesh.m_Ibo.Bind ();

				m_Mesh.CheckVao ();
				m_Mesh.m_Vao.Bind ();

				GL.DrawElements ( BeginMode.Triangles, m_Mesh.m_Indices, m_Mesh.m_Ibo.ElementType, 0 );
			}
		}

		private void LoadAssets ()
		{
			m_Texture = TextureFactory.Load ( "Assets/guid.JPG" );

			m_Program = GpuPrograms.GpuProgramFactory.Load ( "Assets/Simple.gfx" );

			uniform_mview = m_Program.GetUniformLocation ( "modelview" );
			uniform_diffuse = m_Program.GetUniformLocation ( "diffuse" );

			if ( uniform_mview == -1 )
			{
				Console.WriteLine ( "Error binding attributes" );
			}

			m_Mesh = MeshPrimitive.CreateBox ();


			m_Cube = new Model ();
			m_Cube.Position = new SharpDX.Vector3 ( 0.0f, 0.0f, -3.0f );
		}
	}

	struct Rasterization
	{
		public bool RASTERIZER_DISCARD;
		public bool MULTISAMPLE;
		public bool SAMPLE_SHADING;

		public Rasterization ( int x )
			: this ()
		{
			float point;
			GL.GetMultisample ( GetMultisamplePName.SamplePosition, 0, out point );

			//GL.MinSampleShading()
		}

		struct Point
		{
			public float PointSize;

			public Point ( int x )
				: this ()
			{
				GL.Enable ( EnableCap.VertexProgramPointSize );

				GL.PointParameter ( PointParameterName.PointDistanceAttenuation, 0f );
			}
		}

		struct Line
		{
			public float LineWidth;
			public bool LineSmooth;

			public Line ( int x )
				: this ()
			{
				//GL.Enable ( EnableCap.LineSmooth );
			}
		}

		struct Polygon
		{
			public bool PolygonSmooth;
			public bool CullFace;
			public FrontFaceDirection Dir;
			public CullFaceMode CullMode;

			public bool PolygonOffsetFill;
			public bool PolygonOffsetLine;
			public bool PolygonOffsetPoint;

			public Polygon ( int x )
				: this ()
			{
				//GL.Enable ( EnableCap.CullFace );
				//GL.FrontFace ( FrontFaceDirection.Cw );
				//GL.CullFace ( CullFaceMode.Front );
				//GL.Enable ( EnableCap.PolygonOffsetPoint );
				//GL.PolygonOffset()
			}
		}
	}
}
