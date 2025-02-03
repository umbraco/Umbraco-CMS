using Umbraco.Cms.Core.Persistence.Repositories;

namespace Umbraco.Cms.Core.Cache;

public class DictionaryNullValueCachePolicyResolver: INullValueCachePolicyResolver<IDictionaryRepository>
{
    public string GetNullValue() => "Umbraco.Cms.Core.Cache.DefaultRepositoryCachePolicy.NullRepresentationInCache";
}
