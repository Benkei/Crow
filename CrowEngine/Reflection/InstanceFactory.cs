using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace CrowEngine.Reflection
{
	public static class InstanceFactory
	{
		private static Dictionary<Type, Func<object>> m_CreateInstance = new Dictionary<Type, Func<object>> ();

		public static object Construct ( Type instanceType )
		{
			Func<object> creator;
			lock ( m_CreateInstance )
			{
				if ( !m_CreateInstance.TryGetValue ( instanceType, out creator ) )
				{
					creator = Creator<object> ( instanceType );
					m_CreateInstance.Add ( instanceType, creator );
				}
			}
			return creator ();
		}

		internal static Func<T> Creator<T> ( Type instanceType )
		{
			lock ( m_CreateInstance )
			{
				if ( instanceType == typeof ( string ) )
					return Expression.Lambda<Func<T>> ( Expression.Constant ( string.Empty ) ).Compile ();

				if ( instanceType.IsValueType || instanceType.GetConstructor ( Type.EmptyTypes ) != null )
					return Expression.Lambda<Func<T>> ( Expression.New ( instanceType ) ).Compile ();

				return () => (T)FormatterServices.GetUninitializedObject ( instanceType );
			}
		}
	}

	public static class InstanceFactory<T>
	{
		public static readonly Func<T> Instance = InstanceFactory.Creator<T> ( typeof ( T ) );
	}
}
