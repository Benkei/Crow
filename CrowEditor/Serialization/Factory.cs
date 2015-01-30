using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrowSerialization.LitJson;

namespace CrowEditor.Serialization
{
	static class Factory
	{
		static Factory ()
		{
			JsonMapper.RegisterExporter<Guid> ( ( obj, writer ) => writer.Write ( obj.ToString ( "N" ) ) );
			JsonMapper.RegisterImporter<string, Guid> ( ( input ) => Guid.Parse ( input ) );
		}

		public static T Load<T> ( string filePath )
		{
			using ( var file = new StreamReader ( filePath, Encoding.UTF8 ) )
			{
				var reader = new JsonReader ( file );
				reader.AllowComments = true;
				return JsonMapper.ToObject<T> ( reader );
			}
		}

		public static void Save ( string filePath, object obj, bool prettyPrint = true )
		{
			using ( var file = new StreamWriter ( filePath, false, Encoding.UTF8 ) )
			{
				var writer = new JsonWriter ( file );
				writer.PrettyPrint = prettyPrint;
				JsonMapper.ToJson ( obj, writer );
			}
		}

		public static ProjektFile LoadProjectFile ( string filePath )
		{
			using ( var stream = new StreamReader ( filePath, Encoding.UTF8 ) )
			{
				var reader = new JsonReader ( stream );
				return JsonMapper.ToObject<ProjektFile> ( reader );
			}
		}
	}
}
