using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowEngine
{
	public class Material
	{
		Dictionary<string, Variable> m_VariableMap = new Dictionary<string, Variable> ();



		public enum ValueType : byte
		{
			None, 
			Half, Single, Double, 
			Byte, Int, Long,
		}
		unsafe struct Variable
		{
			public ValueType Type;
			public fixed byte Data[4 * 4 * sizeof ( int )];
		}

	}
}
