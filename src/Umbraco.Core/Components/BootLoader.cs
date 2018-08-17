using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using LightInject;
using Umbraco.Core.Collections;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Components
{
    // note: this class is NOT thread-safe in any ways

    internal class BootLoader
    {
        private readonly IServiceContainer _container;
        private readonly ProfilingLogger _proflog;
        private readonly ILogger _logger;
        private IUmbracoComponent[] _components;
        private bool _booted;

        private const int LogThresholdMilliseconds = 100;

        /// <summary>
        /// Initializes a new instance of the <see cref="BootLoader"/> class.
        /// </summary>
        /// <param name="container">The application container.</param>
        public BootLoader(IServiceContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _proflog = container.GetInstance<ProfilingLogger>();
            _logger = container.GetInstance<ILogger>();
        }

        private class EnableInfo
        {
            public bool Enabled;
            public int Weight = -1;
        }

        public void Boot(IEnumerable<Type> componentTypes, RuntimeLevel level)
        {
            if (_booted) throw new InvalidOperationException("Can not boot, has already booted.");

            var orderedComponentTypes = PrepareComponentTypes(componentTypes, level);

            InstanciateComponents(orderedComponentTypes);
            ComposeComponents(level);

            using (var scope = _container.GetInstance<IScopeProvider>().CreateScope())
            {
                InitializeComponents();
                scope.Complete();
            }

            // rejoice!
            _booted = true;
        }

        private IEnumerable<Type> PrepareComponentTypes(IEnumerable<Type> componentTypes, RuntimeLevel level)
        {
            using (_proflog.DebugDuration<BootLoader>("Preparing component types.", "Prepared component types."))
            {
                return PrepareComponentTypes2(componentTypes, level);
            }
        }

        private IEnumerable<Type> PrepareComponentTypes2(IEnumerable<Type> componentTypes, RuntimeLevel level)
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
            var requirements = new Dictionary<Type, List<Type>>();
            foreach (var type in componentTypeList) requirements[type] = null;
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
            List<Type> sortedComponentTypes;
            try
            {
                sortedComponentTypes = graph.GetSortedItems().Select(x => x.Key).ToList();
            }
            catch (Exception e)
            {
                // in case of an error, force-dump everything to log
                _logger.Info<BootLoader>("Component Report:\r\n{ComponentReport}", GetComponentsReport(requirements));
                _logger.Error<BootLoader>(e, "Failed to sort compontents.");
                throw;
            }

            // bit verbose but should help for troubleshooting
            var text = "Ordered Components: " + Environment.NewLine + string.Join(Environment.NewLine, sortedComponentTypes) + Environment.NewLine;
            Console.WriteLine(text);
            _logger.Debug<BootLoader>("Ordered Components: {SortedComponentTypes}", sortedComponentTypes);

            return sortedComponentTypes;
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

        private void InstanciateComponents(IEnumerable<Type> types)
        {
            using (_proflog.DebugDuration<BootLoader>("Instanciating components.", "Instanciated components."))
            {
                _components = types.Select(x => (IUmbracoComponent) Activator.CreateInstance(x)).ToArray();
            }
        }

        private void ComposeComponents(RuntimeLevel level)
        {
            using (_proflog.DebugDuration<BootLoader>($"Composing components. (log when >{LogThresholdMilliseconds}ms)", "Composed components."))
            {
                var composition = new Composition(_container, level);
                foreach (var component in _components)
                {
                    var componentType = component.GetType();
                    using (_proflog.DebugDuration<BootLoader>($"Composing {componentType.FullName}.", $"Composed {componentType.FullName}.", thresholdMilliseconds: LogThresholdMilliseconds))
                    {
                        component.Compose(composition);
                    }
                }
            }
        }

        private void InitializeComponents()
        {
            // use a container scope to ensure that PerScope instances are disposed
            // components that require instances that should not survive should register them with PerScope lifetime
            using (_proflog.DebugDuration<BootLoader>($"Initializing components. (log when >{LogThresholdMilliseconds}ms)", "Initialized components."))
            using (_container.BeginScope())
            {
                foreach (var component in _components)
                {
                    var componentType = component.GetType();
                    var initializers = componentType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(x => x.Name == "Initialize" && x.IsGenericMethod == false);
                    using (_proflog.DebugDuration<BootLoader>($"Initializing {componentType.FullName}.", $"Initialised {componentType.FullName}.", thresholdMilliseconds: LogThresholdMilliseconds))
                    {
                        foreach (var initializer in initializers)
                        {
                            var parameters = initializer.GetParameters()
                                .Select(x => GetParameter(componentType, x.ParameterType))
                                .ToArray();
                            initializer.Invoke(component, parameters);
                        }
                    }
                }
            }
        }

        private object GetParameter(Type componentType, Type parameterType)
        {
            object param;

            try
            {
                param = _container.TryGetInstance(parameterType);
            }
            catch (Exception e)
            {
                throw new BootFailedException($"Could not get parameter of type {parameterType.FullName} for component {componentType.FullName}.", e);
            }

            if (param == null) throw new BootFailedException($"Could not get parameter of type {parameterType.FullName} for component {componentType.FullName}.");
            return param;
        }

        public void Terminate()
        {
            if (_booted == false)
            {
                _proflog.Logger.Warn<BootLoader>("Cannot terminate, has not booted.");
                return;
            }

            using (_proflog.DebugDuration<BootLoader>($"Terminating. (log components when >{LogThresholdMilliseconds}ms)", "Terminated."))
            {
                for (var i = _components.Length - 1; i >= 0; i--) // terminate components in reverse order
                {
                    var component = _components[i];
                    var componentType = component.GetType();
                    using (_proflog.DebugDuration<BootLoader>($"Terminating {componentType.FullName}.", $"Terminated {componentType.FullName}.", thresholdMilliseconds: LogThresholdMilliseconds))
                    {
                        component.Terminate();
                    }
                }
            }
        }
    }
}
