using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrowSquish
{
	public static partial class Squish
	{
		public static unsafe void CompressImageParallel ( IntPtr rgba, int width, int height, IntPtr blocks, ModusFlags flags )
		{
			int bytesPerBlock = ((flags & ModusFlags.kDxt1) != 0) ? 8 : 16;

			Parallel.ForEach (
				SteppedRange ( 0, width, 4, 0, height, 4 ),
				delegate ( Point2 value, ParallelLoopState loopState, long index )
				{
					// build the 4x4 block of pixels
					byte* sourceRgba = stackalloc byte[16 * 4];
					int* targetPixel = (int*)sourceRgba;
					int mask = 0;
					for ( int py = 0; py < 4; ++py )
					{
						for ( int px = 0; px < 4; ++px )
						{
							// get the source pixel in the image
							int sx = value.X + px;
							int sy = value.Y + py;

							// enable if we're in the image
							if ( sx < width && sy < height )
							{
								int* sourcePixel = (int*)(((byte*)rgba) + 4 * (width * sy + sx));
								// copy the rgba value
								*targetPixel = *sourcePixel;
								targetPixel++;

								// enable this pixel
								mask |= (1 << (4 * py + px));
							}
							else
							{
								// skip this pixel as its outside the image
								targetPixel++;
							}
						}
					}

					// compress it into the output
					CompressMasked (
						(IntPtr)sourceRgba, mask,
						(IntPtr)(((byte*)blocks) + (bytesPerBlock * index)), flags
					);
				}
			);
		}

		public static unsafe void DecompressImageParallel ( IntPtr rgba, int width, int height, IntPtr blocks, ModusFlags flags )
		{
			int bytesPerBlock = ((flags & ModusFlags.kDxt1) != 0) ? 8 : 16;

			Parallel.ForEach (
				SteppedRange ( 0, width, 4, 0, height, 4 ),
				delegate ( Point2 value, ParallelLoopState loopState, long index )
				{
					// decompress the block
					byte* targetRgba = stackalloc byte[4 * 16];
					Decompress ( (IntPtr)targetRgba, (IntPtr)(((byte*)blocks) + (bytesPerBlock * index)), flags );

					// write the decompressed pixels to the correct image locations
					int* sourcePixel = (int*)targetRgba;
					for ( int py = 0; py < 4; ++py )
					{
						for ( int px = 0; px < 4; ++px )
						{
							// get the target location
							int sx = value.X + px;
							int sy = value.Y + py;
							if ( sx < width && sy < height )
							{
								int* targetPixel = (int*)((byte*)rgba + 4 * (width * sy + sx));

								// copy the rgba value
								*targetPixel = *sourcePixel;
								sourcePixel++;
							}
							else
							{
								// skip this pixel as its outside the image
								sourcePixel++;
							}
						}
					}
				}
			);
		}


		private static IEnumerable<Point2> SteppedRange (
			int fromInclusiveX, int toExclusiveX, int stepX,
			int fromInclusiveY, int toExclusiveY, int stepY )
		{
			for ( ; fromInclusiveY < toExclusiveY; fromInclusiveY += stepY )
			{
				for ( int x = fromInclusiveX; x < toExclusiveX; x += stepX )
				{
					yield return new Point2 () { X = x, Y = fromInclusiveY }; ;
				}
			}
		}

		struct Point2
		{
			public int X, Y;
		}
	}
}
