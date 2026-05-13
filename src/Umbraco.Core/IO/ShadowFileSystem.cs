using System.Text.RegularExpressions;

namespace Umbraco.Cms.Core.IO;

/// <summary>
/// Represents a file system that shadows another file system, tracking changes without modifying the original.
/// </summary>
/// <remarks>
/// This file system captures all file operations in memory and can later apply them to the inner file system.
/// </remarks>
internal sealed partial class ShadowFileSystem : IFileSystem
{
    private readonly IFileSystem _sfs;

    private Dictionary<string, ShadowNode>? _nodes;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShadowFileSystem"/> class.
    /// </summary>
    /// <param name="fs">The inner file system being shadowed.</param>
    /// <param name="sfs">The shadow file system for storing temporary files.</param>
    public ShadowFileSystem(IFileSystem fs, IFileSystem sfs)
    {
        Inner = fs;
        _sfs = sfs;
    }

    /// <summary>
    /// Gets the inner file system being shadowed.
    /// </summary>
    public IFileSystem Inner { get; }

    /// <inheritdoc />
    public bool CanAddPhysical => true;

    /// <summary>
    /// Gets the dictionary of shadow nodes tracking file and directory changes.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="StringComparer.OrdinalIgnoreCase"/> so the shadow exposes case-insensitive
    /// path semantics (matching Windows file system behavior) while preserving the original case
    /// of paths. Preserving case is required for <see cref="Complete"/>: the stored key is also
    /// used to locate the shadow file via <c>_sfs.GetFullPath</c>, which on case-sensitive
    /// file systems (e.g. Linux) must match the case the file was actually written with.
    /// </remarks>
    private Dictionary<string, ShadowNode> Nodes => _nodes ??= new Dictionary<string, ShadowNode>(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public IEnumerable<string> GetDirectories(string path)
    {
        var normPath = NormPath(path);
        KeyValuePair<string, ShadowNode>[] shadows = Nodes.Where(kvp => IsChild(normPath, kvp.Key)).ToArray();
        IEnumerable<string> directories = Inner.GetDirectories(path);
        return directories
            .Except(shadows
                .Where(kvp => (kvp.Value.IsDir && kvp.Value.IsDelete) || (kvp.Value.IsFile && kvp.Value.IsExist))
                .Select(kvp => kvp.Key))
            .Union(shadows.Where(kvp => kvp.Value.IsDir && kvp.Value.IsExist).Select(kvp => kvp.Key))
            .Distinct();
    }

    /// <inheritdoc />
    public void DeleteDirectory(string path) => DeleteDirectory(path, false);

    /// <inheritdoc />
    public void DeleteDirectory(string path, bool recursive)
    {
        if (DirectoryExists(path) == false)
        {
            return;
        }

        var normPath = NormPath(path);
        if (recursive)
        {
            Nodes[normPath] = new ShadowNode(true, true, normPath);
            var remove = Nodes.Where(x => IsDescendant(normPath, x.Key)).ToList();
            foreach (KeyValuePair<string, ShadowNode> kvp in remove)
            {
                Nodes.Remove(kvp.Key);
            }

            Delete(path, true);
        }
        else
        {
            // actual content
            if (Nodes.Any(x => IsChild(normPath, x.Key) && x.Value.IsExist) // shadow content
                || Inner.GetDirectories(path).Any() || Inner.GetFiles(path).Any())
            {
                throw new InvalidOperationException("Directory is not empty.");
            }

            Nodes[path] = new ShadowNode(true, true, normPath);
            var remove = Nodes.Where(x => IsChild(normPath, x.Key)).ToList();
            foreach (KeyValuePair<string, ShadowNode> kvp in remove)
            {
                Nodes.Remove(kvp.Key);
            }

            Delete(path, false);
        }
    }

    /// <inheritdoc />
    public bool DirectoryExists(string path)
    {
        if (Nodes.TryGetValue(NormPath(path), out ShadowNode? sf))
        {
            return sf.IsDir && sf.IsExist;
        }

        return Inner.DirectoryExists(path);
    }

    /// <inheritdoc />
    public void AddFile(string path, Stream stream) => AddFile(path, stream, true);

    /// <inheritdoc />
    public void AddFile(string path, Stream stream, bool overrideIfExists)
    {
        var normPath = NormPath(path);
        if (Nodes.TryGetValue(normPath, out ShadowNode? sf) && sf.IsExist && (sf.IsDir || overrideIfExists == false))
        {
            throw new InvalidOperationException($"A file at path '{path}' already exists");
        }

        var parts = normPath.Split(Constants.CharArrays.ForwardSlash);
        for (var i = 0; i < parts.Length - 1; i++)
        {
            var dirPath = string.Join("/", parts.Take(i + 1));
            if (Nodes.TryGetValue(dirPath, out ShadowNode? sd))
            {
                if (sd.IsFile)
                {
                    throw new InvalidOperationException("Invalid path.");
                }

                if (sd.IsDelete)
                {
                    Nodes[dirPath] = new ShadowNode(false, true, dirPath);
                }
            }
            else
            {
                if (Inner.DirectoryExists(dirPath))
                {
                    continue;
                }

                if (Inner.FileExists(dirPath))
                {
                    throw new InvalidOperationException("Invalid path.");
                }

                Nodes[dirPath] = new ShadowNode(false, true, dirPath);
            }
        }

        var canonicalPath = sf?.CanonicalPath ?? path;
        _sfs.AddFile(canonicalPath, stream, overrideIfExists);
        Nodes[normPath] = new ShadowNode(false, false, canonicalPath);
    }

    /// <inheritdoc />
    public IEnumerable<string> GetFiles(string path) => GetFiles(path, null);

    /// <inheritdoc />
    public IEnumerable<string> GetFiles(string path, string? filter)
    {
        var normPath = NormPath(path);
        KeyValuePair<string, ShadowNode>[] shadows = Nodes.Where(kvp => IsChild(normPath, kvp.Key)).ToArray();
        IEnumerable<string> files = filter != null ? Inner.GetFiles(path, filter) : Inner.GetFiles(path);
        WildcardExpression? wildcard = filter == null ? null : new WildcardExpression(filter);
        return files
            .Except(shadows.Where(kvp => (kvp.Value.IsFile && kvp.Value.IsDelete) || kvp.Value.IsDir)
                .Select(kvp => kvp.Key))
            .Union(shadows
                .Where(kvp => kvp.Value.IsFile && kvp.Value.IsExist && (wildcard == null || wildcard.IsMatch(kvp.Key)))
                .Select(kvp => kvp.Key))
            .Distinct();
    }

    /// <inheritdoc />
    public Stream OpenFile(string path)
    {
        if (Nodes.TryGetValue(NormPath(path), out ShadowNode? sf))
        {
            return sf.IsDir || sf.IsDelete ? Stream.Null : _sfs.OpenFile(sf.CanonicalPath);
        }

        return Inner.OpenFile(path);
    }

    /// <inheritdoc />
    public void DeleteFile(string path)
    {
        if (FileExists(path) == false)
        {
            return;
        }

        var normPath = NormPath(path);
        Nodes[normPath] = new ShadowNode(true, false, normPath);
    }

    /// <inheritdoc />
    public void MoveFile(string source, string target, bool overrideIfExists = true)
    {
        var normSource = NormPath(source);
        var normTarget = NormPath(target);
        if (Nodes.TryGetValue(normSource, out ShadowNode? sf) == false || sf.IsDir || sf.IsDelete)
        {
            if (Inner.FileExists(source) == false)
            {
                throw new FileNotFoundException("Source file does not exist.");
            }
        }

        if (Nodes.TryGetValue(normTarget, out ShadowNode? tf) && tf.IsExist && (tf.IsDir || overrideIfExists == false))
        {
            throw new IOException($"A file at path '{target}' already exists");
        }

        var parts = normTarget.Split(Constants.CharArrays.ForwardSlash);
        for (var i = 0; i < parts.Length - 1; i++)
        {
            var dirPath = string.Join("/", parts.Take(i + 1));
            if (Nodes.TryGetValue(dirPath, out ShadowNode? sd))
            {
                if (sd.IsFile)
                {
                    throw new InvalidOperationException("Invalid path.");
                }

                if (sd.IsDelete)
                {
                    Nodes[dirPath] = new ShadowNode(false, true, dirPath);
                }
            }
            else
            {
                if (Inner.DirectoryExists(dirPath))
                {
                    continue;
                }

                if (Inner.FileExists(dirPath))
                {
                    throw new InvalidOperationException("Invalid path.");
                }

                Nodes[dirPath] = new ShadowNode(false, true, dirPath);
            }
        }

        var sourceCanonical = sf?.CanonicalPath ?? normSource;
        var targetCanonical = tf?.CanonicalPath ?? normTarget;
        _sfs.MoveFile(sourceCanonical, targetCanonical, overrideIfExists);
        Nodes[normSource] = new ShadowNode(true, false, sourceCanonical);
        Nodes[normTarget] = new ShadowNode(false, false, targetCanonical);
    }

    /// <inheritdoc />
    public bool FileExists(string path)
    {
        if (Nodes.TryGetValue(NormPath(path), out ShadowNode? sf))
        {
            return sf.IsFile && sf.IsExist;
        }

        return Inner.FileExists(path);
    }

    /// <inheritdoc />
    public string GetRelativePath(string fullPathOrUrl) => Inner.GetRelativePath(fullPathOrUrl);

    /// <inheritdoc />
    public string GetFullPath(string path)
    {
        if (Nodes.TryGetValue(NormPath(path), out ShadowNode? sf))
        {
            return sf.IsDir || sf.IsDelete ? string.Empty : _sfs.GetFullPath(sf.CanonicalPath);
        }

        return Inner.GetFullPath(path);
    }

    /// <inheritdoc />
    public string GetUrl(string? path) => Inner.GetUrl(path);

    /// <inheritdoc />
    public DateTimeOffset GetLastModified(string path)
    {
        if (Nodes.TryGetValue(NormPath(path), out ShadowNode? sf) == false)
        {
            return Inner.GetLastModified(path);
        }

        if (sf.IsDelete)
        {
            throw new InvalidOperationException("Invalid path.");
        }

        return _sfs.GetLastModified(sf.CanonicalPath);
    }

    /// <inheritdoc />
    public DateTimeOffset GetCreated(string path)
    {
        if (Nodes.TryGetValue(NormPath(path), out ShadowNode? sf) == false)
        {
            return Inner.GetCreated(path);
        }

        if (sf.IsDelete)
        {
            throw new InvalidOperationException("Invalid path.");
        }

        return _sfs.GetCreated(sf.CanonicalPath);
    }

    /// <inheritdoc />
    public long GetSize(string path)
    {
        if (Nodes.TryGetValue(NormPath(path), out ShadowNode? sf) == false)
        {
            return Inner.GetSize(path);
        }

        if (sf.IsDelete || sf.IsDir)
        {
            throw new InvalidOperationException("Invalid path.");
        }

        return _sfs.GetSize(sf.CanonicalPath);
    }

    /// <inheritdoc />
    public void AddFile(string path, string physicalPath, bool overrideIfExists = true, bool copy = false)
    {
        var normPath = NormPath(path);
        if (Nodes.TryGetValue(normPath, out ShadowNode? sf) && sf.IsExist && (sf.IsDir || overrideIfExists == false))
        {
            throw new InvalidOperationException($"A file at path '{path}' already exists");
        }

        var parts = normPath.Split(Constants.CharArrays.ForwardSlash);
        for (var i = 0; i < parts.Length - 1; i++)
        {
            var dirPath = string.Join("/", parts.Take(i + 1));
            if (Nodes.TryGetValue(dirPath, out ShadowNode? sd))
            {
                if (sd.IsFile)
                {
                    throw new InvalidOperationException("Invalid path.");
                }

                if (sd.IsDelete)
                {
                    Nodes[dirPath] = new ShadowNode(false, true, dirPath);
                }
            }
            else
            {
                if (Inner.DirectoryExists(dirPath))
                {
                    continue;
                }

                if (Inner.FileExists(dirPath))
                {
                    throw new InvalidOperationException("Invalid path.");
                }

                Nodes[dirPath] = new ShadowNode(false, true, dirPath);
            }
        }

        var canonicalPath = sf?.CanonicalPath ?? path;
        _sfs.AddFile(canonicalPath, physicalPath, overrideIfExists, copy);
        Nodes[normPath] = new ShadowNode(false, false, canonicalPath);
    }

    /// <summary>
    /// Applies all shadow changes to the inner file system.
    /// </summary>
    /// <exception cref="AggregateException">Thrown when one or more changes could not be applied.</exception>
    public void Complete()
    {
        if (_nodes == null)
        {
            return;
        }

        var exceptions = new List<Exception>();
        foreach (KeyValuePair<string, ShadowNode> kvp in _nodes)
        {
            if (kvp.Value.IsExist)
            {
                if (kvp.Value.IsFile)
                {
                    try
                    {
                        if (Inner.CanAddPhysical)
                        {
                            Inner.AddFile(kvp.Key, _sfs.GetFullPath(kvp.Value.CanonicalPath)); // overwrite, move
                        }
                        else
                        {
                            using (Stream stream = _sfs.OpenFile(kvp.Value.CanonicalPath))
                            {
                                Inner.AddFile(kvp.Key, stream, true);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        exceptions.Add(new Exception("Could not save file \"" + kvp.Key + "\".", e));
                    }
                }
            }
            else
            {
                try
                {
                    if (kvp.Value.IsDir)
                    {
                        Inner.DeleteDirectory(kvp.Key, true);
                    }
                    else
                    {
                        Inner.DeleteFile(kvp.Key);
                    }
                }
                catch (Exception e)
                {
                    exceptions.Add(new Exception(
                        "Could not delete " + (kvp.Value.IsDir ? "directory" : "file") + " \"" + kvp.Key + "\".", e));
                }
            }
        }

        _nodes.Clear();

        if (exceptions.Count == 0)
        {
            return;
        }

        throw new AggregateException("Failed to apply all changes (see exceptions).", exceptions);
    }

    /// <summary>
    /// Normalizes a path's directory separators to forward slashes.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <returns>The normalized path.</returns>
    /// <remarks>
    /// Case is preserved. Case-insensitive matching is handled by <see cref="Nodes"/>'s
    /// <see cref="StringComparer.OrdinalIgnoreCase"/> comparer.
    /// </remarks>
    private static string NormPath(string path) => path.Replace("\\", "/");

    /// <summary>
    /// Determines whether the input path is a direct child of the specified path.
    /// </summary>
    /// <param name="path">The parent path.</param>
    /// <param name="input">The input path to check.</param>
    /// <returns><c>true</c> if input is a direct child of path; otherwise, <c>false</c>.</returns>
    /// <remarks>Values can be "" (root), "foo", "foo/bar"...</remarks>
    private static bool IsChild(string path, string input)
    {
        if (input.StartsWith(path, StringComparison.OrdinalIgnoreCase) == false || input.Length < path.Length + 2)
        {
            return false;
        }

        if (path.Length > 0 && input[path.Length] != '/')
        {
            return false;
        }

        var pos = input.IndexOf('/', path.Length + 1);
        return pos < 0;
    }

    /// <summary>
    /// Determines whether the input path is a descendant (direct or indirect) of the specified path.
    /// </summary>
    /// <param name="path">The ancestor path.</param>
    /// <param name="input">The input path to check.</param>
    /// <returns><c>true</c> if input is a descendant of path; otherwise, <c>false</c>.</returns>
    private static bool IsDescendant(string path, string input)
    {
        if (input.StartsWith(path, StringComparison.OrdinalIgnoreCase) == false || input.Length < path.Length + 2)
        {
            return false;
        }

        return path.Length == 0 || input[path.Length] == '/';
    }

    /// <summary>
    /// Marks files and directories for deletion in the shadow.
    /// </summary>
    /// <param name="path">The path to delete.</param>
    /// <param name="recurse">If <c>true</c>, recursively mark descendants for deletion.</param>
    private void Delete(string path, bool recurse)
    {
        foreach (var file in Inner.GetFiles(path))
        {
            var normFile = NormPath(file);
            Nodes[normFile] = new ShadowNode(true, false, normFile);
        }

        foreach (var dir in Inner.GetDirectories(path))
        {
            var normDir = NormPath(dir);
            Nodes[normDir] = new ShadowNode(true, true, normDir);
            if (recurse)
            {
                Delete(dir, true);
            }
        }
    }

    /// <summary>
    /// Represents a wildcard expression for matching file names.
    /// </summary>
    /// <remarks>Copied from System.Web.Util.Wildcard internal.</remarks>
    internal sealed partial class WildcardExpression
    {
        private static readonly Regex MetaRegex = GetMetaRegex();

        [GeneratedRegex("[\\+\\{\\\\\\[\\|\\(\\)\\.\\^\\$]")]
        private static partial Regex GetMetaRegex();

        private static readonly Regex QuestRegex = GetQuestRegex();

        [GeneratedRegex("\\?")]
        private static partial Regex GetQuestRegex();

        private static readonly Regex StarRegex = GetStarRegex();

        [GeneratedRegex("\\*")]
        private static partial Regex GetStarRegex();

        private static readonly Regex CommaRegex = GetCommaRegex();

        [GeneratedRegex(",")]
        private static partial Regex GetCommaRegex();

        private readonly bool _caseInsensitive;
        private readonly string _pattern;
        private Regex? _regex;

        /// <summary>
        /// Initializes a new instance of the <see cref="WildcardExpression"/> class.
        /// </summary>
        /// <param name="pattern">The wildcard pattern (e.g., "*.txt").</param>
        /// <param name="caseInsensitive">Whether matching should be case-insensitive.</param>
        public WildcardExpression(string pattern, bool caseInsensitive = true)
        {
            _pattern = pattern;
            _caseInsensitive = caseInsensitive;
        }

        /// <summary>
        /// Determines whether the input string matches the wildcard pattern.
        /// </summary>
        /// <param name="input">The input string to match.</param>
        /// <returns><c>true</c> if the input matches the pattern; otherwise, <c>false</c>.</returns>
        public bool IsMatch(string input)
        {
            EnsureRegex(_pattern);
            return _regex?.IsMatch(input) ?? false;
        }

        /// <summary>
        /// Ensures that the regex is compiled from the pattern.
        /// </summary>
        /// <param name="pattern">The wildcard pattern to compile.</param>
        private void EnsureRegex(string pattern)
        {
            if (_regex != null)
            {
                return;
            }

            RegexOptions options = RegexOptions.None;

            // match right-to-left (for speed) if the pattern starts with a *
            if (pattern.Length > 0 && pattern[0] == '*')
            {
                options = RegexOptions.RightToLeft | RegexOptions.Singleline;
            }
            else
            {
                options = RegexOptions.Singleline;
            }

            // case insensitivity
            if (_caseInsensitive)
            {
                options |= RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
            }

            // Remove regex metacharacters
            pattern = MetaRegex.Replace(pattern, "\\$0");

            // Replace wildcard metacharacters with regex codes
            pattern = QuestRegex.Replace(pattern, ".");
            pattern = StarRegex.Replace(pattern, ".*");
            pattern = CommaRegex.Replace(pattern, "\\z|\\A");

            // anchor the pattern at beginning and end, and return the regex
            _regex = new Regex("\\A" + pattern + "\\z", options);
        }
    }

    /// <summary>
    /// Represents a node in the shadow file system tracking the state of a file or directory.
    /// </summary>
    private sealed class ShadowNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShadowNode"/> class.
        /// </summary>
        /// <param name="isDelete">Whether this node represents a deletion.</param>
        /// <param name="isdir">Whether this node represents a directory.</param>
        /// <param name="canonicalPath">The original-case path tracked by this node.</param>
        public ShadowNode(bool isDelete, bool isdir, string canonicalPath)
        {
            IsDelete = isDelete;
            IsDir = isdir;
            CanonicalPath = canonicalPath;
        }

        /// <summary>
        /// Gets a value indicating whether this node represents a deletion.
        /// </summary>
        public bool IsDelete { get; }

        /// <summary>
        /// Gets a value indicating whether this node represents a directory.
        /// </summary>
        public bool IsDir { get; }

        /// <summary>
        /// Gets the original-case path tracked by this node. For existing-file nodes this is
        /// the path used the first time the file was staged in the current shadow scope.
        /// </summary>
        /// <remarks>
        /// All operations against the inner shadow file system (<c>_sfs</c>) must use this
        /// path so that re-staging the same logical path with a different case still reaches
        /// the same on-disk file on case-sensitive file systems (e.g. Linux).
        /// </remarks>
        public string CanonicalPath { get; }

        /// <summary>
        /// Gets a value indicating whether this node represents an existing item (not deleted).
        /// </summary>
        public bool IsExist => IsDelete == false;

        /// <summary>
        /// Gets a value indicating whether this node represents a file (not a directory).
        /// </summary>
        public bool IsFile => IsDir == false;
    }
}
