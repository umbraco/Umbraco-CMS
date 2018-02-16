using System.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.Manifest;

namespace Umbraco.Core.PropertyEditors
{
    public class PropertyEditorCollection : BuilderCollectionBase<IConfiguredDataEditor>
    {
        public PropertyEditorCollection(DataEditorCollection dataEditors, ManifestParser manifestParser)
            : base(dataEditors
                .Where(x => (x.Type & EditorType.PropertyValue) > 0)
                .Cast<IConfiguredDataEditor>()
                .Union(manifestParser.Manifest.PropertyEditors))
        { }

        public PropertyEditorCollection(DataEditorCollection dataEditors)
            : base(dataEditors
                .Where(x => (x.Type & EditorType.PropertyValue) > 0)
                .Cast<IConfiguredDataEditor>())
        { }

        // note: virtual so it can be mocked
        public virtual IConfiguredDataEditor this[string alias]
            => this.SingleOrDefault(x => x.Alias == alias);
        
        public virtual bool TryGet(string alias, out IConfiguredDataEditor editor)
        {
            editor = this.FirstOrDefault(x => x.Alias == alias);
            return editor != null;
        }
    }
}