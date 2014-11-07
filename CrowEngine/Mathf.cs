using SharpDX;
using System;

namespace CrowEngine
{
	public static class Mathf
	{
		/// <summary>
		/// The value for which all absolute numbers smaller than are considered equal to zero.
		/// </summary>
		public const float ZeroTolerance = 1e-6f;

		/// <summary>
		/// A value specifying the approximation of π which is 180 degrees.
		/// </summary>
		public const float Pi = 3.141592653589793239f;

		/// <summary>
		/// A value specifying the approximation of 2π which is 360 degrees.
		/// </summary>
		public const float TwoPi = 6.283185307179586477f;

		/// <summary>
		/// A value specifying the approximation of π/2 which is 90 degrees.
		/// </summary>
		public const float PiOver2 = 1.570796326794896619f;

		/// <summary>
		/// A value specifying the approximation of π/4 which is 45 degrees.
		/// </summary>
		public const float PiOver4 = 0.785398163397448310f;

		/// <summary>
		/// Converts degrees to radians.
		/// </summary>
		public const float Deg2Rad = (float)(Pi / 180.0);
		/// <summary>
		/// Converts radians to degrees.
		/// </summary>
		public const float Rad2Deg = (float)(180.0 / Pi);


		/// <summary>
		/// Convert a quaternion to yaw pitch roll vector
		/// from http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToEuler/index.htm
		/// </summary>
		/// <param name="qu"></param>
		/// <returns></returns>
		public static void QuaternionToYawPitchRoll ( ref Quaternion qu, out Vector3 result )
		{
			const float Epsilon = 0.0009765625f;
			const float Threshold = 0.5f - Epsilon;
			// result
			// x = yaw
			// y = pitch
			// z = roll
			float TEST = qu.X * qu.Y + qu.Z * qu.W;
			if ( TEST < -Threshold || TEST > Threshold )
			{
				float sign = Math.Sign ( TEST );
				result.X = sign * 2f * (float)Math.Atan2 ( qu.X, qu.W );
				result.Y = sign * Mathf.PiOver2;
				result.Z = 0;
			}
			else
			{
				float XX = qu.X * qu.X;
				float XZ = qu.X * qu.Z;
				float XW = qu.X * qu.W;

				float YY = qu.Y * qu.Y;
				float YW = qu.Y * qu.W;
				float YZ = qu.Y * qu.Z;

				float ZZ = qu.Z * qu.Z;

				result.X = (float)Math.Atan2 ( 2 * YW - 2 * XZ, 1 - 2 * YY - 2 * ZZ );
				result.Y = (float)Math.Atan2 ( 2 * XW - 2 * YZ, 1 - 2 * XX - 2 * ZZ );
				result.Z = (float)Math.Asin ( 2 * TEST );
			}
		}
		/// <summary>
		/// Create a direction Quaternion from a forward and up vector
		/// </summary>
		/// <param name="forward"></param>
		/// <param name="upwards"></param>
		/// <param name="result"></param>
		public static void QuaternionLookRotation ( ref Vector3 forward, ref Vector3 upwards, out Quaternion result )
		{
			Matrix mat;
			MatrixLookDirectionRH ( ref forward, ref upwards, out mat );
			mat.Transpose ();
			Quaternion.RotationMatrix ( ref mat, out result );
		}
		/// <summary>
		/// Creates a right-handed, look direction matrix.
		/// </summary>
		/// <param name="forward"></param>
		/// <param name="upwards"></param>
		/// <returns></returns>
		public static void MatrixLookDirectionRH ( ref Vector3 forward, ref Vector3 upwards, out Matrix result )
		{
			Vector3 xaxis, yaxis, zaxis;
			zaxis = forward; zaxis.Normalize ();
			Vector3.Cross ( ref upwards, ref zaxis, out xaxis ); xaxis.Normalize ();
			Vector3.Cross ( ref zaxis, ref xaxis, out yaxis );
			result.M11 = xaxis.X; result.M21 = xaxis.Y; result.M31 = xaxis.Z; result.M41 = 0;
			result.M12 = yaxis.X; result.M22 = yaxis.Y; result.M32 = yaxis.Z; result.M42 = 0;
			result.M13 = zaxis.X; result.M23 = zaxis.Y; result.M33 = zaxis.Z; result.M43 = 0;
			result.M14 = 0; result.M24 = 0; result.M34 = 0; result.M44 = 1f;
		}
		/// <summary>
		/// Creates a left-handed, look direction matrix.
		/// </summary>
		/// <param name="forward"></param>
		/// <param name="upwards"></param>
		/// <returns></returns>
		public static void MatrixLookDirectionLH ( ref Vector3 forward, ref Vector3 upwards, out Matrix result )
		{
			Vector3 xaxis, yaxis, zaxis;
			zaxis = -forward; zaxis.Normalize ();
			Vector3.Cross ( ref upwards, ref zaxis, out xaxis ); xaxis.Normalize ();
			Vector3.Cross ( ref zaxis, ref xaxis, out yaxis );
			result.M11 = xaxis.X; result.M21 = xaxis.Y; result.M31 = xaxis.Z; result.M41 = 0;
			result.M12 = yaxis.X; result.M22 = yaxis.Y; result.M32 = yaxis.Z; result.M42 = 0;
			result.M13 = zaxis.X; result.M23 = zaxis.Y; result.M33 = zaxis.Z; result.M43 = 0;
			result.M14 = 0; result.M24 = 0; result.M34 = 0; result.M44 = 1f;
		}

		//public static void QuaternionFromToDirection ( ref Vector3 fromDirection, ref Vector3 toDirection, out Quaternion result ) {
		//}

		public static void QuaternionDelta ( ref Quaternion from, ref Quaternion to, out Quaternion result )
		{
			Quaternion.Invert ( ref from, out result );
			Quaternion.Multiply ( ref result, ref to, out result );
		}
	}
}
