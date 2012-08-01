using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Tests.TestHelpers;
using umbraco.interfaces;

namespace Umbraco.Tests
{
	[TestFixture]
	public class CacheRefresherFactoryTests
	{
		[SetUp]
		public void Initialize()
		{
			TestHelper.SetupLog4NetForTests();

			//this ensures its reset
			PluginManager.Current = new PluginManager();

			//for testing, we'll specify which assemblies are scanned for the PluginTypeResolver
			PluginManager.Current.AssembliesToScan = new[]
				{
					this.GetType().Assembly
				};
		}

		[Test]
		public void Get_All_Instances()
		{			
			var factory = new umbraco.presentation.cache.Factory();
			Assert.AreEqual(2, factory.GetAll().Count());
		}

		#region Classes for tests
		public class CacheRefresher1 : ICacheRefresher
		{
			public Guid UniqueIdentifier
			{
				get { return new Guid("21350A89-4C0D-48B2-B25E-3EE19BFD59FF"); }
			}

			public string Name
			{
				get { return "CacheRefresher1"; }
			}

			public void RefreshAll()
			{
				throw new NotImplementedException();
			}

			public void Refresh(int Id)
			{
				throw new NotImplementedException();
			}

			public void Remove(int Id)
			{
				throw new NotImplementedException();
			}

			public void Refresh(Guid Id)
			{
				throw new NotImplementedException();
			}
		}

		public class CacheRefresher2 : ICacheRefresher
		{
			public Guid UniqueIdentifier
			{
				get { return new Guid("9266CB73-1FDE-4CD9-8DBC-159C2D39BE5D"); }
			}

			public string Name
			{
				get { return "CacheRefresher2"; }
			}

			public void RefreshAll()
			{
				throw new NotImplementedException();
			}

			public void Refresh(int Id)
			{
				throw new NotImplementedException();
			}

			public void Remove(int Id)
			{
				throw new NotImplementedException();
			}

			public void Refresh(Guid Id)
			{
				throw new NotImplementedException();
			}
		}
		#endregion

	}
}