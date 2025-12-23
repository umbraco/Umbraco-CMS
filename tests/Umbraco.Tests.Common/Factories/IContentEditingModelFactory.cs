using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Services;

public interface IContentEditingModelFactory
{
    public Task<ContentUpdateModel> CreateFromAsync(IContent content);
}