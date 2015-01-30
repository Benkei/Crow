#region Header

/**
 * JsonMapper.cs
 *   JSON to .Net object and object to JSON conversions.
 *
 * The authors disclaim copyright to this source code. For more details, see
 * the COPYING file included with this distribution.
 **/

#endregion Header

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace CrowSerialization.LitJson
{
	internal struct PropertyMetadata
	{
		public MemberInfo Info;
		public bool IsField;
		public Type Type;
	}

	internal struct ArrayMetadata
	{
		public bool IsArray;
		public bool IsList;

		private Type element_type;

		public Type ElementType
		{
			get { return element_type == null ? typeof ( JsonData ) : element_type; }
			set { element_type = value; }
		}
	}

	internal struct ObjectMetadata
	{
		public bool IsDictionary;
		public Dictionary<string, PropertyMetadata> Properties;

		private Type element_type;

		public Type ElementType
		{
			get { return element_type == null ? typeof ( JsonData ) : element_type; }
			set { element_type = value; }
		}
	}

	internal delegate void ExporterFunc ( object obj, JsonWriter writer );

	internal delegate object ImporterFunc ( object input );

	public delegate void ExporterFunc<T> ( T obj, JsonWriter writer );

	public delegate TValue ImporterFunc<TJson, TValue> ( TJson input );

	public delegate IJsonWrapper WrapperFactory ();

	public class JsonMapper
	{
		private const BindingFlags Bindings = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

		private const int max_nesting_depth = 100;

		#region Fields

		private static IFormatProvider datetime_format;

		private static readonly Dictionary<Type, ExporterFunc> base_exporters_table;
		private static readonly Dictionary<Type, ExporterFunc> custom_exporters_table;

		private static readonly Dictionary<Type, Dictionary<Type, ImporterFunc>> base_importers_table;

		private static readonly Dictionary<Type, Dictionary<Type, ImporterFunc>> custom_importers_table;

		private static readonly Dictionary<Type, ArrayMetadata> array_metadata;

		private static readonly Dictionary<Type, Dictionary<Type, Func<object, object>>> conv_ops;

		private static readonly Dictionary<Type, ObjectMetadata> object_metadata;

		private static readonly Dictionary<Type, List<PropertyMetadata>> type_properties;

		private static JsonWriter static_writer;

		#endregion Fields

		#region Constructors

		static JsonMapper ()
		{
			array_metadata = new Dictionary<Type, ArrayMetadata> ();
			conv_ops = new Dictionary<Type, Dictionary<Type, Func<object, object>>> ();
			object_metadata = new Dictionary<Type, ObjectMetadata> ();
			type_properties = new Dictionary<Type, List<PropertyMetadata>> ();

			static_writer = new JsonWriter ();

			datetime_format = DateTimeFormatInfo.InvariantInfo;

			base_exporters_table = new Dictionary<Type, ExporterFunc> ();
			custom_exporters_table = new Dictionary<Type, ExporterFunc> ();

			base_importers_table = new Dictionary<Type, Dictionary<Type, ImporterFunc>> ();
			custom_importers_table = new Dictionary<Type, Dictionary<Type, ImporterFunc>> ();

			RegisterBaseExporters ();
			RegisterBaseImporters ();
		}

		#endregion Constructors

		#region Private Methods

		private static ArrayMetadata AddArrayMetadata ( Type type )
		{
			lock ( array_metadata )
			{
				ArrayMetadata data;
				if ( array_metadata.TryGetValue ( type, out data ) )
					return data;

				data.IsArray = type.IsArray;

				if ( !data.IsArray && type.GetInterfaces ().Contains ( typeof ( IList ) ) )
					data.IsList = true;

				foreach ( PropertyInfo p_info in type.GetProperties ( Bindings ) )
				{
					if ( p_info.Name == "Item" || !(p_info.CanRead && p_info.CanWrite) )
						continue;

					ParameterInfo[] parameters = p_info.GetIndexParameters ();

					if ( parameters.Length != 1 )
						continue;

					if ( parameters[0].ParameterType == typeof ( int ) )
						data.ElementType = p_info.PropertyType;
				}

				array_metadata.Add ( type, data );
				return data;
			}
		}

		private static ObjectMetadata AddObjectMetadata ( Type type )
		{
			lock ( object_metadata )
			{
				ObjectMetadata data;
				if ( object_metadata.TryGetValue ( type, out data ) )
					return data;

				if ( type.GetInterfaces ().Contains ( typeof ( IDictionary ) ) )
					data.IsDictionary = true;

				data.Properties = new Dictionary<string, PropertyMetadata> ();

				foreach ( PropertyInfo p_info in type.GetProperties ( Bindings ) )
				{
					if ( !(p_info.CanRead && p_info.CanWrite) )
						continue;

					if ( p_info.Name == "Item" )
					{
						ParameterInfo[] parameters = p_info.GetIndexParameters ();

						if ( parameters.Length != 1 )
							continue;

						if ( parameters[0].ParameterType == typeof ( string ) )
							data.ElementType = p_info.PropertyType;

						continue;
					}

					PropertyMetadata p_data = new PropertyMetadata ();
					p_data.Info = p_info;
					p_data.Type = p_info.PropertyType;

					data.Properties.Add ( p_info.Name, p_data );
				}

				foreach ( FieldInfo f_info in type.GetFields ( Bindings ) )
				{
					PropertyMetadata p_data = new PropertyMetadata ();
					p_data.Info = f_info;
					p_data.IsField = true;
					p_data.Type = f_info.FieldType;

					data.Properties.Add ( f_info.Name, p_data );
				}

				object_metadata.Add ( type, data );
				return data;
			}
		}

		private static List<PropertyMetadata> AddTypeProperties ( Type type )
		{
			lock ( type_properties )
			{
				List<PropertyMetadata> props;

				if ( type_properties.TryGetValue ( type, out props ) )
					return props;

				props = new List<PropertyMetadata> ();
				foreach ( PropertyInfo p_info in type.GetProperties ( Bindings ) )
				{
					if ( p_info.Name == "Item" || !(p_info.CanRead && p_info.CanWrite && p_info.GetIndexParameters ().Length == 0) )
						continue;

					PropertyMetadata p_data = new PropertyMetadata ();
					p_data.Info = p_info;
					p_data.IsField = false;
					props.Add ( p_data );
				}

				foreach ( FieldInfo f_info in type.GetFields ( Bindings ) )
				{
					PropertyMetadata p_data = new PropertyMetadata ();
					p_data.Info = f_info;
					p_data.IsField = true;

					props.Add ( p_data );
				}

				type_properties.Add ( type, props );
				return props;
			}
		}

		private static Func<object, object> GetConvOp ( Type t1, Type t2 )
		{
			lock ( conv_ops )
			{
				Dictionary<Type, Func<object, object>> dic;
				Func<object, object> callback;
				if ( !conv_ops.TryGetValue ( t1, out dic ) )
					conv_ops.Add ( t1, dic = new Dictionary<Type, Func<object, object>> () );
				if ( !dic.TryGetValue ( t2, out callback ) )
				{
					MethodInfo op = t1.GetMethod ( "op_Implicit", new Type[] { t2 } );
					callback = (Func<object, object>)Delegate.CreateDelegate ( typeof ( Func<object, object> ), op );
					dic.Add ( t2, callback );
				}
				return callback;
			}
		}

		private static object ReadValue ( Type inst_type, object instance, JsonData reader )
		{
			if ( reader == null )
				return null;

			Type underlying_type = Nullable.GetUnderlyingType ( inst_type );
			Type value_type = underlying_type ?? inst_type;

			switch ( reader.GetJsonType () )
			{
				//case JsonType.None: break;
				case JsonType.Object:

					#region MyRegion

					{
						ObjectMetadata data = AddObjectMetadata ( value_type );

						if ( instance == null )
							instance = InstanceFactory.CreateInstance ( value_type );

						foreach ( DictionaryEntry item in reader )
						{
							string property = (string)item.Key;
							JsonData value = (JsonData)item.Value;

							PropertyMetadata prop;
							if ( data.Properties.TryGetValue ( property, out prop ) )
							{
								if ( prop.IsField )
								{
									((FieldInfo)prop.Info).SetValue ( instance, ReadValue ( prop.Type, null, value ) );
								}
								else
								{
									((PropertyInfo)prop.Info).SetValue ( instance, ReadValue ( prop.Type, null, value ), null );
								}
							}
							else
							{
								if ( data.IsDictionary )
								{
									((IDictionary)instance).Add ( property, ReadValue ( data.ElementType, null, value ) );
								}
							}
						}
					}

					#endregion MyRegion

					break;

				case JsonType.Array:

					#region MyRegion

					{
						ArrayMetadata data = AddArrayMetadata ( inst_type );

						if ( !data.IsArray && !data.IsList )
							throw new JsonException ( String.Format ( "Type {0} can't act as an array", inst_type ) );

						IList list;
						Type elem_type;

						if ( data.IsArray )
						{
							list = new ArrayList ();
							elem_type = inst_type.GetElementType ();
						}
						else
						{
							list = (IList)InstanceFactory.CreateInstance ( inst_type );
							elem_type = data.ElementType;
						}

						foreach ( JsonData item in (IEnumerable)reader )
						{
							object element = ReadValue ( elem_type, null, item );
							list.Add ( element );
						}

						if ( data.IsArray )
						{
							instance = Array.CreateInstance ( elem_type, list.Count );
							list.CopyTo ( (Array)instance, 0 );
						}
						else
							instance = list;
					}

					#endregion MyRegion

					break;

				case JsonType.String:
				case JsonType.Int:
				case JsonType.Long:
				case JsonType.Double:
				case JsonType.Boolean:

					#region MyRegion

					instance = reader.GetValue ();
					Type json_type = instance.GetType ();

					if ( value_type.IsAssignableFrom ( json_type ) )
						return instance;

					// Maybe it's an enum
					if ( value_type.IsEnum )
						return Enum.ToObject ( value_type, instance );

					ImporterFunc importer;

					// If there's a custom importer that fits, use it
					if ( custom_importers_table.ContainsKey ( json_type ) &&
						custom_importers_table[json_type].TryGetValue ( value_type, out importer ) )
					{
						return importer ( instance );
					}

					// Maybe there's a base importer that works
					if ( base_importers_table.ContainsKey ( json_type ) &&
						base_importers_table[json_type].TryGetValue ( value_type, out importer ) )
					{
						return importer ( instance );
					}

					try
					{
						return Convert.ChangeType ( instance, value_type, CultureInfo.InvariantCulture );
					}
					catch ( Exception )
					{
						// Try using an implicit conversion operator
						var conv_op = GetConvOp ( value_type, json_type );
						if ( conv_op != null )
							return conv_op ( instance );
					}

					// No luck
					throw new JsonException ( string.Format ( "Can't assign value '{0}' (type {1}) to type {2}", instance, json_type, inst_type ) );

					#endregion MyRegion
			}
			return instance;
		}

		private static object ReadValue ( Type inst_type, Func<Type, object> factory, JsonReader reader )
		{
			reader.Read ();

			Type underlying_type = Nullable.GetUnderlyingType ( inst_type );
			Type value_type = underlying_type ?? inst_type;

			object instance = null;

			switch ( reader.Token )
			{
				//case JsonToken.None:
				//	break;

				case JsonToken.ObjectStart:

					#region MyRegion

					{
						ObjectMetadata t_data = AddObjectMetadata ( value_type );

						instance = factory ( value_type ); // InstanceFactory.CreateInstance ( value_type );

						while ( true )
						{
							reader.Read ();

							if ( reader.Token == JsonToken.ObjectEnd )
								break;

							string property = (string)reader.Value;

							PropertyMetadata prop_data;
							if ( t_data.Properties.TryGetValue ( property, out prop_data ) )
							{
								if ( prop_data.IsField )
								{
									((FieldInfo)prop_data.Info).SetValue ( instance, ReadValue ( prop_data.Type, factory, reader ) );
								}
								else
								{
									((PropertyInfo)prop_data.Info).SetValue ( instance, ReadValue ( prop_data.Type, factory, reader ), null );
								}
							}
							else
							{
								if ( !t_data.IsDictionary )
								{
									if ( !reader.SkipNonMembers )
									{
										throw new JsonException ( string.Format ( "The type {0} doesn't have the property '{1}'", inst_type, property ) );
									}
									else
									{
										ReadSkip ( reader );
										continue;
									}
								}

								((IDictionary)instance).Add ( property, ReadValue ( t_data.ElementType, factory, reader ) );
							}
						}
					}

					#endregion MyRegion

					break;

				//case JsonToken.PropertyName:
				//	break;
				//case JsonToken.ObjectEnd:
				//	break;

				case JsonToken.ArrayStart:

					#region MyRegion

					{
						ArrayMetadata t_data = AddArrayMetadata ( inst_type );

						if ( !t_data.IsArray && !t_data.IsList )
							throw new JsonException ( String.Format (
									"Type {0} can't act as an array",
									inst_type ) );

						IList list;
						Type elem_type;

						if ( !t_data.IsArray )
						{
							list = (IList)InstanceFactory.CreateInstance ( inst_type );
							elem_type = t_data.ElementType;
						}
						else
						{
							list = new ArrayList ();
							elem_type = inst_type.GetElementType ();
						}

						while ( true )
						{
							object item = ReadValue ( elem_type, factory, reader );
							if ( item == null && reader.Token == JsonToken.ArrayEnd )
								break;

							list.Add ( item );
						}

						if ( t_data.IsArray )
						{
							int n = list.Count;
							instance = Array.CreateInstance ( elem_type, n );

							for ( int i = 0; i < n; i++ )
								((Array)instance).SetValue ( list[i], i );
						}
						else
							instance = list;
					}

					#endregion MyRegion

					break;

				case JsonToken.ArrayEnd: return null;

				case JsonToken.Int:
				case JsonToken.Long:
				case JsonToken.Double:
				case JsonToken.String:
				case JsonToken.Boolean:

					#region MyRegion

					Type json_type = reader.Value.GetType ();

					if ( value_type.IsAssignableFrom ( json_type ) )
						return reader.Value;

					// Maybe it's an enum
					if ( value_type.IsEnum )
						return Enum.ToObject ( value_type, reader.Value );

					if ( reader.Token == JsonToken.String && value_type == typeof ( byte[] ) )
						return Convert.FromBase64String ( (string)reader.Value );

					ImporterFunc importer;

					// If there's a custom importer that fits, use it
					if ( custom_importers_table.ContainsKey ( json_type ) &&
						custom_importers_table[json_type].TryGetValue ( value_type, out importer ) )
					{
						return importer ( reader.Value );
					}

					// Maybe there's a base importer that works
					if ( base_importers_table.ContainsKey ( json_type ) &&
						base_importers_table[json_type].TryGetValue ( value_type, out importer ) )
					{
						return importer ( reader.Value );
					}

					try
					{
						return Convert.ChangeType ( reader.Value, value_type, CultureInfo.InvariantCulture );
					}
					catch ( Exception )
					{
						// Try using an implicit conversion operator
						var conv_op = GetConvOp ( value_type, json_type );

						if ( conv_op != null )
							return conv_op ( reader.Value );
					}

					// No luck
					throw new JsonException ( String.Format (
							"Can't assign value '{0}' (type {1}) to type {2}",
							reader.Value, json_type, inst_type ) );

					#endregion MyRegion

				case JsonToken.Null:
					if ( inst_type.IsClass || underlying_type != null )
					{
						return null;
					}

					throw new JsonException ( String.Format ( "Can't assign null to an instance of type {0}", inst_type ) );
			}

			return instance;
		}

		private static IJsonWrapper ReadValue ( WrapperFactory factory, JsonReader reader )
		{
			reader.Read ();

			if ( reader.Token == JsonToken.ArrayEnd ||
				reader.Token == JsonToken.Null )
				return null;

			IJsonWrapper instance = factory ();

			if ( reader.Token == JsonToken.String )
			{
				instance.SetString ( (string)reader.Value );
				return instance;
			}

			if ( reader.Token == JsonToken.Double )
			{
				instance.SetDouble ( (double)reader.Value );
				return instance;
			}

			if ( reader.Token == JsonToken.Int )
			{
				instance.SetInt ( (int)reader.Value );
				return instance;
			}

			if ( reader.Token == JsonToken.Long )
			{
				instance.SetLong ( (long)reader.Value );
				return instance;
			}

			if ( reader.Token == JsonToken.Boolean )
			{
				instance.SetBoolean ( (bool)reader.Value );
				return instance;
			}

			if ( reader.Token == JsonToken.ArrayStart )
			{
				instance.SetJsonType ( JsonType.Array );

				while ( true )
				{
					IJsonWrapper item = ReadValue ( factory, reader );
					if ( item == null && reader.Token == JsonToken.ArrayEnd )
						break;

					((IList)instance).Add ( item );
				}
			}
			else if ( reader.Token == JsonToken.ObjectStart )
			{
				instance.SetJsonType ( JsonType.Object );

				while ( true )
				{
					reader.Read ();

					if ( reader.Token == JsonToken.ObjectEnd )
						break;

					string property = (string)reader.Value;

					((IDictionary)instance)[property] = ReadValue ( factory, reader );
				}
			}

			return instance;
		}

		private static void ReadSkip ( JsonReader reader )
		{
			ToWrapper ( delegate { return new JsonMockWrapper (); }, reader );
		}

		private static void RegisterBaseExporters ()
		{
			base_exporters_table[typeof ( byte )] =
				delegate ( object obj, JsonWriter writer )
				{
					writer.Write ( Convert.ToInt32 ( (byte)obj ) );
				};

			base_exporters_table[typeof ( char )] =
				delegate ( object obj, JsonWriter writer )
				{
					writer.Write ( Convert.ToString ( (char)obj ) );
				};

			base_exporters_table[typeof ( DateTime )] =
				delegate ( object obj, JsonWriter writer )
				{
					writer.Write ( Convert.ToString ( (DateTime)obj,
													datetime_format ) );
				};

			base_exporters_table[typeof ( decimal )] =
				delegate ( object obj, JsonWriter writer )
				{
					writer.Write ( (decimal)obj );
				};

			base_exporters_table[typeof ( sbyte )] =
				delegate ( object obj, JsonWriter writer )
				{
					writer.Write ( Convert.ToInt32 ( (sbyte)obj ) );
				};

			base_exporters_table[typeof ( short )] =
				delegate ( object obj, JsonWriter writer )
				{
					writer.Write ( Convert.ToInt32 ( (short)obj ) );
				};

			base_exporters_table[typeof ( ushort )] =
				delegate ( object obj, JsonWriter writer )
				{
					writer.Write ( Convert.ToInt32 ( (ushort)obj ) );
				};

			base_exporters_table[typeof ( uint )] =
				delegate ( object obj, JsonWriter writer )
				{
					writer.Write ( Convert.ToUInt64 ( (uint)obj ) );
				};

			base_exporters_table[typeof ( ulong )] =
				delegate ( object obj, JsonWriter writer )
				{
					writer.Write ( (ulong)obj );
				};
		}

		private static void RegisterBaseImporters ()
		{
			ImporterFunc importer;

			importer = delegate ( object input )
			{
				return Convert.ToByte ( (int)input );
			};
			RegisterImporter ( base_importers_table, typeof ( int ),
							  typeof ( byte ), importer );

			importer = delegate ( object input )
			{
				return Convert.ToUInt64 ( (int)input );
			};
			RegisterImporter ( base_importers_table, typeof ( int ),
							  typeof ( ulong ), importer );

			importer = delegate ( object input )
			{
				return Convert.ToSByte ( (int)input );
			};
			RegisterImporter ( base_importers_table, typeof ( int ),
							  typeof ( sbyte ), importer );

			importer = delegate ( object input )
			{
				return Convert.ToInt16 ( (int)input );
			};
			RegisterImporter ( base_importers_table, typeof ( int ),
							  typeof ( short ), importer );

			importer = delegate ( object input )
			{
				return Convert.ToUInt16 ( (int)input );
			};
			RegisterImporter ( base_importers_table, typeof ( int ),
							  typeof ( ushort ), importer );

			importer = delegate ( object input )
			{
				return Convert.ToUInt32 ( (int)input );
			};
			RegisterImporter ( base_importers_table, typeof ( int ),
							  typeof ( uint ), importer );

			importer = delegate ( object input )
			{
				return Convert.ToSingle ( (int)input );
			};
			RegisterImporter ( base_importers_table, typeof ( int ),
							  typeof ( float ), importer );

			importer = delegate ( object input )
			{
				return Convert.ToDouble ( (int)input );
			};
			RegisterImporter ( base_importers_table, typeof ( int ),
							  typeof ( double ), importer );

			importer = delegate ( object input )
			{
				return Convert.ToDecimal ( (double)input );
			};
			RegisterImporter ( base_importers_table, typeof ( double ),
							  typeof ( decimal ), importer );

			importer = delegate ( object input )
			{
				return Convert.ToUInt32 ( (long)input );
			};
			RegisterImporter ( base_importers_table, typeof ( long ),
							  typeof ( uint ), importer );

			importer = delegate ( object input )
			{
				return Convert.ToChar ( (string)input );
			};
			RegisterImporter ( base_importers_table, typeof ( string ),
							  typeof ( char ), importer );

			importer = delegate ( object input )
			{
				return Convert.ToDateTime ( (string)input, datetime_format );
			};
			RegisterImporter ( base_importers_table, typeof ( string ),
							  typeof ( DateTime ), importer );
		}

		private static void RegisterImporter (
			Dictionary<Type, Dictionary<Type, ImporterFunc>> table,
			Type json_type, Type value_type, ImporterFunc importer )
		{
			Dictionary<Type, ImporterFunc> dict;
			if ( !table.TryGetValue ( json_type, out dict ) )
				table.Add ( json_type, dict = new Dictionary<Type, ImporterFunc> () );

			dict[value_type] = importer;
		}

		private static void WriteValue ( object obj, JsonWriter writer, bool writer_is_private, int depth )
		{
			if ( depth > max_nesting_depth )
				throw new JsonException (
					String.Format ( "Max allowed object depth reached while trying to export from type {0}", obj.GetType () ) );

			if ( obj == null )
			{
				writer.Write ( null );
				return;
			}

			Type obj_type = obj.GetType ();

			switch ( Type.GetTypeCode ( obj_type ) )
			{
				case TypeCode.Empty: break;
				case TypeCode.Object: break;
				case TypeCode.DBNull: break;
				case TypeCode.Boolean: writer.Write ( (bool)obj ); return;

				case TypeCode.Char: writer.Write ( (char)obj ); return;
				case TypeCode.SByte: writer.Write ( (sbyte)obj ); return;
				case TypeCode.Byte: writer.Write ( (byte)obj ); return;
				case TypeCode.Int16: writer.Write ( (short)obj ); return;
				case TypeCode.UInt16: writer.Write ( (ushort)obj ); return;
				case TypeCode.Int32: writer.Write ( (int)obj ); return;
				case TypeCode.UInt32: writer.Write ( (uint)obj ); return;
				case TypeCode.Int64: writer.Write ( (long)obj ); return;
				case TypeCode.UInt64: writer.Write ( (ulong)obj ); return;

				case TypeCode.Single: writer.Write ( (float)obj ); return;
				case TypeCode.Double: writer.Write ( (double)obj ); return;
				case TypeCode.Decimal: writer.Write ( (decimal)obj ); return;

				case TypeCode.DateTime: writer.Write ( ((DateTime)obj).ToString ( CultureInfo.InvariantCulture ) ); return;
				case TypeCode.String: writer.Write ( (string)obj ); return;
			}

			if ( obj is IJsonWrapper )
			{
				if ( writer_is_private )
					writer.TextWriter.Write ( ((IJsonWrapper)obj).ToJson () );
				else
					((IJsonWrapper)obj).ToJson ( writer );

				return;
			}

			if ( obj is byte[] )
			{
				writer.Write ( Convert.ToBase64String ( (byte[])obj ) );

				return;
			}

			if ( obj is Array )
			{
				writer.WriteArrayStart ();
				foreach ( object elem in (Array)obj )
					WriteValue ( elem, writer, writer_is_private, depth + 1 );
				writer.WriteArrayEnd ();

				return;
			}

			if ( obj is IList )
			{
				writer.WriteArrayStart ();
				foreach ( object elem in (IList)obj )
					WriteValue ( elem, writer, writer_is_private, depth + 1 );
				writer.WriteArrayEnd ();

				return;
			}

			if ( obj is IDictionary )
			{
				writer.WriteObjectStart ();
				foreach ( DictionaryEntry entry in (IDictionary)obj )
				{
					writer.WritePropertyName ( (string)entry.Key );
					WriteValue ( entry.Value, writer, writer_is_private,
								depth + 1 );
				}
				writer.WriteObjectEnd ();

				return;
			}

			ExporterFunc exporter;

			// See if there's a custom exporter for the object
			if ( custom_exporters_table.TryGetValue ( obj_type, out exporter ) )
			{
				exporter ( obj, writer );

				return;
			}

			// If not, maybe there's a base exporter
			if ( base_exporters_table.TryGetValue ( obj_type, out exporter ) )
			{
				exporter ( obj, writer );

				return;
			}

			// Last option, let's see if it's an enum
			if ( obj is Enum )
			{
				Type e_type = Enum.GetUnderlyingType ( obj_type );

				if ( e_type == typeof ( long )
					|| e_type == typeof ( uint )
					|| e_type == typeof ( ulong ) )
					writer.Write ( (ulong)obj );
				else
					writer.Write ( (int)obj );

				return;
			}

			// Okay, so it looks like the input should be exported as an
			// object
			List<PropertyMetadata> props = AddTypeProperties ( obj_type );

			writer.WriteObjectStart ();
			foreach ( PropertyMetadata p_data in props )
			{
				writer.WritePropertyName ( p_data.Info.Name );
				if ( p_data.IsField )
				{
					WriteValue ( ((FieldInfo)p_data.Info).GetValue ( obj ),
								writer, writer_is_private, depth + 1 );
				}
				else
				{
					WriteValue ( ((PropertyInfo)p_data.Info).GetValue ( obj, null ),
								writer, writer_is_private, depth + 1 );
				}
			}
			writer.WriteObjectEnd ();
		}

		#endregion Private Methods

		public static string ToJson ( object obj )
		{
			lock ( static_writer )
			{
				static_writer.Reset ();

				WriteValue ( obj, static_writer, true, 0 );

				return static_writer.ToString ();
			}
		}

		public static void ToJson ( object obj, JsonWriter writer )
		{
			WriteValue ( obj, writer, false, 0 );
		}

		public static JsonData ToJsonData ( JsonReader reader )
		{
			return (JsonData)ToWrapper ( delegate { return new JsonData (); }, reader );
		}

		public static JsonData ToJsonData ( string json )
		{
			return (JsonData)ToWrapper ( delegate { return new JsonData (); }, json );
		}

		public static object ToObject ( Type type, JsonData reader )
		{
			return ReadValue ( type, null, reader );
		}

		public static object ToObject ( Type type, Func<Type, object> factory, JsonReader reader )
		{
			return ReadValue ( type, factory, reader );
		}

		public static object ToObject ( Type type, JsonReader reader )
		{
			return ReadValue ( type, InstanceFactory.CreateInstance, reader );
		}

		public static object ToObject ( Type type, string json )
		{
			JsonReader reader = new JsonReader ( json );
			return ReadValue ( type, InstanceFactory.CreateInstance, reader );
		}

		public static T ToObject<T> ( JsonReader reader )
		{
			return (T)ToObject ( typeof ( T ), reader );
		}

		public static T ToObject<T> ( string json )
		{
			return (T)ToObject ( typeof ( T ), json );
		}

		public static IJsonWrapper ToWrapper ( WrapperFactory factory, JsonReader reader )
		{
			return ReadValue ( factory, reader );
		}

		public static IJsonWrapper ToWrapper ( WrapperFactory factory, string json )
		{
			JsonReader reader = new JsonReader ( json );
			return ReadValue ( factory, reader );
		}

		public static void FillObject ( object obj, JsonData reader )
		{
			ReadValue ( obj.GetType (), obj, reader );
		}

		public static void RegisterExporter<T> ( ExporterFunc<T> exporter )
		{
			ExporterFunc exporter_wrapper =
				delegate ( object obj, JsonWriter writer )
				{
					exporter ( (T)obj, writer );
				};

			custom_exporters_table[typeof ( T )] = exporter_wrapper;
		}

		public static void RegisterImporter<TJson, TValue> ( ImporterFunc<TJson, TValue> importer )
		{
			ImporterFunc importer_wrapper =
				delegate ( object input )
				{
					return importer ( (TJson)input );
				};

			RegisterImporter ( custom_importers_table, typeof ( TJson ),
							  typeof ( TValue ), importer_wrapper );
		}

		public static void UnregisterExporters ()
		{
			custom_exporters_table.Clear ();
		}

		public static void UnregisterImporters ()
		{
			custom_importers_table.Clear ();
		}
	}
}