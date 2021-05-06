using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Examine.Lucene.UmbracoExamine
{
    [TestFixture]
    public abstract class ExamineBaseTest : UmbracoIntegrationTest
    {
        protected IndexInitializer IndexInitializer => Services.GetRequiredService<IndexInitializer>();

        protected IHostingEnvironment HostingEnvironment => Services.GetRequiredService<IHostingEnvironment>();

        protected IRuntimeState RunningRuntimeState { get; } = Mock.Of<IRuntimeState>(x => x.Level == RuntimeLevel.Run);

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddSingleton<IndexInitializer>();
        }

        [OneTimeSetUp]
        public void InitializeFixture()
        {

            // var logger = new SerilogLogger<object>(new FileInfo(TestHelper.MapPathForTestFiles("~/unit-test.config")));
            //_profilingLogger = new ProfilingLogger(NullLoggerFactory.Instance.CreateLogger<ProfilingLogger>(), new LogProfiler(NullLogger<LogProfiler>.Instance));
        }

        //private IProfilingLogger _profilingLogger;
        //protected override IProfilingLogger ProfilingLogger => _profilingLogger;

        ///// <summary>
        ///// sets up resolvers before resolution is frozen
        ///// </summary>
        //protected override void Compose()
        //{
        //    base.Compose();
        //    var requestHandlerSettings = new RequestHandlerSettings();
        //    Builder.Services.AddUnique<IShortStringHelper>(_ => new DefaultShortStringHelper(Microsoft.Extensions.Options.Options.Create(requestHandlerSettings)));
        //}
    }
}
