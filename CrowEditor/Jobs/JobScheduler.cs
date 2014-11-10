using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowEditor.Jobs
{
	class JobScheduler : List<IJob>
	{
		public void AddJob ( IJob job )
		{
			lock ( this )
			{
				Add ( job );
			}
		}

		public bool Execute ()
		{
			if ( Count > 0 )
			{
				lock ( this )
				{
					for ( int i = Count - 1; i >= 0; i-- )
					{
						//try
						//{
							this[i].Execute ();
						//}
						//catch ( Exception ex )
						//{
						//	Console.WriteLine ( ex.ToString () );
						//}
						RemoveAt ( i );
					}
				}
				return true;
			}
			return false;
		}
	}
}
