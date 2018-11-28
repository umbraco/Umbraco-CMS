using System.IO;
using NUnit.Framework;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Logging.Serilog;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.UmbracoExamine
{
    [TestFixture]
    public abstract class ExamineBaseTest : TestWithDatabaseBase
    {
        [OneTimeSetUp]
        public void InitializeFixture()
        {
            var logger = new SerilogLogger(new FileInfo(TestHelper.MapPathForTest("~/unit-test.config")));
            _profilingLogger = new ProfilingLogger(logger, new LogProfiler(logger));
        }

        private IProfilingLogger _profilingLogger;
        protected override IProfilingLogger ProfilingLogger => _profilingLogger;

        /// <summary>
        /// sets up resolvers before resolution is frozen
        /// </summary>
        protected override void Compose()
        {
            base.Compose();

            Composition.RegisterSingleton<IShortStringHelper>(_ => new DefaultShortStringHelper(SettingsForTests.GetDefaultUmbracoSettings()));
        }
    }
}
