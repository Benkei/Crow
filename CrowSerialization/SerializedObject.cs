using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CrowSerialization
{
	// ## dont change the order! ##
	// based on TypeCode
	// Empty = 0,
	// Object = 1,
	// DBNull = 2,
	// Boolean = 3,
	// Char = 4,
	// SByte = 5,
	// Byte = 6,
	// Int16 = 7,
	// UInt16 = 8,
	// Int32 = 9,
	// UInt32 = 10,
	// Int64 = 11,
	// UInt64 = 12,
	// Single = 13,
	// Double = 14,
	// Decimal = 15,
	// DateTime = 16,
	// String = 18,
	public enum ObjectType
	{
		None,

		Object,
		Array,

		Boolean,
		Char,
		SByte,
		Byte,
		Int16,
		UInt16,
		Int32,
		UInt32,
		Int64,
		UInt64,
		Single,
		Double,
		Decimal,

		String = 18,

		// Blob
	}


	[StructLayout ( LayoutKind.Explicit )]
	struct UnionValue
	{
		[FieldOffset ( 0 )]
		public bool m_Boolean;

		[FieldOffset ( 0 )]
		public float m_Single;

		[FieldOffset ( 0 )]
		public double m_Double;

		[FieldOffset ( 0 )]
		public decimal m_Decimal;

		[FieldOffset ( 0 )]
		public sbyte m_Int8;

		[FieldOffset ( 0 )]
		public short m_Int16;

		[FieldOffset ( 0 )]
		public int m_Int32;

		[FieldOffset ( 0 )]
		public long m_Int64;
	}


	public sealed class SerializedObject
	{
		private ObjectType m_Type;
		private UnionValue inst_value;
		// List<SerializedObject>
		// IndexedDictionary<string, SerializedObject>
		private object inst_object;

		public SerializedObject () { }

		public SerializedObject ( object value )
		{
			if ( value == null )
			{
				m_Type = ObjectType.None;
			}
			else
			{
				var code = System.Type.GetTypeCode ( value.GetType () );
				if ( code == TypeCode.DBNull || code == TypeCode.DateTime )
				{
					m_Type = ObjectType.None;
				}
				else
				{
					m_Type = (ObjectType)code;
					if ( m_Type == ObjectType.Object )
					{
						ObjectMetadata meta;
						ReflectionCache.GetObjectMetadata ( value.GetType (), out meta );
						if ( meta.IsArray || meta.IsList )
						{
							m_Type = ObjectType.Array;
						}
					}
				}
			}
		}


		public ObjectType Type
		{
			get { return m_Type; }
			set
			{
				m_Type = value;
				inst_value = new UnionValue ();
				inst_object = null;
			}
		}

		public object Value
		{
			get
			{
				switch ( m_Type )
				{
					case ObjectType.Boolean: return inst_value.m_Boolean;
					case ObjectType.Char: return inst_value.m_Int16;
					case ObjectType.SByte: return inst_value.m_Int8;
					case ObjectType.Byte: return inst_value.m_Int8;
					case ObjectType.Int16: return inst_value.m_Int16;
					case ObjectType.UInt16: return inst_value.m_Int16;
					case ObjectType.Int32: return inst_value.m_Int32;
					case ObjectType.UInt32: return inst_value.m_Int32;
					case ObjectType.Int64: return inst_value.m_Int64;
					case ObjectType.UInt64: return inst_value.m_Int64;
					case ObjectType.Single: return inst_value.m_Single;
					case ObjectType.Double: return inst_value.m_Double;
					case ObjectType.Decimal: return inst_value.m_Decimal;
					case ObjectType.String: return (string)inst_object;
				}
				return null;
			}
			set
			{
				switch ( m_Type )
				{
					case ObjectType.Boolean: inst_value.m_Boolean = (bool)value; break;
					case ObjectType.Char: inst_value.m_Int16 = (short)(char)value; break;
					case ObjectType.SByte: inst_value.m_Int8 = (sbyte)value; break;
					case ObjectType.Byte: inst_value.m_Int8 = (sbyte)(byte)value; break;
					case ObjectType.Int16: inst_value.m_Int16 = (short)value; break;
					case ObjectType.UInt16: inst_value.m_Int16 = (short)(ushort)value; break;
					case ObjectType.Int32: inst_value.m_Int32 = (int)value; break;
					case ObjectType.UInt32: inst_value.m_Int32 = (int)(uint)value; break;
					case ObjectType.Int64: inst_value.m_Int64 = (long)value; break;
					case ObjectType.UInt64: inst_value.m_Int64 = (long)(ulong)value; break;
					case ObjectType.Single: inst_value.m_Single = (float)value; break;
					case ObjectType.Double: inst_value.m_Double = (double)value; break;
					case ObjectType.Decimal: inst_value.m_Decimal = (decimal)value; break;
					case ObjectType.String: inst_object = value; break;
				}
			}
		}

		public int PropertyCount
		{
			get
			{
				if ( m_Type == ObjectType.Object )
					return ((IndexedDictionary<string, SerializedObject>)inst_object).Count;
				if ( m_Type == ObjectType.Array )
					return ((List<SerializedObject>)inst_object).Count;
				return 0;
			}
		}

		public KeyValuePair<string, SerializedObject> this[int index]
		{
			get
			{
				if ( m_Type == ObjectType.Object )
					return ((IndexedDictionary<string, SerializedObject>)inst_object)[index];
				if ( m_Type == ObjectType.Array )
					return new KeyValuePair<string, SerializedObject> ( null, ((List<SerializedObject>)inst_object)[index] );

				throw new InvalidOperationException ();
			}
			set
			{
				if ( m_Type == ObjectType.Object )
					((IndexedDictionary<string, SerializedObject>)inst_object)[index] = value;
				else if ( m_Type == ObjectType.Array )
					((List<SerializedObject>)inst_object)[index] = value.Value;
				else
					throw new InvalidOperationException ();
			}
		}

		public SerializedObject this[string key]
		{
			get
			{
				if ( m_Type != ObjectType.Object )
					throw new InvalidOperationException ();

				return ((IndexedDictionary<string, SerializedObject>)inst_object)[key];
			}
			set
			{
				if ( m_Type != ObjectType.Object )
					throw new InvalidOperationException ();

				((IndexedDictionary<string, SerializedObject>)inst_object)[key] = value;
			}
		}

		public KeyValuePair<string, SerializedObject> GetProperty ( int index )
		{
			if ( m_Type != ObjectType.Object )
				throw new InvalidOperationException ();

			return ((IndexedDictionary<string, SerializedObject>)inst_object)[index];
		}

		public void Add ( object value )
		{
			if ( m_Type != ObjectType.Array )
				throw new InvalidOperationException ();

			((List<SerializedObject>)inst_object).Add ( new SerializedObject ( value ) );
		}

		public void Add ( string key, object value )
		{
			if ( m_Type != ObjectType.Object )
				throw new InvalidOperationException ();

			Add ( key, new SerializedObject ( value ) );
		}

		public void Add ( string key, SerializedObject value )
		{
			if ( m_Type != ObjectType.Object )
				throw new InvalidOperationException ();

			((IndexedDictionary<string, SerializedObject>)inst_object).Add ( key, value );
		}

		public void Clear ()
		{
			inst_value = new UnionValue ();
			inst_object = null;
		}
	}


	class IndexedDictionary<TKey, TValue>
	{
		HashSet<TKey> m_Keys;
		List<KeyValuePair<TKey, TValue>> m_Values;

		IEqualityComparer<TKey> m_KeyComparer;

		public IndexedDictionary ()
		{
			m_KeyComparer = EqualityComparer<TKey>.Default;
			m_Keys = new HashSet<TKey> ( m_KeyComparer );
			m_Values = new List<KeyValuePair<TKey, TValue>> ();
			//System.Collections.Specialized.OrderedDictionary
		}

		public KeyValuePair<TKey, TValue> this[int index]
		{
			get { return m_Values[index]; }
			set
			{
				var old = m_Values[index];
				if ( !m_KeyComparer.Equals ( old.Key, value.Key ) )
				{
					m_Keys.Remove ( old.Key );
					m_Keys.Add ( value.Key );
				}
				m_Values[index] = value;
			}
		}

		public TValue this[TKey key]
		{
			get
			{
				if ( m_Keys.Contains ( key ) )
				{
					var idx = IndexOf ( ref key );
					if ( idx != -1 )
						return m_Values[idx].Value;
				}
				throw new KeyNotFoundException ();
			}
			set
			{
				if ( m_Keys.Contains ( key ) )
				{
					var idx = IndexOf ( ref key );
					if ( idx != -1 )
						m_Values[idx] = new KeyValuePair<TKey, TValue> ( key, value );
				}
				throw new KeyNotFoundException ();
			}
		}

		public int Count
		{
			get { return m_Values.Count; }
		}


		public void Add ( TKey key, TValue value )
		{
			if ( m_Keys.Contains ( key ) )
				throw new ArgumentNullException ();

			m_Keys.Add ( key );
			m_Values.Add ( new KeyValuePair<TKey, TValue> ( key, value ) );
		}

		public void Clear ()
		{
			m_Keys.Clear ();
			m_Values.Clear ();
		}

		private int IndexOf ( ref TKey key )
		{
			for ( int i = m_Values.Count - 1; i >= 0; i-- )
			{
				if ( m_KeyComparer.Equals ( m_Values[i].Key, key ) )
				{
					return i;
				}
			}
			return -1;
		}
	}
}
