using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.Mapping;

public abstract class TabsAndPropertiesMapper
{
    protected TabsAndPropertiesMapper(ICultureDictionary cultureDictionary, ILocalizedTextService localizedTextService)
        : this(cultureDictionary, localizedTextService, new List<string>())
    {
    }

    protected TabsAndPropertiesMapper(ICultureDictionary cultureDictionary, ILocalizedTextService localizedTextService, IEnumerable<string> ignoreProperties)
    {
        CultureDictionary = cultureDictionary ?? throw new ArgumentNullException(nameof(cultureDictionary));
        LocalizedTextService = localizedTextService ?? throw new ArgumentNullException(nameof(localizedTextService));
        IgnoreProperties = ignoreProperties ?? throw new ArgumentNullException(nameof(ignoreProperties));
    }

    protected ICultureDictionary CultureDictionary { get; }

    protected ILocalizedTextService LocalizedTextService { get; }

    protected IEnumerable<string?> IgnoreProperties { get; set; }

    /// <summary>
    ///     Returns a collection of custom generic properties that exist on the generic properties tab
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerable<ContentPropertyDisplay> GetCustomGenericProperties(IContentBase content) =>
        Enumerable.Empty<ContentPropertyDisplay>();

    /// <summary>
    ///     Maps properties on to the generic properties tab
    /// </summary>
    /// <param name="content"></param>
    /// <param name="tabs"></param>
    /// <param name="context"></param>
    /// <remarks>
    ///     The generic properties tab is responsible for
    ///     setting up the properties such as Created date, updated date, template selected, etc...
    /// </remarks>
    protected virtual void MapGenericProperties(IContentBase content, List<Tab<ContentPropertyDisplay>> tabs, MapperContext context)
    {
        // add the generic properties tab, for properties that don't belong to a tab
        // get the properties, map and translate them, then add the tab
        var noGroupProperties = content.GetNonGroupedProperties()
            .Where(x => IgnoreProperties.Contains(x.Alias) == false) // skip ignored
            .ToList();
        List<ContentPropertyDisplay> genericProperties = MapProperties(content, noGroupProperties, context);

        IEnumerable<ContentPropertyDisplay> customProperties = GetCustomGenericProperties(content);
        if (customProperties != null)
        {
            genericProperties.AddRange(customProperties);
        }

        if (genericProperties.Count > 0)
        {
            tabs.Add(new Tab<ContentPropertyDisplay>
            {
                Id = 0,
                Label = LocalizedTextService.Localize("general", "properties"),
                Alias = "Generic properties",
                Properties = genericProperties,
            });
        }
    }

    /// <summary>
    ///     Maps a list of <see cref="Property" /> to a list of <see cref="ContentPropertyDisplay" />
    /// </summary>
    /// <param name="content"></param>
    /// <param name="properties"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    protected virtual List<ContentPropertyDisplay> MapProperties(IContentBase content, List<IProperty> properties, MapperContext context) =>
        context.MapEnumerable<IProperty, ContentPropertyDisplay>(properties.OrderBy(x => x.PropertyType?.SortOrder))
            .WhereNotNull().ToList();
}

/// <summary>
///     Creates the tabs collection with properties assigned for display models
/// </summary>
public class TabsAndPropertiesMapper<TSource> : TabsAndPropertiesMapper
    where TSource : IContentBase
{
    private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;

    public TabsAndPropertiesMapper(ICultureDictionary cultureDictionary, ILocalizedTextService localizedTextService, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider)
        : base(cultureDictionary, localizedTextService) =>
        _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider ??
                                          throw new ArgumentNullException(nameof(contentTypeBaseServiceProvider));

    public virtual IEnumerable<Tab<ContentPropertyDisplay>> Map(TSource source, MapperContext context)
    {
        var tabs = new List<Tab<ContentPropertyDisplay>>();

        // Property groups only exist on the content type (as it's only used for display purposes)
        IContentTypeComposition? contentType = _contentTypeBaseServiceProvider.GetContentTypeOf(source);

        // Merge the groups, as compositions can introduce duplicate aliases
        PropertyGroup[]? groups = contentType?.CompositionPropertyGroups.OrderBy(x => x.SortOrder).ToArray();
        var parentAliases = groups?.Select(x => x.GetParentAlias()).Distinct().ToArray();
        if (groups is not null)
        {
            foreach (IGrouping<string, PropertyGroup> groupsByAlias in groups.GroupBy(x => x.Alias))
            {
                var properties = new List<IProperty>();

                // Merge properties for groups with the same alias
                foreach (PropertyGroup group in groupsByAlias)
                {
                    IEnumerable<IProperty> groupProperties = source.GetPropertiesForGroup(group)
                        .Where(x => IgnoreProperties.Contains(x.Alias) == false); // Skip ignored properties

                    properties.AddRange(groupProperties);
                }

                if (properties.Count == 0 && (!parentAliases?.Contains(groupsByAlias.Key) ?? false))
                {
                    continue;
                }

                // Map the properties
                List<ContentPropertyDisplay> mappedProperties = MapProperties(source, properties, context);

                // Add the tab (the first is closest to the content type, e.g. local, then direct composition)
                PropertyGroup g = groupsByAlias.First();

                tabs.Add(new Tab<ContentPropertyDisplay>
                {
                    Id = g.Id,
                    Key = g.Key,
                    Type = g.Type.ToString(),
                    Alias = g.Alias,
                    Label = LocalizedTextService.UmbracoDictionaryTranslate(CultureDictionary, g.Name),
                    Properties = mappedProperties,
                });
            }
        }

        MapGenericProperties(source, tabs, context);

        // Activate the first tab, if any
        if (tabs.Count > 0)
        {
            tabs[0].IsActive = true;
        }

        return tabs;
    }
}
