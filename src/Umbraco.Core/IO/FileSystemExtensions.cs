using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Extensions;

/// <summary>
/// Provides extension methods for file system operations.
/// </summary>
public static class FileSystemExtensions
{
    /// <summary>
    /// Computes the SHA1 hash of a stream.
    /// </summary>
    /// <param name="fileStream">The stream to hash.</param>
    /// <returns>The hexadecimal string representation of the hash.</returns>
    public static string GetStreamHash(this Stream fileStream)
    {
        if (fileStream.CanSeek)
        {
            fileStream.Seek(0, SeekOrigin.Begin);
        }

        using HashAlgorithm alg = SHA1.Create();

        // create a string output for the hash
        var stringBuilder = new StringBuilder();
        var hashedByteArray = alg.ComputeHash(fileStream);
        foreach (var b in hashedByteArray)
        {
            stringBuilder.Append(b.ToString("x2"));
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    ///     Attempts to open the file at <code>filePath</code> up to <code>maxRetries</code> times,
    ///     with a thread sleep time of <code>sleepPerRetryInMilliseconds</code> between retries.
    /// </summary>
    public static FileStream OpenReadWithRetry(this FileInfo file, int maxRetries = 5, int sleepPerRetryInMilliseconds = 50)
    {
        var retries = maxRetries;

        while (retries > 0)
        {
            try
            {
                return File.OpenRead(file.FullName);
            }
            catch (IOException)
            {
                retries--;

                if (retries == 0)
                {
                    throw;
                }

                Thread.Sleep(sleepPerRetryInMilliseconds);
            }
        }

        throw new ArgumentException("Retries must be greater than zero");
    }

    /// <summary>
    /// Copies a file from one path to another within the file system.
    /// </summary>
    /// <param name="fs">The file system.</param>
    /// <param name="path">The source file path.</param>
    /// <param name="newPath">The destination file path.</param>
    public static void CopyFile(this IFileSystem fs, string path, string newPath)
    {
        using (Stream stream = fs.OpenFile(path))
        {
            fs.AddFile(newPath, stream);
        }
    }

    /// <summary>
    /// Gets the file extension from a path.
    /// </summary>
    /// <param name="fs">The file system.</param>
    /// <param name="path">The file path.</param>
    /// <returns>The file extension including the period.</returns>
    public static string GetExtension(this IFileSystem fs, string path) => Path.GetExtension(fs.GetFullPath(path));

    /// <summary>
    /// Gets the file name from a path.
    /// </summary>
    /// <param name="fs">The file system.</param>
    /// <param name="path">The file path.</param>
    /// <returns>The file name including extension.</returns>
    public static string GetFileName(this IFileSystem fs, string path) => Path.GetFileName(fs.GetFullPath(path));

    // TODO: Currently this is the only way to do this
    /// <summary>
    /// Creates a folder at the specified path.
    /// </summary>
    /// <param name="fs">The file system.</param>
    /// <param name="folderPath">The folder path to create.</param>
    public static void CreateFolder(this IFileSystem fs, string folderPath)
    {
        var path = fs.GetRelativePath(folderPath);
        var tempFile = Path.Combine(path, Guid.NewGuid().ToString("N") + ".tmp");
        using (var s = new MemoryStream())
        {
            fs.AddFile(tempFile, s);
        }

        fs.DeleteFile(tempFile);
    }

    /// <summary>
    ///     Creates a new <see cref="IFileProvider" /> from the file system.
    /// </summary>
    /// <param name="fileSystem">The file system.</param>
    /// <param name="fileProvider">
    ///     When this method returns, contains an <see cref="IFileProvider" /> created from the file
    ///     system.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the <see cref="IFileProvider" /> was successfully created; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryCreateFileProvider(
        this IFileSystem fileSystem,
        [MaybeNullWhen(false)] out IFileProvider fileProvider)
    {
        fileProvider = fileSystem switch
        {
            IFileProviderFactory fileProviderFactory => fileProviderFactory.Create(),
            _ => null,
        };

        return fileProvider != null;
    }
}
