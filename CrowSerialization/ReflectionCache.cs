using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CrowSerialization
{
	internal struct PropertyMetadata
	{
		public MemberInfo Info;
		public bool IsField;
		public Type Type;
	}

	internal static class ReflectionCache
	{
		private static Dictionary<Type, List<PropertyMetadata>> m_TypeProperties = new Dictionary<Type, List<PropertyMetadata>> ();
		
		public static void GetTypeProperties ( Type type, out List<PropertyMetadata> propList )
		{
			lock ( m_TypeProperties )
			{
				if ( !m_TypeProperties.TryGetValue ( type, out propList ) )
					return;

				var fields = type.GetFields ();
				var props = type.GetProperties ();

				propList = new List<PropertyMetadata> ( fields.Length + props.Length );

				PropertyMetadata data = new PropertyMetadata ();

				data.IsField = true;
				foreach ( var item in fields )
				{
					data.Info = item;
					data.Type = item.FieldType;
					propList.Add ( data );
				}

				data.IsField = false;
				foreach ( var item in props )
				{
					if ( item.CanWrite && item.CanRead )
					{
						data.Info = item;
						data.Type = item.PropertyType;
						propList.Add ( data );
					}
				}

				m_TypeProperties.Add ( type, propList );
			}
		}

	}
}
