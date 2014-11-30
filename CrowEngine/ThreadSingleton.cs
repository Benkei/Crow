using System;
using CrowEngine.Reflection;

namespace CrowEngine
{
	static class ThreadSingleton<T>
		where T : class, new ()
	{
		[ThreadStatic]
		private static T m_Instance;

		public static T Instance
		{
			get { return m_Instance ?? (m_Instance = InstanceFactory<T>.Instance ()); }
		}
	}
}
