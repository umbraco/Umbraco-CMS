using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Web.Common;

public interface IUmbracoHelperAccessor
{
    bool TryGetUmbracoHelper([MaybeNullWhen(false)] out UmbracoHelper umbracoHelper);
}
