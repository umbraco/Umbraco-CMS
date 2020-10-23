using System;
using System.Collections;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Diagnostics;
using Umbraco.Core.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence;
using Umbraco.Core.PropertyEditors;
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

        public void AssertPropertyValuesAreEqual(object actual, object expected, string dateTimeFormat = null, Func<IEnumerable, IEnumerable> sorter = null, string[] ignoreProperties = null)
        {
            const int dateDeltaMilliseconds = 1000; // 1s

            var properties = expected.GetType().GetProperties();
            foreach (var property in properties)
            {
                // ignore properties that are attributed with EditorBrowsableState.Never
                var att = property.GetCustomAttribute<EditorBrowsableAttribute>(false);
                if (att != null && att.State == EditorBrowsableState.Never)
                    continue;

                // ignore explicitly ignored properties
                if (ignoreProperties != null && ignoreProperties.Contains(property.Name))
                    continue;

                var actualValue = property.GetValue(actual, null);
                var expectedValue = property.GetValue(expected, null);

                AssertAreEqual(property, expectedValue, actualValue, sorter, dateDeltaMilliseconds);
            }
        }

        private static void AssertListsAreEqual(PropertyInfo property, IEnumerable expected, IEnumerable actual, Func<IEnumerable, IEnumerable> sorter = null, int dateDeltaMilliseconds = 0)
        {


            if (sorter == null)
            {
                // this is pretty hackerific but saves us some code to write
                sorter = enumerable =>
                {
                    // semi-generic way of ensuring any collection of IEntity are sorted by Ids for comparison
                    var entities = enumerable.OfType<IEntity>().ToList();
                    return entities.Count > 0 ? (IEnumerable) entities.OrderBy(x => x.Id) : entities;
                };
            }

            var expectedListEx = sorter(expected).Cast<object>().ToList();
            var actualListEx = sorter(actual).Cast<object>().ToList();

            if (actualListEx.Count != expectedListEx.Count)
                Assert.Fail("Collection {0}.{1} does not match. Expected IEnumerable containing {2} elements but was IEnumerable containing {3} elements", property.PropertyType.Name, property.Name, expectedListEx.Count, actualListEx.Count);

            for (var i = 0; i < actualListEx.Count; i++)
                AssertAreEqual(property, expectedListEx[i], actualListEx[i], sorter, dateDeltaMilliseconds);
        }

        private static void AssertAreEqual(PropertyInfo property, object expected, object actual, Func<IEnumerable, IEnumerable> sorter = null, int dateDeltaMilliseconds = 0)
        {
            if (!(expected is string) && expected is IEnumerable)
            {
                // sort property collection by alias, not by property ids
                // on members, built-in properties don't have ids (always zero)
                if (expected is PropertyCollection)
                    sorter = e => ((PropertyCollection) e).OrderBy(x => x.Alias);

                // compare lists
                AssertListsAreEqual(property, (IEnumerable) actual, (IEnumerable) expected, sorter, dateDeltaMilliseconds);
            }
            else if (expected is DateTime expectedDateTime)
            {
                // compare date & time with delta
                var actualDateTime = (DateTime) actual;
                var delta = (actualDateTime - expectedDateTime).TotalMilliseconds;
                Assert.IsTrue(Math.Abs(delta) <= dateDeltaMilliseconds, "Property {0}.{1} does not match. Expected: {2} but was: {3}", property.DeclaringType.Name, property.Name, expected, actual);
            }
            else if (expected is Property expectedProperty)
            {
                // compare values
                var actualProperty = (Property) actual;
                var expectedPropertyValues = expectedProperty.Values.OrderBy(x => x.Culture).ThenBy(x => x.Segment).ToArray();
                var actualPropertyValues = actualProperty.Values.OrderBy(x => x.Culture).ThenBy(x => x.Segment).ToArray();
                if (expectedPropertyValues.Length != actualPropertyValues.Length)
                    Assert.Fail($"{property.DeclaringType.Name}.{property.Name}: Expected {expectedPropertyValues.Length} but got {actualPropertyValues.Length}.");
                for (var i = 0; i < expectedPropertyValues.Length; i++)
                {
                    // This is not pretty, but since a property value can be a datetime we can't just always compare them as is.
                    // This is made worse by the fact that PublishedValue is not always set, meaning we can't lump it all into the same if block
                    if (expectedPropertyValues[i].EditedValue is DateTime expectedEditDateTime)
                    {
                        var actualEditDateTime = (DateTime) actualPropertyValues[i].EditedValue;
                        AssertDateTime(expectedEditDateTime, actualEditDateTime, $"{property.DeclaringType.Name}.{property.Name}: Expected draft value \"{expectedPropertyValues[i].EditedValue}\" but got \"{actualPropertyValues[i].EditedValue}\".", dateDeltaMilliseconds);
                    }
                    else
                    {
                        Assert.AreEqual(expectedPropertyValues[i].EditedValue, actualPropertyValues[i].EditedValue, $"{property.DeclaringType.Name}.{property.Name}: Expected draft value \"{expectedPropertyValues[i].EditedValue}\" but got \"{actualPropertyValues[i].EditedValue}\".");
                    }

                    if (expectedPropertyValues[i].PublishedValue is DateTime expectedPublishDateTime)
                    {
                        var actualPublishedDateTime = (DateTime) actualPropertyValues[i].PublishedValue;
                        AssertDateTime(expectedPublishDateTime, actualPublishedDateTime, $"{property.DeclaringType.Name}.{property.Name}: Expected published value \"{expectedPropertyValues[i].PublishedValue}\" but got \"{actualPropertyValues[i].PublishedValue}\".", dateDeltaMilliseconds);
                    }
                    else
                    {
                        Assert.AreEqual(expectedPropertyValues[i].PublishedValue, actualPropertyValues[i].PublishedValue, $"{property.DeclaringType.Name}.{property.Name}: Expected published value \"{expectedPropertyValues[i].PublishedValue}\" but got \"{actualPropertyValues[i].PublishedValue}\".");
                    }
                }
            }
            else if (expected is IDataEditor expectedEditor)
            {
                Assert.IsInstanceOf<IDataEditor>(actual);
                var actualEditor = (IDataEditor) actual;
                Assert.AreEqual(expectedEditor.Alias,  actualEditor.Alias);
                // what else shall we test?
            }
            else
            {
                // directly compare values
                Assert.AreEqual(expected, actual, "Property {0}.{1} does not match. Expected: {2} but was: {3}", property.DeclaringType.Name, property.Name,
                    expected?.ToString() ?? "<null>", actual?.ToString() ?? "<null>");
            }
        }

        private static void AssertDateTime(DateTime expected, DateTime actual, string failureMessage,
            int dateDeltaMiliseconds = 0)
        {
            var delta = (actual - expected).TotalMilliseconds;
            Assert.IsTrue(Math.Abs(delta) <= dateDeltaMiliseconds, failureMessage);
        }
    }
}
