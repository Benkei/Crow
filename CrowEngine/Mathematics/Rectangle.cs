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
using System.Globalization;
using System.Runtime.InteropServices;

namespace CrowEngine.Mathematics
{
	/// <summary>
	/// Define a Rectangle. This structure is slightly different from System.Drawing.Rectangle as it
	/// is internally storing Left,Top,Right,Bottom instead of Left,Top,Width,Height.
	/// </summary>
	[StructLayout ( LayoutKind.Explicit, Pack = 4 )]
	public struct Rectangle : IEquatable<Rectangle>
	{
		/// <summary>
		/// The left.
		/// </summary>
		[FieldOffset ( 0 )]
		public int Left;

		/// <summary>
		/// The top.
		/// </summary>
		[FieldOffset ( 4 )]
		public int Top;

		/// <summary>
		/// The right.
		/// </summary>
		[FieldOffset ( 8 )]
		public int Right;

		/// <summary>
		/// The bottom.
		/// </summary>
		[FieldOffset ( 12 )]
		public int Bottom;

		/// <summary>
		/// alias fields for Left and Top
		/// </summary>
		[FieldOffset ( 0 )]
		public Int2 Minimum;

		/// <summary>
		/// alias fields for Right and Bottom
		/// </summary>
		[FieldOffset ( 8 )]
		public Int2 Maximum;

		/// <summary>
		/// An empty rectangle.
		/// </summary>
		public static readonly Rectangle Empty;

		static Rectangle ()
		{
			Empty = new Rectangle ();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Rectangle"/> struct.
		/// </summary>
		/// <param name="x">The left.</param>
		/// <param name="y">The top.</param>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		public Rectangle ( int x, int y, int width, int height )
		{
			Minimum = new Int2 ();
			Maximum = new Int2 ();
			Left = x;
			Top = y;
			Right = x + width;
			Bottom = y + height;
		}

		/// <summary>
		/// Gets or sets the X position.
		/// </summary>
		/// <value>The X position.</value>
		public int X
		{
			get { return Left; }
			set { Right = value + Width; Left = value; }
		}

		/// <summary>
		/// Gets or sets the Y position.
		/// </summary>
		/// <value>The Y position.</value>
		public int Y
		{
			get { return Top; }
			set { Bottom = value + Height; Top = value; }
		}

		/// <summary>
		/// Gets or sets the width.
		/// </summary>
		/// <value>The width.</value>
		public int Width
		{
			get { return Right - Left; }
			set { Right = Left + value; }
		}

		/// <summary>
		/// Gets or sets the height.
		/// </summary>
		/// <value>The height.</value>
		public int Height
		{
			get { return Bottom - Top; }
			set { Bottom = Top + value; }
		}

		/// <summary>
		/// Gets a value that indicates whether the rectangle is empty.
		/// </summary>
		/// <value><c>true</c> if [is empty]; otherwise, <c>false</c>.</value>
		public bool IsEmpty
		{
			get { return (Width == 0) && (Height == 0) && (X == 0) && (Y == 0); }
		}

		/// <summary>
		/// Gets or sets the location.
		/// </summary>
		/// <value>The location.</value>
		public Int2 Location
		{
			get { return Minimum; }
			set { Minimum = value; }
		}

		/// <summary>
		/// Gets the Point that specifies the center of the rectangle.
		/// </summary>
		/// <value>The center.</value>
		public Int2 Center
		{
			get { return new Int2 ( X + (Width >> 1), Y + (Height >> 1) ); }
		}

		/// <summary>
		/// Gets or sets the size of the rectangle.
		/// </summary>
		/// <value>The size of the rectangle.</value>
		public Int2 Size
		{
			get { return new Int2 ( Width, Height ); }
			set { Width = value.X; Height = value.Y; }
		}

		/// <summary>
		/// Gets the position of the top-left corner of the rectangle.
		/// </summary>
		/// <value>The top-left corner of the rectangle.</value>
		public Int2 TopLeft
		{
			get { return new Int2 ( Left, Top ); }
		}

		/// <summary>
		/// Gets the position of the top-right corner of the rectangle.
		/// </summary>
		/// <value>The top-right corner of the rectangle.</value>
		public Int2 TopRight
		{
			get { return new Int2 ( Right, Top ); }
		}

		/// <summary>
		/// Gets the position of the bottom-left corner of the rectangle.
		/// </summary>
		/// <value>The bottom-left corner of the rectangle.</value>
		public Int2 BottomLeft
		{
			get { return new Int2 ( Left, Bottom ); }
		}

		/// <summary>
		/// Gets the position of the bottom-right corner of the rectangle.
		/// </summary>
		/// <value>The bottom-right corner of the rectangle.</value>
		public Int2 BottomRight
		{
			get { return new Int2 ( Right, Bottom ); }
		}

		/// <summary>
		/// Gets or sets the component at the specified index.
		/// </summary>
		public int this[int index]
		{
			get
			{
				switch ( index )
				{
					case 0: return Minimum.X;
					case 1: return Minimum.Y;
					case 2: return Maximum.X;
					case 3: return Maximum.Y;
				}
				throw new ArgumentOutOfRangeException ( "index", "Indices for Rectangle run from 0 to 3, inclusive." );
			}
			set
			{
				switch ( index )
				{
					case 0: Minimum.X = value; return;
					case 1: Minimum.Y = value; return;
					case 2: Maximum.X = value; return;
					case 3: Maximum.Y = value; return;
				}
				throw new ArgumentOutOfRangeException ( "index", "Indices for Rectangle run from 0 to 3, inclusive." );
			}
		}

		/// <summary>
		/// Changes the position of the rectangle.
		/// </summary>
		/// <param name="amount">The values to adjust the position of the rectangle by.</param>
		public void Offset ( Int2 amount )
		{
			Offset ( amount.X, amount.Y );
		}

		/// <summary>
		/// Changes the position of the rectangle.
		/// </summary>
		/// <param name="offsetX">Change in the x-position.</param>
		/// <param name="offsetY">Change in the y-position.</param>
		public void Offset ( int offsetX, int offsetY )
		{
			X += offsetX;
			Y += offsetY;
		}

		/// <summary>
		/// Pushes the edges of the rectangle out by the horizontal and vertical values specified.
		/// </summary>
		/// <param name="horizontalAmount">Value to push the sides out by.</param>
		/// <param name="verticalAmount">Value to push the top and bottom out by.</param>
		public void Inflate ( int horizontalAmount, int verticalAmount )
		{
			X -= horizontalAmount;
			Y -= verticalAmount;
			Width += horizontalAmount * 2;
			Height += verticalAmount * 2;
		}

		/// <summary>
		/// Determines whether this rectangle contains a specified point represented by its x- and y-coordinates.
		/// </summary>
		/// <param name="x">The x-coordinate of the specified point.</param>
		/// <param name="y">The y-coordinate of the specified point.</param>
		public bool Contains ( int x, int y )
		{
			return (X <= x) && (x < Right) && (Y <= y) && (y < Bottom);
		}

		/// <summary>
		/// Determines whether this rectangle contains a specified Point.
		/// </summary>
		/// <param name="value">The Point to evaluate.</param>
		public bool Contains ( Int2 value )
		{
			bool result;
			Contains ( ref value, out result );
			return result;
		}

		/// <summary>
		/// Determines whether this rectangle contains a specified Point.
		/// </summary>
		/// <param name="value">The Point to evaluate.</param>
		/// <param name="result">
		/// [OutAttribute] true if the specified Point is contained within this rectangle; false otherwise.
		/// </param>
		public void Contains ( ref Int2 value, out bool result )
		{
			result = (X <= value.X) && (value.X < Right) && (Y <= value.Y) && (value.Y < Bottom);
		}

		/// <summary>
		/// Determines whether this rectangle entirely contains a specified rectangle.
		/// </summary>
		/// <param name="value">The rectangle to evaluate.</param>
		public bool Contains ( Rectangle value )
		{
			bool result;
			Contains ( ref value, out result );
			return result;
		}

		/// <summary>
		/// Determines whether this rectangle entirely contains a specified rectangle.
		/// </summary>
		/// <param name="value">The rectangle to evaluate.</param>
		/// <param name="result">
		/// [OutAttribute] On exit, is true if this rectangle entirely contains the specified
		/// rectangle, or false if not.
		/// </param>
		public void Contains ( ref Rectangle value, out bool result )
		{
			result = (X <= value.X) && (value.Right <= Right) && (Y <= value.Y) && (value.Bottom <= Bottom);
		}

		/// <summary>
		/// Checks, if specified point is inside <see cref="Rectangle"/>.
		/// </summary>
		/// <param name="x">X point coordinate.</param>
		/// <param name="y">Y point coordinate.</param>
		/// <returns><c>true</c> if point is inside <see cref="Rectangle"/>, otherwise <c>false</c>.</returns>
		public bool Contains ( float x, float y )
		{
			return (x >= Left && x <= Right && y >= Top && y <= Bottom);
		}

		/// <summary>
		/// Determines whether a specified rectangle intersects with this rectangle.
		/// </summary>
		/// <param name="value">The rectangle to evaluate.</param>
		public bool Intersects ( Rectangle value )
		{
			bool result;
			Intersects ( ref value, out result );
			return result;
		}

		/// <summary>
		/// Determines whether a specified rectangle intersects with this rectangle.
		/// </summary>
		/// <param name="value">The rectangle to evaluate</param>
		/// <param name="result">
		/// [OutAttribute] true if the specified rectangle intersects with this one; false otherwise.
		/// </param>
		public void Intersects ( ref Rectangle value, out bool result )
		{
			result = (value.X < Right) && (X < value.Right) && (value.Y < Bottom) && (Y < value.Bottom);
		}

		/// <summary>
		/// Creates a rectangle defining the area where one rectangle overlaps with another rectangle.
		/// </summary>
		/// <param name="value1">The first rectangle to compare.</param>
		/// <param name="value2">The second rectangle to compare.</param>
		/// <returns>The intersection rectangle.</returns>
		public static Rectangle Intersect ( Rectangle value1, Rectangle value2 )
		{
			Rectangle result;
			Intersect ( ref value1, ref value2, out result );
			return result;
		}

		/// <summary>
		/// Creates a rectangle defining the area where one rectangle overlaps with another rectangle.
		/// </summary>
		/// <param name="value1">The first rectangle to compare.</param>
		/// <param name="value2">The second rectangle to compare.</param>
		/// <param name="result">[OutAttribute] The area where the two first parameters overlap.</param>
		public static void Intersect ( ref Rectangle value1, ref Rectangle value2, out Rectangle result )
		{
			int newLeft = (value1.X > value2.X) ? value1.X : value2.X;
			int newTop = (value1.Y > value2.Y) ? value1.Y : value2.Y;
			int newRight = (value1.Right < value2.Right) ? value1.Right : value2.Right;
			int newBottom = (value1.Bottom < value2.Bottom) ? value1.Bottom : value2.Bottom;
			if ( (newRight > newLeft) && (newBottom > newTop) )
			{
				result = new Rectangle ( newLeft, newTop, newRight - newLeft, newBottom - newTop );
			}
			else
			{
				result = Empty;
			}
		}

		/// <summary>
		/// Creates a new rectangle that exactly contains two other rectangles.
		/// </summary>
		/// <param name="value1">The first rectangle to contain.</param>
		/// <param name="value2">The second rectangle to contain.</param>
		/// <returns>The union rectangle.</returns>
		public static Rectangle Union ( Rectangle value1, Rectangle value2 )
		{
			Rectangle result;
			Union ( ref value1, ref value2, out result );
			return result;
		}

		/// <summary>
		/// Creates a new rectangle that exactly contains two other rectangles.
		/// </summary>
		/// <param name="value1">The first rectangle to contain.</param>
		/// <param name="value2">The second rectangle to contain.</param>
		/// <param name="result">
		/// [OutAttribute] The rectangle that must be the union of the first two rectangles.
		/// </param>
		public static void Union ( ref Rectangle value1, ref Rectangle value2, out Rectangle result )
		{
			var left = Math.Min ( value1.Left, value2.Left );
			var right = Math.Max ( value1.Right, value2.Right );
			var top = Math.Min ( value1.Top, value2.Top );
			var bottom = Math.Max ( value1.Bottom, value2.Bottom );
			result = new Rectangle ( left, top, right - left, bottom - top );
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		/// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance;
		/// otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals ( object obj )
		{
			if ( ReferenceEquals ( null, obj ) ) return false;
			if ( obj.GetType () != typeof ( Rectangle ) ) return false;
			return Equals ( (Rectangle)obj );
		}

		/// <summary>
		/// Determines whether the specified <see cref="Rectangle"/> is equal to this instance.
		/// </summary>
		/// <param name="other">The <see cref="Rectangle"/> to compare with this instance.</param>
		/// <returns>
		/// <c>true</c> if the specified <see cref="Rectangle"/> is equal to this instance;
		/// otherwise, <c>false</c>.
		/// </returns>
		public bool Equals ( Rectangle other )
		{
			return other.Left == Left && other.Top == Top && other.Right == Right && other.Bottom == Bottom;
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data
		/// structures like a hash table.
		/// </returns>
		public override int GetHashCode ()
		{
			unchecked
			{
				int result = Left;
				result = (result * 397) ^ Top;
				result = (result * 397) ^ Right;
				result = (result * 397) ^ Bottom;
				return result;
			}
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator == ( Rectangle left, Rectangle right )
		{
			return left.Equals ( right );
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator != ( Rectangle left, Rectangle right )
		{
			return !(left == right);
		}

		/// <summary>
		/// Performs an implicit conversion to the <see cref="RectangleF"/> structure.
		/// </summary>
		/// <remarks>Performs direct converstion from int to float.</remarks>
		/// <param name="value">The source <see cref="Rectangle"/> value.</param>
		/// <returns>The converted structure.</returns>
		public static explicit operator RectangleF ( Rectangle value )
		{
			return new RectangleF ( value.X, value.Y, value.Width, value.Height );
		}

		public static explicit operator Vector4 ( Rectangle value )
		{
			return new Vector4 ( value.Left, value.Top, value.Right, value.Bottom );
		}

		public override string ToString ()
		{
			return string.Format ( CultureInfo.InvariantCulture, "X:{0} Y:{1} Width:{2} Height:{3}", X, Y, Width, Height );
		}
	}
}