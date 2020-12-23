using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.DependencyInjection;
using Umbraco.Extensions;
using Umbraco.Infrastructure.PublishedCache.DependencyInjection;
using Umbraco.Tests.Integration.Extensions;
using Umbraco.Tests.Integration.Implementations;
using Umbraco.Web.Common.DependencyInjection;

namespace Umbraco.Tests.Integration
{

    [TestFixture]
    public class RuntimeTests
    {
        [TearDown]
        public void TearDown()
        {
            MyComponent.Reset();
            MyComposer.Reset();
        }

        [SetUp]
        public void Setup()
        {
            MyComponent.Reset();
            MyComposer.Reset();
        }

        /// <summary>
        /// Calling AddUmbracoCore to configure the container and UseUmbracoCore to start the runtime
        /// </summary>
        [Test]
        public async Task UseUmbracoCore()
        {
            var testHelper = new TestHelper();

            IHostBuilder hostBuilder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    IWebHostEnvironment webHostEnvironment = testHelper.GetWebHostEnvironment();
                    services.AddSingleton(testHelper.DbProviderFactoryCreator);
                    services.AddRequiredNetCoreServices(testHelper, webHostEnvironment);

                    // Add it!
                    TypeLoader typeLoader = services.AddTypeLoader(
                        GetType().Assembly,
                        webHostEnvironment,
                        testHelper.GetHostingEnvironment(),
                        testHelper.ConsoleLoggerFactory,
                        AppCaches.NoCache,
                        hostContext.Configuration,
                        testHelper.Profiler);

                    var builder = new UmbracoBuilder(
                        services,
                        hostContext.Configuration,
                        typeLoader,
                        testHelper.ConsoleLoggerFactory);

                    builder.Services.AddUnique(AppCaches.NoCache);
                    builder.AddConfiguration()
                        .AddUmbracoCore()
                        .AddNuCache()
                        .Build();

                    services.AddRouting(); // LinkGenerator
                });

            IHost host = await hostBuilder.StartAsync();
            var app = new ApplicationBuilder(host.Services);

            app.UseUmbracoCore();

            // assert results
            IRuntimeState runtimeState = app.ApplicationServices.GetRequiredService<IRuntimeState>();
            IMainDom mainDom = app.ApplicationServices.GetRequiredService<IMainDom>();

            Assert.IsTrue(mainDom.IsMainDom);
            Assert.IsNull(runtimeState.BootFailedException);
            Assert.IsTrue(MyComponent.IsInit);

            await host.StopAsync();

            Assert.IsTrue(MyComponent.IsTerminated);
        }

        public class MyComposer : IUserComposer
        {
            public void Compose(IUmbracoBuilder builder)
            {
                builder.Components().Append<MyComponent>();
                IsComposed = true;
            }

            public static void Reset() => IsComposed = false;

            public static bool IsComposed { get; private set; }
        }

        public class MyComponent : IComponent
        {
            public static bool IsInit { get; private set; }

            public static bool IsTerminated { get; private set; }

            private readonly ILogger<MyComponent> _logger;

            public MyComponent(ILogger<MyComponent> logger) => _logger = logger;

            public void Initialize() => IsInit = true;

            public void Terminate() => IsTerminated = true;

            public static void Reset()
            {
                IsTerminated = false;
                IsInit = false;
            }
        }
    }
}
