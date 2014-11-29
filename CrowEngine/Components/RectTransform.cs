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

	public struct RectangleBounds
	{
		public Vector2 Minimum;
		public Vector2 Maximum;

		public Vector2 Size
		{
			get { return Maximum - Minimum; }
		}
	}

	public class RectTransform : Transform
	{
		private RectAnchor m_Anchor;
		private Vector3 m_AnchoredPosition;
		// wenn anchor point ist es wight & height
		// anchor auf stretch da ist offset zur kante
		private Vector2 m_SizeDelta;
		private Vector2 m_Pivot;
		private RectangleBounds m_Rectangle;

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
			get { return (Vector2)m_AnchoredPosition; }
			set { m_AnchoredPosition.X = value.X; m_AnchoredPosition.Y = value.Y; }
		}
		public Vector3 AnchoredPosition3D
		{
			get { return m_AnchoredPosition; }
			set { m_AnchoredPosition = value; }
		}
		//
		// Summary:
		//     The normalized position in the parent RectTransform that the upper right
		//     corner is anchored to.
		public RectAnchor Anchor
		{
			get { return m_Anchor; }
			set { m_Anchor = value; }
		}
		//
		// Summary:
		//     The normalized position in this RectTransform that it rotates around.
		public Vector2 Pivot
		{
			get { return m_Pivot; }
			set { m_Pivot = value; }
		}
		//
		// Summary:
		//     The calculated rectangle in the local space of the Transform.
		public RectangleBounds Rect
		{
			get
			{
				var p = Parent as RectTransform;
				if ( p == null )
				{
					m_Rectangle.Minimum = new Vector2 ();
					m_Rectangle.Maximum = m_SizeDelta;
				}
				else
				{
					var host = p.Rect;
					if ( m_Anchor.IsStretch )
					{
						var padding = m_SizeDelta * 0.5f;
						m_Rectangle.Minimum = host.Size;
						m_Rectangle.Minimum.X *= m_Anchor.Minimum.X;
						m_Rectangle.Minimum.Y *= m_Anchor.Minimum.Y;
						m_Rectangle.Minimum += host.Minimum;
						m_Rectangle.Maximum -= padding;

						m_Rectangle.Maximum = -host.Size;
						m_Rectangle.Maximum.X *= m_Anchor.Maximum.X;
						m_Rectangle.Maximum.Y *= m_Anchor.Maximum.Y;
						m_Rectangle.Maximum += host.Maximum;
						m_Rectangle.Maximum += padding;
					}
					else
					{
						var point = host.Minimum;
						point += Vector2.Multiply ( host.Size, m_Anchor.Point );
						point += (Vector2)m_AnchoredPosition;

						point -= Vector2.Multiply ( m_SizeDelta, m_Pivot );

						m_Rectangle.Minimum = point;
						m_Rectangle.Maximum = point + m_SizeDelta;
					}
				}
				return m_Rectangle;
			}
		}
		//
		// Summary:
		//     The size of this RectTransform relative to the distances between the anchors.
		public Vector2 SizeDelta
		{
			get { return m_SizeDelta; }
			set { m_SizeDelta = value; }
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
	}
}
