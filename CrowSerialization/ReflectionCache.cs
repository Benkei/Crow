using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CrowSerialization
{
	internal struct ObjectMetadata
	{
		public Type element_type;
		public bool is_dictionary;

		public Dictionary<string, PropertyMetadata> properties;

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
		const BindingFlags Bindings = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

		private static Dictionary<Type, ObjectMetadata> m_ObjectMetadata = new Dictionary<Type, ObjectMetadata> ();

		public static void GetTypeProperties ( Type type, out Dictionary<string, PropertyMetadata> propList )
		{
			ObjectMetadata metadata;
			GetObjectMetadata ( type, out metadata );
			propList = metadata.properties;
		}

		public static void GetObjectMetadata ( Type type, out ObjectMetadata metadata )
		{
			lock ( m_ObjectMetadata )
			{
				if ( m_ObjectMetadata.TryGetValue ( type, out metadata ) )
					return;

				var fields = type.GetFields ( Bindings );
				var props = type.GetProperties ( Bindings );

				metadata = new ObjectMetadata ();

				//if ( typeof ( IDictionary ).IsAssignableFrom ( type ) )
				//	data.IsDictionary = true;

				metadata.properties = new Dictionary<string, PropertyMetadata> ( fields.Length + props.Length );

				PropertyMetadata data = new PropertyMetadata ();
				data.IsField = true;
				foreach ( var item in fields )
				{
					data.Info = item;
					data.Type = item.FieldType;

					metadata.properties.Add ( item.Name, data );
				}

				data.IsField = false;
				foreach ( var item in props )
				{
					if ( item.CanRead && item.CanWrite && item.GetIndexParameters ().Length == 0 )
					{
						data.Info = item;
						data.Type = item.PropertyType;

						metadata.properties.Add ( item.Name, data );
					}
				}

				m_ObjectMetadata.Add ( type, metadata );
			}
		}
	}
}
