using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using CrowEngine;
using CrowEngine.Components;
using CrowEngine.Mathematics;

namespace CrowTestConsole
{
	class Program
	{
		class TestComp : Behavior, IActivatable
		{
			public void OnEnable ()
			{
			}

			public void OnDisable ()
			{
			}
		}

		[STAThread]
		static void Main ( string[] args )
		{
			//ProfileOptimization.SetProfileRoot ( @"C:\MyAppFolder" );
			ProfileOptimization.StartProfile ( "JITProfile" );

			Crow.Init ();

			var lib = new SharpFont.Library ();
			var v = lib.Version;

			var face = lib.NewFace ( "DejaVuSansMono.ttf", 0 );
			face.SetPixelSizes ( 50, 50 );

			var text = "hallo du!";

			Vector2 baseLine = new Vector2 ( 0, 50 );
			uint lastIdx = 0;
			for ( int i = 0; i < text.Length; i++ )
			{
				uint idx = face.GetCharIndex ( text[i] );

				if ( face.HasKerning && lastIdx != 0 && idx != 0 )
				{
					var kerning = face.GetKerning ( lastIdx, idx, SharpFont.KerningMode.Default );
					baseLine.X += (float)SharpFont.Fixed16Dot16.FromRawValue ( kerning.X );

					Console.WriteLine ( "kerning: " + kerning );
				}

				face.LoadGlyph ( idx, SharpFont.LoadFlags.NoBitmap, SharpFont.LoadTarget.Normal );

				//var glyphOffset = new Vector2 (
				//	baseLine.X + (float)SharpFont.Fixed16Dot16.FromRawValue ( glyph.Metrics.HorizontalBearingX ),
				//	baseLine.Y - (float)SharpFont.Fixed16Dot16.FromRawValue ( glyph.Metrics.HorizontalBearingY )
				//);

				var glyph = face.Glyph;

				glyph.RenderGlyph ( SharpFont.RenderMode.Normal );

				glyph = face.Glyph; // hack

				var advance = new Vector2 (
					(float)SharpFont.Fixed16Dot16.FromRawValue ( glyph.Advance.X ),
					(float)SharpFont.Fixed16Dot16.FromRawValue ( glyph.Advance.Y )
				);

				var img = glyph.Bitmap;

				if ( img.Rows > 0 && img.Width > 0 )
					using ( System.Drawing.Bitmap bit = new System.Drawing.Bitmap ( img.Width, img.Rows, System.Drawing.Imaging.PixelFormat.Format32bppArgb ) )
					{
						for ( int y = 0; y < img.Rows; y++ )
						{
							for ( int x = 0; x < img.Width; x++ )
							{
								byte pix;

								unsafe
								{
									pix = ((byte*)img.Buffer)[y * img.Width + x];
								}

								var color = bit.GetPixel ( x, y );
								color = System.Drawing.Color.FromArgb (
									255,
									Math.Max ( color.R, pix ),
									Math.Max ( color.G, pix ),
									Math.Max ( color.B, pix )
								);
								bit.SetPixel ( x, y, color );
							}
						}
						bit.Save ( text[i] + "_char.png", System.Drawing.Imaging.ImageFormat.Png );
					}


				baseLine += advance;
				lastIdx = idx;
			}



			var go = new GameObject ( "Root" );
			var trans = go.AddComponent<RectTransform> ();
			trans.AnchoredPosition = new Vector2 ( 600, 600 ) * 0.5f;
			trans.SizeDelta = new Vector2 ( 600, 600 );

			go.AddComponent<TestComp> ();

			var nodex = new GameObject ( "Child 5" );
			var child = nodex.AddComponent<Transform> ();
			child.Parent = trans;

			nodex = new GameObject ( "Child 6" );
			nodex.AddComponent<Transform> ();
			nodex.Transform.Parent = child;
			child = nodex.Transform;
			nodex = new GameObject ( "Child 7" );
			nodex.AddComponent<Transform> ();
			nodex.Transform.Parent = child;
			child = nodex.Transform;
			nodex = new GameObject ( "Child 8" );
			nodex.AddComponent<Transform> ();
			nodex.Transform.Parent = child;
			child = nodex.Transform;

			var node1 = new GameObject ( "Child 0" );
			var childTrans = node1.AddComponent<RectTransform> ();
			childTrans.Parent = trans;
			childTrans.AnchoredPosition = new Vector2 ( -150, 130 );
			childTrans.SizeDelta = new Vector2 ( 50, 50 );

			var node2 = new GameObject ( "Child 1" );
			var childTrans2 = node2.AddComponent<RectTransform> ();
			childTrans2.Parent = trans;
			childTrans2.AnchorMaximum = Vector2.One;
			childTrans2.AnchorMinimum = Vector2.Zero;
			//childTrans2.AnchoredPosition = new Vector2 ( -10, -10 );
			childTrans2.SizeDelta = new Vector2 ( -10, -10 );

			var rect = childTrans2.Rectangle;

			foreach ( var node in go.Transform.GetDeepEnumerable () )
			{
				Console.WriteLine ( new string ( ' ', node.Depth ) + node.GameObject.Name );
			}


			var scene = new Scene ();
			scene.AddGameObject ( go );

			foreach ( var item in scene.IterateComponents ( typeof ( IActivatable ) ) )
			{
				Console.WriteLine ( ((Component)item).GameObject.Name );
			}

			using ( var game = new Tutorial () )
			{
				game.Run ( 30, 30 );
			}
		}
	}
}
