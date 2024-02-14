// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Diagnostics;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Common;

/// <summary>
///     Common helper properties and methods useful to testing
/// </summary>
public abstract class TestHelperBase
{
    private readonly ITypeFinder _typeFinder;
    private IIOHelper _ioHelper;
    private UriUtility _uriUtility;
    private string _workingDir;

    protected TestHelperBase(Assembly entryAssembly)
    {
        MainDom = new SimpleMainDom();
        _typeFinder = new TypeFinder(NullLoggerFactory.Instance.CreateLogger<TypeFinder>(), new DefaultUmbracoAssemblyProvider(entryAssembly, NullLoggerFactory.Instance));
    }

    /// <summary>
    ///     Gets the working directory of the test project.
    /// </summary>
    public string WorkingDirectory
    {
        get
        {
            if (_workingDir != null)
            {
                return _workingDir;
            }

            // Azure DevOps can only store a database in certain locations so we will need to detect if we are running
            // on a build server and if so we'll use the temp path.
            var dir = string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("System_DefaultWorkingDirectory"))
                ? Path.Combine(Assembly.GetExecutingAssembly().GetRootDirectorySafe(), "TEMP")
                : Path.Combine(Path.GetTempPath(), "UmbracoTests", "TEMP");

            if (!Directory.Exists(dir))
            {
                _ = Directory.CreateDirectory(dir);
            }

            _workingDir = dir;
            return _workingDir;
        }
    }

    public IShortStringHelper ShortStringHelper { get; } =
        new DefaultShortStringHelper(new DefaultShortStringHelperConfig());
    public IScopeProvider ScopeProvider
    {
        get
        {
            var loggerFactory = NullLoggerFactory.Instance;
            var fileSystems = new FileSystems(
                loggerFactory,
                Mock.Of<IIOHelper>(),
                Mock.Of<IOptions<GlobalSettings>>(),
                Mock.Of<IHostingEnvironment>());
            var mediaFileManager = new MediaFileManager(
                Mock.Of<IFileSystem>(),
                Mock.Of<IMediaPathScheme>(),
                loggerFactory.CreateLogger<MediaFileManager>(),
                Mock.Of<IShortStringHelper>(),
                Mock.Of<IServiceProvider>(),
                Options.Create(new ContentSettings()));
            var databaseFactory = new Mock<IUmbracoDatabaseFactory>();
            var database = new Mock<IUmbracoDatabase>();
            var sqlContext = new Mock<ISqlContext>();

            var lockingMechanism = new Mock<IDistributedLockingMechanism>();
            lockingMechanism.Setup(x => x.ReadLock(It.IsAny<int>(), It.IsAny<TimeSpan?>()))
                .Returns(Mock.Of<IDistributedLock>());
            lockingMechanism.Setup(x => x.WriteLock(It.IsAny<int>(), It.IsAny<TimeSpan?>()))
                .Returns(Mock.Of<IDistributedLock>());

            var lockingMechanismFactory = new Mock<IDistributedLockingMechanismFactory>();
            lockingMechanismFactory.Setup(x => x.DistributedLockingMechanism)
                .Returns(lockingMechanism.Object);

            // Setup mock of database factory to return mock of database.
            databaseFactory.Setup(x => x.CreateDatabase()).Returns(database.Object);
            databaseFactory.Setup(x => x.SqlContext).Returns(sqlContext.Object);

            // Setup mock of database to return mock of sql SqlContext
            database.Setup(x => x.SqlContext).Returns(sqlContext.Object);

            var syntaxProviderMock = new Mock<ISqlSyntaxProvider>();

            // Setup mock of ISqlContext to return syntaxProviderMock
            sqlContext.Setup(x => x.SqlSyntax).Returns(syntaxProviderMock.Object);

            return new ScopeProvider(
                new AmbientScopeStack(),
                new AmbientScopeContextStack(),
                lockingMechanismFactory.Object,
                databaseFactory.Object,
                fileSystems,
                new TestOptionsMonitor<CoreDebugSettings>(new CoreDebugSettings()),
                mediaFileManager,
                loggerFactory,
                Mock.Of<IEventAggregator>());
        }
    }

    public IJsonSerializer JsonSerializer { get; } = new JsonNetSerializer();

    public IVariationContextAccessor VariationContextAccessor { get; } = new TestVariationContextAccessor();

    public abstract IBulkSqlInsertProvider BulkSqlInsertProvider { get; }

    public abstract IMarchal Marchal { get; }

    public CoreDebugSettings CoreDebugSettings { get; } = new();

    public IIOHelper IOHelper
    {
        get
        {
            if (_ioHelper == null)
            {
                var hostingEnvironment = GetHostingEnvironment();

                if (TestEnvironment.IsWindows)
                {
                    _ioHelper = new IOHelperWindows(hostingEnvironment);
                }
                else if (TestEnvironment.IsLinux)
                {
                    _ioHelper = new IOHelperLinux(hostingEnvironment);
                }
                else if (TestEnvironment.IsOSX)
                {
                    _ioHelper = new IOHelperOSX(hostingEnvironment);
                }
                else
                {
                    throw new NotSupportedException("Unexpected OS");
                }
            }

            return _ioHelper;
        }
    }

    public IMainDom MainDom { get; }

    public UriUtility UriUtility
    {
        get
        {
            if (_uriUtility == null)
            {
                _uriUtility = new UriUtility(GetHostingEnvironment());
            }

            return _uriUtility;
        }
    }

    public ITypeFinder GetTypeFinder() => _typeFinder;

    public TypeLoader GetMockedTypeLoader() =>
        new(
            Mock.Of<ITypeFinder>(),
            new VaryingRuntimeHash(),
            Mock.Of<IAppPolicyCache>(),
            new DirectoryInfo(GetHostingEnvironment()
                .MapPathContentRoot(Constants.SystemDirectories.TempData)),
            Mock.Of<ILogger<TypeLoader>>(),
            Mock.Of<IProfiler>());

    /// <summary>
    ///     Some test files are copied to the /bin (/bin/debug) on build, this is a utility to return their physical path based
    ///     on a virtual path name
    /// </summary>
    public virtual string MapPathForTestFiles(string relativePath)
    {
        if (!relativePath.StartsWith("~/"))
        {
            throw new ArgumentException("relativePath must start with '~/'", nameof(relativePath));
        }

        var codeBase = typeof(TestHelperBase).Assembly.CodeBase;
        var uri = new Uri(codeBase);
        var path = uri.LocalPath;
        var bin = Path.GetDirectoryName(path);

        return relativePath.Replace("~/", bin + "/");
    }

    public IUmbracoVersion GetUmbracoVersion() => new UmbracoVersion();

    public IServiceCollection GetRegister() => new ServiceCollection();

    public abstract IHostingEnvironment GetHostingEnvironment();

    public abstract IApplicationShutdownRegistry GetHostingEnvironmentLifetime();

    public abstract IIpResolver GetIpResolver();

    public IRequestCache GetRequestCache() => new DictionaryAppCache();

    public IPublishedUrlProvider GetPublishedUrlProvider()
    {
        var mock = new Mock<IPublishedUrlProvider>();

        return mock.Object;
    }

    public ILoggingConfiguration GetLoggingConfiguration(IHostingEnvironment hostingEnv = null)
    {
        hostingEnv ??= GetHostingEnvironment();
        return new LoggingConfiguration(
            Path.Combine(hostingEnv.ApplicationPhysicalPath, "umbraco", "logs"));
    }
}
