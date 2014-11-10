﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LitJson;

namespace CrowEditor.Serialization
{
	static class Factory
	{
		static Factory ()
		{
			JsonMapper.RegisterExporter<Guid> ( ( obj, writer ) => writer.Write ( obj.ToString ( "N" ) ) );
			JsonMapper.RegisterImporter<string, Guid> ( ( input ) => Guid.Parse ( input ) );

			JsonMapper.RegisterExporter<DateTime> ( ( obj, writer ) => writer.Write ( obj.Ticks ) );
			JsonMapper.RegisterImporter<long, DateTime> ( ( input ) => new DateTime ( input ) );

			JsonMapper.RegisterExporter<JsonBinary> (
				( obj, writer ) => writer.Write ( Convert.ToBase64String ( obj.Data ) ) );
			JsonMapper.RegisterImporter<string, JsonBinary> (
				( input ) => new JsonBinary () { Data = Convert.FromBase64String ( input ) } );
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
			using ( var file = new StreamReader ( filePath, Encoding.UTF8 ) )
			{
				return JsonMapper.ToObject<ProjektFile> ( file );
			}
		}
	}
}