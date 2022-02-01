using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.Exceptions;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Logging.Serilog;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Migrations.Upgrade;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Security;
using Umbraco.Core.Sync;

namespace Umbraco.Core.Runtime
{
    /// <summary>
    /// Represents the Core Umbraco runtime.
    /// </summary>
    /// <remarks>Does not handle any of the web-related aspects of Umbraco (startup, etc). It
    /// should be possible to use this runtime in console apps.</remarks>
    public class CoreRuntime : IRuntime
    {
        private ComponentCollection _components;
        private IFactory _factory;
        private RuntimeState _state;

        [Obsolete("Use the ctor with all parameters instead")]
        public CoreRuntime()
        {
        }

        public CoreRuntime(ILogger logger, IMainDom mainDom)
        {
            MainDom = mainDom;
            Logger = logger;
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// Gets the profiler.
        /// </summary>
        protected IProfiler Profiler { get; private set; }

        /// <summary>
        /// Gets the profiling logger.
        /// </summary>
        protected IProfilingLogger ProfilingLogger { get; private set; }

        /// <inheritdoc />
        public IRuntimeState State => _state;

        public IMainDom MainDom { get; private set; }

        /// <inheritdoc/>
        public virtual IFactory Boot(IRegister register)
        {
            // create and register the essential services
            // ie the bare minimum required to boot

#pragma warning disable CS0618 // Type or member is obsolete
            // loggers
            // TODO: Removes this in netcore, this is purely just backwards compat ugliness
            var logger = GetLogger();
            if (logger != Logger)
                Logger = logger;
#pragma warning restore CS0618 // Type or member is obsolete

            var profiler = Profiler = GetProfiler();
            var profilingLogger = ProfilingLogger = new ProfilingLogger(logger, profiler);

            // the boot loader boots using a container scope, so anything that is PerScope will
            // be disposed after the boot loader has booted, and anything else will remain.
            // note that this REQUIRES that perWebRequestScope has NOT been enabled yet, else
            // the container will fail to create a scope since there is no http context when
            // the application starts.
            // the boot loader is kept in the runtime for as long as Umbraco runs, and components
            // are NOT disposed - which is not a big deal as long as they remain lightweight
            // objects.

            using (var timer = profilingLogger.TraceDuration<CoreRuntime>(
                $"Booting Umbraco {UmbracoVersion.SemanticVersion.ToSemanticString()}.",
                "Booted.",
                "Boot failed."))
            {
                logger.Info<CoreRuntime>("Booting site '{HostingSiteName}', app '{HostingApplicationID}', path '{HostingPhysicalPath}', server '{MachineName}'.",
                    HostingEnvironment.SiteName,
                    HostingEnvironment.ApplicationID,
                    HostingEnvironment.ApplicationPhysicalPath,
                    NetworkHelper.MachineName);
                logger.Debug<CoreRuntime, string>("Runtime: {Runtime}", GetType().FullName);

                // application environment
                ConfigureUnhandledException();
                ConfigureApplicationRootPath();

                Boot(register, timer);
            }

            return _factory;
        }

        /// <summary>
        /// Boots the runtime within a timer.
        /// </summary>
        protected virtual IFactory Boot(IRegister register, DisposableTimer timer)
        {
            Composition composition = null;

            try
            {
                // Setup event listener
                UnattendedInstalled += CoreRuntime_UnattendedInstalled;

                // throws if not full-trust
                new AspNetHostingPermission(AspNetHostingPermissionLevel.Unrestricted).Demand();

                // run handlers
                RuntimeOptions.DoRuntimeBoot(ProfilingLogger);

                // application caches
                var appCaches = GetAppCaches();

                // database factory
                var databaseFactory = GetDatabaseFactory();

                // configs
                var configs = GetConfigs();

                // type loader
                var typeLoader = new TypeLoader(appCaches.RuntimeCache, configs.Global().LocalTempPath, ProfilingLogger);

                // runtime state
                // beware! must use '() => _factory.GetInstance<T>()' and NOT '_factory.GetInstance<T>'
                // as the second one captures the current value (null) and therefore fails
                _state = new RuntimeState(Logger,
                    configs.Settings(), configs.Global(),
                    new Lazy<IMainDom>(() => _factory.GetInstance<IMainDom>()),
                    new Lazy<IServerRegistrar>(() => _factory.GetInstance<IServerRegistrar>()))
                {
                    Level = RuntimeLevel.Boot
                };

                // TODO: remove this in netcore, this is purely backwards compat hacks with the empty ctor
                if (MainDom == null)
                {
                    MainDom = new MainDom(Logger, new MainDomSemaphoreLock(Logger));
                }


                // create the composition
                composition = new Composition(register, typeLoader, ProfilingLogger, _state, configs);
                composition.RegisterEssentials(Logger, Profiler, ProfilingLogger, MainDom, appCaches, databaseFactory, typeLoader, _state);

                // run handlers
                RuntimeOptions.DoRuntimeEssentials(composition, appCaches, typeLoader, databaseFactory);



                // register runtime-level services
                // there should be none, really - this is here "just in case"
                Compose(composition);

                // acquire the main domain - if this fails then anything that should be registered with MainDom will not operate
                AcquireMainDom(MainDom);

                // determine our runtime level
                DetermineRuntimeLevel(databaseFactory, ProfilingLogger);

                // get composers, and compose
                var composerTypes = ResolveComposerTypes(typeLoader);

                IEnumerable<Attribute> enableDisableAttributes;
                using (ProfilingLogger.DebugDuration<CoreRuntime>("Scanning enable/disable composer attributes"))
                {
                    enableDisableAttributes = typeLoader.GetAssemblyAttributes(typeof(EnableComposerAttribute), typeof(DisableComposerAttribute));
                }

                var composers = new Composers(composition, composerTypes, enableDisableAttributes, ProfilingLogger);
                composers.Compose();

                // create the factory
                _factory = Current.Factory = composition.CreateFactory();

                // determines if unattended install is enabled and performs it if required
                DoUnattendedInstall(databaseFactory);

                // determine our runtime level (AFTER UNATTENDED INSTALL)
                // TODO: Feels kinda weird to call this again
                DetermineRuntimeLevel(databaseFactory, ProfilingLogger);

                // if level is Run and reason is UpgradeMigrations, that means we need to perform an unattended upgrade
                if (_state.Reason == RuntimeLevelReason.UpgradeMigrations && _state.Level == RuntimeLevel.Run)
                {
                    // do the upgrade
                    DoUnattendedUpgrade(_factory.GetInstance<DatabaseBuilder>());

                    // upgrade is done, set reason to Run
                    _state.Reason = RuntimeLevelReason.Run;
                }

                // create & initialize the components
                _components = _factory.GetInstance<ComponentCollection>();
                _components.Initialize();
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

                // if something goes wrong above, we may end up with no factory
                // meaning nothing can get the runtime state, etc - so let's try
                // to make sure we have a factory
                if (_factory == null)
                {
                    try
                    {
                        _factory = Current.Factory = composition?.CreateFactory();
                    }
                    catch
                    {
                        // In this case we are basically dead, we do not have a factory but we need
                        // to report on the state so we need to manually set that, this is the only time
                        // we ever do this.
                        Current.RuntimeState = _state;
                    }
                }

                Debugger.Break();

                // throwing here can cause w3wp to hard-crash and we want to avoid it.
                // instead, we're logging the exception and setting level to BootFailed.
                // various parts of Umbraco such as UmbracoModule and UmbracoDefaultOwinStartup
                // understand this and will nullify themselves, while UmbracoModule will
                // throw a BootFailedException for every requests.
            }

            return _factory;
        }

        private void CoreRuntime_UnattendedInstalled(IRuntime sender, UnattendedInstallEventArgs e)
        {
            var unattendedName = Environment.GetEnvironmentVariable("UnattendedUserName");
            var unattendedEmail = Environment.GetEnvironmentVariable("UnattendedUserEmail");
            var unattendedPassword = Environment.GetEnvironmentVariable("UnattendedUserPassword");

            var fileExists = false;
            var filePath = IOHelper.MapPath("~/App_Data/unattended.user.json");

            // No values store in ENV vars - try fallback file of /app_data/unattended.user.json
            if (unattendedName.IsNullOrWhiteSpace()
                || unattendedEmail.IsNullOrWhiteSpace()
                || unattendedPassword.IsNullOrWhiteSpace())
            {

                fileExists = File.Exists(filePath);
                if (fileExists == false)
                {
                    return;
                }

                // Attempt to deserialize JSON
                try
                {
                    var fileContents = File.ReadAllText(filePath);
                    var credentials = JsonConvert.DeserializeObject<UnattendedUserConfig>(fileContents);

                    unattendedName = credentials.Name;
                    unattendedEmail = credentials.Email;
                    unattendedPassword = credentials.Password;
                }
                catch (Exception ex)
                {

                    throw;
                }
            }

            // ENV Variables & JSON still empty
            if (unattendedName.IsNullOrWhiteSpace()
                || unattendedEmail.IsNullOrWhiteSpace()
                || unattendedPassword.IsNullOrWhiteSpace())
            {
                return;
            }


            // Update user details
            var currentProvider = MembershipProviderExtensions.GetUsersMembershipProvider();
            var admin = Current.Services.UserService.GetUserById(Constants.Security.SuperUserId);
            if (admin == null)
            {
                throw new InvalidOperationException("Could not find the super user!");
            }

            var membershipUser = currentProvider.GetUser(Constants.Security.SuperUserId, true);
            if (membershipUser == null)
            {
                throw new InvalidOperationException($"No user found in membership provider with id of {Constants.Security.SuperUserId}.");
            }

            try
            {
                var success = membershipUser.ChangePassword("default", unattendedPassword.Trim());
                if (success == false)
                {
                    throw new FormatException("Password must be at least " + currentProvider.MinRequiredPasswordLength + " characters long and contain at least " + currentProvider.MinRequiredNonAlphanumericCharacters + " symbols");
                }
            }
            catch (Exception)
            {
                throw new FormatException("Password must be at least " + currentProvider.MinRequiredPasswordLength + " characters long and contain at least " + currentProvider.MinRequiredNonAlphanumericCharacters + " symbols");
            }

            admin.Email = unattendedEmail.Trim();
            admin.Name = unattendedName.Trim();
            admin.Username = unattendedEmail.Trim();

            Current.Services.UserService.Save(admin);

            // Delete JSON file if it existed to tidy
            if (fileExists)
            {
                File.Delete(filePath);
            }
        }

        private void DoUnattendedInstall(IUmbracoDatabaseFactory databaseFactory)
        {
            // unattended install is not enabled
            if (RuntimeOptions.InstallUnattended == false) return;

            var localVersion = UmbracoVersion.LocalVersion; // the local, files, version
            var codeVersion = _state.SemanticVersion; // the executing code version

            // local version and code version is not equal, an unattended install cannot be performed
            if (localVersion != codeVersion) return;

            // no connection string set
            if (databaseFactory.Configured == false) return;

            // create SQL CE database if not existing and database provider is SQL CE
            if (databaseFactory.ProviderName == Constants.DbProviderNames.SqlCe)
            {
                var dataSource = new SqlCeConnectionStringBuilder(databaseFactory.ConnectionString).DataSource;
                var dbFilePath = dataSource.Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory").ToString());

                if(File.Exists(dbFilePath) == false)
                {
                    var engine = new SqlCeEngine(databaseFactory.ConnectionString);
                    engine.CreateDatabase();
                }
            }

            var tries = 5;
            var connect = false;
            for (var i = 0;;)
            {
                connect = databaseFactory.CanConnect;
                if (connect || ++i == tries) break;
                Logger.Debug<CoreRuntime>("Could not immediately connect to database, trying again.");
                Thread.Sleep(1000);
            }

            // could not connect to the database
            if (connect == false) return;

            using (var database = databaseFactory.CreateDatabase())
            {
                var hasUmbracoTables = database.IsUmbracoInstalled(Logger);

                // database has umbraco tables, assume Umbraco is already installed
                if (hasUmbracoTables) return;

                // all conditions fulfilled, do the install
                Logger.Info<CoreRuntime>("Starting unattended install.");

                try
                {
                    database.BeginTransaction();
                    var creator = new DatabaseSchemaCreator(database, Logger);
                    creator.InitializeDatabaseSchema();
                    database.CompleteTransaction();

                    // Emit an event that unattended install completed
                    // Then this event can be listened for and create an unattended user
                    UnattendedInstalled?.Invoke(this, new UnattendedInstallEventArgs());

                    Logger.Info<CoreRuntime>("Unattended install completed.");
                }
                catch (Exception ex)
                {
                    Logger.Error<CoreRuntime>(ex, "Error during unattended install.");
                    database.AbortTransaction();

                    throw new UnattendedInstallException(
                        "The database configuration failed with the following message: " + ex.Message
                        + "\n Please check log file for additional information (can be found in '/App_Data/Logs/')");
                }
            }
        }

        private void DoUnattendedUpgrade(DatabaseBuilder databaseBuilder)
        {
            var plan = new UmbracoPlan();
            using (ProfilingLogger.TraceDuration<CoreRuntime>("Starting unattended upgrade.", "Unattended upgrade completed."))
            {
                var result = databaseBuilder.UpgradeSchemaAndData(plan);
                if (result.Success == false)
                    throw new UnattendedInstallException("An error occurred while running the unattended upgrade.\n" + result.Message);
            }
        }

        protected virtual void ConfigureUnhandledException()
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
                Logger.Error<CoreRuntime>(exception, msg);
            };
        }

        protected virtual void ConfigureApplicationRootPath()
        {
            var path = GetApplicationRootPath();
            if (string.IsNullOrWhiteSpace(path) == false)
                IOHelper.SetRootDirectory(path);
        }

        private bool AcquireMainDom(IMainDom mainDom)
        {
            using (var timer = ProfilingLogger.DebugDuration<CoreRuntime>("Acquiring MainDom.", "Acquired."))
            {
                try
                {
                    return mainDom.IsMainDom;
                }
                catch
                {
                    timer?.Fail();
                    throw;
                }
            }
        }

        // internal/virtual for tests (i.e. hack, do not port to netcore)
        internal virtual void DetermineRuntimeLevel(IUmbracoDatabaseFactory databaseFactory, IProfilingLogger profilingLogger)
        {
            using (var timer = profilingLogger.DebugDuration<CoreRuntime>("Determining runtime level.", "Determined."))
            {
                try
                {
                    _state.DetermineRuntimeLevel(databaseFactory);

                    profilingLogger.Debug<CoreRuntime,RuntimeLevel,RuntimeLevelReason>("Runtime level: {RuntimeLevel} - {RuntimeLevelReason}", _state.Level, _state.Reason);

                    if (_state.Level == RuntimeLevel.Upgrade)
                    {
                        profilingLogger.Debug<CoreRuntime>("Configure database factory for upgrades.");
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
        }

        private IEnumerable<Type> ResolveComposerTypes(TypeLoader typeLoader)
        {
            using (var timer = ProfilingLogger.TraceDuration<CoreRuntime>("Resolving composer types.", "Resolved."))
            {
                try
                {
                    return GetComposerTypes(typeLoader);
                }
                catch
                {
                    timer?.Fail();
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public virtual void Terminate()
        {
            _components?.Terminate();
            UnattendedInstalled -= CoreRuntime_UnattendedInstalled;
        }

        /// <summary>
        /// Composes the runtime.
        /// </summary>
        public virtual void Compose(Composition composition)
        {
            // Nothing
        }

        #region Getters

        // getters can be implemented by runtimes inheriting from CoreRuntime

        /// <summary>
        /// Gets all composer types.
        /// </summary>
        protected virtual IEnumerable<Type> GetComposerTypes(TypeLoader typeLoader)
            => typeLoader.GetTypes<IComposer>();

        [Obsolete("Don't use this method, the logger should be injected into the " + nameof(CoreRuntime))]
        protected virtual ILogger GetLogger()
            => Logger ?? SerilogLogger.CreateWithDefaultConfiguration(); // TODO: Remove this in netcore, this purely just backwards compat ugliness

        /// <summary>
        /// Gets a profiler.
        /// </summary>
        protected virtual IProfiler GetProfiler()
            => new LogProfiler(Logger);

        /// <summary>
        /// Gets the application caches.
        /// </summary>
        protected virtual AppCaches GetAppCaches()
        {
            // need the deep clone runtime cache provider to ensure entities are cached properly, ie
            // are cloned in and cloned out - no request-based cache here since no web-based context,
            // is overridden by the web runtime

            return new AppCaches(
                new DeepCloneAppCache(new ObjectCacheAppCache()),
                NoAppCache.Instance,
                new IsolatedCaches(type => new DeepCloneAppCache(new ObjectCacheAppCache())));
        }

        // by default, returns null, meaning that Umbraco should auto-detect the application root path.
        // override and return the absolute path to the Umbraco site/solution, if needed
        protected virtual string GetApplicationRootPath()
            => null;

        /// <summary>
        /// Gets the database factory.
        /// </summary>
        /// <remarks>This is strictly internal, for tests only.</remarks>
        protected internal virtual IUmbracoDatabaseFactory GetDatabaseFactory()
            => new UmbracoDatabaseFactory(Logger, new Lazy<IMapperCollection>(() => _factory.GetInstance<IMapperCollection>()));

        /// <summary>
        /// Gets the configurations.
        /// </summary>
        protected virtual Configs GetConfigs()
        {
            var configs = new Configs();
            configs.AddCoreConfigs();
            return configs;
        }

        #endregion

        /// <summary>
        /// Event to be used to notify when the Unattended Install has finished
        /// </summary>
        public static event TypedEventHandler<IRuntime, UnattendedInstallEventArgs> UnattendedInstalled;

        [DataContract]
        public class UnattendedUserConfig
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "email")]
            public string Email { get; set; }

            [DataMember(Name = "password")]
            public string Password { get; set; }
        }
    }
}
