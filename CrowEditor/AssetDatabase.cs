using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrowEditor.Jobs;
using CrowEditor.Serialization;

namespace CrowEditor
{
	public static class AssetDatabase
	{
		internal static string m_RootProjektFolder;
		internal static PathDictionary<Guid> m_PathToGuid = new PathDictionary<Guid> ();
		internal static ProjektFile m_ProjektFile;

		public static bool LoadProjectFile ( string filePath )
		{
			m_RootProjektFolder = Path.GetDirectoryName ( filePath );

			m_ProjektFile = Factory.Load<ProjektFile> ( filePath );

			m_PathToGuid.Clear ();
			foreach ( var item in m_ProjektFile.Assets )
			{
				m_PathToGuid.Add ( item.Key.Trim (), item.Value.Guid );
			}


			return true;
		}

		public static bool SaveProjectFile ( string filePath )
		{
			if ( m_ProjektFile == null )
				m_ProjektFile = new ProjektFile ();

			m_ProjektFile.Assets.Clear ();
			foreach ( var item in m_PathToGuid )
			{
				var info = new AssetInfo ();
				info.Guid = item.Value;
				m_ProjektFile.Assets.Add ( item.Key, info );
			}

			Factory.Save ( filePath, m_ProjektFile, true );

			return true;
		}

	}

	public class PathDictionary<T> : SortedDictionary<string, T>
	{
		public PathDictionary ()
			: base ( StringComparer.InvariantCultureIgnoreCase )
		{ }
	}
}
