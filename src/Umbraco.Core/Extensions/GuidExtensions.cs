namespace Umbraco.Cms.Core.Extensions;

public static class GuidExtensions
{
    public static bool IsFakeGuid(this Guid guid)
    {
        var bytes = guid.ToByteArray();

        // Our fake guid is a 32 bit int, converted to a byte representation,
        // so we can check if everything but the first 4 bytes are 0, if so, we know it's a fake guid.
        return bytes[4..].All(x => x == 0);
    }

    public static int ToInt(this Guid guid)
    {
        var bytes = guid.ToByteArray();
        return BitConverter.ToInt32(bytes, 0);
    }
}
