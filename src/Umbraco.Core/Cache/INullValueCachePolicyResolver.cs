namespace Umbraco.Cms.Core.Cache;

public interface INullValueCachePolicyResolver<TDictionaryRepository>
{
    public string? GetNullValue();
}
