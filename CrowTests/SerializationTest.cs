using System;
using System.Collections.Generic;
using System.IO;
using CrowSerialization.UbJson;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CrowTests
{
	[TestClass]
	public class SerializationTest
	{
		class MainClass
		{
			public string Muh = "Ich bins";
			public int Alter = 23123;
			public float Size = 234f;

			public Subclass SubData = new Subclass ();
		}

		class Subclass
		{
			public Subclass Parent;
			public decimal Values = 2341253240984982374;
			public char Muh = 'X';
			public DateTime Time = new DateTime ( 2000, 10, 22, 10, 30, 54 );
			public List<string> Names = new List<string> () { "kutsi", "kevin", "gott", "jesus" };
			public Dictionary<string, int> Prios = new Dictionary<string, int> () { { "Muh", 23 }, { "Nyan", 5845 } };
		}

		[TestMethod]
		public void TestReadWrite ()
		{
			//var dict = new Dictionary<string, int> ();
			//Type objType = dict.GetType ();
			//var b = objType.IsGenericType ;
			//var genericType = objType.GetGenericTypeDefinition ();
			//var itemTypes = objType.GetGenericArguments ();

			var ser = new UbjsonSerializer ();

			var obj = new MainClass ();

			var mem = new MemoryStream ();

			ser.Serialize ( obj, mem );

			mem.Position = 0;

			var serObj = (MainClass)ser.Deserialize ( typeof ( MainClass ), mem );

			Assert.AreEqual ( obj.Alter, serObj.Alter );
			Assert.AreEqual ( obj.Muh, serObj.Muh );
			Assert.AreEqual ( obj.Size, serObj.Size );
		}


		class Verion0
		{
			public string HeyDuDa = ";sdasdagsv";
			public int Sasd = 34;
			public Subclass Muh = new Subclass ();
			public double MOffu = 45345;
			public string DiePost = "sdkjlfnvkdjn";
		}
		class Verion1
		{
			public string HeyDuDa;
			public int Sasd;
			public double MOffu;
			public string DiePost;
		}


		[TestMethod]
		public void TestSkip ()
		{
			var mem = new MemoryStream ();
			var ser = new UbjsonSerializer ();

			var ver0 = new Verion0 ();

			ser.Serialize ( ver0, mem );

			mem.Position = 0;

			var ver1 = ser.Deserialize<Verion1> ( mem );

			Assert.AreEqual ( ver0.HeyDuDa, ver1.HeyDuDa );
			Assert.AreEqual ( ver0.Sasd, ver1.Sasd );
			Assert.AreEqual ( ver0.MOffu, ver1.MOffu );
			Assert.AreEqual ( ver0.DiePost, ver1.DiePost );
		}

	}
}
