using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.PropertyEditors;

public class DataEditorCollection : BuilderCollectionBase<IDataEditor>
{
    public DataEditorCollection(Func<IEnumerable<IDataEditor>> items)
        : base(items)
    {
    }
}
