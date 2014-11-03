using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine
{
	class IndicesBuffer : GLBuffer
	{
		private DrawElementsType m_ElementType;

		public DrawElementsType ElementType
		{
			get { return m_ElementType; }
		}

		public IndicesBuffer ( DrawElementsType elementType )
			: base ( BufferTarget.ElementArrayBuffer )
		{
			m_ElementType = elementType;
		}
	}
}
