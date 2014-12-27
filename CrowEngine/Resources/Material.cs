using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CrowEngine.Mathematics;

namespace CrowEngine
{
	public class Material
	{
		private GLProgram m_Program;
		private Dictionary<string, Uniform> m_UniformMap;
		private Dictionary<string, Uniform4> m_Uniform4Map;
		private Dictionary<string, UniformF4x4> m_UniformF4x4Map;
		private Dictionary<string, UniformD4x4> m_UniformD4x4Map;
		private Dictionary<string, object> m_UniformObjMap;

		public Material ()
		{
		}

		public string[] ProgramKeywords
		{
			get;
			set;
		}


		public bool HasProperty ( string propertyName )
		{
			return (m_UniformMap != null && m_UniformMap.ContainsKey ( propertyName ))
				|| (m_Uniform4Map != null && m_Uniform4Map.ContainsKey ( propertyName ))
				|| (m_UniformF4x4Map != null && m_UniformF4x4Map.ContainsKey ( propertyName ))
				|| (m_UniformD4x4Map != null && m_UniformD4x4Map.ContainsKey ( propertyName ))
				|| (m_UniformObjMap != null && m_UniformObjMap.ContainsKey ( propertyName ));
		}


		public unsafe void GetValue ( string propertyName, out double result )
		{
			Uniform u;
			GetMapValue ( ref m_UniformMap, propertyName, out u, UniformType.Double );
			result = *(double*)u.Data;
		}
		public unsafe void GetValue ( string propertyName, out float result )
		{
			Uniform u;
			GetMapValue ( ref m_UniformMap, propertyName, out u, UniformType.Single );
			result = *(float*)u.Data;
		}
		public unsafe void GetValue ( string propertyName, out int result )
		{
			Uniform u;
			GetMapValue ( ref m_UniformMap, propertyName, out u, UniformType.Int );
			result = *(int*)u.Data;
		}
		public unsafe void GetValue ( string propertyName, out uint result )
		{
			Uniform u;
			GetMapValue ( ref m_UniformMap, propertyName, out u, UniformType.Uint );
			result = *(uint*)u.Data;
		}
		public unsafe void GetValue ( string propertyName, out Vector3 result )
		{
			Uniform4 u;
			GetMapValue ( ref m_Uniform4Map, propertyName, out u, UniformType.Single3 );
			result = *(Vector3*)u.Data;
		}
		public unsafe void GetValue ( string propertyName, out Vector4 result )
		{
			Uniform4 u;
			GetMapValue ( ref m_Uniform4Map, propertyName, out u, UniformType.Single4 );
			result = *(Vector4*)u.Data;
		}
		public unsafe void GetValue ( string propertyName, out Color4 result )
		{
			Uniform4 u;
			GetMapValue ( ref m_Uniform4Map, propertyName, out u, UniformType.Single4 );
			result = *(Color4*)u.Data;
		}
		public unsafe void GetValue ( string propertyName, out Matrix result )
		{
			UniformF4x4 u;
			GetMapValue ( ref m_UniformF4x4Map, propertyName, out u, UniformType.Single4x4 );
			result = *(Matrix*)u.Data;
		}
		public unsafe void GetValue ( string propertyName, out Matrix3x2 result )
		{
			UniformF4x4 u;
			GetMapValue ( ref m_UniformF4x4Map, propertyName, out u, UniformType.Single3x2 );
			result = *(Matrix3x2*)u.Data;
		}
		public unsafe void GetValue ( string propertyName, out Matrix3x3 result )
		{
			UniformF4x4 u;
			GetMapValue ( ref m_UniformF4x4Map, propertyName, out u, UniformType.Single3x3 );
			result = *(Matrix3x3*)u.Data;
		}
		public void GetValue ( string propertyName, out Texture result )
		{
			object obj = null;
			if ( m_UniformObjMap != null )
				m_UniformObjMap.TryGetValue ( propertyName, out obj );
			result = obj as Texture;
		}


		public unsafe void SetValue ( string propertyName, ref double value )
		{
			Uniform u;
			u.Type = UniformType.Double;
			*(double*)u.Data = value;
			SetMapValue ( ref m_UniformMap, propertyName, ref u );
		}
		public unsafe void SetValue ( string propertyName, ref float value )
		{
			Uniform u;
			u.Type = UniformType.Single;
			*(float*)u.Data = value;
			SetMapValue ( ref m_UniformMap, propertyName, ref u );
		}
		public unsafe void SetValue ( string propertyName, ref int value )
		{
			Uniform u;
			u.Type = UniformType.Int;
			*(int*)u.Data = value;
			SetMapValue ( ref m_UniformMap, propertyName, ref u );
		}
		public unsafe void SetValue ( string propertyName, ref uint value )
		{
			Uniform u;
			u.Type = UniformType.Uint;
			*(uint*)u.Data = value;
			SetMapValue ( ref m_UniformMap, propertyName, ref u );
		}
		public unsafe void SetValue ( string propertyName, ref Vector3 value )
		{
			Uniform4 u;
			u.Type = UniformType.Single3;
			*(Vector3*)u.Data = value;
			SetMapValue ( ref m_Uniform4Map, propertyName, ref u );
		}
		public unsafe void SetValue ( string propertyName, ref Vector4 value )
		{
			Uniform4 u;
			u.Type = UniformType.Single4;
			*(Vector4*)u.Data = value;
			SetMapValue ( ref m_Uniform4Map, propertyName, ref u );
		}
		public unsafe void SetValue ( string propertyName, ref Color4 value )
		{
			Uniform4 u;
			u.Type = UniformType.Single4;
			*(Vector4*)u.Data = value;
			SetMapValue ( ref m_Uniform4Map, propertyName, ref u );
		}
		public unsafe void SetValue ( string propertyName, ref Matrix value )
		{
			UniformF4x4 u;
			u.Type = UniformType.Single4x4;
			*(Matrix*)u.Data = value;
			SetMapValue ( ref m_UniformF4x4Map, propertyName, ref u );
		}
		public unsafe void SetValue ( string propertyName, ref Matrix3x2 value )
		{
			UniformF4x4 u;
			u.Type = UniformType.Single3x2;
			*(Matrix3x2*)u.Data = value;
			SetMapValue ( ref m_UniformF4x4Map, propertyName, ref u );
		}
		public unsafe void SetValue ( string propertyName, ref Matrix3x3 value )
		{
			UniformF4x4 u;
			u.Type = UniformType.Single3x3;
			*(Matrix3x3*)u.Data = value;
			SetMapValue ( ref m_UniformF4x4Map, propertyName, ref u );
		}
		public void SetValue ( string propertyName, Texture value )
		{
			var obj = (object)value;
			SetMapValue ( ref m_UniformObjMap, propertyName, ref obj );
		}



		[MethodImpl ( MethodImplOptions.AggressiveInlining )]
		private void GetMapValue<T> ( ref Dictionary<string, T> map, string propertyName, out T value, UniformType type )
			where T : IUniform
		{
			if ( map == null )
				value = default ( T );
			else
				map.TryGetValue ( propertyName, out value );
			if ( value.Type != type )
				value = default ( T );
		}
		[MethodImpl ( MethodImplOptions.AggressiveInlining )]
		private void SetMapValue<T> ( ref Dictionary<string, T> map, string propertyName, ref T value )
		{
			if ( map == null ) map = new Dictionary<string, T> ();
			map[propertyName] = value;
		}


		enum UniformType : byte
		{
			None,
			// Value
			Single, Single2, Single3, Single4,
			Double, Double2, Double3, Double4,
			Int, Int2, Int3, Int4,
			Uint, Uint2, Uint3, Uint4,

			Single2x2, Single2x3, Single2x4,
			Single3x2, Single3x3, Single3x4,
			Single4x2, Single4x3, Single4x4,

			Double2x2, Double2x3, Double2x4,
			Double3x2, Double3x3, Double3x4,
			Double4x2, Double4x3, Double4x4,
		}
		interface IUniform
		{
			UniformType Type { get; }
		}
		unsafe struct Uniform : IUniform
		{
			public UniformType Type;
			public fixed byte Data[sizeof ( double )];
			UniformType IUniform.Type
			{
				get { return Type; }
			}
		}
		unsafe struct Uniform4 : IUniform
		{
			public UniformType Type;
			public fixed byte Data[4 * sizeof ( double )];
			UniformType IUniform.Type
			{
				get { return Type; }
			}
		}
		unsafe struct UniformF4x4 : IUniform
		{
			public UniformType Type;
			public fixed byte Data[4 * 4 * sizeof ( float )];
			UniformType IUniform.Type
			{
				get { return Type; }
			}
		}
		unsafe struct UniformD4x4 : IUniform
		{
			public UniformType Type;
			public fixed byte Data[4 * 4 * sizeof ( double )];
			UniformType IUniform.Type
			{
				get { return Type; }
			}
		}
	}
}
