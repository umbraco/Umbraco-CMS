namespace Umbraco.Cms.Core.Diagnostics;

internal sealed class NoopMarchal : IMarchal
{
    public IntPtr GetExceptionPointers() => IntPtr.Zero;
}
