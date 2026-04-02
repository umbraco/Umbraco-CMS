namespace Umbraco.Cms.Core.Diagnostics;

/// <summary>
/// A no-operation implementation of <see cref="IMarchal"/> that returns null pointers.
/// </summary>
/// <remarks>
/// This implementation is used on platforms where minidump functionality is not supported.
/// </remarks>
internal sealed class NoopMarchal : IMarchal
{
    /// <inheritdoc />
    public IntPtr GetExceptionPointers() => IntPtr.Zero;
}
