﻿using System;
using CrowEngine.Components;

namespace CrowEngine.Components.UI
{
	/*
	public class Canvas : Behavior
	{
		public delegate void WillRenderCanvases ();

		public static event Canvas.WillRenderCanvases willRenderCanvases
		{
			[MethodImpl ( MethodImplOptions.Synchronized )]
			add
			{
				Canvas.willRenderCanvases = (Canvas.WillRenderCanvases)Delegate.Combine ( Canvas.willRenderCanvases, value );
			}
			[MethodImpl ( MethodImplOptions.Synchronized )]
			remove
			{
				Canvas.willRenderCanvases = (Canvas.WillRenderCanvases)Delegate.Remove ( Canvas.willRenderCanvases, value );
			}
		}

		public extern bool isRootCanvas
		{
			get;
		}

		public extern bool overridePixelPerfect
		{
			get;

			set;
		}

		public extern bool overrideSorting
		{
			get;

			set;
		}

		public extern bool pixelPerfect
		{
			get;

			set;
		}

		//public Rect pixelRect
		//{
		//	get
		//	{
		//		Rect result;
		//		this.INTERNAL_get_pixelRect ( out result );
		//		return result;
		//	}
		//}

		public extern float planeDistance
		{
			get;

			set;
		}

		public extern float referencePixelsPerUnit
		{
			get;

			set;
		}

		public extern RenderMode renderMode
		{
			get;

			set;
		}

		public extern float scaleFactor
		{
			get;

			set;
		}

		public extern int sortingLayerID
		{
			get;

			set;
		}

		public extern string sortingLayerName
		{
			get;

			set;
		}

		public extern int sortingOrder
		{
			get;

			set;
		}

		public extern Camera worldCamera
		{
			get;

			set;
		}

		public static void ForceUpdateCanvases ()
		{
			Canvas.SendWillRenderCanvases ();
		}

		private static void SendWillRenderCanvases ()
		{
			//if ( Canvas.willRenderCanvases != null )
			//{
			//	Canvas.willRenderCanvases ();
			//}
		}

		//private extern void INTERNAL_get_pixelRect ( out Rect value );
	}
	*/
}