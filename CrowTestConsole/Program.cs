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
		class TestComp : Behavior, IActivatable
		{
			public void OnEnable ()
			{
			}

			public void OnDisable ()
			{
			}
		}

		[STAThread]
		static void Main ( string[] args )
		{
			var go = new GameObject ( "Root" );
			var trans = go.AddComponent<RectTransform> ();
			trans.AnchoredPosition = new Vector2 ( 600, 600 ) * 0.5f;
			trans.SizeDelta = new Vector2 ( 600, 600 );

			go.AddComponent<TestComp> ();

			var nodex = new GameObject ( "Child 5" );
			var child = nodex.AddComponent<Transform> ();
			child.Parent = trans;

			nodex = new GameObject ( "Child 6" );
			nodex.AddComponent<Transform> ();
			nodex.Transform.Parent = child;
			child = nodex.Transform;
			nodex = new GameObject ( "Child 7" );
			nodex.AddComponent<Transform> ();
			nodex.Transform.Parent = child;
			child = nodex.Transform;
			nodex = new GameObject ( "Child 8" );
			nodex.AddComponent<Transform> ();
			nodex.Transform.Parent = child;
			child = nodex.Transform;

			var node1 = new GameObject ( "Child 0" );
			var childTrans = node1.AddComponent<RectTransform> ();
			childTrans.Parent = trans;
			childTrans.AnchoredPosition = new Vector2 ( -150, 130 );
			childTrans.SizeDelta = new Vector2 ( 50, 50 );

			var node2 = new GameObject ( "Child 1" );
			var childTrans2 = node2.AddComponent<RectTransform> ();
			childTrans2.Parent = trans;
			childTrans2.AnchorMaximum = Vector2.One;
			childTrans2.AnchorMinimum = Vector2.Zero;
			//childTrans2.AnchoredPosition = new Vector2 ( -10, -10 );
			childTrans2.SizeDelta = new Vector2 ( -10, -10 );

			var rect = childTrans2.Rectangle;

			foreach ( var node in go.Transform.GetDeepEnumerable () )
			{
				Console.WriteLine ( new string ( ' ', node.Depth ) + node.GameObject.Name );
			}


			var scene = new Scene ();
			scene.AddGameObject ( go );

			foreach ( var item in scene.IterateComponents ( typeof ( IActivatable ) ) )
			{
				Console.WriteLine ( ((Component)item).GameObject.Name );
			}

			using ( var game = new Tutorial () )
			{
				game.Run ( 30, 30 );
			}
		}
	}
}
