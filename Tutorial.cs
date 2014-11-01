using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine
{
	class Tutorial : GameWindow
	{
		int attribute_vcol;
		int attribute_vpos;
		int uniform_mview;

		GpuProgram m_Program;
		GpuBuffer m_VboPosition;
		GpuBuffer m_VboColor;
		GpuBuffer m_VboModelView;


		public Tutorial ()
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

			m_Program = new GpuProgram ();
			m_Program.LoadShader ( ShaderType.VertexShader, File.ReadAllText ( "vs.glsl", Encoding.ASCII ) );
			m_Program.LoadShader ( ShaderType.FragmentShader, File.ReadAllText ( "fs.glsl", Encoding.ASCII ) );

			m_Program.Link ();

			attribute_vpos = m_Program.GetAttributeLocation ( "vPosition" );
			attribute_vcol = m_Program.GetAttributeLocation ( "vColor" );
			uniform_mview = m_Program.GetUniformLocation ( "modelview" );

			if ( attribute_vpos == -1 || attribute_vcol == -1 || uniform_mview == -1 )
			{
				Console.WriteLine ( "Error binding attributes" );
			}

			var vertdata = new[] { 
				new Vector3 (-0.8f, -0.8f, 0f ),
                new Vector3 ( 0.8f, -0.8f, 0f ),
                new Vector3 ( 0f,  0.8f, 0f )
			};

			var coldata = new[] {
				new Vector3 ( 1f, 0f, 0f ),
                new Vector3 ( 0f, 0f, 1f ),
                new Vector3 ( 0f, 1f, 0f )
			};

			var mviewdata = new[] {
                Matrix4.Identity
            };


			m_VboPosition = new GpuBuffer ();
			m_VboPosition.Bind ( BufferTarget.ArrayBuffer );
			m_VboPosition.SetData (
				BufferTarget.ArrayBuffer,
				BufferUsageHint.StaticDraw,
				vertdata,
				vertdata.Length * Vector3.SizeInBytes );

			m_VboColor = new GpuBuffer ();
			m_VboColor.Bind ( BufferTarget.ArrayBuffer );
			m_VboColor.SetData (
				BufferTarget.ArrayBuffer,
				BufferUsageHint.StaticDraw,
				coldata,
				coldata.Length * Vector3.SizeInBytes );


			GL.ClearColor ( Color4.CornflowerBlue );
			//GL.PointSize ( 5f );
		}

		protected override void OnUpdateFrame ( FrameEventArgs e )
		{
			base.OnUpdateFrame ( e );
		}

		protected override void OnRenderFrame ( FrameEventArgs e )
		{
			base.OnRenderFrame ( e );

			GL.Viewport ( 0, 0, Width, Height );
			GL.Clear ( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );
			GL.Enable ( EnableCap.DepthTest );


			m_VboPosition.Bind ( BufferTarget.ArrayBuffer );
			GL.VertexAttribPointer ( attribute_vpos, 3, VertexAttribPointerType.Float, false, 0, 0 );
			
			m_VboColor.Bind ( BufferTarget.ArrayBuffer );
			GL.VertexAttribPointer ( attribute_vcol, 3, VertexAttribPointerType.Float, true, 0, 0 );

			var ma = Matrix4.Identity;
			GL.UniformMatrix4 ( uniform_mview, false, ref ma );


			GL.EnableVertexAttribArray ( attribute_vpos );
			GL.EnableVertexAttribArray ( attribute_vcol );


			GL.DrawArrays ( PrimitiveType.Triangles, 0, 3 );


			GL.DisableVertexAttribArray ( attribute_vpos );
			GL.DisableVertexAttribArray ( attribute_vcol );


			GL.Flush ();
			SwapBuffers ();
		}
	}
}
