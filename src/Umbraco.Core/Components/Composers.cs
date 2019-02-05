using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Umbraco.Core.Collections;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Components
{
    // note: this class is NOT thread-safe in any ways

    /// <summary>
    /// Handles the composers.
    /// </summary>
    public class Composers
    {
        private readonly Composition _composition;
        private readonly IProfilingLogger _logger;
        private readonly IEnumerable<Type> _composerTypes;

        private const int LogThresholdMilliseconds = 100;

        /// <summary>
        /// Initializes a new instance of the <see cref="Composers"/> class.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="composerTypes">The composer types.</param>
        /// <param name="logger">A profiling logger.</param>
        public Composers(Composition composition, IEnumerable<Type> composerTypes, IProfilingLogger logger)
        {
            _composition = composition ?? throw new ArgumentNullException(nameof(composition));
            _composerTypes = composerTypes ?? throw new ArgumentNullException(nameof(composerTypes));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private class EnableInfo
        {
            public bool Enabled;
            public int Weight = -1;
        }

        /// <summary>
        /// Instantiates and composes the composers.
        /// </summary>
        public void Compose()
        {
            // make sure it is there
            _composition.WithCollectionBuilder<ComponentCollectionBuilder>();

            IEnumerable<Type> orderedComposerTypes;

            using (_logger.DebugDuration<Composers>("Preparing composer types.", "Prepared composer types."))
            {
                orderedComposerTypes = PrepareComposerTypes();
            }

            var composers = InstantiateComposers(orderedComposerTypes);

            using (_logger.DebugDuration<Composers>($"Composing composers. (log when >{LogThresholdMilliseconds}ms)", "Composed composers."))
            {
                foreach (var composer in composers)
                {
                    var componentType = composer.GetType();
                    using (_logger.DebugDuration<Composers>($"Composing {componentType.FullName}.", $"Composed {componentType.FullName}.", thresholdMilliseconds: LogThresholdMilliseconds))
                    {
                        composer.Compose(_composition);
                    }
                }
            }
        }

        private IEnumerable<Type> PrepareComposerTypes()
        {
            // create a list, remove those that cannot be enabled due to runtime level
            var composerTypeList = _composerTypes
                .Where(x =>
                {
                    // use the min/max levels specified by the attribute if any
                    // otherwise, min: user composers are Run, anything else is Unknown (always run)
                    //            max: everything is Run (always run)
                    var attr = x.GetCustomAttribute<RuntimeLevelAttribute>();
                    var minLevel = attr?.MinLevel ?? (x.Implements<IUserComposer>() ? RuntimeLevel.Run : RuntimeLevel.Unknown);
                    var maxLevel = attr?.MaxLevel ?? RuntimeLevel.Run;
                    return _composition.RuntimeState.Level >= minLevel && _composition.RuntimeState.Level <= maxLevel;
                })
                .ToList();

            // enable or disable composers
            EnableDisableComposers(composerTypeList);

            // sort the composers according to their dependencies
            var requirements = new Dictionary<Type, List<Type>>();
            foreach (var type in composerTypeList) requirements[type] = null;
            foreach (var type in composerTypeList)
            {
                GatherRequirementsFromRequireAttribute(type, composerTypeList, requirements);
                GatherRequirementsFromRequiredByAttribute(type, composerTypeList, requirements);
            }

            // only for debugging, this is verbose
            //_logger.Debug<Composers>(GetComposersReport(requirements));

            // sort composers
            var graph = new TopoGraph<Type, KeyValuePair<Type, List<Type>>>(kvp => kvp.Key, kvp => kvp.Value);
            graph.AddItems(requirements);
            List<Type> sortedComposerTypes;
            try
            {
                sortedComposerTypes = graph.GetSortedItems().Select(x => x.Key).ToList();
            }
            catch (Exception e)
            {
                // in case of an error, force-dump everything to log
                _logger.Info<Composers>("Composer Report:\r\n{ComposerReport}", GetComposersReport(requirements));
                _logger.Error<Composers>(e, "Failed to sort composers.");
                throw;
            }

            // bit verbose but should help for troubleshooting
            //var text = "Ordered Composers: " + Environment.NewLine + string.Join(Environment.NewLine, sortedComposerTypes) + Environment.NewLine;
            _logger.Debug<Composers>("Ordered Composers: {SortedComposerTypes}", sortedComposerTypes);

            return sortedComposerTypes;
        }

        private static string GetComposersReport(Dictionary<Type, List<Type>> requirements)
        {
            var text = new StringBuilder();
            text.AppendLine("Composers & Dependencies:");
            text.AppendLine();

            foreach (var kvp in requirements)
            {
                var type = kvp.Key;

                text.AppendLine(type.FullName);
                foreach (var attribute in type.GetCustomAttributes<ComposeAfterAttribute>())
                    text.AppendLine("  -> " + attribute.RequiredType + (attribute.Weak.HasValue
                        ? (attribute.Weak.Value ? " (weak)" : (" (strong" + (requirements.ContainsKey(attribute.RequiredType) ? ", missing" : "") + ")"))
                        : ""));
                foreach (var attribute in type.GetCustomAttributes<ComposeBeforeAttribute>())
                    text.AppendLine("  -< " + attribute.RequiringType);
                foreach (var i in type.GetInterfaces())
                {
                    text.AppendLine("  : " + i.FullName);
                    foreach (var attribute in i.GetCustomAttributes<ComposeAfterAttribute>())
                        text.AppendLine("    -> " + attribute.RequiredType + (attribute.Weak.HasValue
                            ? (attribute.Weak.Value ? " (weak)" : (" (strong" + (requirements.ContainsKey(attribute.RequiredType) ? ", missing" : "") + ")"))
                            : ""));
                    foreach (var attribute in i.GetCustomAttributes<ComposeBeforeAttribute>())
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

        private static void EnableDisableComposers(ICollection<Type> types)
        {
            var enabled = new Dictionary<Type, EnableInfo>();

            // process the enable/disable attributes
            // these two attributes are *not* inherited and apply to *classes* only (not interfaces).
            // remote declarations (when a composer enables/disables *another* composer)
            // have priority over local declarations (when a composer disables itself) so that
            // ppl can enable composers that, by default, are disabled.
            // what happens in case of conflicting remote declarations is unspecified. more
            // precisely, the last declaration to be processed wins, but the order of the
            // declarations depends on the type finder and is unspecified.

            void UpdateEnableInfo(Type composerType, int weight2, Dictionary<Type, EnableInfo> enabled2, bool value)
            {
                if (enabled.TryGetValue(composerType, out var enableInfo) == false) enableInfo = enabled2[composerType] = new EnableInfo();
                if (enableInfo.Weight > weight2) return;

                enableInfo.Enabled = value;
                enableInfo.Weight = weight2;
            }

            var assemblies = types.Select(x => x.Assembly).Distinct();
            foreach (var assembly in assemblies)
            {
                foreach (var attr in assembly.GetCustomAttributes<EnableComposerAttribute>())
                {
                    var type = attr.EnabledType;
                    UpdateEnableInfo(type, 2, enabled, true);
                }

                foreach (var attr in assembly.GetCustomAttributes<DisableComposerAttribute>())
                {
                    var type = attr.DisabledType;
                    UpdateEnableInfo(type, 2, enabled, false);
                }
            }

            foreach (var composerType in types)
            {
                foreach (var attr in composerType.GetCustomAttributes<EnableAttribute>())
                {
                    var type = attr.EnabledType ?? composerType;
                    var weight = type == composerType ? 1 : 3;
                    UpdateEnableInfo(type, weight, enabled, true);
                }

                foreach (var attr in composerType.GetCustomAttributes<DisableAttribute>())
                {
                    var type = attr.DisabledType ?? composerType;
                    var weight = type == composerType ? 1 : 3;
                    UpdateEnableInfo(type, weight, enabled, false);
                }
            }

            // remove composers that end up being disabled
            foreach (var kvp in enabled.Where(x => x.Value.Enabled == false))
                types.Remove(kvp.Key);
        }

        private static void GatherRequirementsFromRequireAttribute(Type type, ICollection<Type> types, IDictionary<Type, List<Type>> requirements)
        {
            // get 'require' attributes
            // these attributes are *not* inherited because we want to "custom-inherit" for interfaces only
            var requireAttributes = type
                .GetInterfaces().SelectMany(x => x.GetCustomAttributes<ComposeAfterAttribute>()) // those marking interfaces
                .Concat(type.GetCustomAttributes<ComposeAfterAttribute>()); // those marking the composer

            // what happens in case of conflicting attributes (different strong/weak for same type) is not specified.
            foreach (var attr in requireAttributes)
            {
                if (attr.RequiredType == type) continue; // ignore self-requirements (+ exclude in implems, below)

                // requiring an interface = require any enabled composer implementing that interface
                // unless strong, and then require at least one enabled composer implementing that interface
                if (attr.RequiredType.IsInterface)
                {
                    var implems = types.Where(x => x != type && attr.RequiredType.IsAssignableFrom(x)).ToList();
                    if (implems.Count > 0)
                    {
                        if (requirements[type] == null) requirements[type] = new List<Type>();
                        requirements[type].AddRange(implems);
                    }
                    else if (attr.Weak == false) // if explicitly set to !weak, is strong, else is weak
                        throw new Exception($"Broken composer dependency: {type.FullName} -> {attr.RequiredType.FullName}.");
                }
                // requiring a class = require that the composer is enabled
                // unless weak, and then requires it if it is enabled
                else
                {
                    if (types.Contains(attr.RequiredType))
                    {
                        if (requirements[type] == null) requirements[type] = new List<Type>();
                        requirements[type].Add(attr.RequiredType);
                    }
                    else if (attr.Weak != true) // if not explicitly set to weak, is strong
                        throw new Exception($"Broken composer dependency: {type.FullName} -> {attr.RequiredType.FullName}.");
                }
            }
        }

        private static void GatherRequirementsFromRequiredByAttribute(Type type, ICollection<Type> types, IDictionary<Type, List<Type>> requirements)
        {
            // get 'required' attributes
            // these attributes are *not* inherited because we want to "custom-inherit" for interfaces only
            var requiredAttributes = type
                .GetInterfaces().SelectMany(x => x.GetCustomAttributes<ComposeBeforeAttribute>()) // those marking interfaces
                .Concat(type.GetCustomAttributes<ComposeBeforeAttribute>()); // those marking the composer

            foreach (var attr in requiredAttributes)
            {
                if (attr.RequiringType == type) continue; // ignore self-requirements (+ exclude in implems, below)

                // required by an interface = by any enabled composer implementing this that interface
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

        private IEnumerable<IComposer> InstantiateComposers(IEnumerable<Type> types)
        {
            IComposer InstantiateComposer(Type type)
            {
                var ctor = type.GetConstructor(Array.Empty<Type>());
                if (ctor == null)
                    throw new InvalidOperationException($"Composer {type.FullName} does not have a parameter-less constructor.");
                return (IComposer) ctor.Invoke(Array.Empty<object>());
            }

            using (_logger.DebugDuration<Composers>("Instantiating composers.", "Instantiated composers."))
            {
                return types.Select(InstantiateComposer).ToArray();
            }
        }
    }
}
