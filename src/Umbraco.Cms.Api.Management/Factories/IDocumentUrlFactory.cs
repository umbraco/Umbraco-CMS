using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IDocumentUrlFactory
{
    Task<IEnumerable<DocumentUrlInfo>> GetUrlsAsync(IContent content);
}
