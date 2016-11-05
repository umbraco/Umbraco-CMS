using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LightInject;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Components
{
    // note: this class is NOT thread-safe in any ways

    internal class BootLoader
    {
        private readonly IServiceContainer _container;
        private readonly ProfilingLogger _proflog;
        private IUmbracoComponent[] _components;
        private bool _booted;

        private const int LogThresholdMilliseconds = 100;

        /// <summary>
        /// Initializes a new instance of the <see cref="BootLoader"/> class.
        /// </summary>
        /// <param name="container">The application container.</param>
        public BootLoader(IServiceContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            _container = container;
            _proflog = container.GetInstance<ProfilingLogger>();
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
            InitializeComponents();

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

        private static IEnumerable<Type> PrepareComponentTypes2(IEnumerable<Type> componentTypes, RuntimeLevel level)
        {
            // create a list, remove those that cannot be enabled due to runtime level
            var componentTypeList = componentTypes
                .Where(x =>
                {
                    var attr = x.GetCustomAttribute<RuntimeLevelAttribute>();
                    return attr == null || level >= attr.MinLevel;
                })
                .ToList();

            // cannot remove that one - ever
            if (componentTypeList.Contains(typeof(UmbracoCoreComponent)) == false)
                componentTypeList.Add(typeof(UmbracoCoreComponent));

            var enabled = new Dictionary<Type, EnableInfo>();

            // process the enable/disable attributes
            // these two attributes are *not* inherited and apply to *classes* only (not interfaces).
            // remote declarations (when a component enables/disables *another* component)
            // have priority over local declarations (when a component disables itself) so that
            // ppl can enable components that, by default, are disabled.
            // what happens in case of conflicting remote declarations is unspecified. more
            // precisely, the last declaration to be processed wins, but the order of the
            // declarations depends on the type finder and is unspecified.
            foreach (var componentType in componentTypeList)
            {
                foreach (var attr in componentType.GetCustomAttributes<EnableComponentAttribute>())
                {
                    var type = attr.EnabledType ?? componentType;
                    EnableInfo enableInfo;
                    if (enabled.TryGetValue(type, out enableInfo) == false) enableInfo = enabled[type] = new EnableInfo();
                    var weight = type == componentType ? 1 : 2;
                    if (enableInfo.Weight > weight) continue;

                    enableInfo.Enabled = true;
                    enableInfo.Weight = weight;
                }
                foreach (var attr in componentType.GetCustomAttributes<DisableComponentAttribute>())
                {
                    var type = attr.DisabledType ?? componentType;
                    if (type == typeof(UmbracoCoreComponent)) throw new InvalidOperationException("Cannot disable UmbracoCoreComponent.");
                    EnableInfo enableInfo;
                    if (enabled.TryGetValue(type, out enableInfo) == false) enableInfo = enabled[type] = new EnableInfo();
                    var weight = type == componentType ? 1 : 2;
                    if (enableInfo.Weight > weight) continue;

                    enableInfo.Enabled = false;
                    enableInfo.Weight = weight;
                }
            }

            // remove components that end up being disabled
            foreach (var kvp in enabled.Where(x => x.Value.Enabled == false))
                componentTypeList.Remove(kvp.Key);

            // sort the components according to their dependencies
            var items = new List<TopologicalSorter.DependencyField<Type>>();
            var temp = new List<Type>(); // reduce allocs
            foreach (var type in componentTypeList)
            {
                temp.Clear();

                //// for tests
                //Console.WriteLine("Components & Dependencies:");
                //Console.WriteLine(type.FullName);
                //foreach (var attribute in type.GetCustomAttributes<RequireComponentAttribute>())
                //    Console.WriteLine("  -> " + attribute.RequiredType + (attribute.Weak.HasValue ? (attribute.Weak.Value ? " (weak)" : " (strong)") : ""));
                //foreach (var i in type.GetInterfaces())
                //{
                //    Console.WriteLine("  " + i.FullName);
                //    foreach (var attribute in i.GetCustomAttributes<RequireComponentAttribute>())
                //        Console.WriteLine("    -> " + attribute.RequiredType + (attribute.Weak.HasValue ? (attribute.Weak.Value ? " (weak)" : " (strong)") : ""));
                //}
                //Console.WriteLine("/");
                //Console.WriteLine();

                // get attributes
                // these attributes are *not* inherited because we want to "custom-inherit" for interfaces only
                var attributes = type
                    .GetInterfaces().SelectMany(x => x.GetCustomAttributes<RequireComponentAttribute>())
                    .Concat(type.GetCustomAttributes<RequireComponentAttribute>());

                // what happens in case of conflicting attributes (different strong/weak for same type) is not specified.
                foreach (var attr in attributes)
                {
                    // requiring an interface = require any enabled component implementing that interface
                    // unless strong, and then require at least one enabled component implementing that interface
                    if (attr.RequiredType.IsInterface)
                    {
                        var implems = componentTypeList.Where(x => attr.RequiredType.IsAssignableFrom(x)).ToList();
                        if (implems.Count > 0)
                            temp.AddRange(implems);
                        else if (attr.Weak == false) // if explicitely set to !weak, is strong, else is weak
                            throw new Exception($"Broken component dependency: {type.FullName} -> {attr.RequiredType.FullName}.");
                    }
                    // requiring a class = require that the component is enabled
                    // unless weak, and then requires it if it is enabled
                    else
                    {
                        if (componentTypeList.Contains(attr.RequiredType))
                            temp.Add(attr.RequiredType);
                        else if (attr.Weak != true) // if not explicitely set to weak, is strong
                            throw new Exception($"Broken component dependency: {type.FullName} -> {attr.RequiredType.FullName}.");
                    }
                }

                var dependsOn = temp.Distinct().Select(x => x.FullName).ToArray();
                items.Add(new TopologicalSorter.DependencyField<Type>(type.FullName, dependsOn, new Lazy<Type>(() => type)));
            }
            return TopologicalSorter.GetSortedItems(items);
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
            var param = _container.TryGetInstance(parameterType);
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
                foreach (var component in _components)
                {
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
