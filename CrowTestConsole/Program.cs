using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrowEngine;
using CrowEngine.Components;
using CrowEngine.Mathematics;

namespace CrowTestConsole
{
	class Program
	{
		[STAThread]
		static void Main ( string[] args )
		{
			var go = new GameObject ();
			var trans = go.AddComponent<RectTransform> ();
			trans.LocalPosition = new Vector3 ( 600, 600, 0 ) * 0.5f;
			trans.SizeDelta = new Vector2 ( 600, 600 );

			var node = new GameObject ();
			var transNode = node.AddComponent<RectTransform> ();
			transNode.Parent = trans;


			//using ( var game = new Tutorial () )
			//{
			//	game.Run ( 30, 30 );
			//}
		}
	}
}
