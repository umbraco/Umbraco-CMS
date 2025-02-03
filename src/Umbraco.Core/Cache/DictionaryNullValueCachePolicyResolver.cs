namespace Umbraco.Cms.Core.Cache;

public class DictionaryNullValueCachePolicyResolver<TDictionaryRepository> : INullValueCachePolicyResolver<TDictionaryRepository>
{
    public string GetNullValue() => "Umbraco.Cms.Core.Cache.DefaultRepositoryCachePolicy.NullRepresentationInCache";
}
