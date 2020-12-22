using Moq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Runtime;
using Umbraco.Tests.Integration.Implementations;
using Umbraco.Web.Common.Extensions;

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
            builder.AddUmbracoCore();

            builder.Services.AddUnique<AppCaches>(AppCaches.NoCache);
            builder.Services.AddUnique<IUmbracoBootPermissionChecker>(Mock.Of<IUmbracoBootPermissionChecker>());
            builder.Services.AddUnique<IMainDom>(testHelper.MainDom);

            return builder;
        }
    }
}
