using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Core.Composing;

// note: this class is NOT thread-safe in any way

/// <summary>
///     Handles the composers.
/// </summary>
internal class ComposerGraph
{
    private readonly IUmbracoBuilder _builder;
    private readonly IEnumerable<Type> _composerTypes;
    private readonly IEnumerable<Attribute> _enableDisableAttributes;
    private readonly ILogger<ComposerGraph> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ComposerGraph" /> class.
    /// </summary>
    /// <param name="builder">The composition.</param>
    /// <param name="composerTypes">The <see cref="IComposer" /> types.</param>
    /// <param name="enableDisableAttributes">
    ///     The <see cref="EnableComposerAttribute" /> and/or
    ///     <see cref="DisableComposerAttribute" /> attributes.
    /// </param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">
    ///     composition
    ///     or
    ///     composerTypes
    ///     or
    ///     enableDisableAttributes
    ///     or
    ///     logger
    /// </exception>
    public ComposerGraph(IUmbracoBuilder builder, IEnumerable<Type> composerTypes, IEnumerable<Attribute> enableDisableAttributes, ILogger<ComposerGraph> logger)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _composerTypes = composerTypes ?? throw new ArgumentNullException(nameof(composerTypes));
        _enableDisableAttributes =
            enableDisableAttributes ?? throw new ArgumentNullException(nameof(enableDisableAttributes));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Instantiates and composes the composers.
    /// </summary>
    public void Compose()
    {
        // make sure it is there
        _builder.WithCollectionBuilder<ComponentCollectionBuilder>();

        IEnumerable<Type> orderedComposerTypes = PrepareComposerTypes();

        foreach (IComposer composer in InstantiateComposers(orderedComposerTypes))
        {
            composer.Compose(_builder);
        }
    }

    internal static string GetComposersReport(Dictionary<Type, List<Type>?> requirements)
    {
        var text = new StringBuilder();
        text.AppendLine("Composers & Dependencies:");
        text.AppendLine("  <  compose before");
        text.AppendLine("  >  compose after");
        text.AppendLine("  :  implements");
        text.AppendLine("  =  depends");
        text.AppendLine();

        bool HasReq(IEnumerable<Type> types, Type type)
        {
            return types.Any(x => type.IsAssignableFrom(x) && !x.IsInterface);
        }

        foreach (KeyValuePair<Type, List<Type>?> kvp in requirements)
        {
            Type type = kvp.Key;

            text.AppendLine(type.FullName);
            foreach (ComposeAfterAttribute attribute in type.GetCustomAttributes<ComposeAfterAttribute>())
            {
                var weak = !(attribute.RequiredType.IsInterface ? attribute.Weak == false : attribute.Weak != true);
                text.AppendLine("  > " + attribute.RequiredType +
                                (weak ? " (weak" : " (strong") +
                                (HasReq(requirements.Keys, attribute.RequiredType) ? ", found" : ", missing") + ")");
            }

            foreach (ComposeBeforeAttribute attribute in type.GetCustomAttributes<ComposeBeforeAttribute>())
            {
                text.AppendLine("  < " + attribute.RequiringType);
            }

            foreach (Type i in type.GetInterfaces())
            {
                text.AppendLine("  : " + i.FullName);
            }

            if (kvp.Value != null)
            {
                foreach (Type t in kvp.Value)
                {
                    text.AppendLine("  = " + t);
                }
            }

            text.AppendLine();
        }

        text.AppendLine("/");
        text.AppendLine();
        return text.ToString();
    }

    internal IEnumerable<Type> PrepareComposerTypes()
    {
        Dictionary<Type, List<Type>?> requirements = GetRequirements();

        // only for debugging, this is verbose
        // _logger.Debug<ComposerGraph>(GetComposersReport(requirements));
        IEnumerable<Type> sortedComposerTypes = SortComposers(requirements);

        // bit verbose but should help for troubleshooting
        // var text = "Ordered Composers: " + Environment.NewLine + string.Join(Environment.NewLine, sortedComposerTypes) + Environment.NewLine;
        _logger.LogDebug("Ordered Composers: {SortedComposerTypes}", sortedComposerTypes);

        return sortedComposerTypes;
    }

    internal Dictionary<Type, List<Type>?> GetRequirements(bool throwOnMissing = true)
    {
        // create a list, remove those that cannot be enabled due to runtime level
        var composerTypeList = _composerTypes.ToList();

        // enable or disable composers
        EnableDisableComposers(_enableDisableAttributes, composerTypeList);

        static void GatherInterfaces<TAttribute>(Type type, Func<TAttribute, Type> getTypeInAttribute, HashSet<Type> iset, List<Type> set2)
            where TAttribute : Attribute
        {
            foreach (TAttribute attribute in type.GetCustomAttributes<TAttribute>())
            {
                Type typeInAttribute = getTypeInAttribute(attribute);
                if (typeInAttribute != null && // if the attribute references a type ...
                    typeInAttribute.IsInterface && // ... which is an interface ...
                    typeof(IComposer).IsAssignableFrom(typeInAttribute) && // ... which implements IComposer ...
                    !iset.Contains(typeInAttribute)) // ... which is not already in the list
                {
                    // add it to the new list
                    iset.Add(typeInAttribute);
                    set2.Add(typeInAttribute);

                    // add all its interfaces implementing IComposer
                    foreach (Type i in typeInAttribute.GetInterfaces()
                                 .Where(x => typeof(IComposer).IsAssignableFrom(x)))
                    {
                        iset.Add(i);
                        set2.Add(i);
                    }
                }
            }
        }

        // gather interfaces too
        var interfaces = new HashSet<Type>(composerTypeList.SelectMany(x =>
            x.GetInterfaces().Where(y => typeof(IComposer).IsAssignableFrom(y))));
        composerTypeList.AddRange(interfaces);
        List<Type> list1 = composerTypeList;
        while (list1.Count > 0)
        {
            var list2 = new List<Type>();
            foreach (Type t in list1)
            {
                GatherInterfaces<ComposeAfterAttribute>(t, a => a.RequiredType, interfaces, list2);
                GatherInterfaces<ComposeBeforeAttribute>(t, a => a.RequiringType, interfaces, list2);
            }

            composerTypeList.AddRange(list2);
            list1 = list2;
        }

        // sort the composers according to their dependencies
        var requirements = new Dictionary<Type, List<Type>?>();
        foreach (Type type in composerTypeList)
        {
            requirements[type] = null;
        }

        foreach (Type type in composerTypeList)
        {
            GatherRequirementsFromAfterAttribute(type, composerTypeList, requirements, throwOnMissing);
            GatherRequirementsFromBeforeAttribute(type, composerTypeList, requirements);
        }

        return requirements;
    }

    internal IEnumerable<Type> SortComposers(Dictionary<Type, List<Type>?> requirements)
    {
        // sort composers
        var graph = new TopoGraph<Type, KeyValuePair<Type, List<Type>?>>(kvp => kvp.Key, kvp => kvp.Value);
        graph.AddItems(requirements);
        List<Type> sortedComposerTypes;
        try
        {
            sortedComposerTypes = graph.GetSortedItems().Select(x => x.Key).Where(x => !x.IsInterface).ToList();
        }
        catch (Exception e)
        {
            // in case of an error, force-dump everything to log
            _logger.LogInformation("Composer Report:\r\n{ComposerReport}", GetComposersReport(requirements));
            _logger.LogError(e, "Failed to sort composers.");
            throw;
        }

        return sortedComposerTypes;
    }

    private static void EnableDisableComposers(IEnumerable<Attribute> enableDisableAttributes, ICollection<Type> types)
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
            if (enabled.TryGetValue(composerType, out EnableInfo? enableInfo) == false)
            {
                enableInfo = enabled2[composerType] = new EnableInfo();
            }

            if (enableInfo.Weight > weight2)
            {
                return;
            }

            enableInfo.Enabled = value;
            enableInfo.Weight = weight2;
        }

        foreach (EnableComposerAttribute attr in enableDisableAttributes.OfType<EnableComposerAttribute>())
        {
            Type type = attr.EnabledType;
            UpdateEnableInfo(type, 2, enabled, true);
        }

        foreach (DisableComposerAttribute attr in enableDisableAttributes.OfType<DisableComposerAttribute>())
        {
            Type type = attr.DisabledType;
            UpdateEnableInfo(type, 2, enabled, false);
        }

        foreach (Type composerType in types)
        {
            foreach (EnableAttribute attr in composerType.GetCustomAttributes<EnableAttribute>())
            {
                Type type = attr.EnabledType ?? composerType;
                var weight = type == composerType ? 1 : 3;
                UpdateEnableInfo(type, weight, enabled, true);
            }

            foreach (DisableAttribute attr in composerType.GetCustomAttributes<DisableAttribute>())
            {
                Type type = attr.DisabledType ?? composerType;
                var weight = type == composerType ? 1 : 3;
                UpdateEnableInfo(type, weight, enabled, false);
            }
        }

        // remove composers that end up being disabled
        foreach (KeyValuePair<Type, EnableInfo> kvp in enabled.Where(x => x.Value.Enabled == false))
        {
            types.Remove(kvp.Key);
        }
    }

    private static void GatherRequirementsFromAfterAttribute(Type type, ICollection<Type> types, IDictionary<Type, List<Type>?> requirements, bool throwOnMissing = true)
    {
        // get 'require' attributes
        // these attributes are *not* inherited because we want to "custom-inherit" for interfaces only
        IEnumerable<ComposeAfterAttribute> afterAttributes = type
            .GetInterfaces().SelectMany(x => x.GetCustomAttributes<ComposeAfterAttribute>()) // those marking interfaces
            .Concat(type.GetCustomAttributes<ComposeAfterAttribute>()); // those marking the composer

        // what happens in case of conflicting attributes (different strong/weak for same type) is not specified.
        foreach (ComposeAfterAttribute attr in afterAttributes)
        {
            if (attr.RequiredType == type)
            {
                continue; // ignore self-requirements (+ exclude in implems, below)
            }

            // requiring an interface = require any enabled composer implementing that interface
            // unless strong, and then require at least one enabled composer implementing that interface
            if (attr.RequiredType.IsInterface)
            {
                var implems = types.Where(x => x != type && attr.RequiredType.IsAssignableFrom(x) && !x.IsInterface)
                    .ToList();
                if (implems.Count > 0)
                {
                    if (requirements[type] == null)
                    {
                        requirements[type] = new List<Type>();
                    }

                    requirements[type]!.AddRange(implems);
                }

                // if explicitly set to !weak, is strong, else is weak
                else if (attr.Weak == false && throwOnMissing)
                {
                    throw new Exception(
                        $"Broken composer dependency: {type.FullName} -> {attr.RequiredType.FullName}.");
                }
            }

            // requiring a class = require that the composer is enabled
            // unless weak, and then requires it if it is enabled
            else
            {
                if (types.Contains(attr.RequiredType))
                {
                    if (requirements[type] == null)
                    {
                        requirements[type] = new List<Type>();
                    }

                    requirements[type]!.Add(attr.RequiredType);
                }

                // if not explicitly set to weak, is strong
                else if (attr.Weak != true && throwOnMissing)
                {
                    throw new Exception(
                        $"Broken composer dependency: {type.FullName} -> {attr.RequiredType.FullName}.");
                }
            }
        }
    }

    private static void GatherRequirementsFromBeforeAttribute(Type type, ICollection<Type> types, IDictionary<Type, List<Type>?> requirements)
    {
        // get 'required' attributes
        // these attributes are *not* inherited because we want to "custom-inherit" for interfaces only
        IEnumerable<ComposeBeforeAttribute> beforeAttributes = type
            .GetInterfaces()
            .SelectMany(x => x.GetCustomAttributes<ComposeBeforeAttribute>()) // those marking interfaces
            .Concat(type.GetCustomAttributes<ComposeBeforeAttribute>()); // those marking the composer

        foreach (ComposeBeforeAttribute attr in beforeAttributes)
        {
            if (attr.RequiringType == type)
            {
                continue; // ignore self-requirements (+ exclude in implems, below)
            }

            // required by an interface = by any enabled composer implementing this that interface
            if (attr.RequiringType.IsInterface)
            {
                var implems = types.Where(x => x != type && attr.RequiringType.IsAssignableFrom(x) && !x.IsInterface)
                    .ToList();
                foreach (Type implem in implems)
                {
                    if (requirements[implem] == null)
                    {
                        requirements[implem] = new List<Type>();
                    }

                    requirements[implem]!.Add(type);
                }
            }

            // required by a class
            else
            {
                if (types.Contains(attr.RequiringType))
                {
                    if (requirements[attr.RequiringType] == null)
                    {
                        requirements[attr.RequiringType] = new List<Type>();
                    }

                    requirements[attr.RequiringType]!.Add(type);
                }
            }
        }
    }

    private static IEnumerable<IComposer> InstantiateComposers(IEnumerable<Type> types)
    {
        foreach (Type type in types)
        {
            ConstructorInfo? ctor = type.GetConstructor(Array.Empty<Type>());

            if (ctor == null)
            {
                throw new InvalidOperationException(
                    $"Composer {type.FullName} does not have a parameter-less constructor.");
            }

            yield return (IComposer)ctor.Invoke(Array.Empty<object>());
        }
    }

    private class EnableInfo
    {
        public bool Enabled { get; set; }

        public int Weight { get; set; } = -1;
    }
}
