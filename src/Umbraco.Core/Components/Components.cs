using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Umbraco.Core.Collections;
using Umbraco.Core.Composing;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Components
{
    // note: this class is NOT thread-safe in any ways

    internal class Components
    {
        private readonly Composition _composition;
        private readonly IProfilingLogger _logger;
        private readonly IEnumerable<Type> _componentTypes;
        private IUmbracoComponent[] _components;

        private const int LogThresholdMilliseconds = 100;

        /// <summary>
        /// Initializes a new instance of the <see cref="Components"/> class.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="componentTypes">The component types.</param>
        /// <param name="logger">A profiling logger.</param>
        public Components(Composition composition, IEnumerable<Type> componentTypes, IProfilingLogger logger)
        {
            _composition = composition ?? throw new ArgumentNullException(nameof(composition));
            _componentTypes = componentTypes ?? throw new ArgumentNullException(nameof(componentTypes));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private class EnableInfo
        {
            public bool Enabled;
            public int Weight = -1;
        }

        public void Compose()
        {
            var orderedComponentTypes = PrepareComponentTypes();

            InstantiateComponents(orderedComponentTypes);
            ComposeComponents();
        }

        public void Initialize()
        {
            using (var scope = _composition.Container.GetInstance<IScopeProvider>().CreateScope())
            {
                InitializeComponents();
                scope.Complete();
            }
        }

        private IEnumerable<Type> PrepareComponentTypes()
        {
            using (_logger.DebugDuration<Components>("Preparing component types.", "Prepared component types."))
            {
                return PrepareComponentTypes2();
            }
        }

        private IEnumerable<Type> PrepareComponentTypes2()
        {
            // create a list, remove those that cannot be enabled due to runtime level
            var componentTypeList = _componentTypes
                .Where(x =>
                {
                    // use the min level specified by the attribute if any
                    // otherwise, user components have Run min level, anything else is Unknown (always run)
                    var attr = x.GetCustomAttribute<RuntimeLevelAttribute>();
                    var minLevel = attr?.MinLevel ?? (x.Implements<IUmbracoUserComponent>() ? RuntimeLevel.Run : RuntimeLevel.Unknown);
                    return _composition.RuntimeLevel >= minLevel;
                })
                .ToList();

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
                _logger.Info<Components>("Component Report:\r\n{ComponentReport}", GetComponentsReport(requirements));
                _logger.Error<Components>(e, "Failed to sort components.");
                throw;
            }

            // bit verbose but should help for troubleshooting
            var text = "Ordered Components: " + Environment.NewLine + string.Join(Environment.NewLine, sortedComponentTypes) + Environment.NewLine;
            Console.WriteLine(text);
            _logger.Debug<Components>("Ordered Components: {SortedComponentTypes}", sortedComponentTypes);

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
                foreach (var attribute in type.GetCustomAttributes<RequiredByComponentAttribute>())
                    text.AppendLine("  -< " + attribute.RequiringType);
                foreach (var i in type.GetInterfaces())
                {
                    text.AppendLine("  : " + i.FullName);
                    foreach (var attribute in i.GetCustomAttributes<RequireComponentAttribute>())
                        text.AppendLine("    -> " + attribute.RequiredType + (attribute.Weak.HasValue
                            ? (attribute.Weak.Value ? " (weak)" : (" (strong" + (requirements.ContainsKey(attribute.RequiredType) ? ", missing" : "") + ")"))
                            : ""));
                    foreach (var attribute in i.GetCustomAttributes<RequiredByComponentAttribute>())
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
                    else if (attr.Weak == false) // if explicitly set to !weak, is strong, else is weak
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
                    else if (attr.Weak != true) // if not explicitly set to weak, is strong
                        throw new Exception($"Broken component dependency: {type.FullName} -> {attr.RequiredType.FullName}.");
                }
            }
        }

        private static void GatherRequirementsFromRequiredAttribute(Type type, ICollection<Type> types, IDictionary<Type, List<Type>> requirements)
        {
            // get 'required' attributes
            // these attributes are *not* inherited because we want to "custom-inherit" for interfaces only
            var requiredAttributes = type
                .GetInterfaces().SelectMany(x => x.GetCustomAttributes<RequiredByComponentAttribute>()) // those marking interfaces
                .Concat(type.GetCustomAttributes<RequiredByComponentAttribute>()); // those marking the component

            foreach (var attr in requiredAttributes)
            {
                if (attr.RequiringType == type) continue; // ignore self-requirements (+ exclude in implems, below)

                // required by an interface = by any enabled component implementing this that interface
                if (attr.RequiringType.IsInterface)
                {
                    var implems = types.Where(x => x != type && attr.RequiringType.IsAssignableFrom(x)).ToList();
                    foreach (var implem in implems)
                    {
                        if (requirements[implem] == null) requirements[implem] = new List<Type>();
                        requirements[implem].Add(type);
                    }
                }
                // required by a class
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

        private void InstantiateComponents(IEnumerable<Type> types)
        {
            IUmbracoComponent InstantiateComponent(Type type)
            {
                var ctor = type.GetConstructor(Array.Empty<Type>());
                if (ctor == null)
                    throw new InvalidOperationException($"Component {type.FullName} does not have a parameter-less.");
                return (IUmbracoComponent) ctor.Invoke(Array.Empty<object>());
            }

            using (_logger.DebugDuration<Components>("Instantiating components.", "Instantiated components."))
            {
                _components = types.Select(InstantiateComponent).ToArray();
            }
        }

        private void ComposeComponents()
        {
            using (_logger.DebugDuration<Components>($"Composing components. (log when >{LogThresholdMilliseconds}ms)", "Composed components."))
            {
                foreach (var component in _components)
                {
                    var componentType = component.GetType();
                    using (_logger.DebugDuration<Components>($"Composing {componentType.FullName}.", $"Composed {componentType.FullName}.", thresholdMilliseconds: LogThresholdMilliseconds))
                    {
                        component.Compose(_composition);
                    }
                }
            }
        }

        private void InitializeComponents()
        {
            // use a container scope to ensure that PerScope instances are disposed
            // components that require instances that should not survive should register them with PerScope lifetime
            using (_logger.DebugDuration<Components>($"Initializing components. (log when >{LogThresholdMilliseconds}ms)", "Initialized components."))
            using (_composition.Container.BeginScope())
            {
                foreach (var component in _components)
                {
                    var componentType = component.GetType();
                    var initializers = componentType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(x => x.Name == "Initialize" && x.IsGenericMethod == false && x.IsStatic == false);
                    using (_logger.DebugDuration<Components>($"Initializing {componentType.FullName}.", $"Initialized {componentType.FullName}.", thresholdMilliseconds: LogThresholdMilliseconds))
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
                param = _composition.Container.TryGetInstance(parameterType);
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
            using (_logger.DebugDuration<Components>($"Terminating. (log components when >{LogThresholdMilliseconds}ms)", "Terminated."))
            {
                for (var i = _components.Length - 1; i >= 0; i--) // terminate components in reverse order
                {
                    var component = _components[i];
                    var componentType = component.GetType();
                    using (_logger.DebugDuration<Components>($"Terminating {componentType.FullName}.", $"Terminated {componentType.FullName}.", thresholdMilliseconds: LogThresholdMilliseconds))
                    {
                        component.Terminate();
                    }
                }
            }
        }
    }
}
