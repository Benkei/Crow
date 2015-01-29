using System;
using System.Collections.Generic;
using CrowEngine.Collections;
using CrowEngine.Mathematics;
using CrowEngine.Resources;

namespace CrowEngine.Components.UI
{
	public enum FillMethod
	{
		Horizontal,
		Vertical,
		Radial90,
		Radial180,
		Radial360,
	}

	public enum Origin180
	{
		Bottom,
		Left,
		Top,
		Right,
	}

	public enum Origin360
	{
		Bottom,
		Right,
		Top,
		Left,
	}

	public enum Origin90
	{
		BottomLeft,
		TopLeft,
		TopRight,
		BottomRight,
	}

	public enum OriginHorizontal
	{
		Left,
		Right,
	}

	public enum OriginVertical
	{
		Bottom,
		Top,
	}

	public enum ImageType
	{
		Simple,
		Sliced,
		Tiled,
		Filled
	}

	public class Image : Graphic
	{
		private static readonly Vector2[] s_Uv = new Vector2[4];
		private static readonly Vector2[] s_UVScratch = new Vector2[4];
		private static readonly Vector2[] s_VertScratch = new Vector2[4];
		private static readonly Vector2[] s_Xy = new Vector2[4];

		// Not serialized until we support read-enabled sprites better.
		private float m_EventAlphaThreshold = 1;

		/// Amount of the Image shown. 0-1 range with 0 being nothing shown, and 1 being the full Image.
		private float m_FillAmount = 1;

		private bool m_FillCenter = true;

		/// Whether the Image should be filled clockwise (true) or counter-clockwise (false).
		private bool m_FillClockwise = true;

		/// Filling method for filled sprites.
		private FillMethod m_FillMethod = FillMethod.Radial360;

		/// Controls the origin point of the Fill process. Value means different things with each fill method.
		private int m_FillOrigin;

		private Sprite m_OverrideSprite;

		private bool m_PreserveAspect;

		private Sprite m_Sprite;

		/// How the Image is drawn.
		private ImageType m_Type;

		internal Image () { }

		public float eventAlphaThreshold
		{
			get { return m_EventAlphaThreshold; }
			set { m_EventAlphaThreshold = value; }
		}

		public float fillAmount
		{
			get { return m_FillAmount; }
			set
			{
				value = MathUtil.Clamp01 ( value );
				if ( m_FillAmount != value )
				{
					m_FillAmount = value;
					SetVerticesDirty ();
				}
			}
		}

		public bool fillCenter
		{
			get { return m_FillCenter; }
			set
			{
				if ( m_FillCenter != value )
				{
					m_FillCenter = value;
					SetVerticesDirty ();
				}
			}
		}

		public bool fillClockwise
		{
			get { return m_FillClockwise; }
			set
			{
				if ( m_FillClockwise != value )
				{
					m_FillClockwise = value;
					SetVerticesDirty ();
				}
			}
		}

		public FillMethod fillMethod
		{
			get { return m_FillMethod; }
			set
			{
				if ( m_FillMethod != value )
				{
					m_FillMethod = value;
					SetVerticesDirty ();
					m_FillOrigin = 0;
				}
			}
		}

		public int fillOrigin
		{
			get { return m_FillOrigin; }
			set
			{
				if ( m_FillOrigin != value )
				{
					m_FillOrigin = value;
					SetVerticesDirty ();
				}
			}
		}

		public virtual float flexibleHeight { get { return -1; } }

		public virtual float flexibleWidth { get { return -1; } }

		public bool hasBorder
		{
			get
			{
				if ( OverrideSprite != null )
				{
					var v = OverrideSprite.Border;
					return v.Left != 0 && v.Top != 0 && v.Right != 0 && v.Bottom != 0;
				}
				return false;
			}
		}

		public virtual int layoutPriority { get { return 0; } }

		/// <summary>
		/// Image's texture comes from the UnityEngine.Image.
		/// </summary>
		public override Texture mainTexture
		{
			get { return OverrideSprite == null ? s_WhiteTexture : OverrideSprite.Texture; }
		}

		public virtual float minHeight { get { return 0; } }

		public virtual float minWidth { get { return 0; } }

		public Sprite OverrideSprite
		{
			get { return m_OverrideSprite == null ? Sprite : m_OverrideSprite; }
			set
			{
				if ( m_OverrideSprite != value )
				{
					m_OverrideSprite = value;
					SetAllDirty ();
				}
			}
		}

		/// <summary>
		/// Whether the Image has a border to work with.
		/// </summary>
		public float PixelsPerUnit
		{
			get
			{
				float spriteUnit = 100;
				if ( Sprite != null )
					spriteUnit = Sprite.PixelsPerUnit;

				float refUnit = 100;
				if ( Canvas != null )
					refUnit = Canvas.ReferencePixelsPerUnit;

				return spriteUnit / refUnit;
			}
		}

		public virtual float PreferredHeight
		{
			get
			{
				if ( OverrideSprite == null )
					return 0;
				if ( Type == ImageType.Sliced || Type == ImageType.Tiled )
					return OverrideSprite.Rect.Height / PixelsPerUnit;
				return OverrideSprite.Rect.Size.Y / PixelsPerUnit;
			}
		}

		public virtual float PreferredWidth
		{
			get
			{
				if ( OverrideSprite == null )
					return 0;
				if ( Type == ImageType.Sliced || Type == ImageType.Tiled )
					return OverrideSprite.Rect.Width / PixelsPerUnit;
				return OverrideSprite.Rect.Size.X / PixelsPerUnit;
			}
		}

		public bool PreserveAspect
		{
			get { return m_PreserveAspect; }
			set
			{
				if ( m_PreserveAspect != value )
				{
					m_PreserveAspect = value;
					SetVerticesDirty ();
				}
			}
		}

		public Sprite Sprite
		{
			get { return m_Sprite; }
			set
			{
				if ( m_Sprite != value )
				{
					m_Sprite = value;
					SetAllDirty ();
				}
			}
		}

		public ImageType Type
		{
			get { return m_Type; }
			set
			{
				if ( m_Type != value )
				{
					m_Type = value;
					SetVerticesDirty ();
				}
			}
		}

		public virtual void CalculateLayoutInputHorizontal ()
		{
		}

		public virtual void CalculateLayoutInputVertical ()
		{
		}

		public virtual bool IsRaycastLocationValid ( Vector2 screenPoint, Camera eventCamera )
		{
			if ( m_EventAlphaThreshold >= 1 )
				return true;

			Sprite sprite = OverrideSprite;
			if ( sprite == null )
				return true;

			Vector2 local;
			RectTransform.ScreenPointToLocalPointInRectangle ( Transform, screenPoint, eventCamera, out local );

			RectangleF rect = GetPixelAdjustedRect ();

			// Convert to have lower left corner as reference point.
			local.X += Transform.Pivot.X * rect.Width;
			local.Y += Transform.Pivot.Y * rect.Height;

			local = MapCoordinate ( local, rect );

			// Normalize local coordinates.
			Rectangle spriteRect = sprite.Rect;
			Vector2 normalized = new Vector2 ( local.X / spriteRect.Width, local.Y / spriteRect.Height );

			// Convert to texture space.
			float x = MathUtil.Lerp ( spriteRect.X, spriteRect.X + spriteRect.Width, normalized.X ) / sprite.Texture.Width ( 0 );
			float y = MathUtil.Lerp ( spriteRect.Y, spriteRect.Y + spriteRect.Height, normalized.Y ) / sprite.Texture.Height ( 0 );

			return sprite.Texture.GetPixelBilinear ( 0, x, y ).A * (1f / 255f) >= m_EventAlphaThreshold;
		}

		public virtual void OnAfterDeserialize ()
		{
			if ( m_FillOrigin < 0 )
				m_FillOrigin = 0;
			else if ( m_FillMethod == FillMethod.Horizontal && m_FillOrigin > 1 )
				m_FillOrigin = 0;
			else if ( m_FillMethod == FillMethod.Vertical && m_FillOrigin > 1 )
				m_FillOrigin = 0;
			else if ( m_FillOrigin > 3 )
				m_FillOrigin = 0;

			m_FillAmount = MathUtil.Clamp ( m_FillAmount, 0f, 1f );
		}

		public virtual void OnBeforeSerialize ()
		{
		}

		public override void SetNativeSize ()
		{
			if ( OverrideSprite != null )
			{
				float w = OverrideSprite.Rect.Width / PixelsPerUnit;
				float h = OverrideSprite.Rect.Height / PixelsPerUnit;
				Transform.AnchorMaximum = Transform.AnchorMinimum;
				Transform.SizeDelta = new Vector2 ( w, h );
				SetAllDirty ();
			}
		}

		protected override void OnFillVBO ( Vector<Vertex> buffer )
		{
			if ( OverrideSprite == null )
			{
				base.OnFillVBO ( buffer );
				return;
			}

			switch ( Type )
			{
				case ImageType.Simple:
					GenerateSimpleSprite ( buffer, m_PreserveAspect );
					break;

				case ImageType.Sliced:
					GenerateSlicedSprite ( buffer );
					break;

				case ImageType.Tiled:
					GenerateTiledSprite ( buffer );
					break;

				case ImageType.Filled:
					GenerateFilledSprite ( buffer, m_PreserveAspect );
					break;
			}
		}

		private static bool RadialCut ( Vector2[] xy, Vector2[] uv, float fill, bool invert, int corner )
		{
			// Nothing to fill
			if ( fill < 0.001f ) return false;

			// Even corners invert the fill direction
			if ( (corner & 1) == 1 ) invert = !invert;

			// Nothing to adjust
			if ( !invert && fill > 0.999f ) return true;

			// Convert 0-1 value into 0 to 90 degrees angle in radians
			float angle = MathUtil.Clamp01 ( fill );
			if ( invert ) angle = 1f - angle;
			angle *= 90f * MathUtil.Deg2Rad;

			// Calculate the effective X and Y factors
			float cos = (float)Math.Cos ( angle );
			float sin = (float)Math.Sin ( angle );

			RadialCut ( xy, cos, sin, invert, corner );
			RadialCut ( uv, cos, sin, invert, corner );
			return true;
		}

		private static void RadialCut ( Vector2[] xy, float cos, float sin, bool invert, int corner )
		{
			int i0 = corner;
			int i1 = ((corner + 1) % 4);
			int i2 = ((corner + 2) % 4);
			int i3 = ((corner + 3) % 4);

			if ( (corner & 1) == 1 )
			{
				if ( sin > cos )
				{
					cos /= sin;
					sin = 1f;

					if ( invert )
					{
						xy[i1].X = MathUtil.Lerp ( xy[i0].X, xy[i2].X, cos );
						xy[i2].X = xy[i1].X;
					}
				}
				else if ( cos > sin )
				{
					sin /= cos;
					cos = 1f;

					if ( !invert )
					{
						xy[i2].Y = MathUtil.Lerp ( xy[i0].Y, xy[i2].Y, sin );
						xy[i3].Y = xy[i2].Y;
					}
				}
				else
				{
					cos = 1f;
					sin = 1f;
				}

				if ( !invert ) xy[i3].X = MathUtil.Lerp ( xy[i0].X, xy[i2].X, cos );
				else xy[i1].Y = MathUtil.Lerp ( xy[i0].Y, xy[i2].Y, sin );
			}
			else
			{
				if ( cos > sin )
				{
					sin /= cos;
					cos = 1f;

					if ( !invert )
					{
						xy[i1].Y = MathUtil.Lerp ( xy[i0].Y, xy[i2].Y, sin );
						xy[i2].Y = xy[i1].Y;
					}
				}
				else if ( sin > cos )
				{
					cos /= sin;
					sin = 1f;

					if ( invert )
					{
						xy[i2].X = MathUtil.Lerp ( xy[i0].X, xy[i2].X, cos );
						xy[i3].X = xy[i2].X;
					}
				}
				else
				{
					cos = 1f;
					sin = 1f;
				}

				if ( invert ) xy[i3].Y = MathUtil.Lerp ( xy[i0].Y, xy[i2].Y, sin );
				else xy[i1].X = MathUtil.Lerp ( xy[i0].X, xy[i2].X, cos );
			}
		}

		private static void AddQuad ( Vector<Vertex> buffer, Vertex v, Vector2 posMin, Vector2 posMax, Vector2 uvMin, Vector2 uvMax )
		{
			v.Position = new Vector3 ( posMin.X, posMin.Y, 0 );
			v.Texcoord0 = uvMin;
			buffer.Add ( v );

			v.Position = new Vector3 ( posMin.X, posMax.Y, 0 );
			v.Texcoord0 = new Vector2 ( uvMin.X, uvMax.Y );
			buffer.Add ( v );

			v.Position = new Vector3 ( posMax.X, posMax.Y, 0 );
			v.Texcoord0 = uvMax;
			buffer.Add ( v );

			v.Position = new Vector3 ( posMax.X, posMin.Y, 0 );
			v.Texcoord0 = new Vector2 ( uvMax.X, uvMin.Y );
			buffer.Add ( v );
		}

		/// <summary>
		/// Generate vertices for a filled Image.
		/// </summary>
		private void GenerateFilledSprite ( Vector<Vertex> buffer, bool preserveAspect )
		{
			if ( m_FillAmount < 0.001f )
				return;

			Vector4 v = GetDrawingDimensions ( preserveAspect );
			RectangleF outer = OverrideSprite != null ? OverrideSprite.GetOuterUV () : new RectangleF ();
			Vertex uiv = Vertex.Default;
			uiv.Color = Color;

			float tx0 = outer.Minimum.X;
			float ty0 = outer.Minimum.Y;
			float tx1 = outer.Maximum.X;
			float ty1 = outer.Maximum.Y;

			// Horizontal and vertical filled sprites are simple -- just end the Image prematurely
			if ( m_FillMethod == FillMethod.Horizontal || m_FillMethod == FillMethod.Vertical )
			{
				if ( fillMethod == FillMethod.Horizontal )
				{
					float fill = (tx1 - tx0) * m_FillAmount;

					if ( m_FillOrigin == 1 )
					{
						v.X = v.Z - (v.Z - v.X) * m_FillAmount;
						tx0 = tx1 - fill;
					}
					else
					{
						v.Z = v.X + (v.Z - v.X) * m_FillAmount;
						tx1 = tx0 + fill;
					}
				}
				else if ( fillMethod == FillMethod.Vertical )
				{
					float fill = (ty1 - ty0) * m_FillAmount;

					if ( m_FillOrigin == 1 )
					{
						v.Y = v.W - (v.W - v.Y) * m_FillAmount;
						ty0 = ty1 - fill;
					}
					else
					{
						v.W = v.Y + (v.W - v.Y) * m_FillAmount;
						ty1 = ty0 + fill;
					}
				}
			}

			s_Xy[0] = new Vector2 ( v.X, v.Y );
			s_Xy[1] = new Vector2 ( v.X, v.W );
			s_Xy[2] = new Vector2 ( v.Z, v.W );
			s_Xy[3] = new Vector2 ( v.Z, v.Y );

			s_Uv[0] = new Vector2 ( tx0, ty0 );
			s_Uv[1] = new Vector2 ( tx0, ty1 );
			s_Uv[2] = new Vector2 ( tx1, ty1 );
			s_Uv[3] = new Vector2 ( tx1, ty0 );

			if ( m_FillAmount < 1f )
			{
				if ( fillMethod == FillMethod.Radial90 )
				{
					if ( RadialCut ( s_Xy, s_Uv, m_FillAmount, m_FillClockwise, m_FillOrigin ) )
					{
						for ( int i = 0; i < 4; ++i )
						{
							uiv.Position = (Vector3)s_Xy[i];
							uiv.Texcoord0 = s_Uv[i];
							buffer.Add ( uiv );
						}
					}
					return;
				}

				if ( fillMethod == FillMethod.Radial180 )
				{
					for ( int side = 0; side < 2; ++side )
					{
						float fx0, fx1, fy0, fy1;
						int even = m_FillOrigin > 1 ? 1 : 0;

						if ( m_FillOrigin == 0 || m_FillOrigin == 2 )
						{
							fy0 = 0f;
							fy1 = 1f;
							if ( side == even ) { fx0 = 0f; fx1 = 0.5f; }
							else { fx0 = 0.5f; fx1 = 1f; }
						}
						else
						{
							fx0 = 0f;
							fx1 = 1f;
							if ( side == even ) { fy0 = 0.5f; fy1 = 1f; }
							else { fy0 = 0f; fy1 = 0.5f; }
						}

						s_Xy[0].X = MathUtil.Lerp ( v.X, v.Z, fx0 );
						s_Xy[1].X = s_Xy[0].X;
						s_Xy[2].X = MathUtil.Lerp ( v.X, v.Z, fx1 );
						s_Xy[3].X = s_Xy[2].X;

						s_Xy[0].Y = MathUtil.Lerp ( v.Y, v.W, fy0 );
						s_Xy[1].Y = MathUtil.Lerp ( v.Y, v.W, fy1 );
						s_Xy[2].Y = s_Xy[1].Y;
						s_Xy[3].Y = s_Xy[0].Y;

						s_Uv[0].X = MathUtil.Lerp ( tx0, tx1, fx0 );
						s_Uv[1].X = s_Uv[0].X;
						s_Uv[2].X = MathUtil.Lerp ( tx0, tx1, fx1 );
						s_Uv[3].X = s_Uv[2].X;

						s_Uv[0].Y = MathUtil.Lerp ( ty0, ty1, fy0 );
						s_Uv[1].Y = MathUtil.Lerp ( ty0, ty1, fy1 );
						s_Uv[2].Y = s_Uv[1].Y;
						s_Uv[3].Y = s_Uv[0].Y;

						float val = m_FillClockwise ? fillAmount * 2f - side : m_FillAmount * 2f - (1 - side);

						if ( RadialCut ( s_Xy, s_Uv, MathUtil.Clamp01 ( val ), m_FillClockwise, ((side + m_FillOrigin + 3) % 4) ) )
						{
							for ( int i = 0; i < 4; ++i )
							{
								uiv.Position = (Vector3)s_Xy[i];
								uiv.Texcoord0 = s_Uv[i];
								buffer.Add ( uiv );
							}
						}
					}
					return;
				}

				if ( fillMethod == FillMethod.Radial360 )
				{
					for ( int corner = 0; corner < 4; ++corner )
					{
						float fx0, fx1, fy0, fy1;

						if ( corner < 2 ) { fx0 = 0f; fx1 = 0.5f; }
						else { fx0 = 0.5f; fx1 = 1f; }

						if ( corner == 0 || corner == 3 ) { fy0 = 0f; fy1 = 0.5f; }
						else { fy0 = 0.5f; fy1 = 1f; }

						s_Xy[0].X = MathUtil.Lerp ( v.X, v.Z, fx0 );
						s_Xy[1].X = s_Xy[0].X;
						s_Xy[2].X = MathUtil.Lerp ( v.X, v.Z, fx1 );
						s_Xy[3].X = s_Xy[2].X;

						s_Xy[0].Y = MathUtil.Lerp ( v.Y, v.W, fy0 );
						s_Xy[1].Y = MathUtil.Lerp ( v.Y, v.W, fy1 );
						s_Xy[2].Y = s_Xy[1].Y;
						s_Xy[3].Y = s_Xy[0].Y;

						s_Uv[0].X = MathUtil.Lerp ( tx0, tx1, fx0 );
						s_Uv[1].X = s_Uv[0].X;
						s_Uv[2].X = MathUtil.Lerp ( tx0, tx1, fx1 );
						s_Uv[3].X = s_Uv[2].X;

						s_Uv[0].Y = MathUtil.Lerp ( ty0, ty1, fy0 );
						s_Uv[1].Y = MathUtil.Lerp ( ty0, ty1, fy1 );
						s_Uv[2].Y = s_Uv[1].Y;
						s_Uv[3].Y = s_Uv[0].Y;

						float val = m_FillClockwise ?
							 m_FillAmount * 4f - ((corner + m_FillOrigin) % 4) :
							 m_FillAmount * 4f - (3 - ((corner + m_FillOrigin) % 4));

						if ( RadialCut ( s_Xy, s_Uv, MathUtil.Clamp01 ( val ), m_FillClockwise, ((corner + 2) % 4) ) )
						{
							for ( int i = 0; i < 4; ++i )
							{
								uiv.Position = (Vector3)s_Xy[i];
								uiv.Texcoord0 = s_Uv[i];
								buffer.Add ( uiv );
							}
						}
					}
					return;
				}
			}

			// Fill the buffer with the quad for the Image
			for ( int i = 0; i < 4; ++i )
			{
				uiv.Position = (Vector3)s_Xy[i];
				uiv.Texcoord0 = s_Uv[i];
				buffer.Add ( uiv );
			}
		}

		private void GenerateSimpleSprite ( Vector<Vertex> buffer, bool preserveAspect )
		{
			var vert = Vertex.Default;
			vert.Color = Color;

			Vector4 v = GetDrawingDimensions ( preserveAspect );
			var uv = OverrideSprite != null ? OverrideSprite.GetOuterUV () : new RectangleF ();

			vert.Position = new Vector3 ( v.X, v.Y, 0 );
			vert.Texcoord0 = uv.Minimum;
			buffer.Add ( ref vert );

			vert.Position = new Vector3 ( v.X, v.W, 0 );
			vert.Texcoord0 = new Vector2 ( uv.Minimum.X, uv.Maximum.Y );
			buffer.Add ( ref vert );

			vert.Position = new Vector3 ( v.Z, v.W, 0 );
			vert.Texcoord0 = uv.Maximum;
			buffer.Add ( ref vert );

			vert.Position = new Vector3 ( v.Z, v.Y, 0 );
			vert.Texcoord0 = new Vector2 ( uv.Maximum.X, uv.Minimum.Y );
			buffer.Add ( ref vert );
		}

		/// <summary>
		/// Update the UI renderer mesh.
		/// </summary>
		/// <summary>
		/// Generate vertices for a simple Image.
		/// </summary>
		/// <summary>
		/// Generate vertices for a 9-sliced Image.
		/// </summary>
		private unsafe void GenerateSlicedSprite ( Vector<Vertex> buffer )
		{
			if ( !hasBorder )
			{
				GenerateSimpleSprite ( buffer, false );
				return;
			}

			RectangleF outer, inner;
			RectangleF border;

			if ( OverrideSprite != null )
			{
				outer = OverrideSprite.GetOuterUV ();
				inner = OverrideSprite.GetInnerUV ();
				border = (RectangleF)OverrideSprite.Border;
				border = (RectangleF)((Vector4)border / PixelsPerUnit);
			}
			else
			{
				outer = inner = new RectangleF ();
				border = new RectangleF ();
			}

			RectangleF rect = GetPixelAdjustedRect ();
			GetAdjustedBorders ( ref border, rect );

			s_VertScratch[0] = new Vector2 ();
			s_VertScratch[1].X = border.X;
			s_VertScratch[1].Y = border.Y;
			s_VertScratch[2].X = rect.Width - border.Maximum.X;
			s_VertScratch[2].Y = rect.Height - border.Maximum.Y;
			s_VertScratch[3] = new Vector2 ( rect.Width, rect.Height );

			for ( int i = 0; i < 4; ++i )
			{
				s_VertScratch[i].X += rect.X;
				s_VertScratch[i].Y += rect.Y;
			}

			s_UVScratch[0] = new Vector2 ( outer.X, outer.Y );
			s_UVScratch[1] = new Vector2 ( inner.X, inner.Y );
			s_UVScratch[2] = inner.Maximum;
			s_UVScratch[3] = outer.Maximum;

			var uiv = Vertex.Default;
			uiv.Color = Color;
			for ( int x = 0; x < 3; ++x )
			{
				int x2 = x + 1;

				for ( int y = 0; y < 3; ++y )
				{
					if ( !m_FillCenter && x == 1 && y == 1 )
					{
						continue;
					}

					int y2 = y + 1;

					AddQuad ( buffer, uiv,
						 new Vector2 ( s_VertScratch[x].X, s_VertScratch[y].Y ),
						 new Vector2 ( s_VertScratch[x2].X, s_VertScratch[y2].Y ),
						 new Vector2 ( s_UVScratch[x].X, s_UVScratch[y].Y ),
						 new Vector2 ( s_UVScratch[x2].X, s_UVScratch[y2].Y ) );
				}
			}
		}

		private void GenerateTiledSprite ( Vector<Vertex> buffer )
		{
			RectangleF outer, inner;
			RectangleF border;
			Vector2 spriteSize;

			if ( OverrideSprite != null )
			{
				outer = OverrideSprite.GetOuterUV ();
				inner = OverrideSprite.GetInnerUV ();
				border = (RectangleF)OverrideSprite.Border;
				spriteSize = OverrideSprite.Rect.Size;
			}
			else
			{
				outer = new RectangleF ();
				inner = new RectangleF ();
				border = new RectangleF ();
				spriteSize = new Vector2 ( 100, 100 );
			}

			RectangleF rect = GetPixelAdjustedRect ();
			float tileWidth = (spriteSize.X - border.X - border.Maximum.X) / PixelsPerUnit;
			float tileHeight = (spriteSize.Y - border.Y - border.Maximum.Y) / PixelsPerUnit;
			border = (RectangleF)((Vector4)border / PixelsPerUnit);
			GetAdjustedBorders ( ref border, rect );

			var uvMin = new Vector2 ( inner.X, inner.Y );
			var uvMax = new Vector2 ( inner.Maximum.X, inner.Maximum.Y );

			var v = Vertex.Default;
			v.Color = Color;

			// Min to max max range for tiled region in coordinates relative to lower left corner.
			float xMin = border.X;
			float xMax = rect.Width - border.Maximum.X;
			float yMin = border.Y;
			float yMax = rect.Height - border.Maximum.Y;

			// Safety check. Useful so Unity doesn't run out of memory if the sprites are too small.
			// Max tiles are 100 x 100.
			if ( (xMax - xMin) > tileWidth * 100 || (yMax - yMin) > tileHeight * 100 )
			{
				tileWidth = (xMax - xMin) / 100;
				tileHeight = (yMax - yMin) / 100;
			}

			var clipped = uvMax;
			if ( m_FillCenter )
			{
				for ( float y1 = yMin; y1 < yMax; y1 += tileHeight )
				{
					float y2 = y1 + tileHeight;
					if ( y2 > yMax )
					{
						clipped.Y = uvMin.Y + (uvMax.Y - uvMin.Y) * (yMax - y1) / (y2 - y1);
						y2 = yMax;
					}

					clipped.X = uvMax.X;
					for ( float x1 = xMin; x1 < xMax; x1 += tileWidth )
					{
						float x2 = x1 + tileWidth;
						if ( x2 > xMax )
						{
							clipped.X = uvMin.X + (uvMax.X - uvMin.X) * (xMax - x1) / (x2 - x1);
							x2 = xMax;
						}
						AddQuad ( buffer, v,
							new Vector2 ( x1, y1 ) + rect.Location,
							new Vector2 ( x2, y2 ) + rect.Location,
							uvMin,
							clipped );
					}
				}
			}

			if ( !hasBorder )
				return;

			// Left and right tiled border
			clipped = uvMax;
			for ( float y1 = yMin; y1 < yMax; y1 += tileHeight )
			{
				float y2 = y1 + tileHeight;
				if ( y2 > yMax )
				{
					clipped.Y = uvMin.Y + (uvMax.Y - uvMin.Y) * (yMax - y1) / (y2 - y1);
					y2 = yMax;
				}
				AddQuad ( buffer, v,
					 new Vector2 ( 0, y1 ) + rect.Location,
					 new Vector2 ( xMin, y2 ) + rect.Location,
					 new Vector2 ( outer.X, uvMin.Y ),
					 new Vector2 ( uvMin.X, clipped.Y ) );
				AddQuad ( buffer, v,
					 new Vector2 ( xMax, y1 ) + rect.Location,
					 new Vector2 ( rect.Width, y2 ) + rect.Location,
					 new Vector2 ( uvMax.X, uvMin.Y ),
					 new Vector2 ( outer.Maximum.X, clipped.Y ) );
			}

			// Bottom and top tiled border
			clipped = uvMax;
			for ( float x1 = xMin; x1 < xMax; x1 += tileWidth )
			{
				float x2 = x1 + tileWidth;
				if ( x2 > xMax )
				{
					clipped.X = uvMin.X + (uvMax.X - uvMin.X) * (xMax - x1) / (x2 - x1);
					x2 = xMax;
				}
				AddQuad ( buffer, v,
					 new Vector2 ( x1, 0 ) + rect.Location,
					 new Vector2 ( x2, yMin ) + rect.Location,
					 new Vector2 ( uvMin.X, outer.Y ),
					 new Vector2 ( clipped.X, uvMin.Y ) );
				AddQuad ( buffer, v,
					 new Vector2 ( x1, yMax ) + rect.Location,
					 new Vector2 ( x2, rect.Height ) + rect.Location,
					 new Vector2 ( uvMin.X, uvMax.Y ),
					 new Vector2 ( clipped.X, outer.Maximum.Y ) );
			}

			// Corners
			AddQuad ( buffer, v,
				 new Vector2 ( 0, 0 ) + rect.Location,
				 new Vector2 ( xMin, yMin ) + rect.Location,
				 new Vector2 ( outer.X, outer.Y ),
				 new Vector2 ( uvMin.X, uvMin.Y ) );
			AddQuad ( buffer, v,
				 new Vector2 ( xMax, 0 ) + rect.Location,
				 new Vector2 ( rect.Width, yMin ) + rect.Location,
				 new Vector2 ( uvMax.X, outer.Y ),
				 new Vector2 ( outer.Maximum.X, uvMin.Y ) );
			AddQuad ( buffer, v,
				 new Vector2 ( 0, yMax ) + rect.Location,
				 new Vector2 ( xMin, rect.Height ) + rect.Location,
				 new Vector2 ( outer.X, uvMax.Y ),
				 new Vector2 ( uvMin.X, outer.Maximum.Y ) );
			AddQuad ( buffer, v,
				 new Vector2 ( xMax, yMax ) + rect.Location,
				 new Vector2 ( rect.Width, rect.Height ) + rect.Location,
				 new Vector2 ( uvMax.X, uvMax.Y ),
				 new Vector2 ( outer.Maximum.X, outer.Maximum.Y ) );
		}

		/// <summary>
		/// Generate vertices for a tiled Image.
		/// </summary>
		private void GetAdjustedBorders ( ref RectangleF border, RectangleF rect )
		{
			var rSize = rect.Size;
			for ( int axis = 0; axis <= 1; axis++ )
			{
				// If the rect is smaller than the combined borders,
				// then there's not room for the borders at their normal size.
				// In order to avoid artefacts with overlapping borders,
				// we scale the borders down to fit.
				float scale = border[axis] + border[axis + 2];
				if ( scale != 0 && rSize[axis] < scale )
				{
					scale = rSize[axis] / scale;
					border[axis] *= scale;
					border[axis + 2] *= scale;
				}
			}
		}

		/// Image's dimensions used for drawing. X = left, Y = bottom, Z = right, W = top.
		private Vector4 GetDrawingDimensions ( bool shouldPreserveAspect )
		{
			Rectangle padding = new Rectangle ();
			Vector2 size = new Vector2 ();

			if ( OverrideSprite != null )
			{
				padding = OverrideSprite.Border;
				size = (Vector2)OverrideSprite.Rect.Size;
			}

			RectangleF r = GetPixelAdjustedRect ();
			// Debug.Log(string.Format("r:{2}, size:{0}, padding:{1}", size, padding, r));

			int spriteW = (int)Math.Round ( size.X );
			int spriteH = (int)Math.Round ( size.Y );

			var v = new Vector4 (
					  padding.X / spriteW,
					  padding.Y / spriteH,
					  (spriteW - padding.Maximum.X) / spriteW,
					  (spriteH - padding.Maximum.Y) / spriteH );

			if ( shouldPreserveAspect && size.LengthSquared () > 0.0f )
			{
				var spriteRatio = size.X / size.Y;
				var rectRatio = r.Width / r.Height;

				if ( spriteRatio > rectRatio )
				{
					var oldHeight = r.Height;
					r.Height = r.Width * (1.0f / spriteRatio);
					r.Y += (oldHeight - r.Height) * Transform.Pivot.Y;
				}
				else
				{
					var oldWidth = r.Width;
					r.Width = r.Height * spriteRatio;
					r.X += (oldWidth - r.Width) * Transform.Pivot.X;
				}
			}

			v = new Vector4 (
				r.X + r.Width * v.X, r.Y + r.Height * v.Y,
				r.X + r.Width * v.Z, r.Y + r.Height * v.W );

			return v;
		}

		/// <summary>
		/// Adjust the specified quad, making it be radially filled instead.
		/// </summary>
		/// <summary>
		/// Adjust the specified quad, making it be radially filled instead.
		/// </summary>
		private Vector2 MapCoordinate ( Vector2 local, RectangleF rect )
		{
			Rectangle spriteRect = Sprite.Rect;
			if ( Type == ImageType.Simple || Type == ImageType.Filled )
				return new Vector2 (
					local.X * spriteRect.Width / rect.Width,
					local.Y * spriteRect.Height / rect.Height );

			RectangleF border = (RectangleF)((Vector4)Sprite.Border / PixelsPerUnit);
			RectangleF adjustedBorder = border;
			GetAdjustedBorders ( ref border, rect );

			for ( int i = 0; i < 2; i++ )
			{
				if ( local[i] <= adjustedBorder[i] )
					continue;

				if ( rect.Size[i] - local[i] <= adjustedBorder[i + 2] )
				{
					local[i] -= (rect.Size[i] - spriteRect.Size[i]);
					continue;
				}

				if ( Type == ImageType.Sliced )
				{
					float lerp = MathUtil.InverseLerp ( adjustedBorder[i], rect.Size[i] - adjustedBorder[i + 2], local[i] );
					local[i] = MathUtil.Lerp ( border[i], spriteRect.Size[i] - border[i + 2], lerp );
					continue;
				}
				else
				{
					local[i] -= adjustedBorder[i];
					local[i] = MathUtil.Repeat ( local[i], spriteRect.Size[i] - border[i] - border[i + 2] );
					local[i] += border[i];
					continue;
				}
			}

			return local;
		}
	}
}