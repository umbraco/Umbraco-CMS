using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

// FIXME: rename to IContentUrlFactory if we want to prepare media etc. for variations
public interface IDocumentUrlFactory
{
    Task<IEnumerable<DocumentUrlInfo>> GetUrlsAsync(IContent content);
}
