using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrowEditor.UIForms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Platform;

namespace CrowEditor
{
	static class CrowEditorApp
	{
		struct RenderView : IEquatable<RenderView>
		{
			public CrowGLControl Control;
			public GLRenderView View;

			public RenderView ( CrowGLControl control, GLRenderView view )
			{
				Control = control;
				View = view;
			}

			public bool Equals ( RenderView other )
			{
				return Control == other.Control && View == other.View;
			}
		}

		static GraphicsContext m_MainGlContext;
		static HashSet<IWindowInfo> m_SwapBuffers = new HashSet<IWindowInfo> ();
		static List<RenderView> m_GLRenderViews = new List<RenderView> ();

		public static void Init ()
		{
			var mode = GraphicsMode.Default;//new GraphicsMode ( ColorFormat.Empty, 0, 0, 0, ColorFormat.Empty, 0, false );
			var window = new OpenTK.NativeWindow ();
			m_MainGlContext = new GraphicsContext ( mode,
				window.WindowInfo,
				1, 0,
				GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug );
		}

		public static void RegisterRenderView ( GLRenderView view, CrowGLControl control )
		{
			lock ( m_GLRenderViews )
			{
				if ( !m_GLRenderViews.Contains ( new RenderView ( control, view ) ) )
				{
					m_GLRenderViews.Add ( new RenderView ( control, view ) );
				}
			}
		}

		public static void GLMakeCurrent ( GLControl control )
		{
			m_MainGlContext.MakeCurrent ( control == null ? null : control.WindowInfo );
		}
		public static void GLSwapBuffers ( GLControl control )
		{
			lock ( m_SwapBuffers )
				m_SwapBuffers.Add ( control.WindowInfo );
		}
		public static void GLUpdateBufferSwap ()
		{
			// Perform a buffer swap
			if ( m_SwapBuffers.Count > 0 )
			{
				lock ( m_SwapBuffers )
				{
					foreach ( var window in m_SwapBuffers )
					{
						m_MainGlContext.MakeCurrent ( window );
						m_MainGlContext.SwapBuffers ();
					}
					m_SwapBuffers.Clear ();
				}
			}
		}
		public static void UpdateRenderView ()
		{
			// Perform rendering
			if ( m_GLRenderViews.Count > 0 )
			{
				lock ( m_GLRenderViews )
				{
					for ( int i = m_GLRenderViews.Count - 1; i >= 0; i-- )
					{
						var renderView = m_GLRenderViews[i];

						lock ( renderView.Control )
						{
							if ( !renderView.Control.IsReady ) continue;
							if ( renderView.Control._Context.IsDisposed ) continue;
							if ( !renderView.Control.IsIdle ) continue;
							m_GLRenderViews.RemoveAt ( i );

							try
							{
								m_MainGlContext.MakeCurrent ( renderView.Control._WindowInfo );
								renderView.View.Render ();
								m_MainGlContext.SwapBuffers ();
								m_MainGlContext.MakeCurrent ( null );
							}
							catch ( OpenTK.Graphics.GraphicsContextException ex )
							{
								Console.WriteLine ( ex.ToString () );
							}
						}
					}
					m_SwapBuffers.Clear ();
				}
			}
		}

	}

}
