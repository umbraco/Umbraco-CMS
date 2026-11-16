using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Perfolizer.Horology;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Runtime;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Tests.Benchmarks;

/// <summary>
/// Compares reading the default language ISO code through <see cref="ILanguageService" />, which creates a scope
/// per call, with reading the cached default culture through <see cref="IDefaultCultureAccessor" />.
/// </summary>
/// <remarks>
/// <para>
/// Run with: <c>dotnet run -c Release --project tests/Umbraco.Tests.Benchmarks -- --filter "*DefaultCultureAccessor*"</c>.
/// </para>
/// <para>
/// The scope provider, language repository and language service are the real implementations, configured as on a
/// single server. The repository's language cache is filled up front, so no benchmark reads the database. The
/// concurrent benchmarks call from every core at once, which is where creating scopes contends on the ambient
/// scope locks.
/// </para>
/// </remarks>
[Config(typeof(InProcessStableRunConfig))]
public class DefaultCultureAccessorBenchmarks
{
    private const int ConcurrentCallsPerInvoke = 10_000;

    private ILanguageService _languageService = null!;
    private IDefaultCultureAccessor _defaultCultureAccessor = null!;

    /// <summary>
    /// Longer iterations than <c>QuickRun</c> so before/after comparisons are stable, run in-process to avoid
    /// the per-benchmark MSBuild step.
    /// </summary>
    private sealed class InProcessStableRunConfig : ManualConfig
    {
        public InProcessStableRunConfig()
        {
            AddJob(Job.Default
                .WithLaunchCount(1)
                .WithIterationTime(new TimeInterval(500, TimeUnit.Millisecond))
                .WithWarmupCount(5)
                .WithIterationCount(10)
                .WithToolchain(InProcessEmitToolchain.Instance));
            AddDiagnoser(MemoryDiagnoser.Default);
        }
    }

    [GlobalSetup]
    public void Setup()
    {
        ILoggerFactory loggerFactory = NullLoggerFactory.Instance;
        ScopeProvider scopeProvider = CreateScopeProvider(loggerFactory);

        var appCaches = new AppCaches(
            NoAppCache.Instance,
            NoAppCache.Instance,
            new IsolatedCaches(_ => new DeepCloneAppCache(new ObjectCacheAppCache())));
        appCaches.IsolatedCaches.GetOrCreate<ILanguage>().Insert(
            RepositoryCacheKeys.GetKey<ILanguage>(),
            () => new DeepCloneableList<ILanguage>(ListCloneBehavior.CloneOnce)
            {
                new Language("en-US", "English (United States)") { Id = 1, IsDefault = true },
                new Language("da-DK", "Danish (Denmark)") { Id = 2 },
            });

        var languageRepository = new LanguageRepository(
            scopeProvider,
            appCaches,
            loggerFactory.CreateLogger<LanguageRepository>(),
            new SingleServerCacheVersionService(),
            Mock.Of<ICacheSyncService>());

        _languageService = new LanguageService(
            scopeProvider,
            loggerFactory,
            Mock.Of<IEventMessagesFactory>(),
            languageRepository,
            Mock.Of<IAuditService>(),
            Mock.Of<IUserIdKeyResolver>(),
            Mock.Of<IIsoCodeValidator>());

        // A real runtime state, as the level is read on every call and a mock's interception would dominate.
        var runtimeState = RuntimeState.Booting();
        runtimeState.Configure(RuntimeLevel.Run, RuntimeLevelReason.Run);

        _defaultCultureAccessor = new DefaultCultureAccessor(
            _languageService,
            runtimeState,
            scopeProvider,
            Mock.Of<IOptionsMonitor<GlobalSettings>>(x => x.CurrentValue == new GlobalSettings()));

        if (LanguageService_GetDefaultIsoCodeAsync() != "en-US" || DefaultCultureAccessor_DefaultCulture() != "en-US")
        {
            throw new InvalidOperationException("The default language was not resolved from the filled cache.");
        }
    }

    [Benchmark(Baseline = true)]
    public string LanguageService_GetDefaultIsoCodeAsync()
        => _languageService.GetDefaultIsoCodeAsync().GetAwaiter().GetResult();

    [Benchmark]
    public string DefaultCultureAccessor_DefaultCulture()
        => _defaultCultureAccessor.DefaultCulture;

    [Benchmark(OperationsPerInvoke = ConcurrentCallsPerInvoke)]
    public void LanguageService_GetDefaultIsoCodeAsync_Concurrent()
        => Parallel.For(0, ConcurrentCallsPerInvoke, _ => LanguageService_GetDefaultIsoCodeAsync());

    [Benchmark(OperationsPerInvoke = ConcurrentCallsPerInvoke)]
    public void DefaultCultureAccessor_DefaultCulture_Concurrent()
        => Parallel.For(0, ConcurrentCallsPerInvoke, _ => DefaultCultureAccessor_DefaultCulture());

    private static ScopeProvider CreateScopeProvider(ILoggerFactory loggerFactory)
    {
        var fileSystems = new FileSystems(
            loggerFactory,
            Mock.Of<IIOHelper>(),
            Options.Create(new GlobalSettings()),
            Mock.Of<IHostingEnvironment>());

        var mediaFileManager = new MediaFileManager(
            Mock.Of<IFileSystem>(),
            Mock.Of<IMediaPathScheme>(),
            loggerFactory.CreateLogger<MediaFileManager>(),
            Mock.Of<IShortStringHelper>(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<Lazy<ICoreScopeProvider>>());

        return new ScopeProvider(
            new AmbientScopeStack(),
            new AmbientScopeContextStack(),
            Mock.Of<IDistributedLockingMechanismFactory>(),
            Mock.Of<IUmbracoDatabaseFactory>(),
            fileSystems,
            Mock.Of<IOptionsMonitor<DebugSettings>>(x => x.CurrentValue == new DebugSettings()),
            mediaFileManager,
            loggerFactory,
            Mock.Of<IEventAggregator>());
    }
}
