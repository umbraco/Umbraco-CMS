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
            var logger = new Logger(new FileInfo(TestHelper.MapPathForTest("~/unit-test-log4net.config")));
            ProfilingLogger = new ProfilingLogger(logger, new LogProfiler(logger));
        }

        protected ProfilingLogger ProfilingLogger { get; private set; }

        /// <summary>
        /// sets up resolvers before resolution is frozen
        /// </summary>
        protected override void Compose()
        {
            base.Compose();

            Container.RegisterSingleton<IShortStringHelper>(_ => new DefaultShortStringHelper(SettingsForTests.GetDefault()));
        }
    }
}
