using System;
using NUnit.Framework;
using SqlCE4Umbraco;
using Umbraco.Core;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using umbraco.DataLayer;
using umbraco.MacroEngines;
using umbraco.MacroEngines.Iron;
using umbraco.businesslogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.interfaces;
using umbraco.uicontrols;
using System.Linq;
using BaseDataType = umbraco.editorControls.BaseDataType;

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
			PluginTypeResolver.Current = new PluginTypeResolver();

			//for testing, we'll specify which assemblies are scanned for the PluginTypeResolver
			PluginTypeResolver.Current.AssembliesToScan = new[]
			    {
			        this.GetType().Assembly
			    };
		}

		[Test]
		public void Find_All_DataTypes()
		{
			umbraco.cms.businesslogic.datatype.controls.Factory.Initialize();
			Assert.AreEqual(2, umbraco.cms.businesslogic.datatype.controls.Factory._controls.Count);
		}

		[Test]
		public void Get_All_Instances()
		{
			umbraco.cms.businesslogic.datatype.controls.Factory.Initialize();
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