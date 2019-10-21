using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    public interface IDataEditorWithMediaPath : IDataEditor
    {
        string GetMediaPath(Property property, string culture = null, string segment = null);
    }
}
