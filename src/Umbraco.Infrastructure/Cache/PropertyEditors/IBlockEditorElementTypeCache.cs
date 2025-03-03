using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Cache.PropertyEditors;

public interface IBlockEditorElementTypeCache
{
    IEnumerable<IContentType> GetMany(IEnumerable<Guid> keys);
    IEnumerable<IContentType> GetAll();
}
