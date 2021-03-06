﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowSerialization
{
	public interface ISerializer
	{
		void Serialize ( object obj, Stream stream );

		object Deserialize ( Type instanceType, Stream stream );
	}
}
