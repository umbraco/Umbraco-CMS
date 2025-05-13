using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Provides a builder collection for <see cref="IDataValueReferenceFactory" /> items.
/// </summary>
public class DataValueReferenceFactoryCollection : BuilderCollectionBase<IDataValueReferenceFactory>
{
    // TODO: We could further reduce circular dependencies with PropertyEditorCollection by not having IDataValueReference implemented
    // by property editors and instead just use the already built in IDataValueReferenceFactory and/or refactor that into a more normal collection

    /// <summary>
    /// Initializes a new instance of the <see cref="DataValueReferenceFactoryCollection" /> class.
    /// </summary>
    /// <param name="items">The items.</param>
    public DataValueReferenceFactoryCollection(Func<IEnumerable<IDataValueReferenceFactory>> items)
        : base(items)
    { }

    /// <summary>
    /// Gets all unique references from the specified properties.
    /// </summary>
    /// <param name="properties">The properties.</param>
    /// <param name="propertyEditors">The property editors.</param>
    /// <returns>
    /// The unique references from the specified properties.
    /// </returns>
    public ISet<UmbracoEntityReference> GetAllReferences(IPropertyCollection properties, PropertyEditorCollection propertyEditors)
    {
        var references = new HashSet<UmbracoEntityReference>();

        // Group by property editor alias to avoid duplicate lookups and optimize value parsing
        foreach (var propertyValuesByPropertyEditorAlias in properties.GroupBy(x => x.PropertyType.PropertyEditorAlias, x => x.Values))
        {
            if (!propertyEditors.TryGet(propertyValuesByPropertyEditorAlias.Key, out IDataEditor? dataEditor))
            {
                continue;
            }

            // Use distinct values to avoid duplicate parsing of the same value
            var values = new HashSet<object?>(properties.Count);
            foreach (IPropertyValue propertyValue in propertyValuesByPropertyEditorAlias.SelectMany(x => x))
            {
                values.Add(propertyValue.EditedValue);
                values.Add(propertyValue.PublishedValue);
            }

            references.UnionWith(GetReferences(dataEditor, values));
        }

        return references;
    }

    /// <summary>
    /// Gets the references.
    /// </summary>
    /// <param name="dataEditor">The data editor.</param>
    /// <param name="values">The values.</param>
    /// <returns>
    /// The references.
    /// </returns>
    public IEnumerable<UmbracoEntityReference> GetReferences(IDataEditor dataEditor, params object?[] values)
        => GetReferences(dataEditor, (IEnumerable<object?>)values);

    /// <summary>
    /// Gets the references.
    /// </summary>
    /// <param name="dataEditor">The data editor.</param>
    /// <param name="values">The values.</param>
    /// <returns>
    /// The references.
    /// </returns>
    public ISet<UmbracoEntityReference> GetReferences(IDataEditor dataEditor, IEnumerable<object?> values) =>
        GetReferencesEnumerable(dataEditor, values).ToHashSet();
    private IEnumerable<UmbracoEntityReference> GetReferencesEnumerable(IDataEditor dataEditor, IEnumerable<object?> values)
    {
        // TODO: We will need to change this once we support tracking via variants/segments
        // for now, we are tracking values from ALL variants
        if (dataEditor.GetValueEditor() is IDataValueReference dataValueReference)
        {
            foreach (UmbracoEntityReference reference in values.SelectMany(dataValueReference.GetReferences))
            {
                yield return reference;
            }
        }

        // Loop over collection that may be add to existing property editors
        // implementation of GetReferences in IDataValueReference.
        // Allows developers to add support for references by a
        // package /property editor that did not implement IDataValueReference themselves
        foreach (IDataValueReferenceFactory dataValueReferenceFactory in this)
        {
            // Check if this value reference is for this datatype/editor
            // Then call it's GetReferences method - to see if the value stored
            // in the dataeditor/property has references to media/content items
            if (dataValueReferenceFactory.IsForEditor(dataEditor))
        {
                IDataValueReference factoryDataValueReference = dataValueReferenceFactory.GetDataValueReference();
                foreach (UmbracoEntityReference reference in values.SelectMany(factoryDataValueReference.GetReferences))
            {
                    yield return reference;
                }
            }
        }
    }

    /// <summary>
    /// Gets all relation type aliases that are automatically tracked.
    /// </summary>
    /// <param name="propertyEditors">The property editors.</param>
    /// <returns>
    /// All relation type aliases that are automatically tracked.
    /// </returns>
    [Obsolete("Use GetAllAutomaticRelationTypesAliases. This will be removed in Umbraco 15.")]
    public ISet<string> GetAutomaticRelationTypesAliases(PropertyEditorCollection propertyEditors) =>
        GetAllAutomaticRelationTypesAliases(propertyEditors);
    public ISet<string> GetAllAutomaticRelationTypesAliases(PropertyEditorCollection propertyEditors)
    {
        // Always add default automatic relation types
        var automaticRelationTypeAliases = new HashSet<string>(Constants.Conventions.RelationTypes.AutomaticRelationTypes);

        // Add relation types for all property editors
        foreach (IDataEditor dataEditor in propertyEditors)
        {
            automaticRelationTypeAliases.UnionWith(GetAutomaticRelationTypesAliases(dataEditor));
        }

        return automaticRelationTypeAliases;
    }
    [Obsolete("Use non-obsolete GetAutomaticRelationTypesAliases. This will be removed in Umbraco 15.")]
    public ISet<string> GetAutomaticRelationTypesAliases(IPropertyCollection properties, PropertyEditorCollection propertyEditors)
    {
        // Always add default automatic relation types
        var automaticRelationTypeAliases = new HashSet<string>(Constants.Conventions.RelationTypes.AutomaticRelationTypes);

        // Only add relation types that are used in the properties
        foreach (IProperty property in properties)
        {
            if (propertyEditors.TryGet(property.PropertyType.PropertyEditorAlias, out IDataEditor? dataEditor))
            {
                automaticRelationTypeAliases.UnionWith(GetAutomaticRelationTypesAliasesEnumerable(dataEditor));
            }
        }

        return automaticRelationTypeAliases;
    }

    /// <summary>
    /// Gets the automatic relation types aliases.
    /// </summary>
    /// <param name="dataEditor">The data editor.</param>
    /// <returns>
    /// The automatic relation types aliases.
    /// </returns>
    public ISet<string> GetAutomaticRelationTypesAliases(IDataEditor dataEditor) =>
        GetAutomaticRelationTypesAliasesEnumerable(dataEditor).ToHashSet();
    private IEnumerable<string> GetAutomaticRelationTypesAliasesEnumerable(IDataEditor dataEditor)
    {
        if (dataEditor.GetValueEditor() is IDataValueReference dataValueReference)
        {
            // Return custom relation types from value editor implementation
            foreach (var alias in dataValueReference.GetAutomaticRelationTypesAliases())
            {
                yield return alias;
            }
        }

        foreach (IDataValueReferenceFactory dataValueReferenceFactory in this)
        {
            if (dataValueReferenceFactory.IsForEditor(dataEditor))
            {
                // Return custom relation types from factory
                foreach (var alias in dataValueReferenceFactory.GetDataValueReference().GetAutomaticRelationTypesAliases())
                {
                    yield return alias;
                }
            }
        }
    }
}
