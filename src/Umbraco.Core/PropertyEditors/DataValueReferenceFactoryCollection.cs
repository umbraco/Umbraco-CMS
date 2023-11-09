using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;

namespace Umbraco.Cms.Core.PropertyEditors;

public class DataValueReferenceFactoryCollection : BuilderCollectionBase<IDataValueReferenceFactory>
{
    public DataValueReferenceFactoryCollection(Func<IEnumerable<IDataValueReferenceFactory>> items)
        : base(items)
    { }

    // TODO: We could further reduce circular dependencies with PropertyEditorCollection by not having IDataValueReference implemented
    // by property editors and instead just use the already built in IDataValueReferenceFactory and/or refactor that into a more normal collection
    public ISet<UmbracoEntityReference> GetAllReferences(IPropertyCollection properties, PropertyEditorCollection propertyEditors)
    {
        var references = new HashSet<UmbracoEntityReference>();

        foreach (IProperty property in properties)
        {
            if (!propertyEditors.TryGet(property.PropertyType.PropertyEditorAlias, out IDataEditor? dataEditor))
            {
                continue;
            }

            // TODO: We will need to change this once we support tracking via variants/segments
            // for now, we are tracking values from ALL variants
            foreach (IPropertyValue propertyValue in property.Values)
            {
                object? value = propertyValue.EditedValue;

                if (dataEditor.GetValueEditor() is IDataValueReference dataValueReference)
                {
                    references.UnionWith(dataValueReference.GetReferences(value));
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
                        references.UnionWith(dataValueReferenceFactory.GetDataValueReference().GetReferences(value));
                    }
                }
            }
        }

        return references;
    }

    /// <summary>
    /// Gets all relation type aliases that are automatically tracked.
    /// </summary>
    /// <param name="propertyEditors">The property editors.</param>
    /// <returns>
    /// All relation type aliases that are automatically tracked.
    /// </returns>
    public ISet<string> GetAutomaticRelationTypesAliases(PropertyEditorCollection propertyEditors)
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

    /// <summary>
    /// Gets the relation type aliases that are automatically tracked for all properties.
    /// </summary>
    /// <param name="properties">The properties.</param>
    /// <param name="propertyEditors">The property editors.</param>
    /// <returns>
    /// The relation type aliases that are automatically tracked for all properties.
    /// </returns>
    public ISet<string> GetAutomaticRelationTypesAliases(IPropertyCollection properties, PropertyEditorCollection propertyEditors)
    {
        // Always add default automatic relation types
        var automaticRelationTypeAliases = new HashSet<string>(Constants.Conventions.RelationTypes.AutomaticRelationTypes);

        // Only add relation types that are used in the properties
        foreach (IProperty property in properties)
        {
            if (propertyEditors.TryGet(property.PropertyType.PropertyEditorAlias, out IDataEditor? dataEditor))
            {
                automaticRelationTypeAliases.UnionWith(GetAutomaticRelationTypesAliases(dataEditor));
            }
        }

        return automaticRelationTypeAliases;
    }

    private IEnumerable<string> GetAutomaticRelationTypesAliases(IDataEditor dataEditor)
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
