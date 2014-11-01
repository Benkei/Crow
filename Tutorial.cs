using System;
using System.IO;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine
{
	class Tutorial : GameWindow
	{
		float time;
		int uniform_mview;
		GpuProgram m_Program;
		Matrix4 m_MeshViewModel = Matrix4.Identity;
		Mesh m_Mesh;

		public Tutorial ()
			: base ( 512, 512, new GraphicsMode ( 32, 24, 0, 4 ) )
		{
			Title = "Hello OpenTK!";

			var version = "GL " + GL.GetInteger ( GetPName.MajorVersion )
				+ "." + GL.GetInteger ( GetPName.MinorVersion );
			Console.WriteLine ( version );

			Console.WriteLine ( GL.GetString ( StringName.Vendor ) );
			Console.WriteLine ( GL.GetString ( StringName.Version ) );
			Console.WriteLine ( GL.GetString ( StringName.Renderer ) );
			//Console.WriteLine ( GL.GetString ( StringName.Extensions ) );
			Console.WriteLine ( GL.GetString ( StringName.ShadingLanguageVersion ) );
			Console.WriteLine ();
		}

		protected override void OnLoad ( EventArgs e )
		{
			base.OnLoad ( e );
			
			m_Program = GpuPrograms.GpuProgramFactory.Load ( "Assets/Simple.gfx" );

			uniform_mview = m_Program.GetUniformLocation ( "modelview" );

			if ( uniform_mview == -1 )
			{
				Console.WriteLine ( "Error binding attributes" );
			}

			m_Mesh = MeshPrimitive.CreateBox ();

			GL.ClearColor ( Color4.CornflowerBlue );
			GL.PointSize ( 5f );
		}

		protected override void OnUpdateFrame ( FrameEventArgs e )
		{
			base.OnUpdateFrame ( e );
			time += (float)e.Time;

			m_MeshViewModel = Matrix4.CreateRotationY ( 0.55f * time )
				* Matrix4.CreateRotationX ( 0.15f * time )
				* Matrix4.CreateTranslation ( 0.0f, 0.0f, -3.0f )
				* Matrix4.CreatePerspectiveFieldOfView ( 1.3f, ClientSize.Width / (float)ClientSize.Height, 1.0f, 40.0f );
		}

		protected override void OnRenderFrame ( FrameEventArgs e )
		{
			base.OnRenderFrame ( e );

			GL.Viewport ( 0, 0, Width, Height );
			GL.Clear ( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );
			GL.Enable ( EnableCap.DepthTest );

			m_Program.Use ();

			GL.UniformMatrix4 ( uniform_mview, false, ref m_MeshViewModel );
			
			m_Mesh.m_Ibo.Bind ();

			m_Mesh.m_Vao.Bind ();

			GL.DrawElements ( BeginMode.Triangles, m_Mesh.m_Indices, m_Mesh.m_Ibo.ElementType, 0 );


			GL.Flush ();
			SwapBuffers ();
		}
	}
}
