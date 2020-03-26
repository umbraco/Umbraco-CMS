
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Data.Common;
using System.Net;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Diagnostics;
using Umbraco.Core.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Net;
using Umbraco.Core.Persistence;
using Umbraco.Core.Runtime;
using Umbraco.Tests.Common;
using Umbraco.Web.BackOffice;
using Umbraco.Web.BackOffice.AspNetCore;
using IHostingEnvironment = Umbraco.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Tests.Integration.Implementations
{
    public class TestHelper : TestHelperBase
    {
        private IBackOfficeInfo _backOfficeInfo;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IApplicationShutdownRegistry _hostingLifetime;
        private readonly IIpResolver _ipResolver;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TestHelper() : base(typeof(TestHelper).Assembly)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            _httpContextAccessor = Mock.Of<IHttpContextAccessor>(x => x.HttpContext == httpContext);
            _ipResolver = new AspNetIpResolver(_httpContextAccessor);

            _hostEnvironment = Mock.Of<IWebHostEnvironment>(x =>
                x.ApplicationName == "UmbracoIntegrationTests"
                && x.ContentRootPath == CurrentAssemblyDirectory
                && x.WebRootPath == CurrentAssemblyDirectory); // same folder for now?

            _hostingEnvironment = new TestHostingEnvironment(
                SettingsForTests.GetDefaultHostingSettings(),
                _hostEnvironment,
                _httpContextAccessor);

            _hostingLifetime = new AspNetCoreApplicationShutdownRegistry(Mock.Of<IHostApplicationLifetime>());

            Logger = new ProfilingLogger(new ConsoleLogger(new MessageTemplates()), Profiler);
        }

        public IUmbracoBootPermissionChecker UmbracoBootPermissionChecker { get; } = new TestUmbracoBootPermissionChecker();

        public AppCaches AppCaches { get; } = new AppCaches(NoAppCache.Instance, NoAppCache.Instance, new IsolatedCaches(type => NoAppCache.Instance));

        public IProfilingLogger Logger { get; private set; }

        public IProfiler Profiler { get; } = new VoidProfiler();

        public IHttpContextAccessor GetHttpContextAccessor() => _httpContextAccessor;

        public IWebHostEnvironment GetWebHostEnvironment() => _hostEnvironment;

        public override IDbProviderFactoryCreator DbProviderFactoryCreator => new SqlServerDbProviderFactoryCreator(Constants.DbProviderNames.SqlServer, DbProviderFactories.GetFactory);

        public override IBulkSqlInsertProvider BulkSqlInsertProvider => new SqlServerBulkSqlInsertProvider();

        public override IMarchal Marchal { get; } = new AspNetCoreMarchal();

        public override IBackOfficeInfo GetBackOfficeInfo()
        {
            if (_backOfficeInfo == null)
                _backOfficeInfo = new AspNetCoreBackOfficeInfo(SettingsForTests.GetDefaultGlobalSettings(GetUmbracoVersion()));
            return _backOfficeInfo;
        }

        public override IHostingEnvironment GetHostingEnvironment() => _hostingEnvironment;
        public override IApplicationShutdownRegistry GetHostingEnvironmentLifetime() => _hostingLifetime;

        public override IIpResolver GetIpResolver() => _ipResolver;

    }
}
