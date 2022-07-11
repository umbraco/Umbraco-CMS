namespace Umbraco.Cms.Core.Diagnostics;

/// <summary>
///     Provides a collection of methods for allocating unmanaged memory, copying unmanaged memory blocks, and converting
///     managed to unmanaged types, as well as other miscellaneous methods used when interacting with unmanaged code.
/// </summary>
public interface IMarchal
{
    /// <summary>
    ///     Retrieves a computer-independent description of an exception, and information about the state that existed for the
    ///     thread when the exception occurred.
    /// </summary>
    /// <returns>A pointer to an EXCEPTION_POINTERS structure.</returns>
    IntPtr GetExceptionPointers();
}
