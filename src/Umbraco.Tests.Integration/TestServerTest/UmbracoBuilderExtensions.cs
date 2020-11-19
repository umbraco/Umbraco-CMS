using System;
using Umbraco.Core.Builder;
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
        public static IUmbracoBuilder AddTestCore(this IUmbracoBuilder builder, TestHelper testHelper)
        {

            return builder.AddUmbracoCore(
                testHelper.GetWebHostEnvironment(),
                typeof(UmbracoBuilderExtensions).Assembly,
                AppCaches.NoCache, // Disable caches in integration tests
                testHelper.GetLoggingConfiguration(),
                builder.Config,
                // TODO: Yep that's extremely ugly
                (lambdaBuilder, globalSettings, connectionStrings, umbVersion, ioHelper,  profiler, hostingEnv,  typeFinder, appCaches, dbProviderFactoryCreator) =>
                {
                   UmbracoIntegrationTest.ConfigureSomeMorebitsForTests(
                        lambdaBuilder,
                        globalSettings,
                        connectionStrings,
                        umbVersion,
                        ioHelper,
                        profiler,
                        hostingEnv,
                        typeFinder,
                        appCaches,
                        dbProviderFactoryCreator,
                        testHelper.MainDom);       // SimpleMainDom
                      
                });
        }
    }
}
