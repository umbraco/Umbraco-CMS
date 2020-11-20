using Moq;
using Umbraco.Core;
using Umbraco.Core.Builder;
using Umbraco.Core.Cache;
using Umbraco.Core.Runtime;
using Umbraco.Extensions;
using Umbraco.Tests.Integration.Implementations;

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
            builder.AddUmbracoCore(
                testHelper.GetWebHostEnvironment(),
                typeof(UmbracoBuilderExtensions).Assembly,
                AppCaches.NoCache, // Disable caches in integration tests
                testHelper.GetLoggingConfiguration(),
                builder.Config);

            builder.Services.AddUnique<IUmbracoBootPermissionChecker>(Mock.Of<IUmbracoBootPermissionChecker>());
            builder.Services.AddUnique<IMainDom>(testHelper.MainDom);

            return builder;
        }
    }
}
