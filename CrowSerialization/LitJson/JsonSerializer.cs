using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace CrowSerialization.LitJson
{
	public class JsonSerializer : ISerializer
	{
		private readonly Dictionary<Type, Dictionary<Type, ImporterFunc>> m_Importers;
		private readonly Dictionary<Type, ExporterFunc> m_Exporters;
		private IFormatProvider m_Culture;

		public JsonSerializer ()
		{
			m_Importers = new Dictionary<Type, Dictionary<Type, ImporterFunc>> ();
			m_Exporters = new Dictionary<Type, ExporterFunc> ();
			m_Culture = CultureInfo.InvariantCulture;

			RegisterBaseImporters ();
			RegisterBaseExporters ();
		}


		public bool EnumAsString { get; set; }


		public void Serialize ( object obj, Stream stream )
		{
			using ( var w = new StreamWriter ( stream ) )
			{
				var writer = new JsonWriter ( w );
				WriteValue ( obj, writer, 0 );
			}
		}

		public object Deserialize ( Type instanceType, Stream stream )
		{
			using ( var r = new StreamReader ( stream ) )
			{
				var reader = new JsonReader ( r );
				return ReadValue ( instanceType, reader );
			}
		}


		private void WriteValue ( object obj, JsonWriter writer, int depth )
		{
			if ( depth > 100 )
				throw new JsonException ( string.Format ( "Max allowed object depth reached while trying to export from type {0}", obj.GetType () ) );

			if ( writer.Write ( obj ) )
			{
				return;
			}

			Type obj_type = obj.GetType ();

			if ( obj is Enum )
			{
				if ( EnumAsString )
				{
					writer.Write ( obj.ToString () );
				}
				else
				{
					object value = Convert.ChangeType ( obj, Enum.GetUnderlyingType ( obj_type ) );
					writer.Write ( value );
				}
				return;
			}

			ExporterFunc exporter;
			if ( m_Exporters.TryGetValue ( obj_type, out exporter ) )
			{
				exporter ( obj, writer );
				return;
			}

			CrowSerialization.ObjectMetadata meta;
			ReflectionCache.GetObjectMetadata ( obj_type, out meta );

			if ( meta.IsArray )
			{
				writer.WriteArrayStart ();
				foreach ( object elem in (Array)obj )
					WriteValue ( elem, writer, depth + 1 );
				writer.WriteArrayEnd ();

				return;
			}

			if ( meta.IsList )
			{
				writer.WriteArrayStart ();
				foreach ( object elem in (IList)obj )
					WriteValue ( elem, writer, depth + 1 );
				writer.WriteArrayEnd ();

				return;
			}

			if ( meta.IsDictionary )
			{
				writer.WriteObjectStart ();
				foreach ( DictionaryEntry entry in (IDictionary)obj )
				{
					writer.WritePropertyName ( (string)entry.Key );
					WriteValue ( entry.Value, writer, depth + 1 );
				}
				writer.WriteObjectEnd ();

				return;
			}

			// handle as normal obj

			writer.WriteObjectStart ();
			foreach ( var p_data in meta.Properties )
			{
				writer.WritePropertyName ( p_data.Key );

				var value = p_data.Value.GetValue ( obj );
				WriteValue ( value, writer, depth + 1 );
			}
			writer.WriteObjectEnd ();
		}

		private object ReadValue ( Type inst_type, JsonReader reader )
		{
			reader.Read ();

			Type underlying_type = Nullable.GetUnderlyingType ( inst_type );
			Type value_type = underlying_type ?? inst_type;

			object instance = null;

			CrowSerialization.ObjectMetadata meta;

			switch ( reader.Token )
			{
				//case JsonToken.None:
				//	break;

				case JsonToken.ObjectStart:

					#region MyRegion

					{
						ReflectionCache.GetObjectMetadata ( value_type, out meta );

						instance = meta.CreateInstance ();

						while ( true )
						{
							reader.Read ();

							if ( reader.Token == JsonToken.ObjectEnd )
								break;

							string propName = (string)reader.Value;

							CrowSerialization.PropertyMetadata propData;
							if ( meta.Properties.TryGetValue ( propName, out propData ) )
							{
								propData.SetValue ( instance, ReadValue ( propData.Type, reader ) );
							}
							else
							{
								if ( meta.IsDictionary )
								{
									((IDictionary)instance).Add ( propName, ReadValue ( meta.ValueType, reader ) );
								}
								else
								{
									if ( !reader.SkipNonMembers )
									{
										throw new JsonException ( string.Format ( "The type {0} doesn't have the property '{1}'", value_type, propName ) );
									}
									else
									{
										SkipReadValue ( reader );
										continue;
									}
								}

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
						ReflectionCache.GetObjectMetadata ( value_type, out meta );

						if ( !meta.IsArray && !meta.IsList )
							throw new JsonException ( string.Format ( "Type {0} can't act as an array", value_type ) );

						IList list = meta.IsArray ? new ArrayList () : (IList)meta.CreateInstance ();
						Type elem_type = meta.ValueType;

						while ( true )
						{
							object item = ReadValue ( elem_type, reader );
							if ( reader.Token == JsonToken.ArrayEnd )
								break;

							list.Add ( item );
						}

						if ( meta.IsArray )
						{
							int n = list.Count;
							instance = Array.CreateInstance ( elem_type, n );

							list.CopyTo ( (Array)instance, 0 );

							//for ( int i = 0; i < n; i++ )
							//	((Array)instance).SetValue ( list[i], i );
						}
						else
							instance = list;
					}

					#endregion MyRegion

					break;

				//case JsonToken.ArrayEnd:
				//	break;

				case JsonToken.Int:
				case JsonToken.Long:
				case JsonToken.Double:
				case JsonToken.String:
				case JsonToken.Boolean:

					#region MyRegion

					Type json_type = reader.Value.GetType ();

					if ( value_type.IsAssignableFrom ( json_type ) )
					{
						// is same type
						instance = reader.Value;
						break;
					}
					if ( value_type.IsEnum )
					{
						if ( reader.Token == JsonToken.String )
							instance = Enum.Parse ( value_type, (string)reader.Value, true );
						else
							instance = Enum.ToObject ( value_type, reader.Value );
						break;
					}
					if ( value_type.IsPrimitive && json_type.IsPrimitive )
					{
						// convert diffent primitive type
						instance = Convert.ChangeType ( reader.Value, value_type, m_Culture );
						break;
					}

					Dictionary<Type, ImporterFunc> table;
					ImporterFunc importer;

					if ( m_Importers.TryGetValue ( json_type, out table )
						&& table.TryGetValue ( value_type, out importer ) )
					{
						instance = importer ( reader.Value );
						break;
					}

					// No luck
					throw new JsonException ( string.Format ( "Can't assign value '{0}' (type {1}) to type {2}", reader.Value, json_type, value_type ) );

					#endregion MyRegion

				case JsonToken.Null:
					if ( value_type.IsClass || underlying_type != null )
					{
						break;
					}
					instance = InstanceFactory.CreateInstance ( value_type );
					break;
			}

			return instance;
		}

		private void SkipReadValue ( JsonReader reader )
		{
			reader.Read ( true );

			switch ( reader.Token )
			{
				case JsonToken.ObjectStart:
					while ( true )
					{
						reader.Read ( true );

						if ( reader.Token == JsonToken.ObjectEnd )
							break;

						SkipReadValue ( reader );
					}
					break;
				case JsonToken.ArrayStart:
					while ( true )
					{
						reader.Read ( true );

						if ( reader.Token == JsonToken.ArrayEnd )
							break;

						SkipReadValue ( reader );
					}
					break;
			}
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

		private void RegisterBaseImporters ()
		{
			RegisterImporter ( m_Importers,
				typeof ( string ), typeof ( byte[] ),
				( value ) => Convert.FromBase64String ( (string)value ) );
			RegisterImporter ( m_Importers,
				typeof ( string ), typeof ( DateTime ),
				( value ) => Convert.ToDateTime ( (string)value, m_Culture ) );
		}

		private void RegisterBaseExporters ()
		{
			m_Exporters[typeof ( byte[] )] = ( obj, writer ) => writer.Write ( Convert.ToBase64String ( (byte[])obj ) );
			m_Exporters[typeof ( DateTime )] = ( obj, writer ) => writer.Write ( ((DateTime)obj).ToString ( m_Culture ) );
		}

	}
}
