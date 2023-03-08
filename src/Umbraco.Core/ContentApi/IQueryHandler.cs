using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.ContentApi;

public interface IQueryHandler
{
    bool CanHandle(string queryString);
}
