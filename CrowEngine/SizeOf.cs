using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CrowEngine
{
	public static class SizeOf<T>
	{
		public static readonly int Value = Marshal.SizeOf<T> ();
	}
}
