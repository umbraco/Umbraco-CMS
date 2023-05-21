using System.Globalization;

namespace Umbraco.Cms.Core;

/// <summary>
///     Used to create a .NET HashCode from multiple objects.
/// </summary>
/// <remarks>
///     .Net has a class the same as this: System.Web.Util.HashCodeCombiner and of course it works for all sorts of things
///     which we've not included here as we just need a quick easy class for this in order to create a unique
///     hash of directories/files to see if they have changed.
///     NOTE: It's probably best to not relying on the hashing result across AppDomains! If you need a constant/reliable
///     hash value
///     between AppDomains use SHA1. This is perfect for hashing things in a very fast way for a single AppDomain.
/// </remarks>
public class HashCodeCombiner
{
    private long _combinedHash = 5381L;

    public void AddInt(int i) => _combinedHash = ((_combinedHash << 5) + _combinedHash) ^ i;

    public void AddObject(object o) => AddInt(o.GetHashCode());

    public void AddDateTime(DateTime d) => AddInt(d.GetHashCode());

    public void AddString(string s)
    {
        if (s != null)
        {
            AddInt(StringComparer.InvariantCulture.GetHashCode(s));
        }
    }

    public void AddCaseInsensitiveString(string s)
    {
        if (s != null)
        {
            AddInt(StringComparer.InvariantCultureIgnoreCase.GetHashCode(s));
        }
    }

    public void AddFileSystemItem(FileSystemInfo f)
    {
        // if it doesn't exist, don't proceed.
        if (!f.Exists)
        {
            return;
        }

        AddCaseInsensitiveString(f.FullName);
        AddDateTime(f.CreationTimeUtc);
        AddDateTime(f.LastWriteTimeUtc);

        // check if it is a file or folder
        if (f is FileInfo fileInfo)
        {
            AddInt(fileInfo.Length.GetHashCode());
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

    public void AddFile(FileInfo f) => AddFileSystemItem(f);

    public void AddFolder(DirectoryInfo d) => AddFileSystemItem(d);

    /// <summary>
    ///     Returns the hex code of the combined hash code
    /// </summary>
    /// <returns></returns>
    public string GetCombinedHashCode() => _combinedHash.ToString("x", CultureInfo.InvariantCulture);
}
