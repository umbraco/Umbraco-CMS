using System;

namespace Umbraco.Core.Diagnostics
{
    internal class NoopMarchal : IMarchal
    {
        public IntPtr GetExceptionPointers() => IntPtr.Zero;
    }
}
