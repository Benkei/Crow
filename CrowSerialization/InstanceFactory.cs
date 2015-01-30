using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace CrowSerialization
{
	internal static class InstanceFactory
	{
		private static Dictionary<Type, Func<object>> m_CreateInstance = new Dictionary<Type, Func<object>> ();

		public static object CreateInstance ( Type instanceType )
		{
			return CreateInstanceCallback ( instanceType ) ();
		}

		public static Func<object> CreateInstanceCallback ( Type instanceType )
		{
			Func<object> creator;
			lock ( m_CreateInstance )
			{
				if ( !m_CreateInstance.TryGetValue ( instanceType, out creator ) )
				{
					creator = CreateInstanceCallback<object> ( instanceType );
					m_CreateInstance.Add ( instanceType, creator );
				}
			}
			return creator;
		}

		public static Func<T> CreateInstanceCallback<T> ( Type instanceType )
		{
			if ( instanceType == typeof ( string ) )
				return Expression.Lambda<Func<T>> ( Expression.Constant ( string.Empty ) ).Compile ();

			if ( instanceType.IsValueType || instanceType.GetConstructor ( Type.EmptyTypes ) != null )
				return Expression.Lambda<Func<T>> ( Expression.Convert ( Expression.New ( instanceType ), typeof ( T ) ) ).Compile ();

			return () => (T)FormatterServices.GetUninitializedObject ( instanceType );
		}
	}

	internal static class InstanceFactory<T>
	{
		public static readonly Func<T> Instance = InstanceFactory.CreateInstanceCallback<T> ( typeof ( T ) );
	}
}