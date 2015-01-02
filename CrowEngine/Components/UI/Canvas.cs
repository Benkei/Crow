using System;
using CrowEngine.Components;
using CrowEngine.Mathematics;

namespace CrowEngine.Components.UI
{
	public class Canvas : Behavior
	{
		public bool IsRootCanvas
		{
			get
			{
				Transform current = Transform.Parent;
				while ( current != null )
				{
					if ( current.GameObject.GetComponent<Canvas> () != null )
						return false;
					current = current.Parent;
				}
				return true;
			}
		}

		public Canvas RootCanvas
		{
			get
			{
				Canvas canvas;
				Canvas lastCanvas = null;
				Transform current = Transform;
				while ( current != null )
				{
					canvas = current.GameObject.GetComponent<Canvas> ();
					if ( canvas != null )
						lastCanvas = canvas;
					current = current.Parent;
				}
				return lastCanvas;
			}
		}

		/// <summary>
		/// Override the sorting of canvas.
		/// </summary>
		public bool OverrideSorting
		{
			get;
			set;
		}

		/// <summary>
		/// Get the render rect for the Canvas.
		/// </summary>
		public Rectangle PixelRect
		{
			get { return new Rectangle (); }
		}

		/// <summary>
		/// How far away from the camera is the Canvas generated.
		/// </summary>
		public float PlaneDistance
		{
			get;
			set;
		}

		/// <summary>
		/// The number of pixels per unit that is considered the default.
		/// </summary>
		public float ReferencePixelsPerUnit
		{
			get;
			set;
		}

		/// <summary>
		/// Is the Canvas in World or Overlay mode?
		/// </summary>
		public RenderMode RenderMode
		{
			get;
			set;
		}

		/// <summary>
		/// Used to scale the entire canvas, while still making it fit the screen.
		/// Only applies with renderMode is Screen Space.
		/// </summary>
		public float ScaleFactor
		{
			get;
			set;
		}

		public int SortingLayerID
		{
			get;
			set;
		}

		public string SortingLayerName
		{
			get;
			set;
		}

		public int SortingOrder
		{
			get;
			set;
		}

		public Camera WorldCamera
		{
			get;
			set;
		}

		public void ForceUpdateCanvases ()
		{
		}
	}
}