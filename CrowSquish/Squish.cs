using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CrowSquish
{
	public static partial class Squish
	{
		/*! @brief Compresses a 4x4 block of pixels.

			@param rgba		The rgba values of the 16 source pixels.
			@param block	Storage for the compressed DXT block.
			@param flags	Compression flags.
	
			The source pixels should be presented as a contiguous array of 16 rgba
			values, with each component as 1 byte each. In memory this should be:
	
				{ r1, g1, b1, a1, .... , r16, g16, b16, a16 }
	
			The flags parameter should specify either kDxt1, kDxt3 or kDxt5 compression, 
			however, DXT1 will be used by default if none is specified. When using DXT1 
			compression, 8 bytes of storage are required for the compressed DXT block. 
			DXT3 and DXT5 compression require 16 bytes of storage per block.
	
			The flags parameter can also specify a preferred colour compressor and 
			colour error metric to use when fitting the RGB components of the data. 
			Possible colour compressors are: kColourClusterFit (the default), 
			kColourRangeFit or kColourIterativeClusterFit. Possible colour error metrics 
			are: kColourMetricPerceptual (the default) or kColourMetricUniform. If no 
			flags are specified in any particular category then the default will be 
			used. Unknown flags are ignored.
	
			When using kColourClusterFit, an additional flag can be specified to
			weight the colour of each pixel by its alpha value. For images that are
			rendered using alpha blending, this can significantly increase the 
			perceived quality.
		*/
		[DllImport ( "squish64.dll", EntryPoint = "Compress", CallingConvention = CallingConvention.Cdecl )]
		static extern void Compress64 ( IntPtr rgba, IntPtr block, ModusFlags flags );
		[DllImport ( "squish32.dll", EntryPoint = "Compress", CallingConvention = CallingConvention.Cdecl )]
		static extern void Compress32 ( IntPtr rgba, IntPtr block, ModusFlags flags );

		public static void Compress ( IntPtr rgba, IntPtr block, ModusFlags flags )
		{
			if ( IntPtr.Size == 4 )
				Compress32 ( rgba, block, flags );
			else
				Compress64 ( rgba, block, flags );
		}

		/*! @brief Compresses a 4x4 block of pixels.

			@param rgba		The rgba values of the 16 source pixels.
			@param mask		The valid pixel mask.
			@param block	Storage for the compressed DXT block.
			@param flags	Compression flags.
	
			The source pixels should be presented as a contiguous array of 16 rgba
			values, with each component as 1 byte each. In memory this should be:
	
				{ r1, g1, b1, a1, .... , r16, g16, b16, a16 }
		
			The mask parameter enables only certain pixels within the block. The lowest
			bit enables the first pixel and so on up to the 16th bit. Bits beyond the
			16th bit are ignored. Pixels that are not enabled are allowed to take
			arbitrary colours in the output block. An example of how this can be used
			is in the CompressImage function to disable pixels outside the bounds of
			the image when the width or height is not divisible by 4.
	
			The flags parameter should specify either kDxt1, kDxt3 or kDxt5 compression, 
			however, DXT1 will be used by default if none is specified. When using DXT1 
			compression, 8 bytes of storage are required for the compressed DXT block. 
			DXT3 and DXT5 compression require 16 bytes of storage per block.
	
			The flags parameter can also specify a preferred colour compressor and 
			colour error metric to use when fitting the RGB components of the data. 
			Possible colour compressors are: kColourClusterFit (the default), 
			kColourRangeFit or kColourIterativeClusterFit. Possible colour error metrics 
			are: kColourMetricPerceptual (the default) or kColourMetricUniform. If no 
			flags are specified in any particular category then the default will be 
			used. Unknown flags are ignored.
	
			When using kColourClusterFit, an additional flag can be specified to
			weight the colour of each pixel by its alpha value. For images that are
			rendered using alpha blending, this can significantly increase the 
			perceived quality.
		*/
		[DllImport ( "squish64.dll", EntryPoint = "CompressMasked", CallingConvention = CallingConvention.Cdecl )]
		static extern void CompressMasked64 ( IntPtr rgba, int mask, IntPtr block, ModusFlags flags );
		[DllImport ( "squish32.dll", EntryPoint = "CompressMasked", CallingConvention = CallingConvention.Cdecl )]
		static extern void CompressMasked32 ( IntPtr rgba, int mask, IntPtr block, ModusFlags flags );

		public static void CompressMasked ( IntPtr rgba, int mask, IntPtr block, ModusFlags flags )
		{
			if ( IntPtr.Size == 4 )
				CompressMasked32 ( rgba, mask, block, flags );
			else
				CompressMasked64 ( rgba, mask, block, flags );
		}

		/*! @brief Decompresses a 4x4 block of pixels.

			@param rgba		Storage for the 16 decompressed pixels.
			@param block	The compressed DXT block.
			@param flags	Compression flags.

			The decompressed pixels will be written as a contiguous array of 16 rgba
			values, with each component as 1 byte each. In memory this is:
	
				{ r1, g1, b1, a1, .... , r16, g16, b16, a16 }
	
			The flags parameter should specify either kDxt1, kDxt3 or kDxt5 compression, 
			however, DXT1 will be used by default if none is specified. All other flags 
			are ignored.
		*/
		[DllImport ( "squish64.dll", EntryPoint = "Decompress", CallingConvention = CallingConvention.Cdecl )]
		static extern void Decompress64 ( IntPtr rgba, IntPtr block, ModusFlags flags );
		[DllImport ( "squish32.dll", EntryPoint = "Decompress", CallingConvention = CallingConvention.Cdecl )]
		static extern void Decompress32 ( IntPtr rgba, IntPtr block, ModusFlags flags );

		public static void Decompress ( IntPtr rgba, IntPtr block, ModusFlags flags )
		{
			if ( IntPtr.Size == 4 )
				Decompress32 ( rgba, block, flags );
			else
				Decompress64 ( rgba, block, flags );
		}

		/*! @brief Computes the amount of compressed storage required.

			@param width	The width of the image.
			@param height	The height of the image.
			@param flags	Compression flags.
	
			The flags parameter should specify either kDxt1, kDxt3 or kDxt5 compression, 
			however, DXT1 will be used by default if none is specified. All other flags 
			are ignored.
	
			Most DXT images will be a multiple of 4 in each dimension, but this 
			function supports arbitrary size images by allowing the outer blocks to
			be only partially used.
		*/
		[DllImport ( "squish64.dll", EntryPoint = "GetStorageRequirements", CallingConvention = CallingConvention.Cdecl )]
		static extern int GetStorageRequirements64 ( int width, int height, ModusFlags flags );
		[DllImport ( "squish32.dll", EntryPoint = "GetStorageRequirements", CallingConvention = CallingConvention.Cdecl )]
		static extern int GetStorageRequirements32 ( int width, int height, ModusFlags flags );

		public static int GetStorageRequirements ( int width, int height, ModusFlags flags )
		{
			if ( IntPtr.Size == 4 )
				return GetStorageRequirements32 ( width, height, flags );
			else
				return GetStorageRequirements64 ( width, height, flags );
		}

		/*! @brief Compresses an image in memory.

			@param rgba		The pixels of the source.
			@param width	The width of the source image.
			@param height	The height of the source image.
			@param blocks	Storage for the compressed output.
			@param flags	Compression flags.
	
			The source pixels should be presented as a contiguous array of width*height
			rgba values, with each component as 1 byte each. In memory this should be:
	
				{ r1, g1, b1, a1, .... , rn, gn, bn, an } for n = width*height
		
			The flags parameter should specify either kDxt1, kDxt3 or kDxt5 compression, 
			however, DXT1 will be used by default if none is specified. When using DXT1 
			compression, 8 bytes of storage are required for each compressed DXT block. 
			DXT3 and DXT5 compression require 16 bytes of storage per block.
	
			The flags parameter can also specify a preferred colour compressor and 
			colour error metric to use when fitting the RGB components of the data. 
			Possible colour compressors are: kColourClusterFit (the default), 
			kColourRangeFit or kColourIterativeClusterFit. Possible colour error metrics 
			are: kColourMetricPerceptual (the default) or kColourMetricUniform. If no 
			flags are specified in any particular category then the default will be 
			used. Unknown flags are ignored.
	
			When using kColourClusterFit, an additional flag can be specified to
			weight the colour of each pixel by its alpha value. For images that are
			rendered using alpha blending, this can significantly increase the 
			perceived quality.
	
			Internally this function calls squish::Compress for each block. To see how
			much memory is required in the compressed image, use
			squish::GetStorageRequirements.
		*/
		[DllImport ( "squish64.dll", EntryPoint = "CompressImage", CallingConvention = CallingConvention.Cdecl )]
		static extern void CompressImage64 ( IntPtr rgba, int width, int height, IntPtr blocks, ModusFlags flags );
		[DllImport ( "squish32.dll", EntryPoint = "CompressImage", CallingConvention = CallingConvention.Cdecl )]
		static extern void CompressImage32 ( IntPtr rgba, int width, int height, IntPtr blocks, ModusFlags flags );

		public static void CompressImage ( IntPtr rgba, int width, int height, IntPtr blocks, ModusFlags flags )
		{
			if ( IntPtr.Size == 4 )
				CompressImage32 ( rgba, width, height, blocks, flags );
			else
				CompressImage64 ( rgba, width, height, blocks, flags );
		}

		/*! @brief Decompresses an image in memory.

			@param rgba		Storage for the decompressed pixels.
			@param width	The width of the source image.
			@param height	The height of the source image.
			@param blocks	The compressed DXT blocks.
			@param flags	Compression flags.
	
			The decompressed pixels will be written as a contiguous array of width*height
			16 rgba values, with each component as 1 byte each. In memory this is:
	
				{ r1, g1, b1, a1, .... , rn, gn, bn, an } for n = width*height
		
			The flags parameter should specify either kDxt1, kDxt3 or kDxt5 compression, 
			however, DXT1 will be used by default if none is specified. All other flags 
			are ignored.

			Internally this function calls squish::Decompress for each block.
		*/
		[DllImport ( "squish64.dll", EntryPoint = "DecompressImage", CallingConvention = CallingConvention.Cdecl )]
		static extern void DecompressImage64 ( IntPtr rgba, int width, int height, IntPtr blocks, ModusFlags flags );
		[DllImport ( "squish32.dll", EntryPoint = "DecompressImage", CallingConvention = CallingConvention.Cdecl )]
		static extern void DecompressImage32 ( IntPtr rgba, int width, int height, IntPtr blocks, ModusFlags flags );

		public static void DecompressImage ( IntPtr rgba, int width, int height, IntPtr blocks, ModusFlags flags )
		{
			if ( IntPtr.Size == 4 )
				DecompressImage32 ( rgba, width, height, blocks, flags );
			else
				DecompressImage64 ( rgba, width, height, blocks, flags );
		}

	}
}
