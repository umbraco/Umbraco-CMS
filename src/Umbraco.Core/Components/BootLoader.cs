using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LightInject;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Components
{
    // note: this class is NOT thread-safe in any ways

    internal class BootLoader
    {
        private readonly ServiceContainer _container;
        private readonly ProfilingLogger _proflog;
        private IUmbracoComponent[] _components;
        private bool _booted;

        private const int LogThresholdMilliseconds = 200;

        /// <summary>
        /// Initializes a new instance of the <see cref="BootLoader"/> class.
        /// </summary>
        /// <param name="container">The application container.</param>
        public BootLoader(ServiceContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            _container = container;
            _proflog = container.GetInstance<ProfilingLogger>();
        }

        // fixme - sort out it all
        // fixme - what about executing when no database? when not configured? see all event handler!
        // rules
        //
        // UmbracoCoreComponent is special and requires every IUmbracoCoreComponent
        // IUmbracoUserComponent is special and requires UmbracoCoreComponent
        //
        // process Enable/Disable for *all* regardless of whether they'll end up being enabled or disabled
        // process Require *only* for those that end up being enabled
        // requiring something that's disabled is going to cause an exception
        //
        // works:
        // gets the list of all discovered components
        // handles dependencies and order via topological graph
        // handle enable/disable (precedence?)
        // OR vice-versa as, if it's disabled, it has no dependency!
        // BUT then the order is pretty much random - bah
        // for each component, run Compose
        // for each component, discover & run Initialize methods
        //
        // should we register components on a clone? (benchmark!)
        // should we get then in a scope => disposed?
        // do we want to keep them around?
        // +
        // what's with ServiceProvider and PluginManager?
        //
        // do we need events?
        // initialize, starting, started
        // become
        // ?, compose, initialize

        private class EnableInfo
        {
            public bool Enabled;
            public int Weight = -1;
        }

        public void Boot(IEnumerable<Type> componentTypes)
        {
            if (_booted) throw new InvalidOperationException("Can not boot, has already booted.");

            using (_proflog.TraceDuration<BootLoader>($"Booting Umbraco {UmbracoVersion.GetSemanticVersion().ToSemanticString()} on {NetworkHelper.MachineName}.", "Booted."))
            {
                var orderedComponentTypes = PrepareComponentTypes(componentTypes);
                InstanciateComponents(orderedComponentTypes);
                ComposeComponents();
                InitializeComponents();
            }

            // rejoice!
            _booted = true;
        }

        private IEnumerable<Type> PrepareComponentTypes(IEnumerable<Type> componentTypes)
        {
            using (_proflog.DebugDuration<BootLoader>("Preparing component types.", "Prepared component types."))
            {
                return PrepareComponentTypes2(componentTypes);
            }
        }

        private static IEnumerable<Type> PrepareComponentTypes2(IEnumerable<Type> componentTypes)
        {
            var componentTypeList = componentTypes.ToList();

            if (componentTypeList.Contains(typeof(UmbracoCoreComponent)) == false)
                componentTypeList.Add(typeof(UmbracoCoreComponent));

            var enabled = new Dictionary<Type, EnableInfo>();

            // process the enable/disable attributes
            // remote declarations (when a component enables/disables *another* component)
            // have priority over local declarations (when a component disables itself) so that
            // ppl can enable components that, by default, are disabled
            // what happens in case of conflicting remote declarations is unspecified. more
            // precisely, the last declaration to be processed wins, but the order of the
            // declarations depends on the type finder and is unspecified
            // we *could* fix this by adding a weight property to both attributes...
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

                // get attributes
                // these attributes are *not* inherited because we want to custom-inherit for interfaces only
                var attributes = type
                    .GetInterfaces().SelectMany(x => x.GetCustomAttributes<RequireComponentAttribute>())
                    .Concat(type.GetCustomAttributes<RequireComponentAttribute>());

                // requiring an interface => require any enabled component implementing that interface
                // requiring a class => require only that class
                foreach (var attr in attributes)
                {
                    if (attr.RequiredType.IsInterface)
                        temp.AddRange(componentTypeList.Where(x => attr.RequiredType.IsAssignableFrom(x)));
                    else
                        temp.Add(attr.RequiredType);
                }

                var dependsOn = temp.Distinct().ToArray();

                // check for broken dependencies
                foreach (var broken in temp.Where(x => componentTypeList.Contains(x) == false))
                    throw new Exception($"Broken component dependency: {type.FullName} -> {broken.FullName}.");

                items.Add(new TopologicalSorter.DependencyField<Type>(type.FullName, dependsOn.Select(x => x.FullName).ToArray(), new Lazy<Type>(() => type)));
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

        private void ComposeComponents()
        {
            using (_proflog.DebugDuration<BootLoader>($"Composing components. (log when >{LogThresholdMilliseconds}ms)", "Composed components."))
            {
                foreach (var component in _components)
                {
                    var componentType = component.GetType();
                    using (_proflog.DebugDuration<BootLoader>($"Composing {componentType.FullName}.", $"Composed {componentType.FullName}.", LogThresholdMilliseconds))
                    {
                        component.Compose(_container);
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
                    var initializers = componentType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                        .Where(x => x.Name == "Initialize" && x.IsGenericMethod == false);
                    using (_proflog.DebugDuration<BootLoader>($"Initializing {componentType.FullName}.", $"Initialised {componentType.FullName}.", LogThresholdMilliseconds))
                    {
                        foreach (var initializer in initializers)
                        {
                            var parameters = initializer.GetParameters()
                                .Select(x => _container.GetInstance(x.ParameterType))
                                .ToArray();
                            initializer.Invoke(component, parameters);
                        }
                    }
                }
            }
        }

        public void Terminate()
        {
            if (_booted == false) throw new InvalidOperationException("Cannot terminate, has not booted.");

            using (_proflog.DebugDuration<BootLoader>($"Terminating Umbraco. (log components when >{LogThresholdMilliseconds}ms)", "Terminated Umbraco."))
            {
                foreach (var component in _components)
                {
                    var componentType = component.GetType();
                    using (_proflog.DebugDuration<BootLoader>($"Terminating {componentType.FullName}.", $"Terminated {componentType.FullName}.", LogThresholdMilliseconds))
                    {
                        component.Terminate();
                    }
                }
            }
        }
    }
}
