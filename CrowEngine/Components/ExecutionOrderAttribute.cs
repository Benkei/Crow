using System;

namespace CrowEngine.Components
{
	/// <summary>
	/// 
	/// </summary>
	[AttributeUsage ( AttributeTargets.Class, Inherited = false, AllowMultiple = false )]
	public sealed class ExecutionOrderAttribute : Attribute
	{
		/// <summary>
		/// get the execution order
		/// </summary>
		public int Order { get; private set; }


		public ExecutionOrderAttribute ( int order )
		{
			Order = order;
		}
	}
}
