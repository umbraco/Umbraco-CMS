using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core;

/// <summary>
///     Represents a reference to an <see cref="UmbracoContext" /> instance.
/// </summary>
/// <remarks>
///     <para>
///         A reference points to an <see cref="UmbracoContext" /> and it may own it (when it
///         is a root reference) or just reference it. A reference must be disposed after it has
///         been used. Disposing does nothing if the reference is not a root reference. Otherwise,
///         it disposes the <see cref="UmbracoContext" /> and clears the
///         <see cref="IUmbracoContextAccessor" />.
///     </para>
/// </remarks>
public class UmbracoContextReference : IDisposable
{
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private bool _disposedValue;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoContextReference" /> class.
    /// </summary>
    public UmbracoContextReference(IUmbracoContext umbracoContext, bool isRoot, IUmbracoContextAccessor umbracoContextAccessor)
    {
        IsRoot = isRoot;

        UmbracoContext = umbracoContext;
        _umbracoContextAccessor = umbracoContextAccessor;
    }

    /// <summary>
    ///     Gets the <see cref="UmbracoContext" />.
    /// </summary>
    public IUmbracoContext UmbracoContext { get; }

    /// <summary>
    ///     Gets a value indicating whether the reference is a root reference.
    /// </summary>
    public bool IsRoot { get; }

    public void Dispose() => Dispose(true);

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                if (IsRoot)
                {
                    UmbracoContext.Dispose();
                    _umbracoContextAccessor.Clear();
                }
            }

            _disposedValue = true;
        }
    }
}
