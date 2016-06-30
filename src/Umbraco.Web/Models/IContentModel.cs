using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Models
{
    public interface IContentModel
    {
        IPublishedContent Content { get; }
    }
}
