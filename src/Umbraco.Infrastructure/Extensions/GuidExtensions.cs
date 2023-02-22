namespace Umbraco.Cms.Infrastructure.Extensions;

public static class GuidExtensions
{
    internal static bool IsFakeGuid(this Guid guid)
    {
        var bytes = guid.ToByteArray();

        // Our fake guid is a 32 bit int, converted to a byte representation,
        // so we can check if everything but the first 4 bytes are 0, if so, we know it's a fake guid.
        if (bytes[4..].All(x => x == 0))
        {
            return true;
        }

        return false;
    }

    internal static int ToInt(this Guid guid)
    {
        var bytes = guid.ToByteArray();
        return BitConverter.ToInt32(bytes, 0);
    }
}
