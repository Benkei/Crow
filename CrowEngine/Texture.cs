﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine
{
	abstract class Texture : GLHandler
	{
		public Texture ()
		{
			m_Handler = GL.GenTexture ();
		}

		public void Delete ()
		{
			GL.DeleteTexture ( m_Handler );
		}
	}
}
