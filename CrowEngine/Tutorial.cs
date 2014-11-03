﻿using System;
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
		int uniform_diffuse;
		GLProgram m_Program;
		Mesh m_Mesh;
		Texture2D m_Texture;

		Model m_Cube;

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

			GL.ClearColor ( Color4.CornflowerBlue );
			GL.PointSize ( 5f );
		}

		protected override void OnUpdateFrame ( FrameEventArgs e )
		{
			base.OnUpdateFrame ( e );
			time += (float)e.Time;

			m_Cube.Rotation = SharpDX.Quaternion.RotationAxis ( SharpDX.Vector3.UnitY, 0.55f * time )
				* SharpDX.Quaternion.RotationAxis ( SharpDX.Vector3.UnitX, 0.15f * time );
		}

		protected override unsafe void OnRenderFrame ( FrameEventArgs e )
		{
			base.OnRenderFrame ( e );

			GL.Viewport ( 0, 0, Width, Height );
			GL.Clear ( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );
			GL.Enable ( EnableCap.DepthTest );
			
			m_Program.Use ();

			var matrix = m_Cube.m_LocalMatrix;
			matrix *= SharpDX.Matrix.PerspectiveFovRH ( 1.3f, ClientSize.Width / (float)ClientSize.Height, 1.0f, 40.0f );

			GL.UniformMatrix4 ( uniform_mview, 1, false, (float*)&matrix );
			GL.Uniform1 ( uniform_diffuse, 0 );

			GL.ActiveTexture ( TextureUnit.Texture0 + 0 );
			GL.BindTexture ( TextureTarget.Texture2D, m_Texture.Handler );

			m_Mesh.m_Ibo.Bind ();

			m_Mesh.m_Vao.Bind ();

			GL.DrawElements ( BeginMode.Triangles, m_Mesh.m_Indices, m_Mesh.m_Ibo.ElementType, 0 );


			GL.Flush ();
			SwapBuffers ();
		}
	}
}