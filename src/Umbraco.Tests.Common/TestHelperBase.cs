using System;
using System.IO;
using System.Reflection;
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
        private readonly ITypeFinder _typeFinder;
        private IConfigsFactory _configsFactory;
        private UriUtility _uriUtility;
        private IIOHelper _ioHelper;
        private Configs _configs;
        private IUmbracoVersion _umbracoVersion;

        public TestHelperBase(Assembly entryAssembly)
        {
            SettingsForTests = new SettingsForTests();            
            MainDom = new SimpleMainDom();
            _typeFinder = new TypeFinder(Mock.Of<ILogger>(), new DefaultUmbracoAssemblyProvider(entryAssembly));
        }

        public ITypeFinder GetTypeFinder() => _typeFinder;

        public TypeLoader GetMockedTypeLoader()
        {
            return new TypeLoader(IOHelper, Mock.Of<ITypeFinder>(), Mock.Of<IAppPolicyCache>(), new DirectoryInfo(IOHelper.MapPath("~/App_Data/TEMP")), Mock.Of<IProfilingLogger>());
        }

        public Configs GetConfigs()
        {
            if (_configs == null)
                _configs = GetConfigsFactory().Create();
            return _configs;
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
            if (_configsFactory == null)
                _configsFactory = new ConfigsFactory();
            return _configsFactory;
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
        public ICoreDebugSettings CoreDebugSettings { get; } =  new CoreDebugSettings();

        public IIOHelper IOHelper
        {
            get
            {
                if (_ioHelper == null)
                    _ioHelper = new IOHelper(GetHostingEnvironment(), SettingsForTests.GenerateMockGlobalSettings());
                return _ioHelper;
            }
        }

        public IMainDom MainDom { get; }
        public UriUtility UriUtility
        {
            get
            {
                if (_uriUtility == null)
                    _uriUtility = new UriUtility(GetHostingEnvironment());
                return _uriUtility;
            }
        }

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
            if (_umbracoVersion == null)
                _umbracoVersion = new UmbracoVersion(GetConfigs().Global());
            return _umbracoVersion;
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
