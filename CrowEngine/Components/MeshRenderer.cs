using System;
using System.Collections.Generic;

namespace CrowEngine.Components
{
	public sealed class MeshRenderer : Renderer
	{
		public Mesh Mesh
		{
			get;
			set;
		}

		//public override Matrix Matrix
		//{
		//	get { return Transform.Matrix; }
		//}

		//public override Matrix LocalMatrix
		//{
		//	get { return Transform.LocalMatrix; }
		//}

		//public override BoundingBox Bounds
		//{
		//	get { throw new NotImplementedException (); }
		//}


		//public override void Render ( DeviceContext context )
		//{
		//	// Set vertex buffer, stride and offset.
		//	var mesh = (CrowEngine.RenderSystems.DX11.DX11Mesh)Mesh;
		//	VertexBufferBinding binding = new VertexBufferBinding ()
		//	{
		//		Buffer = mesh.m_VerticesBuffer,
		//		Stride = mesh.m_VertexStride,
		//		Offset = 0
		//	};
		//	// Set the vertex buffer to active in the input assembler so it can be rendered.
		//	context.InputAssembler.SetVertexBuffers ( 0, binding );
		//	// Set the index buffer to active in the input assembler so it can be rendered.
		//	context.InputAssembler.SetIndexBuffer ( mesh.m_IndicesBuffer, SharpDX.DXGI.Format.R16_UInt, 0 );
		//	// Set the type of primitive that should be rendered from this vertex buffer, in this case triangles.
		//	context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;


		//	// get indices count;
		//	context.DrawIndexed ( 999, 0, 0 );
		//}
	}
}
