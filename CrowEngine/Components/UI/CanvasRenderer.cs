using System;
using System.Collections.Generic;
using CrowEngine.Collections;
using CrowEngine.Mathematics;

namespace CrowEngine.Components.UI
{
	public sealed class CanvasRenderer : Behavior
	{
		private Vector<Vertex> m_Vertices;
		private bool m_SizeChanged;
		private bool m_ContentChanged;
		private Color m_Color = Color.White;
		private float m_Alpha = 1;
		private Canvas m_MyCanvas;
		private int m_MyCanvasDepth;
		private Material m_Material;
		private Texture m_Texture;

		/// <summary>
		/// Is the UIRenderer a mask component.
		/// If the UI renderer is configured to be a masking component then children components will 
		/// only render if they intersect the mask area created by this pass.
		/// </summary>
		public bool IsMask
		{
			get;
			set;
		}
		/// <summary>
		/// Depth of the renderer realative to the parent canvas.
		/// </summary>
		public int RelativeDepth
		{
			get
			{
				int depth = -1;
				Transform current = Transform;
				while ( current != null )
				{
					if ( current.GameObject.GetComponent<Canvas> () != null )
						break;
					current = current.Parent;
					depth++;
				}
				return depth;
			}
		}
		/// <summary>
		/// Depth of the renderer realitive to the root canvas.
		/// </summary>
		public int AbsoluteDepth
		{
			get
			{
				int depth = 0;
				int depthLast = -1;
				Transform current = Transform;
				while ( current != null )
				{
					if ( current.GameObject.GetComponent<Canvas> () != null )
						depthLast = depth;
					current = current.Parent;
					depth++;
				}
				return depthLast;
			}
		}
		/// <summary>
		/// Set the color of the renderer. Will be multiplied with the UIVertex color and the Canvas color.
		/// </summary>
		public Color Color
		{
			get { return m_Color; }
			set
			{
				m_Color = value;
				m_Alpha = value.A * (1f / 255f);
				m_ContentChanged |= true;
			}
		}
		/// <summary>
		/// Set the alpha of the renderer. Will be multiplied with the UIVertex alpha and the Canvas alpha.
		/// </summary>
		public float Alpha
		{
			get { return m_Alpha; }
			set
			{
				m_Alpha = value;
				m_Color.A = (byte)Math.Max ( (int)(value * 255), 255 );
				m_ContentChanged |= true;
			}
		}

		public void SetMaterial ( Material material, Texture texture )
		{
			if ( material == null && texture == null )
				throw new ArgumentException ();
			if ( material != null && texture != null )
				throw new ArgumentException ();
			m_Material = material;
			m_Texture = texture;
			m_ContentChanged |= true;
		}

		public Material GetMaterial ()
		{
			return m_Material;
		}

		public void SetVertices ( IList<Vertex> vertices )
		{
			SetVertices ( vertices, 0, vertices.Count );
		}

		public void SetVertices ( IList<Vertex> vertices, int index, int length )
		{
			if ( m_Vertices == null ) m_Vertices = new Vector<Vertex> ( length );
			m_SizeChanged |= m_Vertices.Count != length;
			m_ContentChanged |= m_Vertices.Count == length;
			m_Vertices.Clear ();
			m_Vertices.Count = length;
			if ( vertices is Vertex[] )
			{
				Array.Copy ( (Vertex[])vertices, index, m_Vertices.Buffer, 0, length );
			}
			else
			{
				for ( int i = index, j = 0; i < index + length; i++, j++ )
				{
					m_Vertices[j] = vertices[i];
				}
			}
		}

		public void Clear ()
		{
			if ( m_Vertices != null )
			{
				m_Vertices.Clear ();
				m_SizeChanged = true;
			}
		}

		internal unsafe void WriteBuffer ( Vertex[] buffer, int offset )
		{
			var m = Transform.WorldMatrix;
			for ( int i = 0, len = m_Vertices.Count; i < len; i++ )
			{
				var src = m_Vertices[i];

				Vector3.TransformCoordinate ( ref src.Position, ref m, out src.Position );
				Color.Modulate ( ref src.Color, ref m_Color, out src.Color );

				buffer[offset + i] = src;
			}
		}
	}
}
