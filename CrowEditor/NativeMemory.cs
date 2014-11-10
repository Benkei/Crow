using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CrowEditor
{
	/// <summary>
	/// Aligned native memory
	/// </summary>
	public struct NativeMemory : IDisposable
	{
		public IntPtr Pointer;

		private NativeMemory ( IntPtr pointer )
		{
			Pointer = pointer;
		}

		public IntPtr ReadIntPtr ( int ofs )
		{
			return Pointer == IntPtr.Zero ? IntPtr.Zero : Marshal.ReadIntPtr ( Pointer, ofs );
		}

		public string ReadStringAnsi ( int len )
		{
			return Pointer == IntPtr.Zero ? null : Marshal.PtrToStringAnsi ( Pointer, len );
		}

		public string ReadStringAnsi ()
		{
			return Pointer == IntPtr.Zero ? null : Marshal.PtrToStringAnsi ( Pointer );
		}

		public void Write ( int ofs, IntPtr value )
		{
			if ( Pointer != IntPtr.Zero )
			{
				Marshal.WriteIntPtr ( Pointer, ofs, value );
			}
		}

		public void Dispose ()
		{
			NativeUtil.FreeMemory ( Pointer );
			Pointer = IntPtr.Zero;
		}

		public static NativeMemory Allocate ( int sizeInBytes, int align = 16 )
		{
			return new NativeMemory ( sizeInBytes > 0 ? NativeUtil.AllocateMemory ( sizeInBytes, align ) : IntPtr.Zero );
		}

		public static explicit operator IntPtr ( NativeMemory value )
		{
			return value.Pointer;
		}

		public static explicit operator NativeMemory ( IntPtr value )
		{
			return new NativeMemory ( value );
		}
	}

}
