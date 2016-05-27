using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Web.PublishedCache.NuCache.Navigable
{
    interface INavigableData
    {
        IPublishedContent GetById(bool preview, int contentId);
        IEnumerable<IPublishedContent> GetAtRoot(bool preview);
    }
}
