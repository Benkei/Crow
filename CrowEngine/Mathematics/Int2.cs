// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Runtime.InteropServices;

namespace CrowEngine.Mathematics
{
	/// <summary>
	/// Represents a two dimensional mathematical int vector.
	/// </summary>
	[StructLayout ( LayoutKind.Sequential, Pack = 4 )]
	public struct Int2 : IEquatable<Int2>, IFormattable
	{
		/// <summary>
		/// The size of the <see cref="Int2"/> type, in bytes.
		/// </summary>
		public static readonly int SizeInBytes = Marshal.SizeOf ( typeof ( Int2 ) );

		/// <summary>
		/// A <see cref="Int2"/> with all of its components set to zero.
		/// </summary>
		public static readonly Int2 Zero = new Int2 ();

		/// <summary>
		/// The X unit <see cref="Int2"/> (1, 0).
		/// </summary>
		public static readonly Int2 UnitX = new Int2 ( 1, 0 );

		/// <summary>
		/// The Y unit <see cref="Int2"/> (0, 1).
		/// </summary>
		public static readonly Int2 UnitY = new Int2 ( 0, 1 );

		/// <summary>
		/// A <see cref="Int3"/> with all of its components set to one.
		/// </summary>
		public static readonly Int2 One = new Int2 ( 1, 1 );

		/// <summary>
		/// The X component of the vector.
		/// </summary>
		public int X;

		/// <summary>
		/// The Y component of the vector.
		/// </summary>
		public int Y;

		/// <summary>
		/// Initializes a new instance of the <see cref="Int2"/> struct.
		/// </summary>
		/// <param name="value">The value that will be assigned to all components.</param>
		public Int2 ( int value )
		{
			X = value;
			Y = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Int2"/> struct.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		public Int2 ( int x, int y )
		{
			X = x;
			Y = y;
		}


		public int this[int index]
		{
			get
			{
				switch ( index )
				{
					case 0: return X;
					case 1: return Y;
				}
				throw new IndexOutOfRangeException ();
			}
			set
			{
				switch ( index )
				{
					case 0: X = value; return;
					case 1: Y = value; return;
				}
				throw new IndexOutOfRangeException ();
			}
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="other">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		/// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance;
		/// otherwise, <c>false</c>.
		/// </returns>
		public bool Equals ( Int2 other )
		{
			return other.X == X && other.Y == Y;
		}

		/// <inheritdoc/>
		public override bool Equals ( object obj )
		{
			if ( ReferenceEquals ( null, obj ) ) return false;
			if ( obj.GetType () != typeof ( Int2 ) ) return false;
			return Equals ( (Int2)obj );
		}

		/// <inheritdoc/>
		public override int GetHashCode ()
		{
			unchecked
			{
				return (X * 397) ^ Y;
			}
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator == ( Int2 left, Int2 right )
		{
			return left.Equals ( right );
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator != ( Int2 left, Int2 right )
		{
			return !left.Equals ( right );
		}

		public override string ToString ()
		{
			return string.Format ( "X:{0} Y:{1}", X, Y );
		}

		public string ToString ( IFormatProvider formatProvider )
		{
			return string.Format ( formatProvider, "X:{0} Y:{1}", X, Y );
		}

		public string ToString ( string format, IFormatProvider formatProvider )
		{
			if ( format == null )
				ToString ( formatProvider );
			return string.Format ( formatProvider, "X:{0} Y:{1}", X.ToString ( format, formatProvider ),
				Y.ToString ( format, formatProvider ) );
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="Vector2"/> to <see cref="Int2"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator Int2 ( Vector2 value )
		{
			return new Int2 ( (int)value.X, (int)value.Y );
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="Int2"/> to <see cref="Vector2"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator Vector2 ( Int2 value )
		{
			return new Vector2 ( value.X, value.Y );
		}
	}
}