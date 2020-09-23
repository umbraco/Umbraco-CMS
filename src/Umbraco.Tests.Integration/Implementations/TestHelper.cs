using System;
using System.Data.Common;
using System.IO;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Diagnostics;
using Umbraco.Core.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Runtime;
using Umbraco.Net;
using Umbraco.Tests.Common;
using Umbraco.Web.Common.AspNetCore;
using IHostingEnvironment = Umbraco.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Tests.Integration.Implementations
{
    public class TestHelper : TestHelperBase
    {
        private IBackOfficeInfo _backOfficeInfo;
        private IHostingEnvironment _hostingEnvironment;
        private readonly IApplicationShutdownRegistry _hostingLifetime;
        private readonly IIpResolver _ipResolver;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string _tempWorkingDir;

        public TestHelper() : base(typeof(TestHelper).Assembly)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            _httpContextAccessor = Mock.Of<IHttpContextAccessor>(x => x.HttpContext == httpContext);
            _ipResolver = new AspNetCoreIpResolver(_httpContextAccessor);

            var contentRoot = Assembly.GetExecutingAssembly().GetRootDirectorySafe();
            var hostEnvironment = new Mock<IWebHostEnvironment>();
            // this must be the assembly name for the WebApplicationFactory to work
            hostEnvironment.Setup(x => x.ApplicationName).Returns(GetType().Assembly.GetName().Name);
            hostEnvironment.Setup(x => x.ContentRootPath).Returns(() => contentRoot);
            hostEnvironment.Setup(x => x.ContentRootFileProvider).Returns(() => new PhysicalFileProvider(contentRoot));
            hostEnvironment.Setup(x => x.WebRootPath).Returns(() => WorkingDirectory);
            hostEnvironment.Setup(x => x.WebRootFileProvider).Returns(() => new PhysicalFileProvider(WorkingDirectory));
            // we also need to expose it as the obsolete interface since netcore's WebApplicationFactory casts it
            hostEnvironment.As<Microsoft.AspNetCore.Hosting.IHostingEnvironment>();

            _hostEnvironment = hostEnvironment.Object;

            _hostingLifetime = new AspNetCoreApplicationShutdownRegistry(Mock.Of<IHostApplicationLifetime>());
            ConsoleLoggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ProfilingLogger = new ProfilingLogger(ConsoleLoggerFactory.CreateLogger("ProfilingLogger"), Profiler);
        }


        public override string WorkingDirectory
        {
            get
            {
                // For Azure Devops we can only store a database in certain locations so we will need to detect if we are running
                // on a build server and if so we'll use the %temp% path.

                if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("System_DefaultWorkingDirectory")))
                {
                    // we are using Azure Devops!

                    if (_tempWorkingDir != null) return _tempWorkingDir;

                    var temp = Path.Combine(Environment.ExpandEnvironmentVariables("%temp%"), "UmbracoTemp");
                    Directory.CreateDirectory(temp);
                    _tempWorkingDir = temp;
                    return _tempWorkingDir;

                }
                else
                {
                    return base.WorkingDirectory;
                }
            }
        }

        public IUmbracoBootPermissionChecker UmbracoBootPermissionChecker { get; } =
            new TestUmbracoBootPermissionChecker();

        public AppCaches AppCaches { get; } = new AppCaches(NoAppCache.Instance, NoAppCache.Instance,
            new IsolatedCaches(type => NoAppCache.Instance));

        public ILoggerFactory ConsoleLoggerFactory { get; private set; }
        public IProfilingLogger ProfilingLogger { get; private set; }

        public IProfiler Profiler { get; } = new VoidProfiler();

        public IHttpContextAccessor GetHttpContextAccessor() => _httpContextAccessor;

        public IWebHostEnvironment GetWebHostEnvironment() => _hostEnvironment;

        public override IDbProviderFactoryCreator DbProviderFactoryCreator =>
            new SqlServerDbProviderFactoryCreator(DbProviderFactories.GetFactory);

        public override IBulkSqlInsertProvider BulkSqlInsertProvider => new SqlServerBulkSqlInsertProvider();

        public override IMarchal Marchal { get; } = new AspNetCoreMarchal();

        public override IBackOfficeInfo GetBackOfficeInfo()
        {
            if (_backOfficeInfo == null)
            {
                var globalSettings = new GlobalSettings();
                var mockedOptionsMonitorOfGlobalSettings = Mock.Of<IOptionsMonitor<GlobalSettings>>(x => x.CurrentValue == globalSettings);
                _backOfficeInfo = new AspNetCoreBackOfficeInfo(mockedOptionsMonitorOfGlobalSettings);
            }

            return _backOfficeInfo;
        }

        public override IHostingEnvironment GetHostingEnvironment()
            => _hostingEnvironment ??= new TestHostingEnvironment(
                GetIOptionsMonitorOfHostingSettings(),
                _hostEnvironment);

        private IOptionsMonitor<HostingSettings> GetIOptionsMonitorOfHostingSettings()
        {
            var hostingSettings = new HostingSettings();
            return Mock.Of<IOptionsMonitor<HostingSettings>>(x => x.CurrentValue == hostingSettings);
        }

        public override IApplicationShutdownRegistry GetHostingEnvironmentLifetime() => _hostingLifetime;

        public override IIpResolver GetIpResolver() => _ipResolver;

        /// <summary>
        /// Some test files are copied to the /bin (/bin/debug) on build, this is a utility to return their physical path based on a virtual path name
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public override string MapPathForTestFiles(string relativePath)
        {
            if (!relativePath.StartsWith("~/"))
                throw new ArgumentException("relativePath must start with '~/'", nameof(relativePath));

            var codeBase = typeof(TestHelperBase).Assembly.CodeBase;
            var uri = new Uri(codeBase);
            var path = uri.LocalPath;
            var bin = Path.GetDirectoryName(path);

            return relativePath.Replace("~/", bin + "/");
        }
    }
}
