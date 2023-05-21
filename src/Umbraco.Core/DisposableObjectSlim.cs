namespace Umbraco.Cms.Core;

/// <summary>
///     Abstract implementation of managed IDisposable.
/// </summary>
/// <remarks>
///     This is for objects that do NOT have unmanaged resources.
///     Can also be used as a pattern for when inheriting is not possible.
///     See also: https://msdn.microsoft.com/en-us/library/b1yfkh5e%28v=vs.110%29.aspx
///     See also: https://lostechies.com/chrispatterson/2012/11/29/idisposable-done-right/
///     Note: if an object's ctor throws, it will never be disposed, and so if that ctor
///     has allocated disposable objects, it should take care of disposing them.
/// </remarks>
public abstract class DisposableObjectSlim : IDisposable
{
    /// <summary>
    ///     Gets a value indicating whether this instance is disposed.
    /// </summary>
    /// <remarks>
    ///     for internal tests only (not thread safe)
    /// </remarks>
    public bool Disposed { get; private set; }

    /// <inheritdoc />
#pragma warning disable CA1063 // Implement IDisposable Correctly
    public void Dispose() => Dispose(true); // We do not use GC.SuppressFinalize because this has no finalizer
#pragma warning restore CA1063 // Implement IDisposable Correctly

    /// <summary>
    ///     Disposes managed resources
    /// </summary>
    protected abstract void DisposeResources();

    /// <summary>
    ///     Disposes managed resources
    /// </summary>
    /// <param name="disposing">True if disposing via Dispose method and not a finalizer. Always true for this class.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!Disposed)
        {
            if (disposing)
            {
                DisposeResources();
            }

            Disposed = true;
        }
    }
}
