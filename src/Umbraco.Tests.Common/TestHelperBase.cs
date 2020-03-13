using System;
using System.IO;
using Moq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Diagnostics;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.Runtime;
using Umbraco.Core.Serialization;
using Umbraco.Core.Strings;
using Umbraco.Core.Sync;
using Umbraco.Net;
using Umbraco.Web;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Common
{
    /// <summary>
    /// Common helper properties and methods useful to testing
    /// </summary>
    public abstract class TestHelperBase
    {
        public TestHelperBase()
        {
            SettingsForTests = new SettingsForTests();
            IOHelper = new IOHelper(GetHostingEnvironment(), SettingsForTests.GenerateMockGlobalSettings());
            MainDom = new MainDom(Mock.Of<ILogger>(), GetHostingEnvironment(), new MainDomSemaphoreLock(Mock.Of<ILogger>(), GetHostingEnvironment()));
            UriUtility = new UriUtility(GetHostingEnvironment());
        }

        public ITypeFinder GetTypeFinder()
        {

            var typeFinder = new TypeFinder(Mock.Of<ILogger>(),
                new DefaultUmbracoAssemblyProvider(typeof(TestHelperBase).Assembly));
            return typeFinder;
        }

        public TypeLoader GetMockedTypeLoader()
        {
            return new TypeLoader(IOHelper, Mock.Of<ITypeFinder>(), Mock.Of<IAppPolicyCache>(), new DirectoryInfo(IOHelper.MapPath("~/App_Data/TEMP")), Mock.Of<IProfilingLogger>());
        }

        public Configs GetConfigs()
        {
            return GetConfigsFactory().Create(IOHelper, Mock.Of<ILogger>());
        }
        public IRuntimeState GetRuntimeState()
        {
            return new RuntimeState(
                Mock.Of<ILogger>(),
                Mock.Of<IGlobalSettings>(),
                new Lazy<IMainDom>(),
                new Lazy<IServerRegistrar>(),
                GetUmbracoVersion(),
                GetHostingEnvironment(),
                GetBackOfficeInfo()
                );
        }

        public abstract IBackOfficeInfo GetBackOfficeInfo();

        public IConfigsFactory GetConfigsFactory()
        {
            return new ConfigsFactory();
        }

        /// <summary>
        /// Gets the current assembly directory.
        /// </summary>
        /// <value>The assembly directory.</value>
        public string CurrentAssemblyDirectory
        {
            get
            {
                var codeBase = typeof(TestHelperBase).Assembly.CodeBase;
                var uri = new Uri(codeBase);
                var path = uri.LocalPath;
                return Path.GetDirectoryName(path);
            }
        }

        public IShortStringHelper ShortStringHelper { get; } = new DefaultShortStringHelper(new DefaultShortStringHelperConfig());
        public IJsonSerializer JsonSerializer { get; } = new JsonNetSerializer();
        public IVariationContextAccessor VariationContextAccessor { get; } = new TestVariationContextAccessor();
        public abstract IDbProviderFactoryCreator DbProviderFactoryCreator { get; }
        public abstract IBulkSqlInsertProvider BulkSqlInsertProvider { get; }
        public abstract IMarchal Marchal { get; }
        public ICoreDebug CoreDebug { get; } =  new CoreDebug();


        public IIOHelper IOHelper { get; }
        public IMainDom MainDom { get; }
        public UriUtility UriUtility { get; }
        public SettingsForTests SettingsForTests { get; }
        public IWebRoutingSettings WebRoutingSettings => SettingsForTests.GenerateMockWebRoutingSettings();

        /// <summary>
        /// Maps the given <paramref name="relativePath"/> making it rooted on <see cref="CurrentAssemblyDirectory"/>. <paramref name="relativePath"/> must start with <code>~/</code>
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <returns></returns>
        public string MapPathForTest(string relativePath)
        {
            if (!relativePath.StartsWith("~/"))
                throw new ArgumentException("relativePath must start with '~/'", "relativePath");

            return relativePath.Replace("~/", CurrentAssemblyDirectory + "/");
        }

        public IUmbracoVersion GetUmbracoVersion()
        {
            return new UmbracoVersion(GetConfigs().Global());
        }

        public IRegister GetRegister()
        {
            return RegisterFactory.Create(GetConfigs().Global());
        }

        public abstract IHostingEnvironment GetHostingEnvironment();

        public abstract IIpResolver GetIpResolver();

        public IRequestCache GetRequestCache()
        {
            return new DictionaryAppCache();
        }

        public IPublishedUrlProvider GetPublishedUrlProvider()
        {
            var mock = new Mock<IPublishedUrlProvider>();

            return mock.Object;
        }
    }
}
