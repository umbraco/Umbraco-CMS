using Umbraco.Core.Models;

namespace Umbraco.Web.Models
{
    public interface IRenderModel
    {
        IPublishedContent Content { get; }
    }
}
