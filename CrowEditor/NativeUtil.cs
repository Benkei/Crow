using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CrowEditor
{
	class NativeUtil
	{
		/// <summary>
		/// Determines whether the specified memory pointer is aligned in memory.
		/// </summary>
		/// <param name="memoryPtr">The memory pointer.</param>
		/// <param name="align">The align.</param>
		/// <returns><c>true</c> if the specified memory pointer is aligned in memory; otherwise, <c>false</c>.</returns>
		public static bool IsMemoryAligned ( IntPtr memoryPtr, int align = 16 )
		{
			return ((long)memoryPtr & (align - 1)) == 0;
		}

		/// <summary>
		/// Allocate an aligned memory buffer.
		/// </summary>
		/// <param name="sizeInBytes">Size of the buffer to allocate.</param>
		/// <param name="align">Alignment, 16 bytes by default.</param>
		/// <returns>A pointer to a buffer aligned.</returns>
		/// <remarks>
		/// To free this buffer, call <see cref="FreeMemory"/>.
		/// </remarks>
		public unsafe static IntPtr AllocateMemory ( int sizeInBytes, int align = 16 )
		{
			int mask = align - 1;
			var memPtr = Marshal.AllocHGlobal ( sizeInBytes + mask + IntPtr.Size );

			var ptr = (long)((byte*)memPtr + sizeof ( void* ) + mask) & ~mask;
			((IntPtr*)ptr)[-1] = memPtr;

			return (IntPtr)ptr;
		}

		/// <summary>
		/// Allocate an aligned memory buffer.
		/// </summary>
		/// <returns>A pointer to a buffer aligned.</returns>
		/// <remarks>
		/// The buffer must have been allocated with <see cref="AllocateMemory"/>.
		/// </remarks>
		public unsafe static void FreeMemory ( IntPtr alignedBuffer )
		{
			if ( alignedBuffer == IntPtr.Zero ) return;

			Marshal.FreeHGlobal ( ((IntPtr*)alignedBuffer)[-1] );
		}

	}
}
