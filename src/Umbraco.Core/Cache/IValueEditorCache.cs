using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Cache;

public interface IValueEditorCache
{
    public IDataValueEditor GetValueEditor(IDataEditor dataEditor, IDataType dataType);

    public void ClearCache(IEnumerable<int> dataTypeIds);
}
