using System.IO;
using NUnit.Framework;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Profiling;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;
using UmbracoExamine;

namespace Umbraco.Tests.UmbracoExamine
{
    [TestFixture]
    public abstract class ExamineBaseTest : BaseUmbracoConfigurationTest
    {
        [TestFixtureSetUp]
        public void InitializeFixture()
        {
            var logger = new Logger(new FileInfo(TestHelper.MapPathForTest("~/unit-test-log4net.config")));
            ProfilingLogger = new ProfilingLogger(logger, new LogProfiler(logger));
        }

        protected ProfilingLogger ProfilingLogger { get; private set; }

        [SetUp]
        public virtual void TestSetup()
        {
            ShortStringHelperResolver.Current = new ShortStringHelperResolver(new DefaultShortStringHelper(SettingsForTests.GetDefault()));

            Resolution.Freeze();
        }

        [TearDown]
        public virtual void TestTearDown()
        {
            //reset all resolvers
            ResolverCollection.ResetAll();
            //reset resolution itself (though this should be taken care of by resetting any of the resolvers above)
            Resolution.Reset();
        }
    }
}