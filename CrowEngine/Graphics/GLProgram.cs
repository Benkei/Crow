using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrowEngine.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace CrowEngine
{
	public class GLProgram : GLObject
	{
		private GLShader m_FragmentShader;
		private GLShader m_VertexShader;
		private GLShader m_GeometryShader;
		private GLShader m_TessEvaluationShader;
		private GLShader m_TessControlShader;
		private GLShader m_ComputeShader;

		//private Dictionary<string, object> 

		public GLProgram ()
		{
			m_Handler = GL.CreateProgram ();
		}

		public bool IsLinked
		{
			get
			{
				int value;
				GL.GetProgram ( m_Handler, GetProgramParameterName.LinkStatus, out value );
				return value == 1;
			}
		}

		public bool IsValid
		{
			get
			{
				int value;
				GL.GetProgram ( m_Handler, GetProgramParameterName.ValidateStatus, out value );
				return value == 1;
			}
		}

		public int AttachedShaders
		{
			get
			{
				int value;
				GL.GetProgram ( m_Handler, GetProgramParameterName.AttachedShaders, out value );
				return value;
			}
		}

		public bool BinaryRetrievable
		{
			get
			{
				int value;
				GL.GetProgram ( m_Handler, GetProgramParameterName.ProgramBinaryRetrievableHint, out value );
				return value == 1;
			}
		}

		public int BinaryLength
		{
			get
			{
				int value;
				GL.GetProgram ( m_Handler, (GetProgramParameterName)All.ProgramBinaryLength, out value );
				return value;
			}
		}

		public void Delete ()
		{
			GL.DeleteProgram ( m_Handler );
		}

		public void Use ()
		{
			GL.UseProgram ( m_Handler );
		}

		public void Link ()
		{
			GL.LinkProgram ( m_Handler );
		}

		public void DetachAllShaders ()
		{
			DetachShader ( ref m_FragmentShader );
			DetachShader ( ref m_VertexShader );
			DetachShader ( ref m_GeometryShader );
			DetachShader ( ref m_TessEvaluationShader );
			DetachShader ( ref m_TessControlShader );
			DetachShader ( ref m_ComputeShader );
		}

		public unsafe void Reflection ()
		{
			if ( !IsLinked )
			{
				Console.WriteLine ( GL.GetProgramInfoLog ( m_Handler ) );
				throw new InvalidOperationException ();
			}

			int i, value, count, nameLength;
			StringBuilder nameBuffer = new StringBuilder ();

			#region get attributes
			GL.GetProgram ( m_Handler, GetProgramParameterName.ActiveAttributes, out count );
			GL.GetProgram ( m_Handler, GetProgramParameterName.ActiveAttributeMaxLength, out nameLength );

			nameBuffer.Length = 0;
			nameBuffer.Capacity = Math.Max ( nameBuffer.Capacity, nameLength );

			for ( i = 0; i < count; i++ )
			{
				int size;
				ActiveAttribType type;
				int length;

				GL.GetActiveAttrib ( m_Handler, i, nameLength, out length, out size, out type, nameBuffer );

				var name = nameBuffer.ToString ();
				var address = GL.GetAttribLocation ( m_Handler, name );

				nameBuffer.Length = 0;
				Console.WriteLine ( "Attribute " + name + " " + type + " " + size );
			}
			#endregion

			#region get uniforms
			GL.GetProgram ( m_Handler, GetProgramParameterName.ActiveUniforms, out count );
			GL.GetProgram ( m_Handler, GetProgramParameterName.ActiveUniformMaxLength, out nameLength );

			nameBuffer.Length = 0;
			nameBuffer.Capacity = Math.Max ( nameBuffer.Capacity, nameLength );

			for ( i = 0; i < count; i++ )
			{
				ActiveUniform uni;

				GL.GetActiveUniforms ( m_Handler, 1, ref i, ActiveUniformParameter.UniformType, out value );
				uni.Type = (ActiveUniformType)value;
				GL.GetActiveUniforms ( m_Handler, 1, ref i, ActiveUniformParameter.UniformSize, out uni.Size );
				//GL.GetActiveUniforms ( m_Handler, 1, ref i, ActiveUniformParameter.UniformNameLength, out value );
				GL.GetActiveUniforms ( m_Handler, 1, ref i, ActiveUniformParameter.UniformBlockIndex, out uni.BlockIndex );
				GL.GetActiveUniforms ( m_Handler, 1, ref i, ActiveUniformParameter.UniformOffset, out uni.Offset );
				GL.GetActiveUniforms ( m_Handler, 1, ref i, ActiveUniformParameter.UniformArrayStride, out uni.ArrayStride );
				GL.GetActiveUniforms ( m_Handler, 1, ref i, ActiveUniformParameter.UniformMatrixStride, out uni.MatrixStride );
				GL.GetActiveUniforms ( m_Handler, 1, ref i, ActiveUniformParameter.UniformIsRowMajor, out value );
				uni.IsRowMajor = value == 1;
				GL.GetActiveUniforms ( m_Handler, 1, ref i, ActiveUniformParameter.UniformAtomicCounterBufferIndex, out uni.AtomicCounterBufferIndex );

				GL.GetActiveUniformName ( m_Handler, i, nameLength, out value, nameBuffer );

				uni.Name = nameBuffer.ToString ();

				if ( uni.BlockIndex == -1 )
					uni.Location = GL.GetUniformLocation ( m_Handler, uni.Name );

				nameBuffer.Length = 0;
			}
			#endregion

			#region get uniform blocks
			GL.GetProgram ( m_Handler, GetProgramParameterName.ActiveUniformBlocks, out count );
			GL.GetProgram ( m_Handler, GetProgramParameterName.ActiveUniformBlockMaxNameLength, out nameLength );

			nameBuffer.Length = 0;
			nameBuffer.Capacity = Math.Max ( nameBuffer.Capacity, nameLength );

			for ( i = 0; i < count; i++ )
			{
				ActiveUniformBlock uni;
				GL.GetActiveUniformBlock ( m_Handler, i, ActiveUniformBlockParameter.UniformBlockBinding, out uni.Binding );
				GL.GetActiveUniformBlock ( m_Handler, i, ActiveUniformBlockParameter.UniformBlockDataSize, out uni.DataSize );
				GL.GetActiveUniformBlock ( m_Handler, i, ActiveUniformBlockParameter.UniformBlockActiveUniforms, out value );
				if ( value > 0 )
				{
					uni.ActiveUniformIndices = new int[value];
					fixed ( int* ptr = uni.ActiveUniformIndices )
						GL.GetActiveUniformBlock ( m_Handler, i, ActiveUniformBlockParameter.UniformBlockActiveUniformIndices, ptr );
				}
				else
				{
					uni.ActiveUniformIndices = Arrays<int>.Empty;
				}
				GL.GetActiveUniformBlock ( m_Handler, i, ActiveUniformBlockParameter.UniformBlockNameLength, out value );
				GL.GetActiveUniformBlockName ( m_Handler, i, nameLength, out value, nameBuffer );
				uni.Name = nameBuffer.ToString ();

				GL.GetActiveUniformBlock ( m_Handler, i, ActiveUniformBlockParameter.UniformBlockReferencedByTessControlShader, out uni.ReferencedByTessControlShader );
				GL.GetActiveUniformBlock ( m_Handler, i, ActiveUniformBlockParameter.UniformBlockReferencedByTessEvaluationShader, out uni.ReferencedByTessEvaluationShader );
				GL.GetActiveUniformBlock ( m_Handler, i, ActiveUniformBlockParameter.UniformBlockReferencedByVertexShader, out uni.ReferencedByVertexShader );
				GL.GetActiveUniformBlock ( m_Handler, i, ActiveUniformBlockParameter.UniformBlockReferencedByGeometryShader, out uni.ReferencedByGeometryShader );
				GL.GetActiveUniformBlock ( m_Handler, i, ActiveUniformBlockParameter.UniformBlockReferencedByFragmentShader, out uni.ReferencedByFragmentShader );
				GL.GetActiveUniformBlock ( m_Handler, i, ActiveUniformBlockParameter.UniformBlockReferencedByComputeShader, out uni.ReferencedByComputeShader );

				uni.BlockIndex = GL.GetUniformBlockIndex ( m_Handler, uni.Name );

				nameBuffer.Length = 0;
			}
			#endregion

			Console.WriteLine ();
		}

		struct ActiveUniform
		{
			public string Name;
			public int Location;
			public ActiveUniformType Type;
			public int Size;
			//public int NameLength;
			public int BlockIndex;
			public int Offset;
			public int ArrayStride;
			public int MatrixStride;
			public bool IsRowMajor;
			public int AtomicCounterBufferIndex;
		}

		struct ActiveUniformBlock
		{
			public string Name;
			public int BlockIndex;

			public int Binding;
			public int DataSize;
			public int[] ActiveUniformIndices;

			public int ReferencedByTessControlShader;
			public int ReferencedByTessEvaluationShader;
			public int ReferencedByVertexShader;
			public int ReferencedByGeometryShader;
			public int ReferencedByFragmentShader;
			public int ReferencedByComputeShader;
		}


		#region Values

		public unsafe void GetValue ( int location, out double result )
		{
			GL.GetUniform ( m_Handler, location, out result );
		}
		public unsafe void GetValue ( int location, out float result )
		{
			GL.GetUniform ( m_Handler, location, out result );
		}
		public unsafe void GetValue ( int location, out int result )
		{
			GL.GetUniform ( m_Handler, location, out result );
		}
		public unsafe void GetValue ( int location, out uint result )
		{
			fixed ( uint* ptr = &result )
				GL.GetUniform ( m_Handler, location, (int*)ptr );
		}
		public unsafe void GetValue ( int location, out Vector3 result )
		{
			fixed ( Vector3* ptr = &result )
				GL.GetUniform ( m_Handler, location, (float*)ptr );
		}
		public unsafe void GetValue ( int location, out Vector4 result )
		{
			fixed ( Vector4* ptr = &result )
				GL.GetUniform ( m_Handler, location, (float*)ptr );
		}
		public unsafe void GetValue ( int location, out Color4 result )
		{
			fixed ( Color4* ptr = &result )
				GL.GetUniform ( m_Handler, location, (float*)ptr );
		}
		public unsafe void GetValue ( int location, out Matrix result )
		{
			fixed ( Matrix* ptr = &result )
				GL.GetUniform ( m_Handler, location, (float*)ptr );
		}
		public unsafe void GetValue ( int location, out Matrix3x2 result )
		{
			fixed ( Matrix3x2* ptr = &result )
				GL.GetUniform ( m_Handler, location, (float*)ptr );
		}
		public unsafe void GetValue ( int location, out Matrix3x3 result )
		{
			fixed ( Matrix3x3* ptr = &result )
				GL.GetUniform ( m_Handler, location, (float*)ptr );
		}

		public unsafe void GetValue ( string propertyName, out double result )
		{
			var loc = GL.GetUniformLocation ( m_Handler, propertyName );
			GetValue ( loc, out result );
		}
		public unsafe void GetValue ( string propertyName, out float result )
		{
			var loc = GL.GetUniformLocation ( m_Handler, propertyName );
			GetValue ( loc, out result );
		}
		public unsafe void GetValue ( string propertyName, out int result )
		{
			var loc = GL.GetUniformLocation ( m_Handler, propertyName );
			GetValue ( loc, out result );
		}
		public unsafe void GetValue ( string propertyName, out uint result )
		{
			var loc = GL.GetUniformLocation ( m_Handler, propertyName );
			GetValue ( loc, out result );
		}
		public unsafe void GetValue ( string propertyName, out Vector3 result )
		{
			var loc = GL.GetUniformLocation ( m_Handler, propertyName );
			GetValue ( loc, out result );
		}
		public unsafe void GetValue ( string propertyName, out Vector4 result )
		{
			var loc = GL.GetUniformLocation ( m_Handler, propertyName );
			GetValue ( loc, out result );
		}
		public unsafe void GetValue ( string propertyName, out Color4 result )
		{
			var loc = GL.GetUniformLocation ( m_Handler, propertyName );
			GetValue ( loc, out result );
		}
		public unsafe void GetValue ( string propertyName, out Matrix result )
		{
			var loc = GL.GetUniformLocation ( m_Handler, propertyName );
			GetValue ( loc, out result );
		}
		public unsafe void GetValue ( string propertyName, out Matrix3x2 result )
		{
			var loc = GL.GetUniformLocation ( m_Handler, propertyName );
			GetValue ( loc, out result );
		}
		public unsafe void GetValue ( string propertyName, out Matrix3x3 result )
		{
			var loc = GL.GetUniformLocation ( m_Handler, propertyName );
			GetValue ( loc, out result );
		}


		public unsafe void SetValue ( int location, ref double value )
		{
			GL.Uniform1 ( location, value );
		}
		public unsafe void SetValue ( int location, ref float value )
		{
			GL.Uniform1 ( location, value );
		}
		public unsafe void SetValue ( int location, ref int value )
		{
			GL.Uniform1 ( location, value );
		}
		public unsafe void SetValue ( int location, ref uint value )
		{
			GL.Uniform1 ( location, value );
		}
		public unsafe void SetValue ( int location, ref Vector3 value )
		{
			fixed ( Vector3* ptr = &value )
				GL.Uniform3 ( location, 1, (float*)ptr );
		}
		public unsafe void SetValue ( int location, ref Vector4 value )
		{
			fixed ( Vector4* ptr = &value )
				GL.Uniform4 ( location, 1, (float*)ptr );
		}
		public unsafe void SetValue ( int location, ref Color4 value )
		{
			fixed ( Color4* ptr = &value )
				GL.Uniform4 ( location, 1, (float*)ptr );
		}
		public unsafe void SetValue ( int location, ref Matrix value )
		{
			fixed ( Matrix* ptr = &value )
				GL.UniformMatrix4 ( location, 1, false, (float*)ptr );
		}
		public unsafe void SetValue ( int location, ref Matrix3x2 value )
		{
			fixed ( Matrix3x2* ptr = &value )
				GL.UniformMatrix4 ( location, 1, false, (float*)ptr );
		}
		public unsafe void SetValue ( int location, ref Matrix3x3 value )
		{
			fixed ( Matrix3x3* ptr = &value )
				GL.UniformMatrix4 ( location, 1, false, (float*)ptr );
		}

		public unsafe void SetValue ( string propertyName, ref double value )
		{
			var loc = GL.GetUniformLocation ( m_Handler, propertyName );
			SetValue ( loc, ref value );
		}
		public unsafe void SetValue ( string propertyName, ref float value )
		{
			var loc = GL.GetUniformLocation ( m_Handler, propertyName );
			SetValue ( loc, ref value );
		}
		public unsafe void SetValue ( string propertyName, ref int value )
		{
			var loc = GL.GetUniformLocation ( m_Handler, propertyName );
			SetValue ( loc, ref value );
		}
		public unsafe void SetValue ( string propertyName, ref uint value )
		{
			var loc = GL.GetUniformLocation ( m_Handler, propertyName );
			SetValue ( loc, ref value );
		}
		public unsafe void SetValue ( string propertyName, ref Vector3 value )
		{
			var loc = GL.GetUniformLocation ( m_Handler, propertyName );
			SetValue ( loc, ref value );
		}
		public unsafe void SetValue ( string propertyName, ref Vector4 value )
		{
			var loc = GL.GetUniformLocation ( m_Handler, propertyName );
			SetValue ( loc, ref value );
		}
		public unsafe void SetValue ( string propertyName, ref Color4 value )
		{
			var loc = GL.GetUniformLocation ( m_Handler, propertyName );
			SetValue ( loc, ref value );
		}
		public unsafe void SetValue ( string propertyName, ref Matrix value )
		{
			var loc = GL.GetUniformLocation ( m_Handler, propertyName );
			SetValue ( loc, ref value );
		}
		public unsafe void SetValue ( string propertyName, ref Matrix3x2 value )
		{
			var loc = GL.GetUniformLocation ( m_Handler, propertyName );
			SetValue ( loc, ref value );
		}
		public unsafe void SetValue ( string propertyName, ref Matrix3x3 value )
		{
			var loc = GL.GetUniformLocation ( m_Handler, propertyName );
			SetValue ( loc, ref value );
		}

		#endregion


		public void Validate ()
		{
			GL.ValidateProgram ( m_Handler );
		}

		public void GetBinary ( byte[] data, out int written, out BinaryFormat format )
		{
			GL.GetProgramBinary ( m_Handler, data.Length, out written, out format, data );
		}

		public void GetBinary ( IntPtr data, int length, out int written, out BinaryFormat format )
		{
			GL.GetProgramBinary ( m_Handler, length, out written, out format, data );
		}

		public void SetBinary ( IntPtr data, int length, BinaryFormat format )
		{
			GL.ProgramBinary ( m_Handler, format, data, length );
		}

		public GLShader GetShader ( ShaderType type )
		{
			switch ( type )
			{
				case ShaderType.ComputeShader:
					return m_ComputeShader;
				case ShaderType.FragmentShader:
					return m_FragmentShader;
				case ShaderType.GeometryShader:
					return m_GeometryShader;
				case ShaderType.TessControlShader:
					return m_TessControlShader;
				case ShaderType.TessEvaluationShader:
					return m_TessEvaluationShader;
				case ShaderType.VertexShader:
					return m_VertexShader;
				default: throw new ArgumentOutOfRangeException ();
			}
		}

		public void SetShader ( GLShader shader )
		{
			GLShader oldShader = null;
			switch ( shader.Type )
			{
				case ShaderType.ComputeShader:
					oldShader = m_ComputeShader;
					m_ComputeShader = shader;
					break;
				case ShaderType.FragmentShader:
					oldShader = m_FragmentShader;
					m_FragmentShader = shader;
					break;
				case ShaderType.GeometryShader:
					oldShader = m_GeometryShader;
					m_GeometryShader = shader;
					break;
				case ShaderType.TessControlShader:
					oldShader = m_TessControlShader;
					m_TessControlShader = shader;
					break;
				case ShaderType.TessEvaluationShader:
					oldShader = m_TessEvaluationShader;
					m_TessEvaluationShader = shader;
					break;
				case ShaderType.VertexShader:
					oldShader = m_VertexShader;
					m_VertexShader = shader;
					break;
			}

			if ( oldShader != shader && oldShader != null )
				GL.DetachShader ( m_Handler, oldShader.Handler );

			if ( oldShader != shader && shader != null )
				GL.AttachShader ( m_Handler, shader.Handler );


			var info = GL.GetProgramInfoLog ( m_Handler );
			if ( !string.IsNullOrWhiteSpace ( info ) )
				Console.WriteLine ( info );
		}

		public void BindAttributeLocation ( string name, int index )
		{
			GL.BindAttribLocation ( m_Handler, index, name );
		}

		public int GetAttributeLocation ( string name )
		{
			return GL.GetAttribLocation ( m_Handler, name );
		}

		public int GetUniformLocation ( string name )
		{
			return GL.GetUniformLocation ( m_Handler, name );
		}


		private void DetachShader ( ref GLShader shader )
		{
			if ( shader != null )
			{
				GL.DetachShader ( m_Handler, shader.Handler );
				shader = null;
			}
		}

	}
}
