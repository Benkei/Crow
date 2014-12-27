using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrowEngine.Components;
using CrowEngine.Mathematics;

namespace CrowEngine.Components.UI
{
	public class CanvasRenderer : Behavior
	{
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
				int depth = 0;
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
				int depthLast = 0;
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
		/// Set the alpha of the renderer. Will be multiplied with the UIVertex alpha and the Canvas alpha.
		/// </summary>
		public Color Color
		{
			get;
			set;
		}
		/// <summary>
		/// Set the color of the renderer. Will be multiplied with the UIVertex color and the Canvas color.
		/// </summary>
		public float Alpha
		{
			get;
			set;
		}

		public void SetMaterial ( Material material, Texture texture )
		{

		}

		public Material GetMaterial ()
		{
			return null;
		}

		public void SetVertices ( IList<Vertex> vertices, int index, int length )
		{
		}

		public void SetVertices ( Vertex[] vertices, int size )
		{
		}

		public void Clear ()
		{

		}
	}
}
