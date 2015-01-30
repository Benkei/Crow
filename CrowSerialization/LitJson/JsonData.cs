#region Header

/**
 * JsonData.cs
 *   Generic type to hold JSON data (objects, arrays, and so on). This is
 *   the default type returned by JsonMapper.ToObject().
 *
 * The authors disclaim copyright to this source code. For more details, see
 * the COPYING file included with this distribution.
 **/

#endregion Header

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.InteropServices;

namespace CrowSerialization.LitJson
{
	using JsonArray = List<JsonData>;
	using JsonDict = OrderedDictionary;

	public class JsonData : IJsonWrapper, IEquatable<JsonData>
	{
		[StructLayout ( LayoutKind.Explicit )]
		private struct JsonValue
		{
			[FieldOffset ( 0 )]
			public bool inst_boolean;

			[FieldOffset ( 0 )]
			public double inst_double;

			[FieldOffset ( 0 )]
			public int inst_int;

			[FieldOffset ( 0 )]
			public long inst_long;
		}

		#region Fields

		private JsonType type;
		private JsonValue inst_value;
		private object inst_object;

		// cache
		private string json;

		#endregion Fields

		#region Properties

		public int Count
		{
			get { return EnsureCollection ().Count; }
		}

		public bool IsArray
		{
			get { return type == JsonType.Array; }
		}

		public bool IsBoolean
		{
			get { return type == JsonType.Boolean; }
		}

		public bool IsDouble
		{
			get { return type == JsonType.Double; }
		}

		public bool IsInt
		{
			get { return type == JsonType.Int; }
		}

		public bool IsLong
		{
			get { return type == JsonType.Long; }
		}

		public bool IsObject
		{
			get { return type == JsonType.Object; }
		}

		public bool IsString
		{
			get { return type == JsonType.String; }
		}

		public ICollection Keys
		{
			get { EnsureDictionary (); return ((JsonDict)inst_object).Keys; }
		}

		#endregion Properties

		#region ICollection Properties

		int ICollection.Count
		{
			get { return Count; }
		}

		bool ICollection.IsSynchronized
		{
			get { return EnsureCollection ().IsSynchronized; }
		}

		object ICollection.SyncRoot
		{
			get { return EnsureCollection ().SyncRoot; }
		}

		#endregion ICollection Properties

		#region IDictionary Properties

		bool IDictionary.IsFixedSize
		{
			get { return ((IDictionary)EnsureDictionary ()).IsFixedSize; }
		}

		bool IDictionary.IsReadOnly
		{
			get { return EnsureDictionary ().IsReadOnly; }
		}

		ICollection IDictionary.Keys
		{
			get { EnsureDictionary (); return ((JsonDict)inst_object).Keys; }
		}

		ICollection IDictionary.Values
		{
			get { EnsureDictionary (); return ((JsonDict)inst_object).Values; }
		}

		#endregion IDictionary Properties

		#region IJsonWrapper Properties

		bool IJsonWrapper.IsArray
		{
			get { return IsArray; }
		}

		bool IJsonWrapper.IsBoolean
		{
			get { return IsBoolean; }
		}

		bool IJsonWrapper.IsDouble
		{
			get { return IsDouble; }
		}

		bool IJsonWrapper.IsInt
		{
			get { return IsInt; }
		}

		bool IJsonWrapper.IsLong
		{
			get { return IsLong; }
		}

		bool IJsonWrapper.IsObject
		{
			get { return IsObject; }
		}

		bool IJsonWrapper.IsString
		{
			get { return IsString; }
		}

		#endregion IJsonWrapper Properties

		#region IList Properties

		bool IList.IsFixedSize
		{
			get { return ((IList)EnsureList ()).IsFixedSize; }
		}

		bool IList.IsReadOnly
		{
			get { return ((IList)EnsureList ()).IsReadOnly; }
		}

		#endregion IList Properties

		#region IDictionary Indexer

		object IDictionary.this[object key]
		{
			get { return EnsureDictionary ()[key]; }
			set
			{
				if ( !(key is String) )
					throw new ArgumentException (
						"The key has to be a string" );

				JsonData data = ToJsonData ( value );

				this[(string)key] = data;
			}
		}

		#endregion IDictionary Indexer

		#region IOrderedDictionary Indexer

		object IOrderedDictionary.this[int idx]
		{
			get { return EnsureDictionary ()[idx]; }
			set { JsonData data = ToJsonData ( value ); EnsureDictionary ()[idx] = data; }
		}

		#endregion IOrderedDictionary Indexer

		#region IList Indexer

		object IList.this[int index]
		{
			get { return EnsureList ()[index]; }
			set
			{
				EnsureList ();
				this[index] = ToJsonData ( value );
			}
		}

		#endregion IList Indexer

		#region Public Indexers

		public JsonData this[string prop_name]
		{
			get { return (JsonData)EnsureDictionary ()[prop_name]; }
			set { EnsureDictionary ()[prop_name] = value; json = null; }
		}

		public JsonData this[int index]
		{
			get
			{
				EnsureCollection ();
				if ( type == JsonType.Array )
					return ((JsonArray)inst_object)[index];
				else
					return (JsonData)((JsonDict)inst_object)[index];
			}
			set
			{
				EnsureCollection ();
				if ( type == JsonType.Array )
					((JsonArray)inst_object)[index] = value;
				else
					((JsonDict)inst_object)[index] = value;
				json = null;
			}
		}

		#endregion Public Indexers

		#region Constructors

		public JsonData ()
		{
		}

		public JsonData ( bool boolean )
		{
			type = JsonType.Boolean;
			inst_value.inst_boolean = boolean;
		}

		public JsonData ( double number )
		{
			type = JsonType.Double;
			inst_value.inst_double = number;
		}

		public JsonData ( int number )
		{
			type = JsonType.Int;
			inst_value.inst_int = number;
		}

		public JsonData ( long number )
		{
			type = JsonType.Long;
			inst_value.inst_long = number;
		}

		public JsonData ( string str )
		{
			type = JsonType.String;
			inst_object = str;
		}

		public JsonData ( object obj )
		{
			if ( obj is Boolean )
			{
				type = JsonType.Boolean;
				inst_value.inst_boolean = (bool)obj;
				return;
			}
			if ( obj is Double )
			{
				type = JsonType.Double;
				inst_value.inst_double = (double)obj;
				return;
			}
			if ( obj is Int32 )
			{
				type = JsonType.Int;
				inst_value.inst_int = (int)obj;
				return;
			}
			if ( obj is Int64 )
			{
				type = JsonType.Long;
				inst_value.inst_long = (long)obj;
				return;
			}
			if ( obj is String )
			{
				type = JsonType.String;
				inst_object = (string)obj;
				return;
			}
			throw new ArgumentException ( "Unable to wrap the given object with JsonData" );
		}

		#endregion Constructors

		#region Implicit Conversions

		public static implicit operator JsonData ( Boolean data )
		{
			return new JsonData ( data );
		}

		public static implicit operator JsonData ( Double data )
		{
			return new JsonData ( data );
		}

		public static implicit operator JsonData ( Int32 data )
		{
			return new JsonData ( data );
		}

		public static implicit operator JsonData ( Int64 data )
		{
			return new JsonData ( data );
		}

		public static implicit operator JsonData ( String data )
		{
			return new JsonData ( data );
		}

		#endregion Implicit Conversions

		#region Explicit Conversions

		public static explicit operator Boolean ( JsonData data )
		{
			if ( data.type != JsonType.Boolean )
				throw new InvalidCastException ( "Instance of JsonData doesn't hold a boolean" );

			return data.inst_value.inst_boolean;
		}

		public static explicit operator Double ( JsonData data )
		{
			if ( data.type != JsonType.Double )
				throw new InvalidCastException ( "Instance of JsonData doesn't hold a double" );

			return data.inst_value.inst_double;
		}

		public static explicit operator Int32 ( JsonData data )
		{
			if ( data.type != JsonType.Int )
				throw new InvalidCastException ( "Instance of JsonData doesn't hold an int32" );

			return data.inst_value.inst_int;
		}

		public static explicit operator Int64 ( JsonData data )
		{
			if ( data.type != JsonType.Long )
				throw new InvalidCastException ( "Instance of JsonData doesn't hold an int64" );

			return data.inst_value.inst_long;
		}

		public static explicit operator String ( JsonData data )
		{
			if ( data.type != JsonType.String )
				throw new InvalidCastException ( "Instance of JsonData doesn't hold a string" );

			return (string)data.inst_object;
		}

		#endregion Explicit Conversions

		#region ICollection Methods

		void ICollection.CopyTo ( Array array, int index )
		{
			EnsureCollection ().CopyTo ( array, index );
		}

		#endregion ICollection Methods

		#region IDictionary Methods

		void IDictionary.Add ( object key, object value )
		{
			JsonData data = ToJsonData ( value );
			EnsureDictionary ().Add ( key, data );
			json = null;
		}

		void IDictionary.Clear ()
		{
			EnsureDictionary ().Clear ();
			json = null;
		}

		bool IDictionary.Contains ( object key )
		{
			return EnsureDictionary ().Contains ( key );
		}

		IDictionaryEnumerator IDictionary.GetEnumerator ()
		{
			return ((IOrderedDictionary)this).GetEnumerator ();
		}

		void IDictionary.Remove ( object key )
		{
			EnsureDictionary ().Remove ( key );
			json = null;
		}

		#endregion IDictionary Methods

		#region IEnumerable Methods

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return EnsureCollection ().GetEnumerator ();
		}

		#endregion IEnumerable Methods

		#region IJsonWrapper Methods

		bool IJsonWrapper.GetBoolean ()
		{
			if ( type != JsonType.Boolean )
				throw new InvalidOperationException (
					"JsonData instance doesn't hold a boolean" );

			return inst_value.inst_boolean;
		}

		double IJsonWrapper.GetDouble ()
		{
			if ( type != JsonType.Double )
				throw new InvalidOperationException (
					"JsonData instance doesn't hold a double" );

			return inst_value.inst_double;
		}

		int IJsonWrapper.GetInt ()
		{
			if ( type != JsonType.Int )
				throw new InvalidOperationException (
					"JsonData instance doesn't hold an int" );

			return inst_value.inst_int;
		}

		long IJsonWrapper.GetLong ()
		{
			if ( type != JsonType.Long )
				throw new InvalidOperationException (
					"JsonData instance doesn't hold a long" );

			return inst_value.inst_long;
		}

		string IJsonWrapper.GetString ()
		{
			if ( type != JsonType.String )
				throw new InvalidOperationException (
					"JsonData instance doesn't hold a string" );

			return (string)inst_object;
		}

		void IJsonWrapper.SetBoolean ( bool val )
		{
			type = JsonType.Boolean;
			inst_value.inst_boolean = val;
			json = null;
		}

		void IJsonWrapper.SetDouble ( double val )
		{
			type = JsonType.Double;
			inst_value.inst_double = val;
			json = null;
		}

		void IJsonWrapper.SetInt ( int val )
		{
			type = JsonType.Int;
			inst_value.inst_int = val;
			json = null;
		}

		void IJsonWrapper.SetLong ( long val )
		{
			type = JsonType.Long;
			inst_value.inst_long = val;
			json = null;
		}

		void IJsonWrapper.SetString ( string val )
		{
			type = JsonType.String;
			inst_object = val;
			json = null;
		}

		string IJsonWrapper.ToJson ()
		{
			return ToJson ();
		}

		void IJsonWrapper.ToJson ( JsonWriter writer )
		{
			ToJson ( writer );
		}

		#endregion IJsonWrapper Methods

		#region IList Methods

		int IList.Add ( object value )
		{
			return Add ( value );
		}

		void IList.Clear ()
		{
			EnsureList ().Clear ();
			json = null;
		}

		bool IList.Contains ( object value )
		{
			return ((IList)EnsureList ()).Contains ( value );
		}

		int IList.IndexOf ( object value )
		{
			return ((IList)EnsureList ()).IndexOf ( value );
		}

		void IList.Insert ( int index, object value )
		{
			((IList)EnsureList ()).Insert ( index, value );
			json = null;
		}

		void IList.Remove ( object value )
		{
			((IList)EnsureList ()).Remove ( value );
			json = null;
		}

		void IList.RemoveAt ( int index )
		{
			EnsureList ().RemoveAt ( index );
			json = null;
		}

		#endregion IList Methods

		#region IOrderedDictionary Methods

		IDictionaryEnumerator IOrderedDictionary.GetEnumerator ()
		{
			return EnsureDictionary ().GetEnumerator ();
		}

		void IOrderedDictionary.Insert ( int idx, object key, object value )
		{
			string property = (string)key;
			JsonData data = ToJsonData ( value );
			this[property] = data;
		}

		void IOrderedDictionary.RemoveAt ( int idx )
		{
			EnsureDictionary ().RemoveAt ( idx );
		}

		#endregion IOrderedDictionary Methods

		#region Private Methods

		private ICollection EnsureCollection ()
		{
			if ( type == JsonType.Array || type == JsonType.Object )
				return (ICollection)inst_object;

			throw new InvalidOperationException (
				"The JsonData instance has to be initialized first" );
		}

		private JsonDict EnsureDictionary ()
		{
			if ( type == JsonType.Object )
				return (JsonDict)inst_object;

			if ( type != JsonType.None )
				throw new InvalidOperationException ( "Instance of JsonData is not a dictionary" );

			type = JsonType.Object;
			inst_object = new JsonDict ( StringComparer.InvariantCulture );

			return (JsonDict)inst_object;
		}

		private JsonArray EnsureList ()
		{
			if ( type == JsonType.Array )
				return (JsonArray)inst_object;

			if ( type != JsonType.None )
				throw new InvalidOperationException (
					"Instance of JsonData is not a list" );

			type = JsonType.Array;
			inst_object = new JsonArray ();

			return (JsonArray)inst_object;
		}

		private JsonData ToJsonData ( object obj )
		{
			if ( obj == null )
				return null;

			if ( obj is JsonData )
				return (JsonData)obj;

			return new JsonData ( obj );
		}

		private static void WriteJson ( IJsonWrapper obj, JsonWriter writer )
		{
			if ( obj == null )
			{
				writer.Write ( null );
				return;
			}

			if ( obj.IsString )
			{
				writer.Write ( obj.GetString () );
				return;
			}

			if ( obj.IsBoolean )
			{
				writer.Write ( obj.GetBoolean () );
				return;
			}

			if ( obj.IsDouble )
			{
				writer.Write ( obj.GetDouble () );
				return;
			}

			if ( obj.IsInt )
			{
				writer.Write ( obj.GetInt () );
				return;
			}

			if ( obj.IsLong )
			{
				writer.Write ( obj.GetLong () );
				return;
			}

			if ( obj.IsArray )
			{
				writer.WriteArrayStart ();
				foreach ( object elem in (IList)obj )
					WriteJson ( (JsonData)elem, writer );
				writer.WriteArrayEnd ();

				return;
			}

			if ( obj.IsObject )
			{
				writer.WriteObjectStart ();

				foreach ( DictionaryEntry entry in ((IDictionary)obj) )
				{
					writer.WritePropertyName ( (string)entry.Key );
					WriteJson ( (JsonData)entry.Value, writer );
				}
				writer.WriteObjectEnd ();

				return;
			}
		}

		#endregion Private Methods

		public object GetValue ()
		{
			switch ( type )
			{
				case JsonType.String: return (string)inst_object;
				case JsonType.Int: return inst_value.inst_int;
				case JsonType.Long: return inst_value.inst_long;
				case JsonType.Double: return inst_value.inst_double;
				case JsonType.Boolean: return inst_value.inst_boolean;
				default: throw new InvalidDataException ();
			}
		}

		public int Add ( object value )
		{
			JsonData data = ToJsonData ( value );

			json = null;

			return ((IList)EnsureList ()).Add ( data );
		}

		public void Clear ()
		{
			if ( IsObject )
			{
				((IDictionary)this).Clear ();
				return;
			}

			if ( IsArray )
			{
				((IList)this).Clear ();
				return;
			}
		}

		public bool Equals ( JsonData x )
		{
			if ( x == null )
				return false;

			if ( x.type != this.type )
				return false;

			switch ( this.type )
			{
				case JsonType.None:
					return true;

				case JsonType.Object:
				case JsonType.Array:
				case JsonType.String:
					return this.inst_object.Equals ( x.inst_object );

				case JsonType.Int:
					return this.inst_value.inst_int.Equals ( x.inst_value.inst_int );

				case JsonType.Long:
					return this.inst_value.inst_long.Equals ( x.inst_value.inst_long );

				case JsonType.Double:
					return this.inst_value.inst_double.Equals ( x.inst_value.inst_double );

				case JsonType.Boolean:
					return this.inst_value.inst_boolean.Equals ( x.inst_value.inst_boolean );
			}

			return false;
		}

		public JsonType GetJsonType ()
		{
			return type;
		}

		public void SetJsonType ( JsonType type )
		{
			if ( this.type == type )
				return;

			switch ( type )
			{
				case JsonType.None:
					break;

				case JsonType.Object:
					inst_object = new JsonDict ( StringComparer.InvariantCulture );
					break;

				case JsonType.Array:
					inst_object = new JsonArray ();
					break;

				case JsonType.String:
					inst_object = default ( String );
					break;

				case JsonType.Int:
					inst_value = default ( JsonValue );
					break;

				case JsonType.Long:
					inst_value = default ( JsonValue );
					break;

				case JsonType.Double:
					inst_value = default ( JsonValue );
					break;

				case JsonType.Boolean:
					inst_value = default ( JsonValue );
					break;
			}

			this.type = type;
		}

		public string ToJson ()
		{
			if ( json != null )
				return json;

			StringWriter sw = new StringWriter ();
			JsonWriter writer = new JsonWriter ( sw );
			writer.Validate = false;

			WriteJson ( this, writer );
			json = sw.ToString ();

			return json;
		}

		public void ToJson ( JsonWriter writer )
		{
			bool old_validate = writer.Validate;

			writer.Validate = false;

			WriteJson ( this, writer );

			writer.Validate = old_validate;
		}

		public override string ToString ()
		{
			switch ( type )
			{
				case JsonType.Array:
					return "JsonData array";

				case JsonType.Boolean:
					return inst_value.inst_boolean.ToString ();

				case JsonType.Double:
					return inst_value.inst_double.ToString ();

				case JsonType.Int:
					return inst_value.inst_int.ToString ();

				case JsonType.Long:
					return inst_value.inst_long.ToString ();

				case JsonType.Object:
					return "JsonData object";

				case JsonType.String:
					return (string)inst_object;
			}

			return "Uninitialized JsonData";
		}
	}
}