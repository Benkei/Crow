using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace CrowEditor.Serialization
{
	class TextureData
	{
		public int Width;
		public int Height;
		public int LevelCount;
		public PixelInternalFormat Format;
		public int DataSize;
		public byte[] Data;
	}
}
