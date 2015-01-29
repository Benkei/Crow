using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CrowEngine.Mathematics;

namespace CrowEngine.Components
{
	public struct RectAnchor
	{
		public Vector2 Minimum;
		public Vector2 Maximum;

		public Vector2 Point
		{
			get { return IsStretch ? Vector2.Zero : Minimum + (Maximum - Minimum); }
			set { Minimum = Maximum = value; }
		}

		public bool IsStretch
		{
			get { return Minimum != Maximum; }
		}
	}

	public class RectTransform : Transform
	{
		private RectAnchor m_Anchor;
		private Vector2 m_AnchoredPosition;
		// anchor modus point: x:wight y:height
		// anchor modus stretch: x:wightoffset y:heightoffset
		private Vector2 m_SizeDelta;
		private Vector2 m_Pivot;
		private RectangleF m_Rectangle;


		public RectTransform ()
		{
			m_Anchor.Point = new Vector2 ( 0.5f, 0.5f );
			m_Pivot = new Vector2 ( 0.5f, 0.5f );
			m_SizeDelta = new Vector2 ( 100f, 100f );
		}

		public RectTransform RootRect
		{
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			get
			{
				RectTransform last = null;
				RectTransform current = Parent as RectTransform;
				while ( current != null )
				{
					last = current;
					current = current.Parent as RectTransform;
				}
				return last;
			}
		}

		// Summary:
		//     The position of the pivot of this RectTransform relative to the anchor reference
		//     point.
		public Vector2 AnchoredPosition
		{
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			get
			{
				if ( (m_Dirty & (Dirty.NeedParentUpdate | Dirty.NeedRectUpdate)) > 0 )
					UpdateFromParent ();
				return m_AnchoredPosition;
			}
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			set
			{
				m_AnchoredPosition = value;
				NeedUpdate ();
			}
		}
		public Vector3 AnchoredPosition3D
		{
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			get
			{
				if ( (m_Dirty & (Dirty.NeedParentUpdate | Dirty.NeedRectUpdate)) > 0 )
					UpdateFromParent ();
				return new Vector3 ( m_AnchoredPosition, m_LocalPosition.Z );
			}
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			set
			{
				m_AnchoredPosition = (Vector2)value;
				m_LocalPosition.X = value.X;
				m_LocalPosition.Y = value.Y;
				NeedUpdate ();
			}
		}
		//
		// Summary:
		//     The normalized position in the parent RectTransform that the upper right
		//     corner is anchored to.
		public Vector2 AnchorMaximum
		{
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			get { return m_Anchor.Maximum; }
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			set
			{
				if ( value.X > 1 ) value.X = 1;
				else if ( value.X < 0 ) value.X = 0;
				if ( value.Y > 1 ) value.Y = 1;
				else if ( value.Y < 0 ) value.Y = 0;
				m_Anchor.Maximum = value;
				NeedUpdate ();
			}
		}
		public Vector2 AnchorMinimum
		{
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			get { return m_Anchor.Minimum; }
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			set
			{
				if ( value.X > 1 ) value.X = 1;
				else if ( value.X < 0 ) value.X = 0;
				if ( value.Y > 1 ) value.Y = 1;
				else if ( value.Y < 0 ) value.Y = 0;
				m_Anchor.Minimum = value;
				NeedUpdate ();
			}
		}
		public Vector2 OffsetMinimum
		{
			get
			{
				if ( (m_Dirty & (Dirty.NeedParentUpdate | Dirty.NeedRectUpdate)) > 0 )
					UpdateFromParent ();
				return m_AnchoredPosition - m_SizeDelta * m_Pivot;
			}
			set
			{
				Vector2 s = value - (m_AnchoredPosition - m_SizeDelta * m_Pivot);
				m_SizeDelta -= s;
				m_AnchoredPosition += s * (Vector2.One - m_Pivot);
				NeedUpdate ();
			}
		}
		public Vector2 OffsetMaximum
		{
			get
			{
				if ( (m_Dirty & (Dirty.NeedParentUpdate | Dirty.NeedRectUpdate)) > 0 )
					UpdateFromParent ();
				return m_AnchoredPosition + m_SizeDelta * (Vector2.One - m_Pivot);
			}
			set
			{
				Vector2 s = value - (m_AnchoredPosition + m_SizeDelta * (Vector2.One - m_Pivot));
				m_SizeDelta += s;
				m_AnchoredPosition += s * m_Pivot;
				NeedUpdate ();
			}
		}
		//
		// Summary:
		//     The normalized position in this RectTransform that it rotates around.
		public Vector2 Pivot
		{
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			get
			{
				if ( (m_Dirty & (Dirty.NeedParentUpdate | Dirty.NeedRectUpdate)) > 0 )
					UpdateFromParent ();
				return m_Pivot;
			}
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			set
			{
				m_Pivot = value;
				NeedUpdate ();
			}
		}
		//
		// Summary:
		//     The calculated rectangle in the local space of the Transform.
		public RectangleF Rectangle
		{
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			get
			{
				if ( (m_Dirty & (Dirty.NeedParentUpdate | Dirty.NeedRectUpdate)) > 0 )
					UpdateFromParent ();
				return m_Rectangle;
			}
		}
		//
		// Summary:
		//     The size of this RectTransform relative to the distances between the anchors.
		public Vector2 SizeDelta
		{
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			get
			{
				if ( (m_Dirty & (Dirty.NeedParentUpdate | Dirty.NeedRectUpdate)) > 0 )
					UpdateFromParent ();
				return m_SizeDelta;
			}
			[MethodImpl ( MethodImplOptions.AggressiveInlining )]
			set
			{
				m_SizeDelta = value;
				NeedUpdate ();
			}
		}


		// Summary:
		//     Get the corners of the calculated rectangle in the local space of its Transform.
		public void GetLocalCorners ( out Vector3 topLeft, out Vector3 topRight,
			out Vector3 bottomRight, out Vector3 bottomLeft )
		{
			var m = LocalMatrix;
			topLeft = new Vector3 ();
			topRight = new Vector3 ();
			bottomLeft = new Vector3 ();
			bottomRight = new Vector3 ();
		}
		//
		// Summary:
		//     Get the corners of the calculated rectangle in world space.
		public void GetWorldCorners ( Vector3 topLeft, Vector3 topRight,
			Vector3 bottomRight, Vector3 bottomLeft )
		{

		}

		public static bool ScreenPointToWorldPointInRectangle ( RectTransform rect, Vector2 screenPoint, Camera cam, out Vector3 worldPoint )
		{
			worldPoint = new Vector3 ();
			Ray ray = ScreenPointToRay ( cam, screenPoint );
			Plane plane = new Plane ( Vector3.Transform ( Vector3.BackwardLH, rect.Rotation ), rect.Position );
			return Collision.RayIntersectsPlane ( ref ray, ref plane, out worldPoint );
		}

		public static bool ScreenPointToLocalPointInRectangle ( RectTransform rect, Vector2 screenPoint, Camera cam, out Vector2 localPoint )
		{
			localPoint = new Vector2 ();
			Vector3 position;
			if ( ScreenPointToWorldPointInRectangle ( rect, screenPoint, cam, out position ) )
			{
				localPoint = (Vector2)rect.InverseTransformPoint ( position );
				return true;
			}
			return false;
		}

		public static Ray ScreenPointToRay ( Camera cam, Vector2 screenPos )
		{
			if ( cam != null )
			{
				return cam.ScreenPointToRay ( screenPos );
			}
			Vector3 origin = (Vector3)screenPos;
			origin.Z -= 100f;
			return new Ray ( origin, Vector3.ForwardLH );
		}


		protected override void NeedUpdate ( bool forceParentUpdate = false )
		{
			m_Dirty |= Dirty.NeedRectUpdate;
			base.NeedUpdate ( forceParentUpdate );
		}

		protected override void UpdateFromParent ()
		{
			var parent = Parent as RectTransform;
			if ( parent == null )
			{
				m_Rectangle.Minimum = new Vector2 ();
				m_Rectangle.Maximum = m_SizeDelta;
			}
			else
			{
				Vector2 tmp;
				if ( m_Anchor.IsStretch )
				{
					var hostSize = parent.Rectangle.Size;
					tmp = m_SizeDelta * 0.5f;
					m_Rectangle.Minimum = Vector2.Multiply ( hostSize, m_Anchor.Minimum );
					m_Rectangle.Minimum -= tmp;
					m_Rectangle.Maximum = Vector2.Multiply ( hostSize, m_Anchor.Maximum );
					m_Rectangle.Maximum += tmp;

					tmp = m_Rectangle.Minimum + m_Rectangle.Size * m_Pivot;
					m_AnchoredPosition = tmp;
					m_LocalPosition.X = tmp.X;
					m_LocalPosition.Y = tmp.Y;
				}
				else
				{
					tmp = -Vector2.Multiply ( m_SizeDelta, m_Pivot );
					m_Rectangle.Minimum = tmp;
					m_Rectangle.Maximum = tmp + m_SizeDelta;

					m_LocalPosition.X = m_AnchoredPosition.X;
					m_LocalPosition.Y = m_AnchoredPosition.Y;
				}
			}
			base.UpdateFromParent ();
		}
	}
}
