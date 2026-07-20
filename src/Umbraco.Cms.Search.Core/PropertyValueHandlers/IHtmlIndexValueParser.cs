using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

public interface IHtmlIndexValueParser
{
    IndexValue? Parse(string html);
}
