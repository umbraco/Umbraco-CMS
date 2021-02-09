using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Diagnostics;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;
using Umbraco.Persistence.SqlCe;
using Umbraco.Tests.Common;
using Umbraco.Web;
using Umbraco.Web.Hosting;
using Umbraco.Web.Routing;
using Constants = Umbraco.Cms.Core.Constants;
using File = System.IO.File;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// Common helper properties and methods useful to testing
    /// </summary>
    public static class TestHelper
    {
        private static readonly TestHelperInternal _testHelperInternal = new TestHelperInternal();
        private static IEmailSender _emailSender;

        private class TestHelperInternal : TestHelperBase
        {
            public TestHelperInternal() : base(typeof(TestHelperInternal).Assembly)
            {

            }

            public override IDbProviderFactoryCreator DbProviderFactoryCreator { get; } = new UmbracoDbProviderFactoryCreator();
            public DatabaseSchemaCreatorFactory DatabaseSchemaCreatorFactory { get; } = new DatabaseSchemaCreatorFactory(Mock.Of<ILogger<DatabaseSchemaCreator>>(), NullLoggerFactory.Instance, new UmbracoVersion());

            public override IBulkSqlInsertProvider BulkSqlInsertProvider { get; } = new SqlCeBulkSqlInsertProvider();

            public override IMarchal Marchal { get; } = new FrameworkMarchal();

            public override IBackOfficeInfo GetBackOfficeInfo()
                => new AspNetBackOfficeInfo(
                    new GlobalSettings(),
                    TestHelper.IOHelper, Mock.Of<ILogger<AspNetBackOfficeInfo>>(), Options.Create(new WebRoutingSettings()));

            public override IHostingEnvironment GetHostingEnvironment()
                => new AspNetHostingEnvironment(Options.Create(new HostingSettings()));

            public override IApplicationShutdownRegistry GetHostingEnvironmentLifetime()
                => new AspNetApplicationShutdownRegistry();

            public override IIpResolver GetIpResolver()
                => new AspNetIpResolver();
        }

        public static ITypeFinder GetTypeFinder() => _testHelperInternal.GetTypeFinder();

        public static TypeLoader GetMockedTypeLoader() => _testHelperInternal.GetMockedTypeLoader();

        //public static Configs GetConfigs() => _testHelperInternal.GetConfigs();

        public static IBackOfficeInfo GetBackOfficeInfo() => _testHelperInternal.GetBackOfficeInfo();

      //  public static IConfigsFactory GetConfigsFactory() => _testHelperInternal.GetConfigsFactory();

        /// <summary>
        /// Gets the working directory of the test project.
        /// </summary>
        /// <value>The assembly directory.</value>
        public static string WorkingDirectory => _testHelperInternal.WorkingDirectory;

        public static IShortStringHelper ShortStringHelper => _testHelperInternal.ShortStringHelper;
        public static IJsonSerializer JsonSerializer => _testHelperInternal.JsonSerializer;
        public static IVariationContextAccessor VariationContextAccessor => _testHelperInternal.VariationContextAccessor;
        public static IDbProviderFactoryCreator DbProviderFactoryCreator => _testHelperInternal.DbProviderFactoryCreator;
        public static DatabaseSchemaCreatorFactory DatabaseSchemaCreatorFactory => _testHelperInternal.DatabaseSchemaCreatorFactory;
        public static IBulkSqlInsertProvider BulkSqlInsertProvider => _testHelperInternal.BulkSqlInsertProvider;
        public static IMarchal Marchal => _testHelperInternal.Marchal;
        public static CoreDebugSettings CoreDebugSettings => _testHelperInternal.CoreDebugSettings;


        public static IIOHelper IOHelper => _testHelperInternal.IOHelper;
        public static IMainDom MainDom => _testHelperInternal.MainDom;
        public static UriUtility UriUtility => _testHelperInternal.UriUtility;

        public static IEmailSender EmailSender { get; } = new EmailSender(Options.Create(new GlobalSettings()));


        /// <summary>
        /// Some test files are copied to the /bin (/bin/debug) on build, this is a utility to return their physical path based on a virtual path name
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public static string MapPathForTestFiles(string relativePath) => _testHelperInternal.MapPathForTestFiles(relativePath);

        public static void InitializeContentDirectories()
        {
            CreateDirectories(new[] { Constants.SystemDirectories.MvcViews, new GlobalSettings().UmbracoMediaPath, Constants.SystemDirectories.AppPlugins });
        }

        public static void CleanContentDirectories()
        {
            CleanDirectories(new[] { Constants.SystemDirectories.MvcViews, new GlobalSettings().UmbracoMediaPath });
        }

        public static void CreateDirectories(string[] directories)
        {
            foreach (var directory in directories)
            {
                var directoryInfo = new DirectoryInfo(IOHelper.MapPath(directory));
                if (directoryInfo.Exists == false)
                    Directory.CreateDirectory(IOHelper.MapPath(directory));
            }
        }

        public static void CleanDirectories(string[] directories)
        {
            var preserves = new Dictionary<string, string[]>
            {
                { Constants.SystemDirectories.MvcViews, new[] {"dummy.txt"} }
            };
            foreach (var directory in directories)
            {
                var directoryInfo = new DirectoryInfo(IOHelper.MapPath(directory));
                var preserve = preserves.ContainsKey(directory) ? preserves[directory] : null;
                if (directoryInfo.Exists)
                    foreach (var x in directoryInfo.GetFiles().Where(x => preserve == null || preserve.Contains(x.Name) == false))
                        x.Delete();
            }
        }

        public static void CleanUmbracoSettingsConfig()
        {
            var currDir = new DirectoryInfo(WorkingDirectory);

            var umbracoSettingsFile = Path.Combine(currDir.Parent.Parent.FullName, "config", "umbracoSettings.config");
            if (File.Exists(umbracoSettingsFile))
                File.Delete(umbracoSettingsFile);
        }

        // TODO: Move to Assertions or AssertHelper
        // FIXME: obsolete the dateTimeFormat thing and replace with dateDelta
        public static void AssertPropertyValuesAreEqual(object actual, object expected, string dateTimeFormat = null, Func<IEnumerable, IEnumerable> sorter = null, string[] ignoreProperties = null)
        {
            const int dateDeltaMilliseconds = 500; // .5s

            var properties = expected.GetType().GetProperties();
            foreach (var property in properties)
            {
                // ignore properties that are attributed with EditorBrowsableState.Never
                var att = property.GetCustomAttribute<EditorBrowsableAttribute>(false);
                if (att != null && att.State == EditorBrowsableState.Never)
                    continue;

                // ignore explicitely ignored properties
                if (ignoreProperties != null && ignoreProperties.Contains(property.Name))
                    continue;

                var actualValue = property.GetValue(actual, null);
                var expectedValue = property.GetValue(expected, null);

                AssertAreEqual(property, expectedValue, actualValue, sorter, dateDeltaMilliseconds);
            }
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
                    Assert.AreEqual(expectedPropertyValues[i].EditedValue, actualPropertyValues[i].EditedValue, $"{property.DeclaringType.Name}.{property.Name}: Expected draft value \"{expectedPropertyValues[i].EditedValue}\" but got \"{actualPropertyValues[i].EditedValue}\".");
                    Assert.AreEqual(expectedPropertyValues[i].PublishedValue, actualPropertyValues[i].PublishedValue, $"{property.DeclaringType.Name}.{property.Name}: Expected published value \"{expectedPropertyValues[i].EditedValue}\" but got \"{actualPropertyValues[i].EditedValue}\".");
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

        public static void DeleteDirectory(string path)
        {
            Try(() =>
            {
                if (Directory.Exists(path) == false) return;
                foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                    File.Delete(file);
            });

            Try(() =>
            {
                if (Directory.Exists(path) == false) return;
                Directory.Delete(path, true);
            });
        }

        public static void TryAssert(Action action, int maxTries = 5, int waitMilliseconds = 200)
        {
            Try<AssertionException>(action, maxTries, waitMilliseconds);
        }

        public static void Try(Action action, int maxTries = 5, int waitMilliseconds = 200)
        {
            Try<Exception>(action, maxTries, waitMilliseconds);
        }

        public static void Try<T>(Action action, int maxTries = 5, int waitMilliseconds = 200)
            where T : Exception
        {
            var tries = 0;
            while (true)
            {
                try
                {
                    action();
                    break;
                }
                catch (T)
                {
                    if (tries++ > maxTries)
                        throw;
                    Thread.Sleep(waitMilliseconds);
                }
            }
        }

        public static IUmbracoVersion GetUmbracoVersion() => _testHelperInternal.GetUmbracoVersion();

        public static IServiceCollection GetRegister() => _testHelperInternal.GetRegister().AddLazySupport();

        public static IHostingEnvironment GetHostingEnvironment() => _testHelperInternal.GetHostingEnvironment();

        public static ILoggingConfiguration GetLoggingConfiguration(IHostingEnvironment hostingEnv) => _testHelperInternal.GetLoggingConfiguration(hostingEnv);

        public static IApplicationShutdownRegistry GetHostingEnvironmentLifetime() => _testHelperInternal.GetHostingEnvironmentLifetime();

        public static IIpResolver GetIpResolver() => _testHelperInternal.GetIpResolver();

        public static IRequestCache GetRequestCache() => _testHelperInternal.GetRequestCache();

        public static IHttpContextAccessor GetHttpContextAccessor(HttpContextBase httpContextBase = null)
        {
            if (httpContextBase is null)
            {
                var httpContextMock = new Mock<HttpContextBase>();

                httpContextMock.Setup(x => x.DisposeOnPipelineCompleted(It.IsAny<IDisposable>()))
                    .Returns(Mock.Of<ISubscriptionToken>());

                httpContextBase = httpContextMock.Object;
            }

            var mock = new Mock<IHttpContextAccessor>();

            mock.Setup(x => x.HttpContext).Returns(httpContextBase);

            return mock.Object;
        }

        public static IPublishedUrlProvider GetPublishedUrlProvider() => _testHelperInternal.GetPublishedUrlProvider();
    }
}
