namespace Umbraco.Cms.Core.Extensions;

/// <summary>
/// Provides extension methods for <see cref="Guid"/>.
/// </summary>
public static class GuidExtensions
{
    /// <summary>
    /// Determines whether the GUID was created from an integer using <see cref="Umbraco.Extensions.IntExtensions.ToGuid(int)"/>.
    /// </summary>
    /// <param name="guid">The GUID to check.</param>
    /// <returns><c>true</c> if the GUID was created from an integer; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// A "fake" GUID is one where only the first 4 bytes contain data (from the original integer)
    /// and all remaining bytes are zero.
    /// </remarks>
    public static bool IsFakeGuid(this Guid guid)
    {
        // Inspect the bytes on the stack to avoid heap allocations.
        Span<byte> bytes = stackalloc byte[16];
        guid.TryWriteBytes(bytes);

        // Our fake guid is a 32 bit int, converted to a byte representation,
        // so we can check if everything but the first 4 bytes are 0, if so, we know it's a fake guid.
        for (var i = 4; i < bytes.Length; i++)
        {
            if (bytes[i] != 0)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Converts a GUID that was created from an integer back to its original integer value.
    /// </summary>
    /// <param name="guid">The GUID to convert.</param>
    /// <returns>The integer value.</returns>
    /// <remarks>
    /// This method should only be used on GUIDs that were created using <see cref="Umbraco.Extensions.IntExtensions.ToGuid(int)"/>.
    /// </remarks>
    public static int ToInt(this Guid guid)
    {
        // Write to a stack-allocated buffer to avoid the heap allocation of Guid.ToByteArray().
        Span<byte> bytes = stackalloc byte[16];
        guid.TryWriteBytes(bytes);
        return BitConverter.ToInt32(bytes);
    }
}
