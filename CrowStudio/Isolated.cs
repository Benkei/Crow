﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowStudio
{
	sealed class Isolated<T> : IDisposable
		where T : MarshalByRefObject
	{
		private AppDomain _domain;
		private T _value;

		public Isolated ()
		{
			AppDomainSetup setup = new AppDomainSetup ()
			{
				LoaderOptimization = LoaderOptimization.MultiDomain,
			};
			_domain = AppDomain.CreateDomain ( "Isolated:" + Guid.NewGuid (), null, setup );

			Type type = typeof ( T );
			_value = (T)_domain.CreateInstanceAndUnwrap ( type.Assembly.FullName, type.FullName );
		}

		public T Value
		{
			get { return _value; }
		}

		public void Dispose ()
		{
			if ( _domain != null )
			{
				AppDomain.Unload ( _domain );

				_domain = null;
			}
		}
	}
}
