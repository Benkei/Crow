using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CrowEngine
{
	public static class Crow
	{
		public static void Init ()
		{
			LoadLibrary ( "freetype.dll" );
		}

		public static void Run ()
		{

		}


		internal static void LoadLibrary ( string name )
		{
			string dllPath = Environment.CurrentDirectory + (IntPtr.Size == 8 ? "\\lib64\\" : "\\lib\\") + name;
			IntPtr hmod = LoadLibraryW ( dllPath );
			if ( hmod == IntPtr.Zero )
				throw new System.ComponentModel.Win32Exception ( Marshal.GetLastWin32Error (), dllPath );
		}

		[DllImport ( "kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true )]
		private static extern IntPtr LoadLibraryW ( string lpFileName );
	}
}
