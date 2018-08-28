using Moq;
using System.IO;
using LightInject;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Profiling;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;
using Umbraco.Examine;

namespace Umbraco.Tests.UmbracoExamine
{
    [TestFixture]
    public abstract class ExamineBaseTest : TestWithDatabaseBase
    {
        [OneTimeSetUp]
        public void InitializeFixture()
        {
            var logger = new Logger(new FileInfo(TestHelper.MapPathForTest("~/unit-test.config")));
            _profilingLogger = new ProfilingLogger(logger, new LogProfiler(logger));
        }

        private ProfilingLogger _profilingLogger;
        protected override ProfilingLogger ProfilingLogger
        {
            get
            {
                return _profilingLogger;
            }
        }

        /// <summary>
        /// sets up resolvers before resolution is frozen
        /// </summary>
        protected override void Compose()
        {
            base.Compose();

            Container.RegisterSingleton<IShortStringHelper>(_ => new DefaultShortStringHelper(SettingsForTests.GetDefaultUmbracoSettings()));
        }
    }
}
