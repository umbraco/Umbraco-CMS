using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Core.Services;

// TODO (V18): Move interface and and implementation to Umbraco.Cms.Core.Models.Factories.

public interface IContentEditingModelFactory
{
    Task<ContentUpdateModel> CreateFromAsync(IContent content);
}
