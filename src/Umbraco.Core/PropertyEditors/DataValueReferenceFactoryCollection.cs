using System.Collections.Generic;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;

namespace Umbraco.Core.PropertyEditors
{
    public class DataValueReferenceFactoryCollection : BuilderCollectionBase<IDataValueReferenceFactory>
    {
        public DataValueReferenceFactoryCollection(IEnumerable<IDataValueReferenceFactory> items)
            : base(items)
        { }

        public IEnumerable<UmbracoEntityReference> GetAllReferences(PropertyCollection properties, PropertyEditorCollection propertyEditors)
        {
            var trackedRelations = new HashSet<UmbracoEntityReference>();

            foreach (var p in properties)
            {
                if (!propertyEditors.TryGet(p.PropertyType.PropertyEditorAlias, out var editor)) continue;

                //TODO: We will need to change this once we support tracking via variants/segments
                // for now, we are tracking values from ALL variants

                foreach(var propertyVal in p.Values)
                {
                    var val = propertyVal.EditedValue;

                    var valueEditor = editor.GetValueEditor();
                    if (valueEditor is IDataValueReference reference)
                    {
                        var refs = reference.GetReferences(val);
                        foreach(var r in refs)
                            trackedRelations.Add(r);
                    }

                    // Loop over collection that may be add to existing property editors
                    // implementation of GetReferences in IDataValueReference.
                    // Allows developers to add support for references by a
                    // package /property editor that did not implement IDataValueReference themselves
                    foreach (var item in this)
                    {
                        // Check if this value reference is for this datatype/editor
                        // Then call it's GetReferences method - to see if the value stored
                        // in the dataeditor/property has referecnes to media/content items
                        if (item.IsForEditor(editor))
                        {
                            foreach(var r in item.GetDataValueReference().GetReferences(val))
                                trackedRelations.Add(r);
                        }
                            
                    }
                }

                
            }

            return trackedRelations;
        }
    }
}
