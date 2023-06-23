using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IContentUrlFactory
{
    Task<IEnumerable<ContentUrlInfo>> GetUrlsAsync(IContent content);
}
