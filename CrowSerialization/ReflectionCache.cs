using System;
using System.Collections.Generic;
using System.Reflection;

namespace CrowSerialization
{
	internal struct ObjectMetadata
	{
		public Type KeyType;
		public Type ValueType; // IList and IDict.
		public bool IsDictionary;
		public bool IsList;
		public Func<object> CreateInstance;

		public Dictionary<string, PropertyMetadata> Properties;
	}

	internal struct PropertyMetadata
	{
		public bool IsField;
		public MemberInfo Info;
		public Type Type;

		public void SetValue ( object obj, object value )
		{
			if ( IsField )
			{
				((FieldInfo)Info).SetValue ( obj, value );
			}
			else
			{
				((PropertyInfo)Info).SetValue ( obj, value, null );
			}
		}

		public object GetValue ( object obj )
		{
			if ( IsField )
			{
				return ((FieldInfo)Info).GetValue ( obj );
			}
			else
			{
				return ((PropertyInfo)Info).GetValue ( obj, null );
			}
		}
	}

	internal static class ReflectionCache
	{
		private const BindingFlags Bindings = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

		private static readonly Dictionary<Type, ObjectMetadata> m_ObjectMetadata = new Dictionary<Type, ObjectMetadata> ();

		public static void GetObjectMetadata ( Type type, out ObjectMetadata metadata )
		{
			lock ( m_ObjectMetadata )
			{
				if ( m_ObjectMetadata.TryGetValue ( type, out metadata ) )
					return;

				metadata = new ObjectMetadata ();
				metadata.CreateInstance = InstanceFactory.CreateInstanceCallback<object> ( type );

				if ( type.IsGenericType )
				{
					foreach ( Type item in type.GetInterfaces () )
					{
						if ( item.IsGenericType )
						{
							var gType = item.GetGenericTypeDefinition ();
							var aType = item.GetGenericArguments ();
							if ( gType == typeof ( IDictionary<,> ) )
							{
								metadata.IsDictionary = true;
								metadata.KeyType = aType[0];
								metadata.ValueType = aType[1];
							}
							if ( gType == typeof ( IList<> ) )
							{
								metadata.IsList = true;
								metadata.ValueType = aType[0];
							}
						}
					}
					if ( metadata.IsDictionary && metadata.IsList )
					{
						metadata.IsDictionary = false;
						metadata.IsList = false;
						metadata.KeyType = null;
						metadata.ValueType = null;
					}
					else
					{
						m_ObjectMetadata.Add ( type, metadata );
						return;
					}
				}

				var fields = type.GetFields ( Bindings );
				var props = type.GetProperties ( Bindings );

				metadata.Properties = new Dictionary<string, PropertyMetadata> ( fields.Length + props.Length );

				PropertyMetadata data = new PropertyMetadata ();
				data.IsField = true;
				foreach ( var item in fields )
				{
					data.Info = item;
					data.Type = item.FieldType;

					metadata.Properties.Add ( item.Name, data );
				}

				data.IsField = false;
				foreach ( var item in props )
				{
					if ( item.CanRead && item.CanWrite && item.GetIndexParameters ().Length == 0 )
					{
						data.Info = item;
						data.Type = item.PropertyType;

						metadata.Properties.Add ( item.Name, data );
					}
				}

				m_ObjectMetadata.Add ( type, metadata );
			}
		}
	}
}