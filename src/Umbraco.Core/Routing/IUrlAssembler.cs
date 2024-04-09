using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Routing;

public interface IUrlAssembler
{
    Uri AssembleUrl(string path, Uri current, UrlMode mode);
}
