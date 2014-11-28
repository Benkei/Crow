using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace CrowStudio
{
	static class Wgl
	{
		[DllImport ( "opengl32.dll", EntryPoint = "wglGetCurrentDC" ), SuppressUnmanagedCodeSecurity]
		public static extern IntPtr GetCurrentDC ();

		[DllImport ( "opengl32.dll", EntryPoint = "wglGetCurrentContext" ), SuppressUnmanagedCodeSecurity]
		public static extern IntPtr GetCurrentContext ();

		//[DllImport ( "opengl32.dll", EntryPoint = "wglCreateContext", SetLastError = true ), SuppressUnmanagedCodeSecurity]
		//public static extern IntPtr CreateContext ( IntPtr hdc );

		[DllImport ( "opengl32.dll", EntryPoint = "wglMakeCurrent", SetLastError = true ), SuppressUnmanagedCodeSecurity]
		public static extern int MakeCurrent ( IntPtr hdc, IntPtr hrc );

		[DllImport ( "opengl32.dll", EntryPoint = "wglSwapBuffers", SetLastError = true ), SuppressUnmanagedCodeSecurity]
		public static extern uint SwapBuffers ( IntPtr hdc );

		//[DllImport ( "opengl32.dll", EntryPoint = "wglGetProcAddress", SetLastError = true ), SuppressUnmanagedCodeSecurity]
		//public static extern IntPtr GetProcAddress ( string functionName );
	}
}
