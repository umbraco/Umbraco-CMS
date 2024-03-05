using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IMediaUrlFactory
{
    IEnumerable<MediaUrlInfo> CreateUrls(IMedia media);
    IEnumerable<MediaUrlInfoResourceSet> CreateUrlSets(IEnumerable<IMedia> mediaItems);
}
