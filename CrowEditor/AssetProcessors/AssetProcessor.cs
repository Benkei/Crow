using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowEditor.AssetProcessors
{
	static class AssetProcessor
	{
		static Dictionary<string, Type> m_Processors = new Dictionary<string, Type> ( StringComparer.InvariantCultureIgnoreCase );

		static AssetProcessor ()
		{
			m_Processors.Add ( ".jpg", typeof ( TextureProcessor ) );
			m_Processors.Add ( ".tif", typeof ( TextureProcessor ) );
		}

		public static void Process ( string filePath, Guid guid )
		{
			const string cacheFolder = "Library/Metadata";

			var root = Path.Combine ( AssetDatabase.m_RootProjektFolder, cacheFolder );

			var _guid = guid.ToString ( "N" );
			var sub = _guid.Substring ( 0, 2 );

			var folder = Path.Combine ( root, sub );
			var metaFile = Path.Combine ( folder, _guid );

			if ( !Directory.Exists ( folder ) )
				Directory.CreateDirectory ( folder );

			var extension = Path.GetExtension ( filePath );
			var sourceFile = Path.Combine ( AssetDatabase.m_RootProjektFolder, filePath );

			Type type;
			if ( m_Processors.TryGetValue ( extension, out type ) )
			{
				var processor = (BaseProcessor)Activator.CreateInstance ( type );
				processor.Setup ( sourceFile, metaFile );
				processor.Run ();
			}
		}
	}
}
