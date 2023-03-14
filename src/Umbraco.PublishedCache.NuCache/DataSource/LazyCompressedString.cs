using System.Diagnostics;
using System.Text;
using K4os.Compression.LZ4;
using Umbraco.Cms.Core.Exceptions;

namespace Umbraco.Cms.Infrastructure.PublishedCache.DataSource;

/// <summary>
///     Lazily decompresses a LZ4 Pickler compressed UTF8 string
/// </summary>
[DebuggerDisplay("{Display}")]
internal struct LazyCompressedString
{
    private readonly object _locker;
    private byte[]? _bytes;
    private string? _str;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="bytes">LZ4 Pickle compressed UTF8 String</param>
    public LazyCompressedString(byte[] bytes)
    {
        _locker = new object();
        _bytes = bytes;
        _str = null;
    }

    /// <summary>
    ///     Used to display debugging output since ToString() can only be called once
    /// </summary>
    private string Display
    {
        get
        {
            if (_str != null)
            {
                return $"Decompressed: {_str}";
            }

            lock (_locker)
            {
                if (_str != null)
                {
                    // double check
                    return $"Decompressed: {_str}";
                }

                if (_bytes == null)
                {
                    // This shouldn't happen
                    throw new PanicException("Bytes have already been cleared");
                }

                return $"Compressed Bytes: {_bytes.Length}";
            }
        }
    }

    public static implicit operator string(LazyCompressedString l) => l.ToString();

    public byte[] GetBytes()
    {
        if (_bytes == null)
        {
            throw new InvalidOperationException("The bytes have already been expanded");
        }

        return _bytes;
    }

    /// <summary>
    ///     Returns the decompressed string from the bytes. This methods can only be called once.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Throws if this is called more than once</exception>
    public string DecompressString()
    {
        if (_str != null)
        {
            return _str;
        }

        lock (_locker)
        {
            if (_str != null)
            {
                // double check
                return _str;
            }

            if (_bytes == null)
            {
                throw new InvalidOperationException("Bytes have already been cleared");
            }

            _str = Encoding.UTF8.GetString(LZ4Pickler.Unpickle(_bytes));
            _bytes = null;
        }

        return _str;
    }

    public override string ToString() => DecompressString();
}
