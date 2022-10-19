using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Core.Web;

/// <summary>
///     Implements a hybrid <see cref="IUmbracoContextAccessor" />.
/// </summary>
public class HybridUmbracoContextAccessor : HybridAccessorBase<IUmbracoContext>, IUmbracoContextAccessor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="HybridUmbracoContextAccessor" /> class.
    /// </summary>
    public HybridUmbracoContextAccessor(IRequestCache requestCache)
        : base(requestCache)
    {
    }

    /// <summary>
    ///     Tries to get the <see cref="UmbracoContext" /> object.
    /// </summary>
    public bool TryGetUmbracoContext([MaybeNullWhen(false)] out IUmbracoContext umbracoContext)
    {
        umbracoContext = Value;

        return umbracoContext is not null;
    }

    /// <summary>
    ///     Clears the current <see cref="UmbracoContext" /> object.
    /// </summary>
    public void Clear() => Value = null;

    /// <summary>
    ///     Sets the <see cref="UmbracoContext" /> object.
    /// </summary>
    /// <param name="umbracoContext"></param>
    public void Set(IUmbracoContext umbracoContext) => Value = umbracoContext;
}
