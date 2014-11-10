using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrowEditor.AssetProcessors;

namespace CrowEditor.Jobs
{
	class AssetValidation : IJob
	{

		public void Execute ()
		{
			const string cacheFolder = "Library/Cache";

			var root = Path.Combine ( AssetDatabase.m_RootProjektFolder, cacheFolder );
			var assets = AssetDatabase.m_PathToGuid;
			foreach ( var item in assets )
			{
				var guid = item.Value.ToString ( "N" );
				var sub = guid.Substring ( 0, 2 );

				var path = Path.Combine ( root, sub );

				Directory.CreateDirectory ( path );

				path = Path.Combine ( path, guid );

				var extension = Path.GetExtension ( item.Key );

				AssetProcessor.Process ( item.Key, item.Value );
			}

		}
	}
}
