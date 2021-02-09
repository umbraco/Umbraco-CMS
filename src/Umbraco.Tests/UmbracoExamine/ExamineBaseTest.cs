using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Strings;
using Umbraco.Core.Logging;
using Umbraco.Core.Logging.Serilog;
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

            // var logger = new SerilogLogger<object>(new FileInfo(TestHelper.MapPathForTestFiles("~/unit-test.config")));
            _profilingLogger = new ProfilingLogger(NullLoggerFactory.Instance.CreateLogger<ProfilingLogger>(), new LogProfiler(NullLogger<LogProfiler>.Instance));
        }

        private IProfilingLogger _profilingLogger;
        protected override IProfilingLogger ProfilingLogger => _profilingLogger;



        /// <summary>
        /// sets up resolvers before resolution is frozen
        /// </summary>
        protected override void Compose()
        {
            base.Compose();
            var requestHandlerSettings = new RequestHandlerSettings();
            Builder.Services.AddUnique<IShortStringHelper>(_ => new DefaultShortStringHelper(Microsoft.Extensions.Options.Options.Create(requestHandlerSettings)));
        }
    }
}
