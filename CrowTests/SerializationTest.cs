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
		class TestStruct
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
		public void TestMethod1 ()
		{
			//var dict = new Dictionary<string, int> ();
			//Type objType = dict.GetType ();
			//var b = objType.IsGenericType ;
			//var genericType = objType.GetGenericTypeDefinition ();
			//var itemTypes = objType.GetGenericArguments ();

			var ser = new UbjsonSerializer ();

			var obj = new TestStruct ();

			var mem = new MemoryStream ();

			ser.Serialize ( obj, mem );

			mem.Position = 0;

			var serObj = (TestStruct)ser.Deserialize ( typeof ( TestStruct ), mem );

			Assert.AreEqual ( obj.Alter, serObj.Alter );
			Assert.AreEqual ( obj.Muh, serObj.Muh );
			Assert.AreEqual ( obj.Size, serObj.Size );
		}
	}
}
