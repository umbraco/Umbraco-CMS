using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;

namespace Umbraco.Cms.Core.PropertyEditors;

public class DataValueReferenceFactoryCollection : BuilderCollectionBase<IDataValueReferenceFactory>
{
    public DataValueReferenceFactoryCollection(Func<IEnumerable<IDataValueReferenceFactory>> items)
        : base(items)
    {
    }

    // TODO: We could further reduce circular dependencies with PropertyEditorCollection by not having IDataValueReference implemented
    // by property editors and instead just use the already built in IDataValueReferenceFactory and/or refactor that into a more normal collection
    public IEnumerable<UmbracoEntityReference> GetAllReferences(
        IPropertyCollection properties,
        PropertyEditorCollection propertyEditors)
    {
        var trackedRelations = new HashSet<UmbracoEntityReference>();

        foreach (IProperty p in properties)
        {
            if (!propertyEditors.TryGet(p.PropertyType.PropertyEditorAlias, out IDataEditor? editor))
            {
                continue;
            }

            // TODO: We will need to change this once we support tracking via variants/segments
            // for now, we are tracking values from ALL variants
            foreach (IPropertyValue propertyVal in p.Values)
            {
                var val = propertyVal.EditedValue;

                IDataValueEditor? valueEditor = editor?.GetValueEditor();
                if (valueEditor is IDataValueReference reference)
                {
                    IEnumerable<UmbracoEntityReference> refs = reference.GetReferences(val);
                    foreach (UmbracoEntityReference r in refs)
                    {
                        trackedRelations.Add(r);
                    }
                }

                // Loop over collection that may be add to existing property editors
                // implementation of GetReferences in IDataValueReference.
                // Allows developers to add support for references by a
                // package /property editor that did not implement IDataValueReference themselves
                foreach (IDataValueReferenceFactory item in this)
                {
                    // Check if this value reference is for this datatype/editor
                    // Then call it's GetReferences method - to see if the value stored
                    // in the dataeditor/property has referecnes to media/content items
                    if (item.IsForEditor(editor))
                    {
                        foreach (UmbracoEntityReference r in item.GetDataValueReference().GetReferences(val))
                        {
                            trackedRelations.Add(r);
                        }
                    }
                }
            }
        }

        return trackedRelations;
    }
}
