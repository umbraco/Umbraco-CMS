using System.IO.Compression;
using Microsoft.AspNetCore.Authentication;

namespace Umbraco.Cms.Api.Management.Security;

/// <summary>
///     An <see cref="IDataSerializer{TModel}" /> for <see cref="AuthenticationTicket" /> that compresses the serialized
///     payload.
/// </summary>
/// <remarks>
///     <para>
///         The back office authentication ticket is stored in its entirety in the authentication cookie. A claims
///         identity holding one claim per start node, allowed section and user group - each repeating a long claim type
///         URI and issuer - serializes to a payload large enough to be split across several cookie chunks, all of which
///         are sent back in a single request header. Compression targets exactly that redundancy.
///     </para>
///     <para>
///         Payloads written without compression remain readable, so cookies issued before this type was introduced
///         continue to authenticate.
///     </para>
/// </remarks>
internal sealed class CompressedTicketSerializer : IDataSerializer<AuthenticationTicket>
{
    /// <summary>
    ///     An upper bound on the decompressed payload size.
    /// </summary>
    private const int MaxDecompressedLength = 4 * 1024 * 1024;

    /// <summary>
    ///     Marks a payload as compressed.
    /// </summary>
    /// <remarks>
    ///     An uncompressed payload begins with a format version written as a little-endian 32 bit integer, so its second
    ///     byte is always zero. A marker with a non-zero second byte therefore cannot be mistaken for one.
    /// </remarks>
    private static readonly byte[] _compressedMarker = [0x55, 0x5A];

    private readonly IDataSerializer<AuthenticationTicket> _inner;
    private readonly bool _compress;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CompressedTicketSerializer" /> class.
    /// </summary>
    /// <param name="inner">The serializer producing the payload to compress.</param>
    /// <param name="compress">
    ///     Whether to compress when writing. Reading always accepts both compressed and uncompressed payloads, so this
    ///     only affects the payloads this instance produces.
    /// </param>
    public CompressedTicketSerializer(IDataSerializer<AuthenticationTicket> inner, bool compress = true)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _compress = compress;
    }

    /// <inheritdoc />
    public byte[] Serialize(AuthenticationTicket model)
    {
        var serialized = _inner.Serialize(model);

        if (_compress is false)
        {
            return serialized;
        }

        var compressed = Compress(serialized);

        // Compression can grow a small payload, in which case the uncompressed form is written instead.
        return compressed.Length < serialized.Length
            ? compressed
            : serialized;
    }

    /// <inheritdoc />
    public AuthenticationTicket? Deserialize(byte[] data)
    {
        if (IsCompressed(data) is false)
        {
            return _inner.Deserialize(data);
        }

        try
        {
            var decompressed = Decompress(data);
            return decompressed is null
                ? null
                : _inner.Deserialize(decompressed);
        }
        catch (Exception)
        {
            // A payload that cannot be read is reported as absent rather than as a failure, so the caller treats it
            // the same as no payload at all.
            return null;
        }
    }

    private static bool IsCompressed(byte[] data)
        => data.Length >= _compressedMarker.Length
           && data[0] == _compressedMarker[0]
           && data[1] == _compressedMarker[1];

    private static byte[] Compress(byte[] data)
    {
        using var output = new MemoryStream();
        output.Write(_compressedMarker);

        using (var deflate = new DeflateStream(output, CompressionLevel.Optimal, leaveOpen: true))
        {
            deflate.Write(data);
        }

        return output.ToArray();
    }

    private static byte[]? Decompress(byte[] data)
    {
        try
        {
            using var input = new MemoryStream(data, _compressedMarker.Length, data.Length - _compressedMarker.Length);
            using var deflate = new DeflateStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();

            // The payload is authenticated by data protection before it reaches here, so an oversized one is not an
            // expected attack. The bound simply keeps a corrupt payload from exhausting memory.
            var buffer = new byte[8192];
            int read;
            while ((read = deflate.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (output.Length + read > MaxDecompressedLength)
                {
                    return null;
                }

                output.Write(buffer, 0, read);
            }

            return output.ToArray();
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }
}
