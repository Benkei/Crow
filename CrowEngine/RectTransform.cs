using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrowEngine.Mathematics;

using Rect = CrowEngine.Mathematics.RectangleF;

namespace CrowEngine.Components
{
	struct RectAnchor
	{
		public Vector2 Minimum;
		public Vector2 Maximum;

		public Vector2 Point
		{
			get { return Minimum + (Maximum - Minimum); }
			set { Minimum = Maximum = value; }
		}

		public bool IsStretch
		{
			get { return Minimum != Maximum; }
		}
	}

	class RectTransform : Transform
	{
		private Vector2 m_AnchoredPosition;
		private Vector3 m_AnchoredPosition3D;

		private RectAnchor m_Anchor;

		private Vector2 m_Pivot = new Vector2 ( 0.5f, 0.5f );

		private Rect m_Rect;

		private Vector2 m_SizeDelta = new Vector2 ( 100f, 100f );

		public RectTransform ()
		{
			m_Anchor.Point = new Vector2 ( 0.5f, 0.5f );
		}

		// Summary:
		//     The position of the pivot of this RectTransform relative to the anchor reference
		//     point.
		public Vector2 AnchoredPosition
		{
			get { return m_AnchoredPosition; }
			set { m_AnchoredPosition = value; }
		}
		public Vector3 AnchoredPosition3D
		{
			get { return m_AnchoredPosition3D; }
			set { m_AnchoredPosition3D = value; }
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
		//public Rect Rect { get; }
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
		public void GetLocalCorners ( Vector3 topLeft, Vector3 topRight,
			Vector3 bottomRight, Vector3 bottomLeft )
		{
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
