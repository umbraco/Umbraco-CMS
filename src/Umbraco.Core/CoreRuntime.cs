using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Web;
using LightInject;
using Semver;
using Umbraco.Core.Cache;
using Umbraco.Core.Components;
using Umbraco.Core.Configuration;
using Umbraco.Core.DI;
using Umbraco.Core.Exceptions;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Plugins;
using Umbraco.Core.Services;

namespace Umbraco.Core
{
    /// <summary>
    /// Represents the Core Umbraco runtime.
    /// </summary>
    /// <remarks>Does not handle any of the web-related aspects of Umbraco (startup, etc). It
    /// should be possible to use this runtime in console apps.</remarks>
    public class CoreRuntime : IRuntime
    {
        private readonly UmbracoApplicationBase _app;
        private BootLoader _bootLoader;
        private RuntimeState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreRuntime"/> class.
        /// </summary>
        /// <param name="umbracoApplication">The Umbraco HttpApplication.</param>
        public CoreRuntime(UmbracoApplicationBase umbracoApplication)
        {
            if (umbracoApplication == null) throw new ArgumentNullException(nameof(umbracoApplication));
            _app = umbracoApplication;
        }

        /// <inheritdoc/>
        public virtual void Boot(ServiceContainer container)
        {
            // some components may want to initialize with the UmbracoApplicationBase
            // well, they should not - we should not do this - however, Compat7 wants
            // it, so let's do it, but we should remove this eventually.
            container.RegisterInstance(_app);

            Compose(container);

            // prepare essential stuff

            var path = GetApplicationRootPath();
            if (string.IsNullOrWhiteSpace(path) == false)
                IOHelper.SetRootDirectory(path);

            _state = (RuntimeState) container.GetInstance<IRuntimeState>();
            _state.Level = RuntimeLevel.Boot;

            Logger = container.GetInstance<ILogger>();
            Profiler = container.GetInstance<IProfiler>();
            ProfilingLogger = container.GetInstance<ProfilingLogger>();

            // the boot loader boots using a container scope, so anything that is PerScope will
            // be disposed after the boot loader has booted, and anything else will remain.
            // note that this REQUIRES that perWebRequestScope has NOT been enabled yet, else
            // the container will fail to create a scope since there is no http context when
            // the application starts.
            // the boot loader is kept in the runtime for as long as Umbraco runs, and components
            // are NOT disposed - which is not a big deal as long as they remain lightweight
            // objects.

            using (var bootTimer = ProfilingLogger.TraceDuration<CoreRuntime>(
                $"Booting Umbraco {UmbracoVersion.SemanticVersion.ToSemanticString()} on {NetworkHelper.MachineName}.",
                "Booted.",
                "Boot failed."))
            {
                // throws if not full-trust
                new AspNetHostingPermission(AspNetHostingPermissionLevel.Unrestricted).Demand();

                try
                {
                    Logger.Debug<CoreRuntime>($"Runtime: {GetType().FullName}");

                    AquireMainDom(container);
                    DetermineRuntimeLevel(container);
                    var componentTypes = ResolveComponentTypes();
                    _bootLoader = new BootLoader(container);
                    _bootLoader.Boot(componentTypes, _state.Level);

                    // this was done in Complete() right before running the Started event handlers
                    // "special case for the user service, we need to tell it if it's an upgrade, if so we need to ensure that
                    // exceptions are bubbled up if a user is attempted to be persisted during an upgrade (i.e. when they auth to login)"
                    //
                    // was *always* setting the value to true which is?! so using the runtime level
                    // and then, it is *never* resetted to false, meaning Umbraco has been running with IsUpgrading being true?
                    // fixme - this is... bad
                    ((UserService) Current.Services.UserService).IsUpgrading = _state.Level == RuntimeLevel.Upgrade;
                }
                catch (Exception e)
                {
                    _state.Level = RuntimeLevel.BootFailed;
                    var bfe = e as BootFailedException ?? new BootFailedException("Boot failed.", e);
                    bootTimer.Fail(exception: bfe); // be sure to log the exception - even if we repeat ourselves

                    // throwing here can cause w3wp to hard-crash and we want to avoid it.
                    // instead, we're logging the exception and setting level to BootFailed.
                    // various parts of Umbraco such as UmbracoModule and UmbracoDefaultOwinStartup
                    // understand this and will nullify themselves, while UmbracoModule will
                    // throw a BootFailedException for every requests.
                }
            }
        }

        private void AquireMainDom(IServiceFactory container)
        {
            using (var timer = ProfilingLogger.DebugDuration<CoreRuntime>("Acquiring MainDom.", "Aquired."))
            {
                try
                {
                    var mainDom = container.GetInstance<MainDom>();
                    mainDom.Acquire();
                }
                catch
                {
                    timer.Fail();
                    throw;
                }
            }
        }

        // internal for tests
        internal void DetermineRuntimeLevel(IServiceFactory container)
        {
            using (var timer = ProfilingLogger.DebugDuration<CoreRuntime>("Determining runtime level.", "Determined."))
            {
                try
                {
                    var dbfactory = container.GetInstance<IDatabaseFactory>();
                    SetRuntimeStateLevel(_state, dbfactory, Logger);
                    Logger.Debug<CoreRuntime>($"Runtime level: {_state.Level}");
                }
                catch
                {
                    timer.Fail();
                    throw;
                }
            }
        }

        private IEnumerable<Type> ResolveComponentTypes()
        {
            using (var timer = ProfilingLogger.TraceDuration<CoreRuntime>("Resolving component types.", "Resolved."))
            {
                try
                {
                    return GetComponentTypes();
                }
                catch
                {
                    timer.Fail();
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public virtual void Terminate()
        {
            using (ProfilingLogger.DebugDuration<CoreRuntime>("Terminating Umbraco.", "Terminated."))
            {
                _bootLoader?.Terminate();
            }
        }

        /// <inheritdoc/>
        public virtual void Compose(ServiceContainer container)
        {
            container.RegisterSingleton<IProfiler, LogProfiler>();
            container.RegisterSingleton<ProfilingLogger>();
            container.RegisterSingleton<IRuntimeState, RuntimeState>();

            container.RegisterSingleton(_ => new CacheHelper(
                // we need to have the dep clone runtime cache provider to ensure
                // all entities are cached properly (cloned in and cloned out)
                new DeepCloneRuntimeCacheProvider(new ObjectCacheRuntimeCacheProvider()),
                new StaticCacheProvider(),
                // we have no request based cache when not running in web-based context
                new NullCacheProvider(),
                new IsolatedRuntimeCache(type =>
                    // we need to have the dep clone runtime cache provider to ensure
                    // all entities are cached properly (cloned in and cloned out)
                    new DeepCloneRuntimeCacheProvider(new ObjectCacheRuntimeCacheProvider()))));
            container.RegisterSingleton(factory => factory.GetInstance<CacheHelper>().RuntimeCache);

            container.RegisterSingleton(factory => new PluginManager(factory.GetInstance<IRuntimeCacheProvider>(), factory.GetInstance<ProfilingLogger>()));

            // register syntax providers
            container.Register<ISqlSyntaxProvider, MySqlSyntaxProvider>("MySqlSyntaxProvider");
            container.Register<ISqlSyntaxProvider, SqlCeSyntaxProvider>("SqlCeSyntaxProvider");
            container.Register<ISqlSyntaxProvider, SqlServerSyntaxProvider>("SqlServerSyntaxProvider");

            // register persistence mappers - required by database factory so needs to be done here
            // means the only place the collection can be modified is in a runtime - afterwards it 
            // has been frozen and it is too late
            var mapperCollectionBuilder = container.RegisterCollectionBuilder<MapperCollectionBuilder>();
            ComposeMapperCollection(mapperCollectionBuilder);

            // register database factory
            // will be initialized with syntax providers and a logger, and will try to configure
            // from the default connection string name, if possible, else will remain non-configured
            // until the database context configures it properly (eg when installing)
            container.RegisterSingleton<IDatabaseFactory, DefaultDatabaseFactory>();

            // register a database accessor - will be replaced
            // by HybridUmbracoDatabaseAccessor in the web runtime
            container.RegisterSingleton<IUmbracoDatabaseAccessor, ThreadStaticUmbracoDatabaseAccessor>();

            // register MainDom
            container.RegisterSingleton<MainDom>();
        }

        protected virtual void ComposeMapperCollection(MapperCollectionBuilder builder)
        {
            // ResolveTypesWithAttribute<BaseMapper, MapperForAttribute>()
            builder
                .Add<AccessMapper>()
                .Add<AuditItemMapper>()
                .Add<ContentMapper>()
                .Add<ContentTypeMapper>()
                .Add<DataTypeDefinitionMapper>()
                .Add<DictionaryMapper>()
                .Add<DictionaryTranslationMapper>()
                .Add<DomainMapper>()
                .Add<LanguageMapper>()
                .Add<MacroMapper>()
                .Add<MediaMapper>()
                .Add<MediaTypeMapper>()
                .Add<MemberGroupMapper>()
                .Add<MemberMapper>()
                .Add<MemberTypeMapper>()
                .Add<MigrationEntryMapper>()
                .Add<PropertyGroupMapper>()
                .Add<PropertyMapper>()
                .Add<PropertyTypeMapper>()
                .Add<RelationMapper>()
                .Add<ServerRegistrationMapper>()
                .Add<TagMapper>()
                .Add<TaskTypeMapper>()
                .Add<TemplateMapper>()
                .Add<UmbracoEntityMapper>()
                .Add<UserMapper>()
                .Add<ExternalLoginMapper>()
                .Add<UserTypeMapper>();
        }

        private void SetRuntimeStateLevel(RuntimeState runtimeState, IDatabaseFactory databaseFactory, ILogger logger)
        {
            var localVersion = LocalVersion; // the local, files, version
            var codeVersion = runtimeState.SemanticVersion; // the executing code version
            var connect = false;

            // we don't know yet
            runtimeState.Level = RuntimeLevel.Unknown;

            if (string.IsNullOrWhiteSpace(localVersion))
            {
                // there is no local version, we are not installed
                logger.Debug<CoreRuntime>("No local version, need to install Umbraco.");
                runtimeState.Level = RuntimeLevel.Install;
            }
            else if (localVersion != codeVersion)
            {
                // there *is* a local version, but it does not match the code version
                // need to upgrade
                logger.Debug<CoreRuntime>($"Local version \"{localVersion}\" != code version, need to upgrade Umbraco.");
                runtimeState.Level = RuntimeLevel.Upgrade;
            }
            else if (databaseFactory.Configured == false)
            {
                // local version *does* match code version, but the database is not configured
                // install (again? this is a weird situation...)
                logger.Debug<CoreRuntime>("Database is not configured, need to install Umbraco.");
                runtimeState.Level = RuntimeLevel.Install;
            }

            // install? not going to test anything else
            if (runtimeState.Level == RuntimeLevel.Install)
                return;

            // else, keep going,
            // anything other than install wants a database - see if we can connect
            for (var i = 0; i < 5; i++)
            {
                connect = databaseFactory.CanConnect;
                if (connect) break;
                logger.Debug<CoreRuntime>(i == 0
                    ? "Could not immediately connect to database, trying again."
                    : "Could not connect to database.");
                Thread.Sleep(1000);
            }

            if (connect == false)
            {
                // cannot connect to configured database, this is bad, fail
                logger.Debug<CoreRuntime>("Could not connect to database.");
                runtimeState.Level = RuntimeLevel.BootFailed;

                // in fact, this is bad enough that we want to throw
                throw new BootFailedException("A connection string is configured but Umbraco could not connect to the database.");
            }

            // if we already know we want to upgrade, no need to look for migrations...
            if (runtimeState.Level == RuntimeLevel.Upgrade)
                return;

            // else
            // look for a matching migration entry - bypassing services entirely - they are not 'up' yet
            // fixme - in a LB scenario, ensure that the DB gets upgraded only once!
            // fixme - eventually move to yol-style guid-based transitions
            bool exists;
            try
            {
                exists = EnsureMigration(databaseFactory, codeVersion);
            }
            catch
            {
                // can connect to the database but cannot access the migration table... need to install
                logger.Debug<CoreRuntime>("Could not check migrations, need to install Umbraco.");
                runtimeState.Level = RuntimeLevel.Install;
                return;
            }

            if (exists)
            {
                // the database version matches the code & files version, all clear, can run
                runtimeState.Level = RuntimeLevel.Run;
                return;
            }

            // the db version does not match... but we do have a migration table
            // so, at least one valid table, so we quite probably are installed & need to upgrade

            // although the files version matches the code version, the database version does not
            // which means the local files have been upgraded but not the database - need to upgrade
            logger.Debug<CoreRuntime>("Database migrations have not executed, need to upgrade Umbraco.");
            runtimeState.Level = RuntimeLevel.Upgrade;
        }

        protected virtual bool EnsureMigration(IDatabaseFactory databaseFactory, SemVersion codeVersion)
        {
            var database = databaseFactory.GetDatabase();
            var codeVersionString = codeVersion.ToString();
            var sql = database.Sql()
                .Select<MigrationDto>()
                .From<MigrationDto>()
                .Where<MigrationDto>(x => x.Name.InvariantEquals(GlobalSettings.UmbracoMigrationName) && x.Version == codeVersionString);
            return database.FirstOrDefault<MigrationDto>(sql) != null;
        }

        private static string LocalVersion
        {
            get
            {
                try
                {
                    // fixme - this should live in its own independent file! NOT web.config!
                    return ConfigurationManager.AppSettings["umbracoConfigurationStatus"];
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        #region Locals

        protected ILogger Logger { get; private set; }

        protected IProfiler Profiler { get; private set; }

        protected ProfilingLogger ProfilingLogger { get; private set; }

        #endregion

        #region Getters

        // getters can be implemented by runtimes inheriting from CoreRuntime

        protected virtual IEnumerable<Type> GetComponentTypes() => Current.PluginManager.ResolveTypes<IUmbracoComponent>();

        // by default, returns null, meaning that Umbraco should auto-detect the application root path.
        // override and return the absolute path to the Umbraco site/solution, if needed
        protected virtual string GetApplicationRootPath() => null;

        #endregion
    }
}
