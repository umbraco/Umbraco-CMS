using System.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.Manifest;

namespace Umbraco.Core.PropertyEditors
{
    public class PropertyEditorCollection : BuilderCollectionBase<IDataEditor>
    {
        public PropertyEditorCollection(DataEditorCollection dataEditors, ManifestParser manifestParser)
            : base(dataEditors
                .Where(x => (x.Type & EditorType.PropertyValue) > 0)
                .Union(manifestParser.Manifest.PropertyEditors))
        { }

        public PropertyEditorCollection(DataEditorCollection dataEditors)
            : base(dataEditors
                .Where(x => (x.Type & EditorType.PropertyValue) > 0))
        { }

        // note: virtual so it can be mocked
        public virtual IDataEditor this[string alias]
            => this.SingleOrDefault(x => x.Alias == alias);
        
        public virtual bool TryGet(string alias, out IDataEditor editor)
        {
            editor = this.FirstOrDefault(x => x.Alias == alias);
            return editor != null;
        }
    }
}