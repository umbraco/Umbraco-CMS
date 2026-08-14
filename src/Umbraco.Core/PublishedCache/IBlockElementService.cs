using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PublishedCache;

public interface IBlockElementService
{
    Task<IPublishedElement?> BuildElementAsync(BlockItemData blockItemData, bool? preview = null);
}
