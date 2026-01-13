using System.Security.Cryptography;
using System.Text;

namespace Umbraco.Cms.Core;

/// <summary>
///     Used to generate a string hash using crypto libraries over multiple objects
/// </summary>
/// <remarks>
///     This should be used to generate a reliable hash that survives AppDomain restarts.
///     This will use the crypto libs to generate the hash and will try to ensure that
///     strings, etc... are not re-allocated so it's not consuming much memory.
/// </remarks>
public class HashGenerator : DisposableObjectSlim
{
    private readonly MemoryStream _ms = new();
    private StreamWriter _writer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HashGenerator"/> class.
    /// </summary>
    public HashGenerator() => _writer = new StreamWriter(_ms, Encoding.Unicode, 1024, true);

    /// <summary>
    ///     Adds an integer value to the hash computation.
    /// </summary>
    /// <param name="i">The integer value to add.</param>
    public void AddInt(int i) => _writer.Write(i);

    /// <summary>
    ///     Adds a long value to the hash computation.
    /// </summary>
    /// <param name="i">The long value to add.</param>
    public void AddLong(long i) => _writer.Write(i);

    /// <summary>
    ///     Adds an object's string representation to the hash computation.
    /// </summary>
    /// <param name="o">The object to add.</param>
    public void AddObject(object o) => _writer.Write(o);

    /// <summary>
    ///     Adds a DateTime value to the hash computation using its ticks.
    /// </summary>
    /// <param name="d">The DateTime value to add.</param>
    public void AddDateTime(DateTime d) => _writer.Write(d.Ticks);

    /// <summary>
    ///     Adds a string to the hash computation.
    /// </summary>
    /// <param name="s">The string to add. If null, nothing is added.</param>
    public void AddString(string s)
    {
        if (s != null)
        {
            _writer.Write(s);
        }
    }

    /// <summary>
    ///     Adds a string to the hash computation in a case-insensitive manner.
    /// </summary>
    /// <param name="s">The string to add. If null, nothing is added.</param>
    /// <remarks>
    ///     The string is converted to uppercase before being added to ensure
    ///     case-insensitive hash generation.
    /// </remarks>
    public void AddCaseInsensitiveString(string s)
    {
        // I've tried to no allocate a new string with this which can be done if we use the CompareInfo.GetSortKey method which will create a new
        // byte array that we can use to write to the output, however this also allocates new objects so i really don't think the performance
        // would be much different. In any case, I'll leave this here for reference. We could write the bytes out based on the sort key,
        // this is how we could deal with case insensitivity without allocating another string
        // for reference see: https://stackoverflow.com/a/10452967/694494
        // we could go a step further and s.Normalize() but we're not really dealing with crazy unicode with this class so far.
        if (s != null)
        {
            _writer.Write(s.ToUpperInvariant());
        }
    }

    /// <summary>
    ///     Adds a file system item (file or directory) to the hash computation.
    /// </summary>
    /// <param name="f">The file system item to add.</param>
    /// <remarks>
    ///     For files, adds the full name, creation time, last write time, and length.
    ///     For directories, recursively adds all files and subdirectories.
    /// </remarks>
    public void AddFileSystemItem(FileSystemInfo f)
    {
        // if it doesn't exist, don't proceed.
        if (f.Exists == false)
        {
            return;
        }

        AddCaseInsensitiveString(f.FullName);
        AddDateTime(f.CreationTimeUtc);
        AddDateTime(f.LastWriteTimeUtc);

        // check if it is a file or folder
        if (f is FileInfo fileInfo)
        {
            AddLong(fileInfo.Length);
        }

        if (f is DirectoryInfo dirInfo)
        {
            foreach (FileInfo d in dirInfo.GetFiles())
            {
                AddFile(d);
            }

            foreach (DirectoryInfo s in dirInfo.GetDirectories())
            {
                AddFolder(s);
            }
        }
    }

    /// <summary>
    ///     Adds a file to the hash computation.
    /// </summary>
    /// <param name="f">The file to add.</param>
    public void AddFile(FileInfo f) => AddFileSystemItem(f);

    /// <summary>
    ///     Adds a folder and its contents to the hash computation.
    /// </summary>
    /// <param name="d">The directory to add.</param>
    public void AddFolder(DirectoryInfo d) => AddFileSystemItem(d);

    /// <summary>
    ///     Returns the generated hash of all added objects.
    /// </summary>
    /// <returns>A hexadecimal string representation of the computed hash.</returns>
    /// <remarks>
    ///     Uses SHA1 when FIPS compliance is required, otherwise uses MD5.
    ///     This method can be called multiple times; subsequent calls will include
    ///     any objects added after the previous call.
    /// </remarks>
    public string GenerateHash()
    {
        // flush,close,dispose the writer,then create a new one since it's possible to keep adding after GenerateHash is called.
        _writer.Flush();
        _writer.Close();
        _writer.Dispose();
        _writer = new StreamWriter(_ms, Encoding.UTF8, 1024, true);

        // Use SHA1 for FIPS compliance, otherwise MD5
        HashAlgorithm hasher = CryptoConfig.AllowOnlyFipsAlgorithms ? SHA1.Create() : MD5.Create();

        using (hasher)
        {
            var buffer = _ms.GetBuffer();

            // get the hashed values created by our selected provider
            var hashedByteArray = hasher.ComputeHash(buffer);

            // create a StringBuilder object
            var stringBuilder = new StringBuilder();

            // loop to each byte
            foreach (var b in hashedByteArray)
            {
                // append it to our StringBuilder
                stringBuilder.Append(b.ToString("x2"));
            }

            // return the hashed value
            return stringBuilder.ToString();
        }
    }

    /// <inheritdoc />
    protected override void DisposeResources()
    {
        _writer.Close();
        _writer.Dispose();
        _ms.Close();
        _ms.Dispose();
    }
}
