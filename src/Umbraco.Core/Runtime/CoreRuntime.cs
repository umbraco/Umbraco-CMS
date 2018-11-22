using LightInject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Umbraco.Core.Cache;
using Umbraco.Core.Collections;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.CompositionRoots;
using Umbraco.Core.Configuration;
using Umbraco.Core.Exceptions;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Logging.Serilog;
using Umbraco.Core.Migrations.Upgrade;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services.Implement;

namespace Umbraco.Core.Runtime
{
    /// <summary>
    /// Represents the Core Umbraco runtime.
    /// </summary>
    /// <remarks>Does not handle any of the web-related aspects of Umbraco (startup, etc). It
    /// should be possible to use this runtime in console apps.</remarks>
    public class CoreRuntime : IRuntime
    {
        private RuntimeState _state;
        private IUmbracoComponent[] _components;
        private const int LogThresholdMilliseconds = 100;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreRuntime"/> class.
        /// </summary>
        public CoreRuntime()
        { }

        /// <inheritdoc/>
        public virtual void Boot(ServiceContainer container)
        {
            // ensure we have some essential directories
            // every other component can then initialize safely
            IOHelper.EnsurePathExists("~/App_Data");
            IOHelper.EnsurePathExists(SystemDirectories.Media);
            IOHelper.EnsurePathExists(SystemDirectories.MvcViews);
            IOHelper.EnsurePathExists(SystemDirectories.MvcViews + "/Partials");
            IOHelper.EnsurePathExists(SystemDirectories.MvcViews + "/MacroPartials");

            container.ConfigureUmbracoCore(); // also sets Current.Container

            // register the essential stuff,
            // ie the global application logger
            // (profiler etc depend on boot manager)
            var logger = GetLogger();
            container.RegisterInstance(logger);
            // now it is ok to use Current.Logger

            ConfigureUnhandledException(logger);
            ConfigureAssemblyResolve(logger);

            // prepare essential stuff

            var path = GetApplicationRootPath();
            if (string.IsNullOrWhiteSpace(path) == false)
                IOHelper.SetRootDirectory(path);

            _state = (RuntimeState)container.GetInstance<IRuntimeState>();
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
                try
                {
                    Logger.Debug<CoreRuntime>("Runtime: {Runtime}", GetType().FullName);

                    DetermineRuntimeLevel(container);
                    var componentTypes = ResolveComponentTypes();

                    var orderedComponentTypes = PrepareComponentTypes(componentTypes, _state.Level);

                    var components = InstanciateComponents(orderedComponentTypes);
                    ComposeComponents(components, container, _state.Level);

                    using (var scope = container.GetInstance<IScopeProvider>().CreateScope())
                    {
                        InitializeComponents(components, container);
                        scope.Complete();
                    }

                    AquireMainDom(container);

                    _components = components;
                }
                catch (Exception e)
                {
                    _state.Level = RuntimeLevel.BootFailed;
                    var bfe = e as BootFailedException ?? new BootFailedException("Boot failed.", e);
                    _state.BootFailedException = bfe;
                    bootTimer.Fail(exception: bfe); // be sure to log the exception - even if we repeat ourselves

                    // throwing here can cause w3wp to hard-crash and we want to avoid it.
                    // instead, we're logging the exception and setting level to BootFailed.
                    // various parts of Umbraco such as UmbracoModule and UmbracoDefaultOwinStartup
                    // understand this and will nullify themselves, while UmbracoModule will
                    // throw a BootFailedException for every requests.
                }
            }

            //fixme
            // after Umbraco has started there is a scope in "context" and that context is
            // going to stay there and never get destroyed nor reused, so we have to ensure that
            // everything is cleared
            //var sa = container.GetInstance<IDatabaseScopeAccessor>();
            //sa.Scope?.Dispose();
        }

        /// <summary>
        /// Gets a logger.
        /// </summary>
        protected virtual ILogger GetLogger()
        {
            return SerilogLogger.CreateWithDefaultConfiguration();
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
                logger.Error<CoreRuntime>(exception, msg);
            };
        }

        protected virtual void ConfigureAssemblyResolve(ILogger logger)
        {
            // When an assembly can't be resolved. In here we can do magic with the assembly name and try loading another.
            // This is used for loading a signed assembly of AutoMapper (v. 3.1+) without having to recompile old code.
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                // ensure the assembly is indeed AutoMapper and that the PublicKeyToken is null before trying to Load again
                // do NOT just replace this with 'return Assembly', as it will cause an infinite loop -> stackoverflow
                if (args.Name.StartsWith("AutoMapper") && args.Name.EndsWith("PublicKeyToken=null"))
                    return Assembly.Load(args.Name.Replace(", PublicKeyToken=null", ", PublicKeyToken=be96cd2c38ef1005"));
                return null;
            };
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
                    var dbfactory = container.GetInstance<IUmbracoDatabaseFactory>();
                    SetRuntimeStateLevel(dbfactory, Logger);

                    Logger.Debug<CoreRuntime>("Runtime level: {RuntimeLevel}", _state.Level);

                    if (_state.Level == RuntimeLevel.Upgrade)
                    {
                        Logger.Debug<CoreRuntime>("Configure database factory for upgrades.");
                        dbfactory.ConfigureForUpgrade();
                    }
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
            using (ProfilingLogger.DebugDuration<CoreRuntime>($"Terminating. (log components when >{LogThresholdMilliseconds}ms)", "Terminated."))
            {
                for (var i = _components.Length - 1; i >= 0; i--) // terminate components in reverse order
                {
                    var component = _components[i];
                    var componentType = component.GetType();
                    using (ProfilingLogger.DebugDuration<CoreRuntime>($"Terminating {componentType.FullName}.", $"Terminated {componentType.FullName}.", thresholdMilliseconds: LogThresholdMilliseconds))
                    {
                        component.Terminate();
                    }
                }
            }
        }

        private void SetRuntimeStateLevel(IUmbracoDatabaseFactory databaseFactory, ILogger logger)
        {
            var localVersion = UmbracoVersion.LocalVersion; // the local, files, version
            var codeVersion = _state.SemanticVersion; // the executing code version
            var connect = false;

            // we don't know yet
            _state.Level = RuntimeLevel.Unknown;

            if (localVersion == null)
            {
                // there is no local version, we are not installed
                logger.Debug<CoreRuntime>("No local version, need to install Umbraco.");
                _state.Level = RuntimeLevel.Install;
            }
            else if (localVersion < codeVersion)
            {
                // there *is* a local version, but it does not match the code version
                // need to upgrade
                logger.Debug<CoreRuntime>("Local version '{LocalVersion}' < code version '{CodeVersion}', need to upgrade Umbraco.", localVersion, codeVersion);
                _state.Level = RuntimeLevel.Upgrade;
            }
            else if (localVersion > codeVersion)
            {
                logger.Warn<CoreRuntime>("Local version '{LocalVersion}' > code version '{CodeVersion}', downgrading is not supported.", localVersion, codeVersion);
                _state.Level = RuntimeLevel.BootFailed;

                // in fact, this is bad enough that we want to throw
                throw new BootFailedException($"Local version \"{localVersion}\" > code version \"{codeVersion}\", downgrading is not supported.");
            }
            else if (databaseFactory.Configured == false)
            {
                // local version *does* match code version, but the database is not configured
                // install (again? this is a weird situation...)
                logger.Debug<CoreRuntime>("Database is not configured, need to install Umbraco.");
                _state.Level = RuntimeLevel.Install;
            }

            // install? not going to test anything else
            if (_state.Level == RuntimeLevel.Install)
                return;

            // else, keep going,
            // anything other than install wants a database - see if we can connect
            // (since this is an already existing database, assume localdb is ready)
            for (var i = 0; i < 5; i++)
            {
                connect = databaseFactory.CanConnect;
                if (connect) break;
                logger.Debug<CoreRuntime>("Could not immediately connect to database, trying again.");
                Thread.Sleep(1000);
            }

            if (connect == false)
            {
                // cannot connect to configured database, this is bad, fail
                logger.Debug<CoreRuntime>("Could not connect to database.");
                _state.Level = RuntimeLevel.BootFailed;

                // in fact, this is bad enough that we want to throw
                throw new BootFailedException("A connection string is configured but Umbraco could not connect to the database.");
            }

            // if we already know we want to upgrade,
            // still run EnsureUmbracoUpgradeState to get the states
            // (v7 will just get a null state, that's ok)

            // else
            // look for a matching migration entry - bypassing services entirely - they are not 'up' yet
            // fixme - in a LB scenario, ensure that the DB gets upgraded only once!
            bool noUpgrade;
            try
            {
                noUpgrade = EnsureUmbracoUpgradeState(databaseFactory, logger);
            }
            catch (Exception e)
            {
                // can connect to the database but cannot check the upgrade state... oops
                logger.Warn<CoreRuntime>(e, "Could not check the upgrade state.");
                throw new BootFailedException("Could not check the upgrade state.", e);
            }

            if (noUpgrade)
            {
                // the database version matches the code & files version, all clear, can run
                _state.Level = RuntimeLevel.Run;
                return;
            }

            // the db version does not match... but we do have a migration table
            // so, at least one valid table, so we quite probably are installed & need to upgrade

            // although the files version matches the code version, the database version does not
            // which means the local files have been upgraded but not the database - need to upgrade
            logger.Debug<CoreRuntime>("Has not reached the final upgrade step, need to upgrade Umbraco.");
            _state.Level = RuntimeLevel.Upgrade;
        }

        protected virtual bool EnsureUmbracoUpgradeState(IUmbracoDatabaseFactory databaseFactory, ILogger logger)
        {
            var umbracoPlan = new UmbracoPlan();
            var stateValueKey = Upgrader.GetStateValueKey(umbracoPlan);

            // no scope, no service - just directly accessing the database
            using (var database = databaseFactory.CreateDatabase())
            {
                _state.CurrentMigrationState = KeyValueService.GetValue(database, stateValueKey);
                _state.FinalMigrationState = umbracoPlan.FinalState;
            }

            logger.Debug<CoreRuntime>("Final upgrade state is {FinalMigrationState}, database contains {DatabaseState}", _state.FinalMigrationState, _state.CurrentMigrationState ?? "<null>");

            return _state.CurrentMigrationState == _state.FinalMigrationState;
        }

        #region Locals

        protected ILogger Logger { get; private set; }

        protected IProfiler Profiler { get; private set; }

        protected ProfilingLogger ProfilingLogger { get; private set; }

        #endregion

        #region Getters

        // getters can be implemented by runtimes inheriting from CoreRuntime

        // fixme - inject! no Current!
        protected virtual IEnumerable<Type> GetComponentTypes() => Current.TypeLoader.GetTypes<IUmbracoComponent>();

        // by default, returns null, meaning that Umbraco should auto-detect the application root path.
        // override and return the absolute path to the Umbraco site/solution, if needed
        protected virtual string GetApplicationRootPath() => null;

        #endregion

        // stuff from the BootLoader
        private IEnumerable<Type> PrepareComponentTypes(IEnumerable<Type> componentTypes, RuntimeLevel level)
        {
            using (ProfilingLogger.DebugDuration<CoreRuntime>("Preparing component types.", "Prepared component types."))
            {
                // create a list, remove those that cannot be enabled due to runtime level
                var componentTypeList = componentTypes
                    .Where(x =>
                    {
                        // use the min level specified by the attribute if any
                        // otherwise, user components have Run min level, anything else is Unknown (always run)
                        var attr = x.GetCustomAttribute<RuntimeLevelAttribute>();
                        var minLevel = attr?.MinLevel ?? (x.Implements<IUmbracoUserComponent>() ? RuntimeLevel.Run : RuntimeLevel.Unknown);
                        return level >= minLevel;
                    })
                    .ToList();

                // cannot remove that one - ever
                if (componentTypeList.Contains(typeof(UmbracoCoreComponent)) == false)
                    componentTypeList.Add(typeof(UmbracoCoreComponent));

                // enable or disable components
                EnableDisableComponents(componentTypeList);

                // sort the components according to their dependencies
                var requirements = componentTypeList.ToDictionary(key => key, _ => (List<Type>)null);
                foreach (var type in componentTypeList)
                {
                    GatherRequirementsFromRequireAttribute(type, componentTypeList, requirements);
                    GatherRequirementsFromRequiredAttribute(type, componentTypeList, requirements);
                }

                // only for debugging, this is verbose
                //_logger.Debug<BootLoader>(GetComponentsReport(requirements));

                // sort components
                var graph = new TopoGraph<Type, KeyValuePair<Type, List<Type>>>(kvp => kvp.Key, kvp => kvp.Value);
                graph.AddItems(requirements);
                try
                {
                    var sortedComponentTypes = graph.GetSortedItems().Select(x => x.Key).ToList();
                    // bit verbose but should help for troubleshooting
                    Logger.Debug<CoreRuntime>("Ordered Components: {SortedComponentTypes}", sortedComponentTypes);

                    return sortedComponentTypes;
                }
                catch (Exception e)
                {
                    // in case of an error, force-dump everything to log
                    Logger.Info<CoreRuntime>("Component Report:\r\n{ComponentReport}", GetComponentsReport(requirements));
                    Logger.Error<CoreRuntime>(e, "Failed to sort compontents.");
                    throw;
                }

            }
        }

        private static string GetComponentsReport(Dictionary<Type, List<Type>> requirements)
        {
            var text = new StringBuilder();
            text.AppendLine("Components & Dependencies:");
            text.AppendLine();

            foreach (var kvp in requirements)
            {
                var type = kvp.Key;

                text.AppendLine(type.FullName);
                foreach (var attribute in type.GetCustomAttributes<RequireComponentAttribute>())
                    text.AppendLine("  -> " + attribute.RequiredType + (attribute.Weak.HasValue
                        ? (attribute.Weak.Value ? " (weak)" : (" (strong" + (requirements.ContainsKey(attribute.RequiredType) ? ", missing" : "") + ")"))
                        : ""));
                foreach (var attribute in type.GetCustomAttributes<RequiredComponentAttribute>())
                    text.AppendLine("  -< " + attribute.RequiringType);
                foreach (var i in type.GetInterfaces())
                {
                    text.AppendLine("  : " + i.FullName);
                    foreach (var attribute in i.GetCustomAttributes<RequireComponentAttribute>())
                        text.AppendLine("    -> " + attribute.RequiredType + (attribute.Weak.HasValue
                            ? (attribute.Weak.Value ? " (weak)" : (" (strong" + (requirements.ContainsKey(attribute.RequiredType) ? ", missing" : "") + ")"))
                            : ""));
                    foreach (var attribute in i.GetCustomAttributes<RequiredComponentAttribute>())
                        text.AppendLine("    -< " + attribute.RequiringType);
                }
                if (kvp.Value != null)
                    foreach (var t in kvp.Value)
                        text.AppendLine("  = " + t);
                text.AppendLine();
            }
            text.AppendLine("/");
            text.AppendLine();
            return text.ToString();
        }

        private static void EnableDisableComponents(ICollection<Type> types)
        {
            var enabled = new Dictionary<Type, EnableInfo>();

            // process the enable/disable attributes
            // these two attributes are *not* inherited and apply to *classes* only (not interfaces).
            // remote declarations (when a component enables/disables *another* component)
            // have priority over local declarations (when a component disables itself) so that
            // ppl can enable components that, by default, are disabled.
            // what happens in case of conflicting remote declarations is unspecified. more
            // precisely, the last declaration to be processed wins, but the order of the
            // declarations depends on the type finder and is unspecified.
            foreach (var componentType in types)
            {
                foreach (var attr in componentType.GetCustomAttributes<EnableComponentAttribute>())
                {
                    var type = attr.EnabledType ?? componentType;
                    if (enabled.TryGetValue(type, out var enableInfo) == false) enableInfo = enabled[type] = new EnableInfo();
                    var weight = type == componentType ? 1 : 2;
                    if (enableInfo.Weight > weight) continue;

                    enableInfo.Enabled = true;
                    enableInfo.Weight = weight;
                }
                foreach (var attr in componentType.GetCustomAttributes<DisableComponentAttribute>())
                {
                    var type = attr.DisabledType ?? componentType;
                    if (type == typeof(UmbracoCoreComponent)) throw new InvalidOperationException("Cannot disable UmbracoCoreComponent.");
                    if (enabled.TryGetValue(type, out var enableInfo) == false) enableInfo = enabled[type] = new EnableInfo();
                    var weight = type == componentType ? 1 : 2;
                    if (enableInfo.Weight > weight) continue;

                    enableInfo.Enabled = false;
                    enableInfo.Weight = weight;
                }
            }

            // remove components that end up being disabled
            foreach (var kvp in enabled.Where(x => x.Value.Enabled == false))
                types.Remove(kvp.Key);
        }

        private static void GatherRequirementsFromRequireAttribute(Type type, ICollection<Type> types, IDictionary<Type, List<Type>> requirements)
        {
            // get 'require' attributes
            // these attributes are *not* inherited because we want to "custom-inherit" for interfaces only
            var requireAttributes = type
                .GetInterfaces().SelectMany(x => x.GetCustomAttributes<RequireComponentAttribute>()) // those marking interfaces
                .Concat(type.GetCustomAttributes<RequireComponentAttribute>()); // those marking the component

            // what happens in case of conflicting attributes (different strong/weak for same type) is not specified.
            foreach (var attr in requireAttributes)
            {
                if (attr.RequiredType == type) continue; // ignore self-requirements (+ exclude in implems, below)

                // requiring an interface = require any enabled component implementing that interface
                // unless strong, and then require at least one enabled component implementing that interface
                if (attr.RequiredType.IsInterface)
                {
                    var implems = types.Where(x => x != type && attr.RequiredType.IsAssignableFrom(x)).ToList();
                    if (implems.Count > 0)
                    {
                        if (requirements[type] == null) requirements[type] = new List<Type>();
                        requirements[type].AddRange(implems);
                    }
                    else if (attr.Weak == false) // if explicitely set to !weak, is strong, else is weak
                        throw new Exception($"Broken component dependency: {type.FullName} -> {attr.RequiredType.FullName}.");
                }
                // requiring a class = require that the component is enabled
                // unless weak, and then requires it if it is enabled
                else
                {
                    if (types.Contains(attr.RequiredType))
                    {
                        if (requirements[type] == null) requirements[type] = new List<Type>();
                        requirements[type].Add(attr.RequiredType);
                    }
                    else if (attr.Weak != true) // if not explicitely set to weak, is strong
                        throw new Exception($"Broken component dependency: {type.FullName} -> {attr.RequiredType.FullName}.");
                }
            }
        }

        private static void GatherRequirementsFromRequiredAttribute(Type type, ICollection<Type> types, IDictionary<Type, List<Type>> requirements)
        {
            // get 'required' attributes
            // fixme explain
            var requiredAttributes = type
                .GetInterfaces().SelectMany(x => x.GetCustomAttributes<RequiredComponentAttribute>())
                .Concat(type.GetCustomAttributes<RequiredComponentAttribute>());

            foreach (var attr in requiredAttributes)
            {
                if (attr.RequiringType == type) continue; // ignore self-requirements (+ exclude in implems, below)

                if (attr.RequiringType.IsInterface)
                {
                    var implems = types.Where(x => x != type && attr.RequiringType.IsAssignableFrom(x)).ToList();
                    foreach (var implem in implems)
                    {
                        if (requirements[implem] == null) requirements[implem] = new List<Type>();
                        requirements[implem].Add(type);
                    }
                }
                else
                {
                    if (types.Contains(attr.RequiringType))
                    {
                        if (requirements[attr.RequiringType] == null) requirements[attr.RequiringType] = new List<Type>();
                        requirements[attr.RequiringType].Add(type);
                    }
                }
            }
        }

        private IUmbracoComponent[] InstanciateComponents(IEnumerable<Type> types)
        {
            using (ProfilingLogger.DebugDuration<CoreRuntime>("Instanciating components.", "Instanciated components."))
            {
                return types.Select(x => (IUmbracoComponent)Activator.CreateInstance(x)).ToArray();
            }
        }

        private void ComposeComponents(IUmbracoComponent[] components, IServiceContainer container, RuntimeLevel level)
        {
            using (ProfilingLogger.DebugDuration<CoreRuntime>($"Composing components. (log when >{LogThresholdMilliseconds}ms)", "Composed components."))
            {
                var composition = new Composition(container, level);
                foreach (var component in components)
                {
                    var componentType = component.GetType();
                    using (ProfilingLogger.DebugDuration<CoreRuntime>($"Composing {componentType.FullName}.", $"Composed {componentType.FullName}.", thresholdMilliseconds: LogThresholdMilliseconds))
                    {
                        component.Compose(composition);
                    }
                }
            }
        }

        private void InitializeComponents(IUmbracoComponent[] components, IServiceContainer container)
        {
            // use a container scope to ensure that PerScope instances are disposed
            // components that require instances that should not survive should register them with PerScope lifetime
            using (ProfilingLogger.DebugDuration<CoreRuntime>($"Initializing components. (log when >{LogThresholdMilliseconds}ms)", "Initialized components."))
            using (container.BeginScope())
            {
                foreach (var component in components)
                {
                    var componentType = component.GetType();
                    var initializers = componentType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(x => x.Name == "Initialize" && x.IsGenericMethod == false);
                    using (ProfilingLogger.DebugDuration<CoreRuntime>($"Initializing {componentType.FullName}.", $"Initialised {componentType.FullName}.", thresholdMilliseconds: LogThresholdMilliseconds))
                    {
                        foreach (var initializer in initializers)
                        {
                            var parameters = initializer.GetParameters()
                                .Select(x => GetParameter(container, componentType, x.ParameterType))
                                .ToArray();
                            initializer.Invoke(component, parameters);
                        }
                    }
                }
            }
        }

        private object GetParameter(IServiceContainer container, Type componentType, Type parameterType)
        {
            object param;

            try
            {
                param = container.TryGetInstance(parameterType);
            }
            catch (Exception e)
            {
                throw new BootFailedException($"Could not get parameter of type {parameterType.FullName} for component {componentType.FullName}.", e);
            }

            if (param == null) throw new BootFailedException($"Could not get parameter of type {parameterType.FullName} for component {componentType.FullName}.");
            return param;
        }

        private class EnableInfo
        {
            public bool Enabled;
            public int Weight = -1;
        }
    }
}
