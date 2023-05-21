using System.Text.RegularExpressions;

namespace Umbraco.Cms.Core.IO;

internal class ShadowFileSystem : IFileSystem
{
    private readonly IFileSystem _sfs;

    private Dictionary<string, ShadowNode>? _nodes;

    public ShadowFileSystem(IFileSystem fs, IFileSystem sfs)
    {
        Inner = fs;
        _sfs = sfs;
    }

    public IFileSystem Inner { get; }

    public bool CanAddPhysical => true;

    private Dictionary<string, ShadowNode> Nodes => _nodes ??= new Dictionary<string, ShadowNode>();

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

    public void DeleteDirectory(string path) => DeleteDirectory(path, false);

    public void DeleteDirectory(string path, bool recursive)
    {
        if (DirectoryExists(path) == false)
        {
            return;
        }

        var normPath = NormPath(path);
        if (recursive)
        {
            Nodes[normPath] = new ShadowNode(true, true);
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

            Nodes[path] = new ShadowNode(true, true);
            var remove = Nodes.Where(x => IsChild(normPath, x.Key)).ToList();
            foreach (KeyValuePair<string, ShadowNode> kvp in remove)
            {
                Nodes.Remove(kvp.Key);
            }

            Delete(path, false);
        }
    }

    public bool DirectoryExists(string path)
    {
        if (Nodes.TryGetValue(NormPath(path), out ShadowNode? sf))
        {
            return sf.IsDir && sf.IsExist;
        }

        return Inner.DirectoryExists(path);
    }

    public void AddFile(string path, Stream stream) => AddFile(path, stream, true);

    public void AddFile(string path, Stream stream, bool overrideIfExists)
    {
        var normPath = NormPath(path);
        if (Nodes.TryGetValue(normPath, out ShadowNode? sf) && sf.IsExist && (sf.IsDir || overrideIfExists == false))
        {
            throw new InvalidOperationException(string.Format("A file at path '{0}' already exists", path));
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
                    Nodes[dirPath] = new ShadowNode(false, true);
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

                Nodes[dirPath] = new ShadowNode(false, true);
            }
        }

        _sfs.AddFile(path, stream, overrideIfExists);
        Nodes[normPath] = new ShadowNode(false, false);
    }

    public IEnumerable<string> GetFiles(string path) => GetFiles(path, null);

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

    public Stream OpenFile(string path)
    {
        if (Nodes.TryGetValue(NormPath(path), out ShadowNode? sf))
        {
            return sf.IsDir || sf.IsDelete ? Stream.Null : _sfs.OpenFile(path);
        }

        return Inner.OpenFile(path);
    }

    public void DeleteFile(string path)
    {
        if (FileExists(path) == false)
        {
            return;
        }

        Nodes[NormPath(path)] = new ShadowNode(true, false);
    }

    public bool FileExists(string path)
    {
        if (Nodes.TryGetValue(NormPath(path), out ShadowNode? sf))
        {
            return sf.IsFile && sf.IsExist;
        }

        return Inner.FileExists(path);
    }

    public string GetRelativePath(string fullPathOrUrl) => Inner.GetRelativePath(fullPathOrUrl);

    public string GetFullPath(string path)
    {
        if (Nodes.TryGetValue(NormPath(path), out ShadowNode? sf))
        {
            return sf.IsDir || sf.IsDelete ? string.Empty : _sfs.GetFullPath(path);
        }

        return Inner.GetFullPath(path);
    }

    public string GetUrl(string? path) => Inner.GetUrl(path);

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

        return _sfs.GetLastModified(path);
    }

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

        return _sfs.GetCreated(path);
    }

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

        return _sfs.GetSize(path);
    }

    public void AddFile(string path, string physicalPath, bool overrideIfExists = true, bool copy = false)
    {
        var normPath = NormPath(path);
        if (Nodes.TryGetValue(normPath, out ShadowNode? sf) && sf.IsExist && (sf.IsDir || overrideIfExists == false))
        {
            throw new InvalidOperationException(string.Format("A file at path '{0}' already exists", path));
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
                    Nodes[dirPath] = new ShadowNode(false, true);
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

                Nodes[dirPath] = new ShadowNode(false, true);
            }
        }

        _sfs.AddFile(path, physicalPath, overrideIfExists, copy);
        Nodes[normPath] = new ShadowNode(false, false);
    }

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
                            Inner.AddFile(kvp.Key, _sfs.GetFullPath(kvp.Key)); // overwrite, move
                        }
                        else
                        {
                            using (Stream stream = _sfs.OpenFile(kvp.Key))
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

    private static string NormPath(string path) => path.ToLowerInvariant().Replace("\\", "/");

    // values can be "" (root), "foo", "foo/bar"...
    private static bool IsChild(string path, string input)
    {
        if (input.StartsWith(path) == false || input.Length < path.Length + 2)
        {
            return false;
        }

        if (path.Length > 0 && input[path.Length] != '/')
        {
            return false;
        }

        var pos = input.IndexOf("/", path.Length + 1, StringComparison.OrdinalIgnoreCase);
        return pos < 0;
    }

    private static bool IsDescendant(string path, string input)
    {
        if (input.StartsWith(path) == false || input.Length < path.Length + 2)
        {
            return false;
        }

        return path.Length == 0 || input[path.Length] == '/';
    }

    private void Delete(string path, bool recurse)
    {
        foreach (var file in Inner.GetFiles(path))
        {
            Nodes[NormPath(file)] = new ShadowNode(true, false);
        }

        foreach (var dir in Inner.GetDirectories(path))
        {
            Nodes[NormPath(dir)] = new ShadowNode(true, true);
            if (recurse)
            {
                Delete(dir, true);
            }
        }
    }

    // copied from System.Web.Util.Wildcard internal
    internal class WildcardExpression
    {
        private static readonly Regex MetaRegex = new("[\\+\\{\\\\\\[\\|\\(\\)\\.\\^\\$]");
        private static readonly Regex QuestRegex = new("\\?");
        private static readonly Regex StarRegex = new("\\*");
        private static readonly Regex CommaRegex = new(",");
        private static readonly Regex SlashRegex = new("(?=/)");
        private static readonly Regex BackslashRegex = new("(?=[\\\\:])");
        private readonly bool _caseInsensitive;
        private readonly string _pattern;
        private Regex? _regex;

        public WildcardExpression(string pattern, bool caseInsensitive = true)
        {
            _pattern = pattern;
            _caseInsensitive = caseInsensitive;
        }

        public bool IsMatch(string input)
        {
            EnsureRegex(_pattern);
            return _regex?.IsMatch(input) ?? false;
        }

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

    private class ShadowNode
    {
        public ShadowNode(bool isDelete, bool isdir)
        {
            IsDelete = isDelete;
            IsDir = isdir;
        }

        public bool IsDelete { get; }

        public bool IsDir { get; }

        public bool IsExist => IsDelete == false;

        public bool IsFile => IsDir == false;
    }
}
