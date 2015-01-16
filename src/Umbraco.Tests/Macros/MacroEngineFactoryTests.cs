using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using umbraco.cms.businesslogic.macro;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Profiling;
using umbraco.interfaces;

namespace Umbraco.Tests.Macros
{
	[TestFixture]
	public class MacroEngineFactoryTests
	{
		[SetUp]
		public void Initialize()
		{
            var logger = new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>());

		    //this ensures its reset
		    PluginManager.Current = new PluginManager(new ActivatorServiceProvider(), new NullCacheProvider(),
		        logger,
		        false)
		    {
		        AssembliesToScan = new[]
		        {
		            this.GetType().Assembly
		        }
		    };

			//for testing, we'll specify which assemblies are scanned for the PluginTypeResolver
		}

        [TearDown]
        public void TearDown()
        {
            PluginManager.Current = null;
        }

		[Test]
		public void Get_All()
		{			
			var engines = MacroEngineFactory.GetAll();
			Assert.AreEqual(2, engines.Count());
		}

		[Test]
		public void Get_Engine()
		{
			var engine1 = MacroEngineFactory.GetEngine("MacroEngine1");
			Assert.IsNotNull(engine1);
		}

		[Test]
		public void Get_By_Filename()
		{
			var engine1 = MacroEngineFactory.GetByFilename("test.me1");
			var engine2 = MacroEngineFactory.GetByFilename("test.me2");
			Assert.IsNotNull(engine1);
			Assert.IsNotNull(engine2);
			Assert.Throws<MacroEngineException>(() => MacroEngineFactory.GetByFilename("test.blah"));

		}

		[Test]
		public void Get_By_Extension()
		{
			var engine1 = MacroEngineFactory.GetByExtension("me1");
			var engine2 = MacroEngineFactory.GetByExtension("me2");
			Assert.IsNotNull(engine1);
			Assert.IsNotNull(engine2);
			Assert.Throws<MacroEngineException>(() => MacroEngineFactory.GetByExtension("blah"));
		}

		#region Classes for tests
		public class MacroEngine1 : IMacroEngine
		{
			public string Name
			{
				get { return "MacroEngine1"; }
			}

			public IEnumerable<string> SupportedExtensions
			{
				get { return new[] {"me1"}; }
			}

			public IEnumerable<string> SupportedUIExtensions
			{
				get { throw new NotImplementedException(); }
			}

			public Dictionary<string, IMacroGuiRendering> SupportedProperties
			{
				get { throw new NotImplementedException(); }
			}

			public bool Validate(string code, string tempFileName, INode currentPage, out string errorMessage)
			{
				throw new NotImplementedException();
			}

			public string Execute(MacroModel macro, INode currentPage)
			{
				throw new NotImplementedException();
			}
		}

		public class MacroEngine2 : IMacroEngine
		{
			public string Name
			{
				get { return "MacroEngine2"; }
			}

			public IEnumerable<string> SupportedExtensions
			{
				get { return new[] { "me2" }; }
			}

			public IEnumerable<string> SupportedUIExtensions
			{
				get { throw new NotImplementedException(); }
			}

			public Dictionary<string, IMacroGuiRendering> SupportedProperties
			{
				get { throw new NotImplementedException(); }
			}

			public bool Validate(string code, string tempFileName, INode currentPage, out string errorMessage)
			{
				throw new NotImplementedException();
			}

			public string Execute(MacroModel macro, INode currentPage)
			{
				throw new NotImplementedException();
			}
		}
		#endregion
	}
}