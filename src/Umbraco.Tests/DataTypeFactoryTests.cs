using System;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.ObjectResolution;
using Umbraco.Tests.TestHelpers;
using umbraco.cms.businesslogic.datatype;
using umbraco.interfaces;
using System.Linq;

namespace Umbraco.Tests
{

	[TestFixture]
	public class DataTypeFactoryTests
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

			DataTypesResolver.Current = new DataTypesResolver(
				PluginManager.Current.ResolveDataTypes());

			Resolution.Freeze();
		}

		[TearDown]
		public void TearDown()
		{
			DataTypesResolver.Reset();
			Resolution.IsFrozen = false;
		}

		[Test]
		public void Get_All_Instances()
		{
			var factory = new umbraco.cms.businesslogic.datatype.controls.Factory();
			Assert.AreEqual(2, factory.GetAll().Count());
		}

		#region Classes for tests
		public class DataType1 : AbstractDataEditor
		{
			public override Guid Id
			{
				get { return new Guid("FBAEA49B-F704-44FE-B725-6C8FE0767CF2"); }
			}

			public override string DataTypeName
			{
				get { return "DataType1"; }
			}

			public override IDataEditor DataEditor
			{
				get { throw new NotImplementedException(); }
			}

			public override IDataPrevalue PrevalueEditor
			{
				get { throw new NotImplementedException(); }
			}

			public override IData Data
			{
				get { throw new NotImplementedException(); }
			}
		}

		public class DataType2 : AbstractDataEditor
		{
			public override Guid Id
			{
				get { return new Guid("3F58099B-96AC-415E-B3F9-BA273F51E681"); }
			}

			public override string DataTypeName
			{
				get { return "DataType2"; }
			}

			public override IDataEditor DataEditor
			{
				get { throw new NotImplementedException(); }
			}

			public override IDataPrevalue PrevalueEditor
			{
				get { throw new NotImplementedException(); }
			}

			public override IData Data
			{
				get { throw new NotImplementedException(); }
			}
		}
		#endregion

	}
}