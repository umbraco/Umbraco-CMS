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
            var trackedRelations = new List<UmbracoEntityReference>();

            foreach (var p in properties)
            {
                if (!propertyEditors.TryGet(p.PropertyType.PropertyEditorAlias, out var editor)) continue;

                //TODO: Support variants/segments! This is not required for this initial prototype which is why there is a check here
                if (!p.PropertyType.VariesByNothing()) continue;
                var val = p.GetValue(); // get the invariant value

                var valueEditor = editor.GetValueEditor();
                if (valueEditor is IDataValueReference reference)
                {
                    var refs = reference.GetReferences(val);
                    trackedRelations.AddRange(refs);
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
                        trackedRelations.AddRange(item.GetDataValueReference().GetReferences(val));
                }
            }

            return trackedRelations;
        }
    }
}
