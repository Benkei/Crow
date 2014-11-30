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
		public Vector3 Position;
		public Vector3 Normal;
		public Color Color;
		public Vector2 Texcoord0;
		public Vector2 Texcoord1;
		public Vector4 Tangent;

		public static readonly Vertex Default = new Vertex ()
		{
			Position = Vector3.Zero,
			Normal = new Vector3 ( 0, -1, 0 ),
			Color = Color.Black,
			Texcoord0 = Vector2.Zero,
			Texcoord1 = Vector2.Zero,
			Tangent = new Vector4 ( 1f, 0f, 0f, -1f ),
		};
	}
}
