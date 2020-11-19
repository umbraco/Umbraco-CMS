using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core.Builder;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Events;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;

namespace Umbraco.Core.Runtime
{
    /// <summary>
    /// Bootstraps the Core Umbraco runtime.
    /// </summary>
    /// <remarks>Does not handle any of the web-related aspects of Umbraco (startup, etc). It
    /// should be possible to use this runtime in console apps.</remarks>
    public class CoreRuntimeBootstrapper
    {
        // runtime state, this instance will get replaced again once the essential services are available to run the check
        private RuntimeState _state = RuntimeState.Booting();
        private readonly IUmbracoBootPermissionChecker _umbracoBootPermissionChecker;
        private readonly GlobalSettings _globalSettings;
        private readonly ConnectionStrings _connectionStrings;

        public CoreRuntimeBootstrapper(
            GlobalSettings globalSettings,
            ConnectionStrings connectionStrings,
            IUmbracoVersion umbracoVersion,
            IIOHelper ioHelper,
            IProfiler profiler,
            IUmbracoBootPermissionChecker umbracoBootPermissionChecker,
            IHostingEnvironment hostingEnvironment,
            IBackOfficeInfo backOfficeInfo,
            IDbProviderFactoryCreator dbProviderFactoryCreator,
            IMainDom mainDom,
            ITypeFinder typeFinder,
            AppCaches appCaches)
        {
            _globalSettings = globalSettings;
            _connectionStrings = connectionStrings;

            IOHelper = ioHelper;
            AppCaches = appCaches;
            UmbracoVersion = umbracoVersion;
            Profiler = profiler;
            HostingEnvironment = hostingEnvironment;
            BackOfficeInfo = backOfficeInfo;
            DbProviderFactoryCreator = dbProviderFactoryCreator;

            _umbracoBootPermissionChecker = umbracoBootPermissionChecker;
            MainDom = mainDom;
            TypeFinder = typeFinder;
        }


        protected IBackOfficeInfo BackOfficeInfo { get; }

        public IDbProviderFactoryCreator DbProviderFactoryCreator { get; }

        /// <summary>
        /// Gets the profiler.
        /// </summary>
        protected IProfiler Profiler { get; }

        /// <summary>
        /// Gets the <see cref="ITypeFinder"/>
        /// </summary>
        protected ITypeFinder TypeFinder { get; }

        /// <summary>
        /// Gets the <see cref="IIOHelper"/>
        /// </summary>
        protected IIOHelper IOHelper { get; }

        protected IHostingEnvironment HostingEnvironment { get; }
        public AppCaches AppCaches { get; }
        public IUmbracoVersion UmbracoVersion { get; }

        /// <inheritdoc />
        public IRuntimeState State => _state;

        public IMainDom MainDom { get; }

        /// <inheritdoc/>
        public virtual void Configure(IUmbracoBuilder builder)
        {
            if (builder is null) throw new ArgumentNullException(nameof(builder));


            // create and register the essential services
            // ie the bare minimum required to boot

            // the boot loader boots using a container scope, so anything that is PerScope will
            // be disposed after the boot loader has booted, and anything else will remain.
            // note that this REQUIRES that perWebRequestScope has NOT been enabled yet, else
            // the container will fail to create a scope since there is no http context when
            // the application starts.
            // the boot loader is kept in the runtime for as long as Umbraco runs, and components
            // are NOT disposed - which is not a big deal as long as they remain lightweight
            // objects.

            var umbracoVersion = new UmbracoVersion();
            var logger = builder.BuilderLoggerFactory.CreateLogger<CoreRuntimeBootstrapper>();
            var profilingLogger = new ProfilingLogger(logger, Profiler);

            using (var timer = profilingLogger.TraceDuration<CoreRuntimeBootstrapper>(
                $"Booting Umbraco {umbracoVersion.SemanticVersion.ToSemanticString()}.",
                "Booted.",
                "Boot failed."))
            {

                logger.LogInformation("Booting site '{HostingSiteName}', app '{HostingApplicationId}', path '{HostingPhysicalPath}', server '{MachineName}'.",
                    HostingEnvironment?.SiteName,
                    HostingEnvironment?.ApplicationId,
                    HostingEnvironment?.ApplicationPhysicalPath,
                    NetworkHelper.MachineName);
                logger.LogDebug("Runtime: {Runtime}", GetType().FullName);

                AppDomain.CurrentDomain.SetData("DataDirectory", HostingEnvironment?.MapPathContentRoot(Constants.SystemDirectories.Data));

                // application environment
                ConfigureUnhandledException(logger);
                try
                {
                    // run handlers
                    OnRuntimeBoot(profilingLogger);

                    // TODO: Don't do this, UmbracoBuilder ctor should handle it...
                    builder.TypeLoader = new TypeLoader(TypeFinder, AppCaches.RuntimeCache,
                        new DirectoryInfo(HostingEnvironment.LocalTempPath),
                        builder.BuilderLoggerFactory.CreateLogger<TypeLoader>(), profilingLogger);

                    builder.Services.AddUnique<ILogger>(logger);
                    builder.Services.AddUnique<ILoggerFactory>(builder.BuilderLoggerFactory);
                    builder.Services.AddUnique<IUmbracoBootPermissionChecker>(_umbracoBootPermissionChecker);
                    builder.Services.AddUnique<IProfiler>(Profiler);
                    builder.Services.AddUnique<IProfilingLogger>(profilingLogger);
                    builder.Services.AddUnique<IMainDom>(MainDom);
                    builder.Services.AddUnique<AppCaches>(AppCaches);
                    builder.Services.AddUnique<IRequestCache>(AppCaches.RequestCache);
                    builder.Services.AddUnique<TypeLoader>(builder.TypeLoader);
                    builder.Services.AddUnique<ITypeFinder>(TypeFinder);
                    builder.Services.AddUnique<IIOHelper>(IOHelper);
                    builder.Services.AddUnique<IUmbracoVersion>(UmbracoVersion);
                    builder.Services.AddUnique<IDbProviderFactoryCreator>(DbProviderFactoryCreator);
                    builder.Services.AddUnique<IHostingEnvironment>(HostingEnvironment);
                    builder.Services.AddUnique<IBackOfficeInfo>(BackOfficeInfo);
                    builder.Services.AddUnique<IRuntime, CoreRuntime>();

                    // NOTE: This instance of IUmbracoDatabaseFactory is only used to determine runtime state.
                    var bootstrapDatabaseFactory = CreateBootstrapDatabaseFactory(builder.BuilderLoggerFactory);

                    // after bootstrapping we let the container wire up for us.
                    builder.Services.AddUnique<IUmbracoDatabaseFactory, UmbracoDatabaseFactory>();
                    builder.Services.AddUnique<ISqlContext>(factory => factory.GetRequiredService<IUmbracoDatabaseFactory>().SqlContext);
                    builder.Services.AddUnique<IBulkSqlInsertProvider>(factory => factory.GetRequiredService<IUmbracoDatabaseFactory>().BulkSqlInsertProvider);

                    // re-create the state object with the essential services
                    _state = new RuntimeState(_globalSettings, UmbracoVersion, bootstrapDatabaseFactory, builder.BuilderLoggerFactory.CreateLogger<RuntimeState>());
                    builder.Services.AddUnique<IRuntimeState>(_state);

                    // run handlers
                    OnRuntimeEssentials(builder, AppCaches, builder.TypeLoader, bootstrapDatabaseFactory);

                    // determine our runtime level
                    DetermineRuntimeLevel(bootstrapDatabaseFactory, logger, profilingLogger);
                }
                catch (Exception e)
                {
                    var bfe = e as BootFailedException ?? new BootFailedException("Boot failed.", e);

                    if (_state != null)
                    {
                        _state.Level = RuntimeLevel.BootFailed;
                        _state.BootFailedException = bfe;
                    }

                    timer?.Fail(exception: bfe); // be sure to log the exception - even if we repeat ourselves

                    Debugger.Break();
                }
            }
        }


        protected virtual void ConfigureUnhandledException(ILogger logger)
        {
            //take care of unhandled exceptions - there is nothing we can do to
            // prevent the launch process to go down but at least we can try
            // and log the exception
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                var exception = (Exception)args.ExceptionObject;
                var isTerminating = args.IsTerminating; // always true?

                var msg = "Unhandled exception in AppDomain";
                if (isTerminating) msg += " (terminating)";
                msg += ".";
                logger.LogError(exception, msg);
            };
        }



        private void DetermineRuntimeLevel(IUmbracoDatabaseFactory databaseFactory, ILogger logger, IProfilingLogger profilingLogger)
        {
            using var timer = profilingLogger.DebugDuration<CoreRuntimeBootstrapper>("Determining runtime level.", "Determined.");

            try
            {
                _state.DetermineRuntimeLevel();

                logger.LogDebug("Runtime level: {RuntimeLevel} - {RuntimeLevelReason}", _state.Level, _state.Reason);

                if (_state.Level == RuntimeLevel.Upgrade)
                {
                    logger.LogDebug("Configure database factory for upgrades.");
                    databaseFactory.ConfigureForUpgrade();
                }
            }
            catch
            {
                _state.Level = RuntimeLevel.BootFailed;
                _state.Reason = RuntimeLevelReason.BootFailedOnException;
                timer?.Fail();
                throw;
            }
        }

        #region Getters

        // getters can be implemented by runtimes inheriting from CoreRuntime

        /// <summary>
        /// Creates the database factory.
        /// </summary>
        /// <remarks>This is strictly internal, for tests only.</remarks>
        protected internal virtual IUmbracoDatabaseFactory CreateBootstrapDatabaseFactory(ILoggerFactory runtimeLoggerFactory)
            => new UmbracoDatabaseFactory(
                runtimeLoggerFactory.CreateLogger<UmbracoDatabaseFactory>(),
                runtimeLoggerFactory,
                Options.Create(_globalSettings),
                Options.Create(_connectionStrings),
                new Lazy<IMapperCollection>(() => new MapperCollection(Enumerable.Empty<BaseMapper>())),
                DbProviderFactoryCreator);


        #endregion

        #region Events

        protected void OnRuntimeBoot(IProfilingLogger profilingLogger)
        {
            RuntimeBooting?.Invoke(this, profilingLogger);
        }

        protected void OnRuntimeEssentials(IUmbracoBuilder builder, AppCaches appCaches, TypeLoader typeLoader, IUmbracoDatabaseFactory databaseFactory)
        {
            RuntimeEssentials?.Invoke(this, new RuntimeEssentialsEventArgs(builder, databaseFactory));
        }

        public event TypedEventHandler<CoreRuntimeBootstrapper, IProfilingLogger> RuntimeBooting;
        public event TypedEventHandler<CoreRuntimeBootstrapper, RuntimeEssentialsEventArgs> RuntimeEssentials;

        #endregion

    }
}
