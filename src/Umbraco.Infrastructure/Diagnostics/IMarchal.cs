using System;

namespace Umbraco.Core.Diagnostics
{
    /// <summary>
    /// Provides a collection of methods for allocating unmanaged memory, copying unmanaged memory blocks, and converting managed to unmanaged types, as well as other miscellaneous methods used when interacting with unmanaged code.
    /// </summary>
    public interface IMarchal
    {
        IntPtr GetExceptionPointers();
    }
}
