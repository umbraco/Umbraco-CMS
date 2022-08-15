using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Manifest;

namespace Umbraco.Cms.Core.PropertyEditors;

public class ParameterEditorCollection : BuilderCollectionBase<IDataEditor>
{
    public ParameterEditorCollection(DataEditorCollection dataEditors, IManifestParser manifestParser)
        : base(() => dataEditors
            .Where(x => (x.Type & EditorType.MacroParameter) > 0)
            .Union(manifestParser.CombinedManifest.PropertyEditors))
    {
    }

    // note: virtual so it can be mocked
    public virtual IDataEditor? this[string alias]
        => this.SingleOrDefault(x => x.Alias == alias);

    public virtual bool TryGet(string alias, out IDataEditor? editor)
    {
        editor = this.FirstOrDefault(x => x.Alias == alias);
        return editor != null;
    }
}
