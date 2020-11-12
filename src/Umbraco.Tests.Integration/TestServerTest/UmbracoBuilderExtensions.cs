using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Runtime;
using Umbraco.Extensions;
using Umbraco.Tests.Integration.Implementations;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Web.Common.Builder;

namespace Umbraco.Tests.Integration.TestServerTest
{
    public static class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Uses a test version of Umbraco Core with a test IRuntime
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IUmbracoBuilder WithTestCore(this IUmbracoBuilder builder, TestHelper testHelper,
            Action<CoreRuntimeBootstrapper, RuntimeEssentialsEventArgs> dbInstallEventHandler)
        {
            return builder.AddWith(nameof(global::Umbraco.Web.Common.Builder.UmbracoBuilderExtensions.WithCore),
                    () =>
                    {
                        builder.Services.AddUmbracoCore(
                            builder.WebHostEnvironment,
                            typeof(UmbracoBuilderExtensions).Assembly,
                            AppCaches.NoCache, // Disable caches in integration tests
                            testHelper.GetLoggingConfiguration(),
                            builder.Config,
                            // TODO: Yep that's extremely ugly
                            (globalSettings, connectionStrings, umbVersion, ioHelper, loggerFactory, profiler, hostingEnv, backOfficeInfo, typeFinder, appCaches, dbProviderFactoryCreator) =>
                            {
                                var runtime = UmbracoIntegrationTest.CreateTestRuntime(
                                    globalSettings,
                                    connectionStrings,
                                    umbVersion,
                                    ioHelper,
                                    loggerFactory,
                                    profiler,
                                    hostingEnv,
                                    backOfficeInfo,
                                    typeFinder,
                                    appCaches,
                                    dbProviderFactoryCreator,
                                    testHelper.MainDom,         // SimpleMainDom
                                    dbInstallEventHandler);     // DB Installation event handler

                                return runtime;
                            });
                    });
        }
    }
}
