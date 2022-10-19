using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Core;

/// <summary>
///     Provides a base class for hybrid accessors.
/// </summary>
/// <typeparam name="T">The type of the accessed object.</typeparam>
/// <remarks>
///     <para>
///         Hybrid accessors store the accessed object in HttpContext if they can,
///         otherwise they rely on the logical call context, to maintain an ambient
///         object that flows with async.
///     </para>
/// </remarks>
public abstract class HybridAccessorBase<T>
    where T : class
{
    private static readonly AsyncLocal<T?> AmbientContext = new();

    private readonly IRequestCache _requestCache;
    private string? _itemKey;

    protected HybridAccessorBase(IRequestCache requestCache)
        => _requestCache = requestCache ?? throw new ArgumentNullException(nameof(requestCache));

    protected string ItemKey => _itemKey ??= GetType().FullName!;

    protected T? Value
    {
        get
        {
            if (!_requestCache.IsAvailable)
            {
                return NonContextValue;
            }

            return (T?)_requestCache.Get(ItemKey);
        }

        set
        {
            if (!_requestCache.IsAvailable)
            {
                NonContextValue = value;
            }
            else if (value == null)
            {
                _requestCache.Remove(ItemKey);
            }
            else
            {
                _requestCache.Set(ItemKey, value);
            }
        }
    }

    // read
    // http://blog.stephencleary.com/2013/04/implicit-async-context-asynclocal.html
    // http://stackoverflow.com/questions/14176028/why-does-logicalcallcontext-not-work-with-async
    // http://stackoverflow.com/questions/854976/will-values-in-my-threadstatic-variables-still-be-there-when-cycled-via-threadpo
    // https://msdn.microsoft.com/en-us/library/dd642243.aspx?f=255&MSPPError=-2147217396 ThreadLocal<T>
    // http://stackoverflow.com/questions/29001266/cleaning-up-callcontext-in-tpl clearing call context
    //
    // anything that is ThreadStatic will stay with the thread and NOT flow in async threads
    // the only thing that flows is the logical call context (safe in 4.5+)

    // no!
    // [ThreadStatic]
    // private static T _value;

    // yes! flows with async!
    private T? NonContextValue
    {
        get => AmbientContext.Value ?? default;
        set => AmbientContext.Value = value;
    }
}
