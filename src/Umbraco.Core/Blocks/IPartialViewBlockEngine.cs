using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Blocks;

public interface IPartialViewBlockEngine
{
    Task<string> ExecuteAsync(IBlockReference<IPublishedElement, IPublishedElement> blockReference);
}
