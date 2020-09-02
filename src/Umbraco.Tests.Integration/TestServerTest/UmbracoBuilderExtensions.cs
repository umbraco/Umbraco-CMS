using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing.LightInject;
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
        public static IUmbracoBuilder WithTestCore(this IUmbracoBuilder builder, TestHelper testHelper, LightInjectContainer container,
            Action<CoreRuntime, RuntimeEssentialsEventArgs> dbInstallEventHandler)
        {
            return builder.AddWith(nameof(global::Umbraco.Web.Common.Builder.UmbracoBuilderExtensions.WithCore),
                    () =>
                    {
                        builder.Services.AddUmbracoCore(
                            builder.WebHostEnvironment,
                            container,
                            typeof(UmbracoBuilderExtensions).Assembly,
                            NoAppCache.Instance,
                            testHelper.GetLoggingConfiguration(),
                            // TODO: Yep that's extremely ugly
                            (a, b, c, d, e, f, g, h, i, j) =>
                                UmbracoIntegrationTest.CreateTestRuntime(a, b, c, d, e, f, g, h, i, j,
                                    testHelper.MainDom,         // SimpleMainDom
                                    dbInstallEventHandler),     // DB Installation event handler
                            out _);
                    });
        }
    }
}
