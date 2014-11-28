using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrowEngine.Mathematics;

namespace CrowEngine.Components.UI
{
	public struct Vertex
	{
		public Vector3 position;
		public Vector3 normal;
		public Color color;
		public Vector2 uv0;
		public Vector2 uv1;
		public Vector4 tangent;

		public static readonly Vertex DefaultVertex = new Vertex ()
		{
			position = Vector3.Zero,
			normal = new Vector3 ( 0, -1, 0 ),
			tangent = new Vector4 ( 1f, 0f, 0f, -1f ),
			color = Color.Black,
			uv0 = Vector2.Zero,
			uv1 = Vector2.Zero
		};
	}
}
