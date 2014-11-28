using System;
using System.Collections.Generic;
using CrowEngine.Components;
using OpenTK;

using Color = CrowEngine.Mathematics.Color;
using Rect = CrowEngine.Mathematics.RectangleF;

namespace CrowEngine.Components.UI
{
	/*
	public abstract class BaseGraphic : Component
	{
		protected static Material s_DefaultText = null;
		protected static Material s_DefaultUI = null;
		protected static Color s_White = Color.White;
		protected static Texture2D s_WhiteTexture = null;
		protected Material m_Material;

		private static readonly ObjectPool<List<Vertex>> s_VboPool = new ObjectPool<List<Vertex>> (
			() => new List<Vertex> ( 300 ),
			null,
			( a ) => a.Clear ()
		);

		[NonSerialized]
		private Canvas m_Canvas;

		[NonSerialized]
		private CanvasRenderer m_CanvasRender;

		private Color m_Color = Color.White;

		[NonSerialized]
		private bool m_MaterialDirty;

		[NonSerialized]
		private RectTransform m_RectTransform;

		[NonSerialized]
		private bool m_VertsDirty;


		public Canvas canvas
		{
			get
			{
				if ( this.m_Canvas == null )
				{
					this.CacheCanvas ();
				}
				return this.m_Canvas;
			}
		}

		public CanvasRenderer canvasRenderer
		{
			get
			{
				if ( this.m_CanvasRender == null )
				{
					this.m_CanvasRender = GameObject.GetComponent<CanvasRenderer> ();
				}
				return this.m_CanvasRender;
			}
		}

		public Color color
		{
			get
			{
				return this.m_Color;
			}
			set
			{
				if ( SetPropertyUtility.SetColor ( ref this.m_Color, value ) )
				{
					this.SetVerticesDirty ();
				}
			}
		}

		public virtual Material defaultMaterial
		{
			get
			{
				return Graphic.defaultGraphicMaterial;
			}
		}

		public int depth
		{
			get
			{
				return this.canvasRenderer.absoluteDepth;
			}
		}

		public virtual Texture mainTexture
		{
			get
			{
				return Graphic.s_WhiteTexture;
			}
		}

		public virtual Material material
		{
			get
			{
				return (!(this.m_Material != null)) ? this.defaultMaterial : this.m_Material;
			}
			set
			{
				if ( this.m_Material == value )
				{
					return;
				}
				this.m_Material = value;
				this.SetMaterialDirty ();
			}
		}

		public virtual Material materialForRendering
		{
			get
			{
				List<Component> list = ComponentListPool.Get ();
				base.GetComponents ( typeof ( IMaterialModifier ), list );
				Material material = this.material;
				for ( int i = 0; i < list.Count; i++ )
				{
					material = (list[i] as IMaterialModifier).GetModifiedMaterial ( material );
				}
				ComponentListPool.Release ( list );
				return material;
			}
		}

		public RectTransform rectTransform
		{
			get
			{
				RectTransform arg_1C_0;
				if ( (arg_1C_0 = this.m_RectTransform) == null )
				{
					arg_1C_0 = (this.m_RectTransform = base.GetComponent<RectTransform> ());
				}
				return arg_1C_0;
			}
		}

		public void CrossFadeAlpha ( float alpha, float duration, bool ignoreTimeScale )
		{
			this.CrossFadeColor ( Graphic.CreateColorFromAlpha ( alpha ), duration, ignoreTimeScale, true, false );
		}

		public void CrossFadeColor ( Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha )
		{
			this.CrossFadeColor ( targetColor, duration, ignoreTimeScale, useAlpha, true );
		}

		public Rect GetPixelAdjustedRect ()
		{
			if ( !this.canvas || !this.canvas.pixelPerfect )
			{
				return this.rectTransform.rect;
			}
			return RectTransformUtility.PixelAdjustRect ( this.rectTransform, this.canvas );
		}

		public Vector2 PixelAdjustPoint ( Vector2 point )
		{
			if ( !this.canvas || !this.canvas.pixelPerfect )
			{
				return point;
			}
			return RectTransformUtility.PixelAdjustPoint ( point, base.transform, this.canvas );
		}

		public virtual bool Raycast ( Vector2 sp, Camera eventCamera )
		{
			Transform transform = base.transform;
			List<Component> list = ComponentListPool.Get ();
			while ( transform != null )
			{
				transform.GetComponents<Component> ( list );
				for ( int i = 0; i < list.Count; i++ )
				{
					ICanvasRaycastFilter canvasRaycastFilter = list[i] as ICanvasRaycastFilter;
					if ( canvasRaycastFilter != null )
					{
						if ( !canvasRaycastFilter.IsRaycastLocationValid ( sp, eventCamera ) )
						{
							ComponentListPool.Release ( list );
							return false;
						}
					}
				}
				transform = transform.parent;
			}
			ComponentListPool.Release ( list );
			return true;
		}

		public virtual void Rebuild ( CanvasUpdate update )
		{
			if ( update == CanvasUpdate.PreRender )
			{
				if ( this.m_VertsDirty )
				{
					this.UpdateGeometry ();
					this.m_VertsDirty = false;
				}
				if ( this.m_MaterialDirty )
				{
					this.UpdateMaterial ();
					this.m_MaterialDirty = false;
				}
			}
		}

		public void RegisterDirtyVerticesCallback ( UnityAction action )
		{
			this.m_OnDirtyVertsCallback = (UnityAction)Delegate.Combine ( this.m_OnDirtyVertsCallback, action );
		}

		public virtual void SetAllDirty ()
		{
			this.SetLayoutDirty ();
			this.SetVerticesDirty ();
			this.SetMaterialDirty ();
		}

		public virtual void SetLayoutDirty ()
		{
			if ( !this.IsActive () )
			{
				return;
			}
			LayoutRebuilder.MarkLayoutForRebuild ( this.rectTransform );
		}

		public virtual void SetMaterialDirty ()
		{
			if ( !this.IsActive () )
			{
				return;
			}
			this.m_MaterialDirty = true;
			CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild ( this );
		}

		public virtual void SetNativeSize ()
		{
		}

		public virtual void SetVerticesDirty ()
		{
			if ( !this.IsActive () )
			{
				return;
			}
			this.m_VertsDirty = true;
			CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild ( this );
			if ( this.m_OnDirtyVertsCallback != null )
			{
				this.m_OnDirtyVertsCallback ();
			}
		}

		public void UnregisterDirtyVerticesCallback ( UnityAction action )
		{
			this.m_OnDirtyVertsCallback = (UnityAction)Delegate.Remove ( this.m_OnDirtyVertsCallback, action );
		}

		protected virtual Transform get_transform ()
		{
			return base.transform;
		}

		protected virtual bool IsDestroyed ()
		{
			return base.IsDestroyed ();
		}

		protected override void OnBeforeTransformParentChanged ()
		{
			GraphicRegistry.UnregisterGraphicForCanvas ( this.canvas, this );
			LayoutRebuilder.MarkLayoutForRebuild ( this.rectTransform );
		}

		protected override void OnDidApplyAnimationProperties ()
		{
			this.SetAllDirty ();
		}

		protected override void OnDisable ()
		{
			CanvasRenderer.onRequestRebuild -= new CanvasRenderer.OnRequestRebuild ( this.OnRebuildRequested );
			GraphicRegistry.UnregisterGraphicForCanvas ( this.canvas, this );
			CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild ( this );
			if ( this.canvasRenderer != null )
			{
				this.canvasRenderer.Clear ();
			}
			LayoutRebuilder.MarkLayoutForRebuild ( this.rectTransform );
			this.SendGraphicEnabledDisabled ();
			base.OnDisable ();
		}

		protected override void OnEnable ()
		{
			base.OnEnable ();
			this.CacheCanvas ();
			GraphicRegistry.RegisterGraphicForCanvas ( this.canvas, this );
			CanvasRenderer.onRequestRebuild += new CanvasRenderer.OnRequestRebuild ( this.OnRebuildRequested );
			if ( Graphic.s_WhiteTexture == null )
			{
				Graphic.s_WhiteTexture = Texture2D.whiteTexture;
			}
			this.SetAllDirty ();
			this.SendGraphicEnabledDisabled ();
		}

		protected virtual void OnFillVBO ( List<Vertex> vbo )
		{
			Rect pixelAdjustedRect = this.GetPixelAdjustedRect ();
			Vector4 vector = new Vector4 ( pixelAdjustedRect.x, pixelAdjustedRect.y, pixelAdjustedRect.x + pixelAdjustedRect.width, pixelAdjustedRect.y + pixelAdjustedRect.height );
			Vertex simpleVert = Vertex.simpleVert;
			simpleVert.color = this.color;
			simpleVert.position = new Vector3 ( vector.x, vector.y );
			simpleVert.uv0 = new Vector2 ( 0f, 0f );
			vbo.Add ( simpleVert );
			simpleVert.position = new Vector3 ( vector.x, vector.w );
			simpleVert.uv0 = new Vector2 ( 0f, 1f );
			vbo.Add ( simpleVert );
			simpleVert.position = new Vector3 ( vector.z, vector.w );
			simpleVert.uv0 = new Vector2 ( 1f, 1f );
			vbo.Add ( simpleVert );
			simpleVert.position = new Vector3 ( vector.z, vector.y );
			simpleVert.uv0 = new Vector2 ( 1f, 0f );
			vbo.Add ( simpleVert );
		}

		protected virtual void OnRebuildRequested ()
		{
			this.SetAllDirty ();
		}

		protected override void OnRectTransformDimensionsChange ()
		{
			if ( base.gameObject.activeInHierarchy )
			{
				if ( CanvasUpdateRegistry.IsRebuildingLayout () )
				{
					this.SetVerticesDirty ();
				}
				else
				{
					this.SetVerticesDirty ();
					this.SetLayoutDirty ();
				}
			}
		}

		protected override void OnTransformParentChanged ()
		{
			if ( !this.IsActive () )
			{
				return;
			}
			this.CacheCanvas ();
			GraphicRegistry.RegisterGraphicForCanvas ( this.canvas, this );
			this.SetAllDirty ();
		}

		protected override void OnValidate ()
		{
			base.OnValidate ();
			this.SetAllDirty ();
		}

		protected virtual void UpdateGeometry ()
		{
			List<Vertex> list = Graphic.s_VboPool.Get ();
			if ( this.rectTransform != null && this.rectTransform.rect.width >= 0f && this.rectTransform.rect.height >= 0f )
			{
				this.OnFillVBO ( list );
			}
			List<Component> list2 = ComponentListPool.Get ();
			base.GetComponents ( typeof ( IVertexModifier ), list2 );
			for ( int i = 0; i < list2.Count; i++ )
			{
				(list2[i] as IVertexModifier).ModifyVertices ( list );
			}
			ComponentListPool.Release ( list2 );
			this.canvasRenderer.SetVertices ( list );
			Graphic.s_VboPool.Release ( list );
		}

		protected virtual void UpdateMaterial ()
		{
			if ( this.IsActive () )
			{
				this.canvasRenderer.SetMaterial ( this.materialForRendering, this.mainTexture );
			}
		}

		private static Color CreateColorFromAlpha ( float alpha )
		{
			Color black = Color.black;
			black.a = alpha;
			return black;
		}

		private void CacheCanvas ()
		{
			this.m_Canvas = base.gameObject.GetComponentInParent<Canvas> ();
		}

		private void CrossFadeColor ( Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha, bool useRGB )
		{
			if ( this.canvasRenderer == null || (!useRGB && !useAlpha) )
			{
				return;
			}
			if ( this.canvasRenderer.GetColor ().Equals ( targetColor ) )
			{
				return;
			}
			ColorTween.ColorTweenMode tweenMode = (!useRGB || !useAlpha) ? ((!useRGB) ? ColorTween.ColorTweenMode.Alpha : ColorTween.ColorTweenMode.RGB) : ColorTween.ColorTweenMode.All;
			ColorTween info = new ColorTween
			{
				duration = duration,
				startColor = this.canvasRenderer.GetColor (),
				targetColor = targetColor
			};
			info.AddOnChangedCallback ( new UnityAction<Color> ( this.canvasRenderer.SetColor ) );
			info.ignoreTimeScale = ignoreTimeScale;
			info.tweenMode = tweenMode;
			this.m_ColorTweenRunner.StartTween ( info );
		}

		private void SendGraphicEnabledDisabled ()
		{
			List<Component> list = ComponentListPool.Get ();
			base.GetComponents ( typeof ( IGraphicEnabledDisabled ), list );
			for ( int i = 0; i < list.Count; i++ )
			{
				((IGraphicEnabledDisabled)list[i]).OnSiblingGraphicEnabledDisabled ();
			}
			ComponentListPool.Release ( list );
		}
	}
	*/
}