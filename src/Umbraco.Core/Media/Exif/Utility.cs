namespace Umbraco.Cms.Core.Media.Exif;

/// <summary>
///     Contains utility functions.
/// </summary>
internal class Utility
{
    /// <summary>
    ///     Reads the entire stream and returns its contents as a byte array.
    /// </summary>
    /// <param name="stream">The <see cref="System.IO.Stream" /> to read.</param>
    /// <returns>Contents of the <paramref name="stream" /> as a byte array.</returns>
    public static byte[] GetStreamBytes(Stream stream)
    {
        using (var mem = new MemoryStream())
        {
            stream.Seek(0, SeekOrigin.Begin);

            var b = new byte[32768];
            int r;
            while ((r = stream.Read(b, 0, b.Length)) > 0)
            {
                mem.Write(b, 0, r);
            }

            return mem.ToArray();
        }
    }
}
