using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using Gwen.Control;

namespace Gwen.Sample.OpenTK
{
	/// <summary>
	/// Demonstrates the GameWindow class.
	/// </summary>
	public class SimpleWindow
	{
		private Gwen.Input.OpenTK input;
		private Gwen.Renderer.OpenTK renderer;
		private Gwen.Skin.Base skin;
		private Gwen.Control.Canvas canvas;
		private UnitTest.UnitTest test;

		const int fps_frames = 50;

		private readonly List<long> ftime;
		private readonly Stopwatch stopwatch;
		private long lastTime;
		private bool altDown = false;

		public SimpleWindow ()
		//: base ( 1024, 768 )
		{
			//Keyboard.KeyDown += Keyboard_KeyDown;
			//Keyboard.KeyUp += Keyboard_KeyUp;

			//Mouse.ButtonDown += Mouse_ButtonDown;
			//Mouse.ButtonUp += Mouse_ButtonUp;
			//Mouse.Move += Mouse_Move;
			//Mouse.WheelChanged += Mouse_Wheel;

			ftime = new List<long> ( fps_frames );
			stopwatch = new Stopwatch ();
		}

		public void Dispose ()
		{
			canvas.Dispose ();
			skin.Dispose ();
			renderer.Dispose ();
		}

		/*
		/// <summary>
		/// Occurs when a key is pressed.
		/// </summary>
		/// <param name="sender">The KeyboardDevice which generated this event.</param>
		/// <param name="e">The key that was pressed.</param>
		void Keyboard_KeyDown ( object sender, KeyboardKeyEventArgs e )
		{
			if ( e.Key == global::OpenTK.Input.Key.Escape )
				Exit ();
			//else if ( e.Key == global::OpenTK.Input.Key.AltLeft )
			//	altDown = true;
			//else if ( altDown && e.Key == global::OpenTK.Input.Key.Enter )
			//	if ( WindowState == WindowState.Fullscreen )
			//		WindowState = WindowState.Normal;
			//	else
			//		WindowState = WindowState.Fullscreen;

			input.ProcessKeyDown ( e );
		}

		void Keyboard_KeyUp ( object sender, KeyboardKeyEventArgs e )
		{
			altDown = false;
			input.ProcessKeyUp ( e );
		}

		void Mouse_ButtonDown ( object sender, MouseButtonEventArgs args )
		{
			input.ProcessMouseMessage ( args );
		}

		void Mouse_ButtonUp ( object sender, MouseButtonEventArgs args )
		{
			input.ProcessMouseMessage ( args );
		}

		void Mouse_Move ( object sender, MouseMoveEventArgs args )
		{
			input.ProcessMouseMessage ( args );
		}

		void Mouse_Wheel ( object sender, MouseWheelEventArgs args )
		{
			input.ProcessMouseMessage ( args );
		}
		*/

		public void OnLoad ( int Width, int Height )
		{
			renderer = new Gwen.Renderer.OpenTK ();
			skin = new Gwen.Skin.TexturedBase ( renderer, "DefaultSkin.png" );
			//skin = new Gwen.Skin.Simple(renderer);
			//skin.DefaultFont = new Font(renderer, "Courier", 10);
			canvas = new Canvas ( skin );

			//input = new Input.OpenTK ( this );
			//input.Initialize ( canvas );

			canvas.SetSize ( Width, Height );
			canvas.ShouldDrawBackground = false;
			//canvas.BackgroundColor = Color.FromArgb ( 255, 150, 170, 170 );
			//canvas.KeyboardInputEnabled = true;

			test = new UnitTest.UnitTest ( canvas );

			stopwatch.Restart ();
			lastTime = 0;
		}

		public void OnResize ( int Width, int Height )
		{
			canvas.SetSize ( Width, Height );
		}

		public void OnUpdateFrame ()
		{
			if ( ftime.Count == fps_frames )
				ftime.RemoveAt ( 0 );

			ftime.Add ( stopwatch.ElapsedMilliseconds - lastTime );
			lastTime = stopwatch.ElapsedMilliseconds;

			if ( stopwatch.ElapsedMilliseconds > 1000 )
			{
				test.Note = String.Format ( "String Cache size: {0} Draw Calls: {1} Vertex Count: {2}", renderer.TextCacheSize, renderer.DrawCallCount, renderer.VertexCount );
				test.Fps = 1000f * ftime.Count / ftime.Sum ();
				stopwatch.Restart ();

				if ( renderer.TextCacheSize > 1000 ) // each cached string is an allocated texture, flush the cache once in a while in your real project
					renderer.FlushTextCache ();
			}
		}

		public void OnRenderFrame ( int Width, int Height )
		{
			GL.MatrixMode ( MatrixMode.Projection );
			GL.LoadIdentity ();
			GL.Ortho ( 0, Width, Height, 0, -1, 1 );

			canvas.RenderCanvas ();
		}

		/// <summary>
		/// Entry point of this example.
		/// </summary>
		//[STAThread]
		//public static void Main()
		//{
		//	using (SimpleWindow example = new SimpleWindow())
		//	{
		//		example.Title = "Gwen-DotNet OpenTK test";
		//		example.VSync = VSyncMode.Off; // to measure performance
		//		example.Run(0.0, 0.0);
		//		//example.TargetRenderFrequency = 60;
		//	}
		//}
	}
}
