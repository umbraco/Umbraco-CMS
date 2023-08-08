using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Persistence.Sqlite;
using Umbraco.Cms.Persistence.SqlServer;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.DependencyInjection;
using Umbraco.Cms.Tests.Integration.Extensions;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Search.baseTests;
using Umbraco.Search.DependencyInjection;
using Umbraco.Cms.Infrastructure.DependencyInjection;
using Umbraco.Search.Examine;
using Umbraco.Search.Examine.Lucene.DependencyInjection;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Umbraco.Search.Examine;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
public class UmbracoSearchExamineTests : UmbracoSearchTests
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<TestUmbracoDatabaseFactoryProvider>();
        var webHostEnvironment = TestHelper.GetWebHostEnvironment();
        services.AddRequiredNetCoreServices(TestHelper, webHostEnvironment);

        // We register this service because we need it for IRuntimeState, if we don't this breaks 900 tests
        services.AddSingleton<IConflictingRouteService, TestConflictingRouteService>();

        services.AddLogger(webHostEnvironment, Configuration);

        // Add it!
        var hostingEnvironment = TestHelper.GetHostingEnvironment();
        var typeLoader = services.AddTypeLoader(
            GetType().Assembly,
            hostingEnvironment,
            TestHelper.ConsoleLoggerFactory,
            global::Umbraco.Cms.Core.Cache.AppCaches.NoCache,
            Configuration,
            TestHelper.Profiler);
        var builder = new UmbracoBuilder(services, Configuration, typeLoader, TestHelper.ConsoleLoggerFactory,
            TestHelper.Profiler, global::Umbraco.Cms.Core.Cache.AppCaches.NoCache, hostingEnvironment);

        builder.AddConfiguration()
            .AddUmbracoCore()
            .AddWebComponents()
            .AddRuntimeMinifier()
            .AddBackOfficeAuthentication()
            .AddBackOfficeIdentity()
            .AddMembersIdentity()
            .AddSearchServices()
            .AddExamineLuceneIndexes()
            .AddUmbracoSqlServerSupport()
            .AddUmbracoSqliteSupport()
            .AddTestServices(TestHelper, false);

        if (TestOptions.Mapper)
        {
            // TODO: Should these just be called from within AddUmbracoCore/AddWebComponents?
            builder
                .AddCoreMappingProfiles()
                .AddWebMappingProfiles();
        }

        services.AddSignalR();
        services.AddMvc();

        CustomTestSetup(builder);

        builder.Build();
    }
}
