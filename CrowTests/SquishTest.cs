using System;
using CrowSquish;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CrowTests
{
	[TestClass]
	public unsafe class SquishTest
	{
		[TestMethod]
		public void TestOneColourRandom ()
		{
			TestOneColourRandom ( ModusFlags.kDxt1 | ModusFlags.kColourRangeFit );
		}
		[TestMethod]
		public void TestOneColour ()
		{
			TestOneColour ( ModusFlags.kDxt1 );
		}
		[TestMethod]
		public void TestTwoColour ()
		{
			TestTwoColour ( ModusFlags.kDxt1 );
		}


		double GetColourError ( byte* a, byte* b )
		{
			double error = 0.0;
			for ( int i = 0; i < 16; ++i )
			{
				for ( int j = 0; j < 3; ++j )
				{
					int index = 4 * i + j;
					int diff = (int)a[index] - (int)b[index];
					error += (double)(diff * diff);
				}
			}
			return error / 16.0;
		}

		void TestOneColour ( ModusFlags flags )
		{
			byte* input = stackalloc byte[4 * 16];
			byte* output = stackalloc byte[4 * 16];
			byte* block = stackalloc byte[16];

			double avg = 0.0, min = double.MaxValue, max = double.MinValue;
			int counter = 0;

			// test all single-channel colours
			for ( int i = 0; i < 16 * 4; ++i )
				input[i] = ((i % 4) == 3) ? (byte)255 : (byte)0;
			for ( int channel = 0; channel < 3; ++channel )
			{
				for ( int value = 0; value < 255; ++value )
				{
					// set the channnel value
					for ( int i = 0; i < 16; ++i )
						input[4 * i + channel] = (byte)value;

					// compress and decompress
					Squish.Compress ( (IntPtr)input, (IntPtr)block, flags );
					Squish.Decompress ( (IntPtr)output, (IntPtr)block, flags );

					// test the results
					double rm = GetColourError ( input, output );
					double rms = Math.Sqrt ( rm );

					// accumulate stats
					min = Math.Min ( min, rms );
					max = Math.Max ( max, rms );
					avg += rm;
					++counter;
				}

				// reset the channel value
				for ( int i = 0; i < 16; ++i )
					input[4 * i + channel] = 0;
			}

			// finish stats
			avg = Math.Sqrt ( avg / counter );

			// show stats
			Console.WriteLine ( "one colour error (min, max, avg): " + min + ", " + max + ", " + avg );
		}

		void TestOneColourRandom ( ModusFlags flags )
		{
			byte* input = stackalloc byte[4 * 16];
			byte* output = stackalloc byte[4 * 16];
			byte* block = stackalloc byte[16];

			double avg = 0.0, min = double.MaxValue, max = double.MinValue;
			int counter = 0;

			var rnd = new Random ();

			// test all single-channel colours
			for ( int test = 0; test < 1000; ++test )
			{
				// set a constant random colour
				for ( int channel = 0; channel < 3; ++channel )
				{
					byte value = (byte)rnd.Next ();
					for ( int i = 0; i < 16; ++i )
						input[4 * i + channel] = value;
				}
				for ( int i = 0; i < 16; ++i )
					input[4 * i + 3] = 255;

				// compress and decompress
				Squish.Compress ( (IntPtr)input, (IntPtr)block, flags );
				Squish.Decompress ( (IntPtr)output, (IntPtr)block, flags );

				// test the results
				double rm = GetColourError ( input, output );
				double rms = Math.Sqrt ( rm );

				// accumulate stats
				min = Math.Min ( min, rms );
				max = Math.Max ( max, rms );
				avg += rm;
				++counter;
			}

			// finish stats
			avg = Math.Sqrt ( avg / counter );

			// show stats
			Console.WriteLine ( "random one colour error (min, max, avg): " + min + ", " + max + ", " + avg );
		}

		void TestTwoColour ( ModusFlags flags )
		{
			byte* input = stackalloc byte[4 * 16];
			byte* output = stackalloc byte[4 * 16];
			byte* block = stackalloc byte[16];

			double avg = 0.0, min = double.MaxValue, max = double.MinValue;
			int counter = 0;

			// test all single-channel colours
			for ( int i = 0; i < 16 * 4; ++i )
				input[i] = ((i % 4) == 3) ? (byte)255 : (byte)0;
			for ( int channel = 0; channel < 3; ++channel )
			{
				for ( int value1 = 0; value1 < 255; ++value1 )
				{
					for ( int value2 = value1 + 1; value2 < 255; ++value2 )
					{
						// set the channnel value
						for ( int i = 0; i < 16; ++i )
							input[4 * i + channel] = (byte)((i < 8) ? value1 : value2);

						// compress and decompress
						Squish.Compress ( (IntPtr)input, (IntPtr)block, flags );
						Squish.Decompress ( (IntPtr)output, (IntPtr)block, flags );

						// test the results
						double rm = GetColourError ( input, output );
						double rms = Math.Sqrt ( rm );

						// accumulate stats
						min = Math.Min ( min, rms );
						max = Math.Max ( max, rms );
						avg += rm;
						++counter;
					}
				}

				// reset the channel value
				for ( int i = 0; i < 16; ++i )
					input[4 * i + channel] = 0;
			}

			// finish stats
			avg = Math.Sqrt ( avg / counter );

			// show stats
			Console.WriteLine ( "two colour error (min, max, avg): " + min + ", " + max + ", " + avg );
		}

	}
}
