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
		}

		[TestMethod]
		public void TestMethod1 ()
		{
			//var dict = new Dictionary<string, int> ();
			//Type objType = dict.GetType ();
			//var b = objType.IsGenericType ;
			//var genericType = objType.GetGenericTypeDefinition ();
			//var itemTypes = objType.GetGenericArguments ();

			var ser = new UbJsonSerializer ();

			var obj = new TestStruct ();

			var mem = new MemoryStream ();

			ser.Serialize ( obj, mem );

			mem.Position = 0;

			var serObj = ser.Deserialize ( typeof ( TestStruct ), mem );
		}
	}
}
