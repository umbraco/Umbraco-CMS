using System.IO;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Logging.Serilog;
using Umbraco.Core.Strings;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Components;
using Umbraco.Tests.TestHelpers;
using NullLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger;

namespace Umbraco.Tests.UmbracoExamine
{
    [TestFixture]
    public abstract class ExamineBaseTest : TestWithDatabaseBase
    {
        [OneTimeSetUp]
        public void InitializeFixture()
        {

            var logger = new SerilogLogger<object>(new FileInfo(TestHelper.MapPathForTestFiles("~/unit-test.config")));
            _profilingLogger = new ProfilingLogger(NullLogger.Instance, new LogProfiler(logger));
        }

        private IProfilingLogger _profilingLogger;
        protected override IProfilingLogger ProfilingLogger => _profilingLogger;



        /// <summary>
        /// sets up resolvers before resolution is frozen
        /// </summary>
        protected override void Compose()
        {
            base.Compose();
            var requestHandlerSettings = new RequestHandlerSettingsBuilder().Build();
            Composition.RegisterUnique<IShortStringHelper>(_ => new DefaultShortStringHelper(Microsoft.Extensions.Options.Options.Create(requestHandlerSettings)));
        }
    }
}
