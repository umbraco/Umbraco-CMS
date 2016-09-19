using Umbraco.Core.Models.PublishedContent;

// ReSharper disable once CheckNamespace
namespace Umbraco.Web.Models
{
    public interface IRenderModel
    {
        IPublishedContent Content { get; }
    }
}
