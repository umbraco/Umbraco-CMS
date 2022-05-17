using System.Runtime.Serialization;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents an abstract class for composition specific ContentType properties and methods
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public abstract class ContentTypeCompositionBase : ContentTypeBase, IContentTypeComposition
{
    private List<IContentTypeComposition> _contentTypeComposition = new();
    private List<int> _removedContentTypeKeyTracker = new();

    protected ContentTypeCompositionBase(IShortStringHelper shortStringHelper, int parentId)
        : base(shortStringHelper, parentId)
    {
    }

    protected ContentTypeCompositionBase(IShortStringHelper shortStringHelper, IContentTypeComposition parent)
        : this(shortStringHelper, parent, string.Empty)
    {
    }

    protected ContentTypeCompositionBase(IShortStringHelper shortStringHelper, IContentTypeComposition parent, string alias)
        : base(shortStringHelper, parent, alias) =>
        AddContentType(parent);

    public IEnumerable<int> RemovedContentTypes => _removedContentTypeKeyTracker;

    /// <summary>
    ///     Gets or sets the content types that compose this content type.
    /// </summary>
    [DataMember]
    public IEnumerable<IContentTypeComposition> ContentTypeComposition
    {
        get => _contentTypeComposition;
        set
        {
            _contentTypeComposition = value.ToList();
            OnPropertyChanged(nameof(ContentTypeComposition));
        }
    }

    /// <inheritdoc />
    [IgnoreDataMember]
    public IEnumerable<PropertyGroup> CompositionPropertyGroups
    {
        get
        {
            // we need to "acquire" composition groups and properties here, ie get our own clones,
            // so that we can change their variation according to this content type variations.
            //
            // it would be nice to cache the resulting enumerable, but alas we cannot, otherwise
            // any change to compositions are ignored and that breaks many things - and tracking
            // changes to refresh the cache would be expensive.
            void AcquireProperty(IPropertyType propertyType)
            {
                propertyType.Variations &= Variations;
                propertyType.ResetDirtyProperties(false);
            }

            return PropertyGroups.Union(ContentTypeComposition.SelectMany(x => x.CompositionPropertyGroups)
                .Select(group =>
                {
                    group = (PropertyGroup)group.DeepClone();
                    if (group.PropertyTypes is not null)
                    {
                        foreach (IPropertyType property in group.PropertyTypes)
                        {
                            AcquireProperty(property);
                        }
                    }

                    return group;
                }));
        }
    }

    /// <inheritdoc />
    [IgnoreDataMember]
    public IEnumerable<IPropertyType> CompositionPropertyTypes
    {
        get
        {
            // we need to "acquire" composition properties here, ie get our own clones,
            // so that we can change their variation according to this content type variations.
            //
            // see note in CompositionPropertyGroups for comments on caching the resulting enumerable
            IPropertyType AcquireProperty(IPropertyType propertyType)
            {
                propertyType = (IPropertyType)propertyType.DeepClone();
                propertyType.Variations &= Variations;
                propertyType.ResetDirtyProperties(false);
                return propertyType;
            }

            return ContentTypeComposition
                .SelectMany(x => x.CompositionPropertyTypes)
                .Select(AcquireProperty)
                .Union(PropertyTypes);
        }
    }

    /// <inheritdoc />
    public IEnumerable<IPropertyType> GetOriginalComposedPropertyTypes() => GetRawComposedPropertyTypes();

    /// <summary>
    ///     Adds a content type to the composition.
    /// </summary>
    /// <param name="contentType">The content type to add.</param>
    /// <returns>True if the content type was added, otherwise false.</returns>
    public bool AddContentType(IContentTypeComposition? contentType)
    {
        if (contentType is null)
        {
            return false;
        }

        if (contentType.ContentTypeComposition.Any(x => x.CompositionAliases().Any(ContentTypeCompositionExists)))
        {
            return false;
        }

        if (string.IsNullOrEmpty(Alias) == false && Alias.Equals(contentType.Alias))
        {
            return false;
        }

        if (ContentTypeCompositionExists(contentType.Alias) == false)
        {
            // Before we actually go ahead and add the ContentType as a Composition we ensure that we don't
            // end up with duplicate PropertyType aliases - in which case we throw an exception.
            var conflictingPropertyTypeAliases = CompositionPropertyTypes.SelectMany(
                x => contentType.CompositionPropertyTypes
                    .Where(y => y.Alias.Equals(x.Alias, StringComparison.InvariantCultureIgnoreCase))
                    .Select(p => p.Alias)).ToList();

            if (conflictingPropertyTypeAliases.Any())
            {
                throw new InvalidCompositionException(Alias, contentType.Alias, conflictingPropertyTypeAliases.ToArray());
            }

            _contentTypeComposition.Add(contentType);

            OnPropertyChanged(nameof(ContentTypeComposition));

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Removes a content type with a specified alias from the composition.
    /// </summary>
    /// <param name="alias">The alias of the content type to remove.</param>
    /// <returns>True if the content type was removed, otherwise false.</returns>
    public bool RemoveContentType(string alias)
    {
        if (ContentTypeCompositionExists(alias))
        {
            IContentTypeComposition? contentTypeComposition = ContentTypeComposition.FirstOrDefault(x => x.Alias == alias);

            // You can't remove a composition from another composition
            if (contentTypeComposition == null)
            {
                return false;
            }

            _removedContentTypeKeyTracker.Add(contentTypeComposition.Id);

            // If the ContentType we are removing has Compositions of its own these needs to be removed as well
            var compositionIdsToRemove = contentTypeComposition.CompositionIds().ToList();
            if (compositionIdsToRemove.Any())
            {
                _removedContentTypeKeyTracker.AddRange(compositionIdsToRemove);
            }

            OnPropertyChanged(nameof(ContentTypeComposition));

            return _contentTypeComposition.Remove(contentTypeComposition);
        }

        return false;
    }

    /// <summary>
    ///     Checks if a ContentType with the supplied alias exists in the list of composite ContentTypes
    /// </summary>
    /// <param name="alias">Alias of a <see cref="ContentType" /></param>
    /// <returns>True if ContentType with alias exists, otherwise returns False</returns>
    public bool ContentTypeCompositionExists(string alias)
    {
        if (ContentTypeComposition.Any(x => x.Alias.Equals(alias)))
        {
            return true;
        }

        if (ContentTypeComposition.Any(x => x.ContentTypeCompositionExists(alias)))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Checks whether a PropertyType with a given alias already exists
    /// </summary>
    /// <param name="alias">Alias of the PropertyType</param>
    /// <returns>Returns <c>True</c> if a PropertyType with the passed in alias exists, otherwise <c>False</c></returns>
    public override bool PropertyTypeExists(string? alias) => CompositionPropertyTypes.Any(x => x.Alias == alias);

    /// <inheritdoc />
    public override bool AddPropertyGroup(string alias, string name) => AddAndReturnPropertyGroup(alias, name) != null;

    /// <inheritdoc />
    public override bool AddPropertyType(IPropertyType propertyType, string propertyGroupAlias, string? propertyGroupName = null)
    {
        // ensure no duplicate alias - over all composition properties
        if (PropertyTypeExists(propertyType.Alias))
        {
            return false;
        }

        // get and ensure a group local to this content type
        PropertyGroup? group;
        var index = PropertyGroups.IndexOfKey(propertyGroupAlias);
        if (index != -1)
        {
            group = PropertyGroups[index];
        }
        else if (!string.IsNullOrEmpty(propertyGroupName))
        {
            group = AddAndReturnPropertyGroup(propertyGroupAlias, propertyGroupName);
            if (group == null)
            {
                return false;
            }
        }
        else
        {
            // No group name specified, so we can't create a new one and add the property type
            return false;
        }

        // add property to group
        propertyType.PropertyGroupId = new Lazy<int>(() => group.Id);
        group.PropertyTypes?.Add(propertyType);

        return true;
    }

    /// <summary>
    ///     Gets a list of ContentType aliases from the current composition
    /// </summary>
    /// <returns>An enumerable list of string aliases</returns>
    /// <remarks>Does not contain the alias of the Current ContentType</remarks>
    public IEnumerable<string> CompositionAliases()
        => ContentTypeComposition
            .Select(x => x.Alias)
            .Union(ContentTypeComposition.SelectMany(x => x.CompositionAliases()));

    /// <summary>
    ///     Gets a list of ContentType Ids from the current composition
    /// </summary>
    /// <returns>An enumerable list of integer ids</returns>
    /// <remarks>Does not contain the Id of the Current ContentType</remarks>
    public IEnumerable<int> CompositionIds()
        => ContentTypeComposition
            .Select(x => x.Id)
            .Union(ContentTypeComposition.SelectMany(x => x.CompositionIds()));

    protected override void PerformDeepClone(object clone)
    {
        base.PerformDeepClone(clone);

        var clonedEntity = (ContentTypeCompositionBase)clone;

        // need to manually assign since this is an internal field and will not be automatically mapped
        clonedEntity._removedContentTypeKeyTracker = new List<int>();
        clonedEntity._contentTypeComposition =
            ContentTypeComposition.Select(x => (IContentTypeComposition)x.DeepClone()).ToList();
    }

    private IEnumerable<IPropertyType> GetRawComposedPropertyTypes(bool start = true)
    {
        IEnumerable<IPropertyType> propertyTypes = ContentTypeComposition
            .Cast<ContentTypeCompositionBase>()
            .SelectMany(x => start ? x.GetRawComposedPropertyTypes(false) : x.CompositionPropertyTypes);

        if (!start)
        {
            propertyTypes = propertyTypes.Union(PropertyTypes);
        }

        return propertyTypes;
    }

    private PropertyGroup? AddAndReturnPropertyGroup(string alias, string name)
    {
        // Ensure we don't have it already
        if (PropertyGroups.Contains(alias))
        {
            return null;
        }

        // Add new group
        var group = new PropertyGroup(SupportsPublishing) { Alias = alias, Name = name };

        // check if it is inherited - there might be more than 1 but we want the 1st, to
        // reuse its sort order - if there are more than 1 and they have different sort
        // orders... there isn't much we can do anyways
        PropertyGroup? inheritGroup = CompositionPropertyGroups.FirstOrDefault(x => x.Alias == alias);
        if (inheritGroup == null)
        {
            // no, just local, set sort order
            PropertyGroup? lastGroup = PropertyGroups.LastOrDefault();
            if (lastGroup != null)
            {
                group.SortOrder = lastGroup.SortOrder + 1;
            }
        }
        else
        {
            // yes, inherited, re-use sort order
            group.SortOrder = inheritGroup.SortOrder;
        }

        // add
        PropertyGroups.Add(group);

        return group;
    }
}
