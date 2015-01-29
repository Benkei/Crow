using System;
using System.Collections.Generic;
using CrowEngine.Collections;
using CrowEngine.Mathematics;
using CrowEngine.Pooling;

namespace CrowEngine.Components.UI
{
	public abstract class Graphic : Component, IActivatable
	{
		protected static Material s_DefaultUI;
		protected static Texture2D s_WhiteTexture;

		protected Material m_Material;

		private static readonly ObjectPool<Vector<Vertex>> s_VboPool = new ObjectPool<Vector<Vertex>> (
					() => new Vector<Vertex> ( 256 ),
					null,
					( x ) => { x.Clear (); x.Capacity = Math.Min ( x.Capacity, 256 ); }
					);

		private Canvas m_Canvas;
		private RectTransform m_RectTransform;
		private CanvasRenderer m_Renderer;
		private Color m_Color = Color.White;
		private bool m_MaterialDirty;
		private bool m_VertsDirty;

		/// <summary>
		/// Default material used to draw everything if no explicit material was specified.
		/// </summary>
		public static Material defaultGraphicMaterial
		{
			get
			{
				//if ( s_DefaultUI == null )
				//	s_DefaultUI = Canvas.GetDefaultCanvasMaterial ();
				return s_DefaultUI;
			}
		}

		public new RectTransform Transform
		{
			get { return base.Transform as RectTransform; }
		}

		public Canvas Canvas
		{
			get
			{
				if ( m_Canvas == null )
					CacheCanvas ();
				return m_Canvas;
			}
		}

		/// <summary>
		/// UI Renderer component.
		/// </summary>
		public CanvasRenderer Renderer
		{
			get
			{
				if ( m_Renderer == null )
					m_Renderer = GameObject.GetComponent<CanvasRenderer> ();
				return m_Renderer;
			}
		}

		public Color Color
		{
			get { return m_Color; }
			set
			{
				if ( m_Color != value )
				{
					m_Color = value;
					SetVerticesDirty ();
				}
			}
		}

		public virtual Material defaultMaterial
		{
			get { return defaultGraphicMaterial; }
		}

		/// <summary>
		/// Absolute depth of the graphic, used by rendering and events -- lowest to highest.
		/// </summary>
		public int depth
		{
			get { return Renderer.AbsoluteDepth; }
		}

		/// <summary>
		/// Returns the texture used to draw this Graphic.
		/// </summary>
		public virtual Texture mainTexture
		{
			get { return s_WhiteTexture; }
		}

		/// <summary>
		/// Returns the material used by this Graphic.
		/// </summary>
		public virtual Material material
		{
			get
			{
				return (m_Material != null) ? m_Material : defaultMaterial;
			}
			set
			{
				if ( m_Material == value )
					return;

				m_Material = value;
				SetMaterialDirty ();
			}
		}

		public virtual Material materialForRendering
		{
			get
			{
				var currentMat = material;
				//var components = ComponentListPool.Get ();
				//GetComponents ( typeof ( IMaterialModifier ), components );
				//for ( var i = 0; i < components.Count; i++ )
				//	currentMat = (components[i] as IMaterialModifier).GetModifiedMaterial ( currentMat );
				//ComponentListPool.Release ( components );
				return currentMat;
			}
		}

		public void CrossFadeAlpha ( float alpha, float duration, bool ignoreTimeScale )
		{
			CrossFadeColor ( CreateColorFromAlpha ( alpha ), duration, ignoreTimeScale, true, false );
		}

		public void CrossFadeColor ( Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha )
		{
			CrossFadeColor ( targetColor, duration, ignoreTimeScale, useAlpha, true );
		}

		public RectangleF GetPixelAdjustedRect ()
		{
			if ( Canvas == null )
				return Transform.Rectangle;

			//return RectTransformUtility.PixelAdjustRect ( rectTransform, canvas );
			return new RectangleF ();
		}

		public Vector2 PixelAdjustPoint ( Vector2 point )
		{
			if ( Canvas == null )
				return point;

			//return RectTransformUtility.PixelAdjustPoint ( point, Transform, canvas );
			return new Vector2 ();
		}

		public virtual bool Raycast ( Vector2 sp, Camera eventCamera )
		{
			//var t = Transform;
			//var components = ComponentListPool.Get ();
			//while ( t != null )
			//{
			//	t.GetComponents ( components );
			//	for ( var i = 0; i < components.Count; i++ )
			//	{
			//		var filter = components[i] as ICanvasRaycastFilter;

			//		if ( filter == null )
			//			continue;

			//		if ( !filter.IsRaycastLocationValid ( sp, eventCamera ) )
			//		{
			//			ComponentListPool.Release ( components );
			//			return false;
			//		}
			//	}
			//	t = t.Parent;
			//}
			//ComponentListPool.Release ( components );
			return true;
		}

		public virtual void Rebuild ( CanvasUpdate update )
		{
			switch ( update )
			{
				case CanvasUpdate.PreRender:
					if ( m_VertsDirty )
					{
						UpdateGeometry ();
						m_VertsDirty = false;
					}
					if ( m_MaterialDirty )
					{
						UpdateMaterial ();
						m_MaterialDirty = false;
					}
					break;
			}
		}

		public virtual void SetAllDirty ()
		{
			SetLayoutDirty ();
			SetVerticesDirty ();
			SetMaterialDirty ();
		}

		public virtual void SetLayoutDirty ()
		{
			//if ( !IsActive () )
			//	return;

			//LayoutRebuilder.MarkLayoutForRebuild ( rectTransform );

			//if ( m_OnDirtyLayoutCallback != null )
			//	m_OnDirtyLayoutCallback ();
		}

		public virtual void SetMaterialDirty ()
		{
			//if ( !IsActive () )
			//	return;

			m_MaterialDirty = true;
			//CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild ( this );

			//if ( m_OnDirtyMaterialCallback != null )
			//	m_OnDirtyMaterialCallback ();
		}

		/// <summary>
		/// Make the Graphic have the native size of its content.
		/// </summary>
		public virtual void SetNativeSize ()
		{
		}

		public virtual void SetVerticesDirty ()
		{
			//if ( !IsActive () )
			//	return;

			m_VertsDirty = true;
			//CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild ( this );

			//if ( m_OnDirtyVertsCallback != null )
			//	m_OnDirtyVertsCallback ();
		}

		// Call from unity if animation properties have changed
		protected void OnDidApplyAnimationProperties ()
		{
			SetAllDirty ();
		}

		/// <summary>
		/// Mark the Graphic and the canvas as having been changed.
		/// </summary>
		public virtual void OnEnable ()
		{
			//			base.OnEnable ();
			//			CacheCanvas ();
			//			GraphicRegistry.RegisterGraphicForCanvas ( canvas, this );

			//#if UNITY_EDITOR
			//				GraphicRebuildTracker.TrackGraphic(this);
			//#endif
			//			if ( s_WhiteTexture == null )
			//				s_WhiteTexture = Texture2D.whiteTexture;

			//			SetAllDirty ();
			//			SendGraphicEnabledDisabled ();
		}

		/// <summary>
		/// Clear references.
		/// </summary>
		public virtual void OnDisable ()
		{
			//#if UNITY_EDITOR
			//				GraphicRebuildTracker.UnTrackGraphic(this);
			//#endif
			//			GraphicRegistry.UnregisterGraphicForCanvas ( canvas, this );
			//			CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild ( this );

			//			if ( canvasRenderer != null )
			//				canvasRenderer.Clear ();

			//			LayoutRebuilder.MarkLayoutForRebuild ( rectTransform );
			//			SendGraphicEnabledDisabled ();
			//			base.OnDisable ();
		}

		/// <summary>
		/// Fill the vertex buffer data.
		/// </summary>
		protected virtual void OnFillVBO ( Vector<Vertex> buffer )
		{
			var r = GetPixelAdjustedRect ();

			var vert = Vertex.Default;
			vert.Color = Color;

			vert.Position = (Vector3)r.Minimum;
			vert.Texcoord0 = new Vector2 ();
			buffer.Add ( ref vert );

			vert.Position = new Vector3 ( r.Minimum.X, r.Maximum.Y, 0 );
			vert.Texcoord0 = new Vector2 ( 0f, 1f );
			buffer.Add ( ref vert );

			vert.Position = (Vector3)r.Maximum;
			vert.Texcoord0 = new Vector2 ( 1f, 1f );
			buffer.Add ( ref vert );

			vert.Position = new Vector3 ( r.Maximum.X, r.Minimum.Y, 0 );
			vert.Texcoord0 = new Vector2 ( 1f, 0f );
			buffer.Add ( ref vert );
		}

		protected void OnRectTransformDimensionsChange ()
		{
			//if ( GameObject.IsActive )
			//{
			//	// prevent double dirtying...
			//	if ( CanvasUpdateRegistry.IsRebuildingLayout () )
			//		SetVerticesDirty ();
			//	else
			//	{
			//		SetVerticesDirty ();
			//		SetLayoutDirty ();
			//	}
			//}
		}

		protected void OnBeforeTransformParentChanged ()
		{
			//GraphicRegistry.UnregisterGraphicForCanvas ( canvas, this );
			//LayoutRebuilder.MarkLayoutForRebuild ( rectTransform );
		}

		protected void OnTransformParentChanged ()
		{
			//if ( !IsActive () )
			//	return;

			//CacheCanvas ();
			//GraphicRegistry.RegisterGraphicForCanvas ( canvas, this );
			//SetAllDirty ();
		}

		/// <summary>
		/// Update the renderer's vertices.
		/// </summary>
		protected virtual void UpdateGeometry ()
		{
			var vbo = s_VboPool.Get ();

			if ( Transform != null && Transform.Rectangle.Width >= 0 && Transform.Rectangle.Height >= 0 )
				OnFillVBO ( vbo );

			//var components = ComponentListPool.Get ();
			//GetComponents ( typeof ( IVertexModifier ), components );
			//for ( var i = 0; i < components.Count; i++ )
			//	(components[i] as IVertexModifier).ModifyVertices ( vbo );
			//ComponentListPool.Release ( components );

			Renderer.SetVertices ( vbo );
			s_VboPool.Recycle ( vbo );
		}

		/// <summary>
		/// Update the renderer's material.
		/// </summary>
		protected virtual void UpdateMaterial ()
		{
			//if ( IsActive () )
			//	canvasRenderer.SetMaterial ( materialForRendering, mainTexture );
		}

		static private Color CreateColorFromAlpha ( float alpha )
		{
			var alphaColor = Color.Black;
			alphaColor.A = (byte)(alpha * 255);
			return alphaColor;
		}

		private void CacheCanvas ()
		{
			m_Canvas = GameObject.GetComponentInParent<Canvas> ();
		}

		private void CrossFadeColor ( Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha, bool useRGB )
		{
			if ( Renderer == null || (!useRGB && !useAlpha) )
				return;

			Color currentColor = Renderer.Color;
			if ( currentColor == targetColor )
				return;

			//ColorTween.ColorTweenMode mode = (useRGB && useAlpha ?
			//											 ColorTween.ColorTweenMode.All :
			//											 (useRGB ? ColorTween.ColorTweenMode.RGB : ColorTween.ColorTweenMode.Alpha));

			//var colorTween = new ColorTween { duration = duration, startColor = canvasRenderer.GetColor (), targetColor = targetColor };
			//colorTween.AddOnChangedCallback ( canvasRenderer.SetColor );
			//colorTween.ignoreTimeScale = ignoreTimeScale;
			//colorTween.tweenMode = mode;
			//m_ColorTweenRunner.StartTween ( colorTween );
		}

		private void SendGraphicEnabledDisabled ()
		{
			//var components = ComponentListPool.Get ();
			//GetComponents ( typeof ( IGraphicEnabledDisabled ), components );

			//for ( int i = 0; i < components.Count; i++ )
			//	((IGraphicEnabledDisabled)components[i]).OnSiblingGraphicEnabledDisabled ();

			//ComponentListPool.Release ( components );
		}
	}
}