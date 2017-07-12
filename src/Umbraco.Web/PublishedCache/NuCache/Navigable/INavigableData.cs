using System.Collections.Generic;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.PublishedCache.NuCache.Navigable
{
    internal interface INavigableData
    {
        IPublishedContent GetById(bool preview, int contentId);
        IEnumerable<IPublishedContent> GetAtRoot(bool preview);
    }
}
