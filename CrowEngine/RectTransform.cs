using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace CrowEngine
{
	class RectTransform : Transform
	{
		// Summary:
		//     The position of the pivot of this RectTransform relative to the anchor reference
		//     point.
		public Vector2 anchoredPosition { get; set; }
		public Vector3 anchoredPosition3D { get; set; }
		//
		// Summary:
		//     The normalized position in the parent RectTransform that the upper right
		//     corner is anchored to.
		public Vector2 anchorMaximum { get; set; }
		//
		// Summary:
		//     The normalized position in the parent RectTransform that the lower left corner
		//     is anchored to.
		public Vector2 anchorMinimum { get; set; }
		//
		// Summary:
		//     The offset of the upper right corner of the rectangle relative to the upper
		//     right anchor.
		public Vector2 offsetMaximum { get; set; }
		//
		// Summary:
		//     The offset of the lower left corner of the rectangle relative to the lower
		//     left anchor.
		public Vector2 offsetMinimum { get; set; }
		//
		// Summary:
		//     The normalized position in this RectTransform that it rotates around.
		public Vector2 pivot { get; set; }
		//
		// Summary:
		//     The calculated rectangle in the local space of the Transform.
		//public Rect rect { get; }
		//
		// Summary:
		//     The size of this RectTransform relative to the distances between the anchors.
		public Vector2 sizeDelta { get; set; }


		public Vector2 AnchorMinium;
		public Vector2 AnchorMaximum;
		public Vector2 AnchorPosition;
	}
}
