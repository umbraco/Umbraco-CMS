using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Compilation;
using Umbraco.Core.Cache;
using Umbraco.Core.Collections;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using File = System.IO.File;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Provides methods to find and instantiate types.
    /// </summary>
    /// <remarks>
    /// <para>This class should be used to get all types, the <see cref="TypeFinder"/> class should never be used directly.</para>
    /// <para>In most cases this class is not used directly but through extension methods that retrieve specific types.</para>
    /// <para>This class caches the types it knows to avoid excessive assembly scanning and shorten startup times, relying
    /// on a hash of the DLLs in the ~/bin folder to check for cache expiration.</para>
    /// </remarks>
    public class TypeLoader
    {
        private const string CacheKey = "umbraco-types.list";

        private readonly IAppPolicyCache _runtimeCache;
        private readonly IProfilingLogger _logger;

        private readonly Dictionary<CompositeTypeTypeKey, TypeList> _types = new Dictionary<CompositeTypeTypeKey, TypeList>();
        private readonly object _locko = new object();
        private readonly object _timerLock = new object();

        private Timer _timer;
        private bool _timing;
        private string _cachedAssembliesHash;
        private string _currentAssembliesHash;
        private IEnumerable<Assembly> _assemblies;
        private bool _reportedChange;
        private readonly string _localTempPath;
        private readonly Lazy<string> _fileBasePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeLoader"/> class.
        /// </summary>
        /// <param name="runtimeCache">The application runtime cache.</param>
        /// <param name="localTempPath">Files storage location.</param>
        /// <param name="logger">A profiling logger.</param>
        public TypeLoader(IAppPolicyCache runtimeCache, string localTempPath, IProfilingLogger logger)
            : this(runtimeCache, localTempPath, logger, true)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeLoader"/> class.
        /// </summary>
        /// <param name="runtimeCache">The application runtime cache.</param>
        /// <param name="localTempPath">Files storage location.</param>
        /// <param name="logger">A profiling logger.</param>
        /// <param name="detectChanges">Whether to detect changes using hashes.</param>
        internal TypeLoader(IAppPolicyCache runtimeCache, string localTempPath, IProfilingLogger logger, bool detectChanges)
        {
            _runtimeCache = runtimeCache ?? throw new ArgumentNullException(nameof(runtimeCache));
            _localTempPath = localTempPath;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _fileBasePath = new Lazy<string>(GetFileBasePath);

            if (detectChanges)
            {
                //first check if the cached hash is string.Empty, if it is then we need
                //do the check if they've changed
                RequiresRescanning = CachedAssembliesHash != CurrentAssembliesHash || CachedAssembliesHash == string.Empty;
                //if they have changed, we need to write the new file
                if (RequiresRescanning)
                {
                    // if the hash has changed, clear out the persisted list no matter what, this will force
                    // rescanning of all types including lazy ones.
                    // http://issues.umbraco.org/issue/U4-4789
                    var typesListFilePath = GetTypesListFilePath();
                    DeleteFile(typesListFilePath, FileDeleteTimeout);

                    WriteCacheTypesHash();
                }
            }
            else
            {
                // if the hash has changed, clear out the persisted list no matter what, this will force
                // rescanning of all types including lazy ones.
                // http://issues.umbraco.org/issue/U4-4789
                var typesListFilePath = GetTypesListFilePath();
                DeleteFile(typesListFilePath, FileDeleteTimeout);

                // always set to true if we're not detecting (generally only for testing)
                RequiresRescanning = true;
            }
        }

        /// <summary>
        /// Initializes a new, test/blank, instance of the <see cref="TypeLoader"/> class.
        /// </summary>
        /// <remarks>The initialized instance cannot get types.</remarks>
        internal TypeLoader()
        { }

        /// <summary>
        /// Gets or sets the set of assemblies to scan.
        /// </summary>
        /// <remarks>
        /// <para>If not explicitly set, defaults to all assemblies except those that are know to not have any of the
        /// types we might scan. Because we only scan for application types, this means we can safely exclude GAC assemblies
        /// for example.</para>
        /// <para>This is for unit tests.</para>
        /// </remarks>
        // internal for tests
        internal IEnumerable<Assembly> AssembliesToScan
        {
            get => _assemblies ?? (_assemblies = TypeFinder.GetAssembliesWithKnownExclusions());
            set => _assemblies = value;
        }

        /// <summary>
        /// Gets the type lists.
        /// </summary>
        /// <remarks>For unit tests.</remarks>
        // internal for tests
        internal IEnumerable<TypeList> TypeLists => _types.Values;

        /// <summary>
        /// Sets a type list.
        /// </summary>
        /// <remarks>For unit tests.</remarks>
        // internal for tests
        internal void AddTypeList(TypeList typeList)
        {
            var tobject = typeof(object); // CompositeTypeTypeKey does not support null values
            _types[new CompositeTypeTypeKey(typeList.BaseType ?? tobject, typeList.AttributeType ?? tobject)] = typeList;
        }

        #region Hashing

        /// <summary>
        /// Gets a value indicating whether the assemblies in bin, app_code, global.asax, etc... have changed since they were last hashed.
        /// </summary>
        private bool RequiresRescanning { get; }

        /// <summary>
        /// Gets the currently cached hash value of the scanned assemblies.
        /// </summary>
        /// <value>The cached hash value, or string.Empty if no cache is found.</value>
        internal string CachedAssembliesHash
        {
            get
            {
                if (_cachedAssembliesHash != null)
                    return _cachedAssembliesHash;

                var typesHashFilePath = GetTypesHashFilePath();
                if (!File.Exists(typesHashFilePath))
                    return string.Empty;

                var hash = File.ReadAllText(typesHashFilePath, Encoding.UTF8);

                _cachedAssembliesHash = hash;
                return _cachedAssembliesHash;
            }
        }

        /// <summary>
        /// Gets the current assemblies hash based on creating a hash from the assemblies in various places.
        /// </summary>
        /// <value>The current hash.</value>
        private string CurrentAssembliesHash
        {
            get
            {
                if (_currentAssembliesHash != null)
                    return _currentAssembliesHash;

                _currentAssembliesHash = GetFileHash(new List<Tuple<FileSystemInfo, bool>>
                    {
                        // the bin folder and everything in it
                        new Tuple<FileSystemInfo, bool>(new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Bin)), false),
                        // the app code folder and everything in it
                        new Tuple<FileSystemInfo, bool>(new DirectoryInfo(IOHelper.MapPath("~/App_Code")), false),
                        // global.asax (the app domain also monitors this, if it changes will do a full restart)
                        new Tuple<FileSystemInfo, bool>(new FileInfo(IOHelper.MapPath("~/global.asax")), false)
                    }, _logger);

                return _currentAssembliesHash;
            }
        }

        /// <summary>
        /// Writes the assembly hash file.
        /// </summary>
        private void WriteCacheTypesHash()
        {
            var typesHashFilePath = GetTypesHashFilePath();
            File.WriteAllText(typesHashFilePath, CurrentAssembliesHash, Encoding.UTF8);
        }

        /// <summary>
        /// Returns a unique hash for a combination of FileInfo objects.
        /// </summary>
        /// <param name="filesAndFolders">A collection of files.</param>
        /// <param name="logger">A profiling logger.</param>
        /// <returns>The hash.</returns>
        /// <remarks>Each file is a tuple containing the FileInfo object and a boolean which indicates whether to hash the
        /// file properties (false) or the file contents (true).</remarks>
        private static string GetFileHash(IEnumerable<Tuple<FileSystemInfo, bool>> filesAndFolders, IProfilingLogger logger)
        {
            using (logger.DebugDuration<TypeLoader>("Determining hash of code files on disk", "Hash determined"))
            {
                // get the distinct file infos to hash
                var uniqInfos = new HashSet<string>();
                var uniqContent = new HashSet<string>();
                using (var generator = new HashGenerator())
                {
                    foreach (var fileOrFolder in filesAndFolders)
                    {
                        var info = fileOrFolder.Item1;
                        if (fileOrFolder.Item2)
                        {
                            // add each unique file's contents to the hash
                            // normalize the content for cr/lf and case-sensitivity
                            if (uniqContent.Add(info.FullName))
                            {
                                if (File.Exists(info.FullName) == false) continue;
                                var content = RemoveCrLf(File.ReadAllText(info.FullName));
                                generator.AddCaseInsensitiveString(content);
                            }
                        }
                        else
                        {
                            // add each unique folder/file to the hash
                            if (uniqInfos.Add(info.FullName))
                            {
                                generator.AddFileSystemItem(info);
                            }
                        }
                    }
                    return generator.GenerateHash();
                }
            }
        }

        // fast! (yes, according to benchmarks)
        private static string RemoveCrLf(string s)
        {
            var buffer = new char[s.Length];
            var count = 0;
            // ReSharper disable once ForCanBeConvertedToForeach - no!
            for (var i = 0; i < s.Length; i++)
            {
                if (s[i] != '\r' && s[i] != '\n')
                    buffer[count++] = s[i];
            }
            return new string(buffer, 0, count);
        }

        /// <summary>
        /// Returns a unique hash for a combination of FileInfo objects.
        /// </summary>
        /// <param name="filesAndFolders">A collection of files.</param>
        /// <param name="logger">A profiling logger.</param>
        /// <returns>The hash.</returns>
        // internal for tests
        internal static string GetFileHash(IEnumerable<FileSystemInfo> filesAndFolders, IProfilingLogger logger)
        {
            using (logger.DebugDuration<TypeLoader>("Determining hash of code files on disk", "Hash determined"))
            {
                using (var generator = new HashGenerator())
                {
                    // get the distinct file infos to hash
                    var uniqInfos = new HashSet<string>();

                    foreach (var fileOrFolder in filesAndFolders)
                    {
                        if (uniqInfos.Contains(fileOrFolder.FullName)) continue;
                        uniqInfos.Add(fileOrFolder.FullName);
                        generator.AddFileSystemItem(fileOrFolder);
                    }
                    return generator.GenerateHash();
                }
            }
        }

        #endregion

        #region Cache

        private const int ListFileOpenReadTimeout = 4000; // milliseconds
        private const int ListFileOpenWriteTimeout = 2000; // milliseconds
        private const int ListFileWriteThrottle = 500; // milliseconds - throttle before writing
        private const int ListFileCacheDuration = 2 * 60; // seconds - duration we cache the entire list
        private const int FileDeleteTimeout = 4000; // milliseconds

        // internal for tests
        internal Attempt<IEnumerable<string>> TryGetCached(Type baseType, Type attributeType)
        {
            var cache = _runtimeCache.GetCacheItem<Dictionary<Tuple<string, string>, IEnumerable<string>>>(CacheKey, ReadCacheSafe, TimeSpan.FromSeconds(ListFileCacheDuration));

            cache.TryGetValue(Tuple.Create(baseType == null ? string.Empty : baseType.FullName, attributeType == null ? string.Empty : attributeType.FullName), out IEnumerable<string> types);
            return types == null
                ? Attempt<IEnumerable<string>>.Fail()
                : Attempt.Succeed(types);
        }

        private Dictionary<Tuple<string, string>, IEnumerable<string>> ReadCacheSafe()
        {
            try
            {
                return ReadCache();
            }
            catch
            {
                try
                {
                    var typesListFilePath = GetTypesListFilePath();
                    DeleteFile(typesListFilePath, FileDeleteTimeout);
                }
                catch
                {
                    // on-purpose, does not matter
                }
            }

            return new Dictionary<Tuple<string, string>, IEnumerable<string>>();
        }

        // internal for tests
        internal Dictionary<Tuple<string, string>, IEnumerable<string>> ReadCache()
        {
            var cache = new Dictionary<Tuple<string, string>, IEnumerable<string>>();

            var typesListFilePath = GetTypesListFilePath();
            if (File.Exists(typesListFilePath) == false)
            {
                return cache;
            }

            using (var stream = GetFileStream(typesListFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, ListFileOpenReadTimeout))
            using (var reader = new StreamReader(stream))
            {
                while (true)
                {
                    var baseType = reader.ReadLine();
                    if (baseType == null)
                    {
                        return cache; // exit
                    }

                    if (baseType.StartsWith("<"))
                    {
                        break; // old xml
                    }

                    var attributeType = reader.ReadLine();
                    if (attributeType == null)
                    {
                        break;
                    }

                    var types = new List<string>();
                    while (true)
                    {
                        var type = reader.ReadLine();
                        if (type == null)
                        {
                            types = null; // break 2 levels
                            break;
                        }
                        if (type == string.Empty)
                        {
                            cache[Tuple.Create(baseType, attributeType)] = types;
                            break;
                        }
                        types.Add(type);
                    }

                    if (types == null)
                    {
                        break;
                    }
                }
            }

            cache.Clear();
            return cache;
        }

        // internal for tests
        internal string GetTypesListFilePath() => _fileBasePath.Value + ".list";

        private string GetTypesHashFilePath() => _fileBasePath.Value + ".hash";

        /// <summary>
        /// Used to produce the Lazy value of _fileBasePath
        /// </summary>
        /// <returns></returns>
        private string GetFileBasePath()
        {
            var fileBasePath = Path.Combine(_localTempPath, "TypesCache", "umbraco-types." + NetworkHelper.FileSafeMachineName);

            // ensure that the folder exists
            var directory = Path.GetDirectoryName(fileBasePath);
            if (directory == null)
            {
                throw new InvalidOperationException($"Could not determine folder for path \"{fileBasePath}\".");
            }

            if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            return fileBasePath;
        }

        // internal for tests
        internal void WriteCache()
        {
            _logger.Debug<TypeLoader>("Writing cache file.");
            var typesListFilePath = GetTypesListFilePath();
            using (var stream = GetFileStream(typesListFilePath, FileMode.Create, FileAccess.Write, FileShare.None, ListFileOpenWriteTimeout))
            using (var writer = new StreamWriter(stream))
            {
                foreach (var typeList in _types.Values)
                {
                    writer.WriteLine(typeList.BaseType == null ? string.Empty : typeList.BaseType.FullName);
                    writer.WriteLine(typeList.AttributeType == null ? string.Empty : typeList.AttributeType.FullName);
                    foreach (var type in typeList.Types)
                    {
                        writer.WriteLine(type.AssemblyQualifiedName);
                    }

                    writer.WriteLine();
                }
            }
        }

        // internal for tests
        internal void UpdateCache()
        {
            void TimerRelease(object o)
            {
                lock (_timerLock)
                {
                    try
                    {
                        WriteCache();
                    }
                    catch { /* bah - just don't die */ }
                    if (!_timing)
                        _timer = null;
                }
            }

            lock (_timerLock)
            {
                if (_timer == null)
                {
                    _timer = new Timer(TimerRelease, null, ListFileWriteThrottle, Timeout.Infinite);
                }
                else
                {
                    _timer.Change(ListFileWriteThrottle, Timeout.Infinite);
                }

                _timing = true;
            }
        }

        /// <summary>
        /// Removes cache files and internal cache.
        /// </summary>
        /// <remarks>Generally only used for resetting cache, for example during the install process.</remarks>
        public void ClearTypesCache()
        {
            var typesListFilePath = GetTypesListFilePath();
            DeleteFile(typesListFilePath, FileDeleteTimeout);

            var typesHashFilePath = GetTypesHashFilePath();
            DeleteFile(typesHashFilePath, FileDeleteTimeout);

            _runtimeCache.Clear(CacheKey);
        }

        private Stream GetFileStream(string path, FileMode fileMode, FileAccess fileAccess, FileShare fileShare, int timeoutMilliseconds)
        {
            const int pauseMilliseconds = 250;
            var attempts = timeoutMilliseconds / pauseMilliseconds;
            while (true)
            {
                try
                {
                    return new FileStream(path, fileMode, fileAccess, fileShare);
                }
                catch
                {
                    if (--attempts == 0)
                    {
                        throw;
                    }

                    _logger.Debug<TypeLoader,string,int,int>("Attempted to get filestream for file {Path} failed, {NumberOfAttempts} attempts left, pausing for {PauseMilliseconds} milliseconds", path, attempts, pauseMilliseconds);
                    Thread.Sleep(pauseMilliseconds);
                }
            }
        }

        private void DeleteFile(string path, int timeoutMilliseconds)
        {
            const int pauseMilliseconds = 250;
            var attempts = timeoutMilliseconds / pauseMilliseconds;
            while (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                }
                catch
                {
                    if (--attempts == 0)
                        throw;

                    _logger.Debug<TypeLoader,string,int,int>("Attempted to delete file {Path} failed, {NumberOfAttempts} attempts left, pausing for {PauseMilliseconds} milliseconds", path, attempts, pauseMilliseconds);
                    Thread.Sleep(pauseMilliseconds);
                }
            }
        }

        #endregion

        #region Get Assembly Attributes

        /// <summary>
        /// Gets the assembly attributes of the specified type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>
        /// The assembly attributes of the specified type <typeparamref name="T" />.
        /// </returns>
        public IEnumerable<T> GetAssemblyAttributes<T>()
            where T : Attribute
        {
            return AssembliesToScan.SelectMany(a => a.GetCustomAttributes<T>()).ToList();
        }

        /// <summary>
        /// Gets all the assembly attributes.
        /// </summary>
        /// <returns>
        /// All assembly attributes.
        /// </returns>
        public IEnumerable<Attribute> GetAssemblyAttributes()
        {
            return AssembliesToScan.SelectMany(a => a.GetCustomAttributes()).ToList();
        }

        /// <summary>
        /// Gets the assembly attributes of the specified <paramref name="attributeTypes" />.
        /// </summary>
        /// <param name="attributeTypes">The attribute types.</param>
        /// <returns>
        /// The assembly attributes of the specified types.
        /// </returns>
        /// <exception cref="ArgumentNullException">attributeTypes</exception>
        public IEnumerable<Attribute> GetAssemblyAttributes(params Type[] attributeTypes)
        {
            if (attributeTypes == null)
                throw new ArgumentNullException(nameof(attributeTypes));

            return AssembliesToScan.SelectMany(a => attributeTypes.SelectMany(at => a.GetCustomAttributes(at))).ToList();
        }

        #endregion

        #region Get Types

        /// <summary>
        /// Gets class types inheriting from or implementing the specified type
        /// </summary>
        /// <typeparam name="T">The type to inherit from or implement.</typeparam>
        /// <param name="cache">Indicates whether to use cache for type resolution.</param>
        /// <param name="specificAssemblies">A set of assemblies for type resolution.</param>
        /// <returns>All class types inheriting from or implementing the specified type.</returns>
        /// <remarks>Caching is disabled when using specific assemblies.</remarks>
        public IEnumerable<Type> GetTypes<T>(bool cache = true, IEnumerable<Assembly> specificAssemblies = null)
        {
            if (_logger == null)
            {
                throw new InvalidOperationException("Cannot get types from a test/blank type loader.");
            }

            // do not cache anything from specific assemblies
            cache &= specificAssemblies == null;

            // if not IDiscoverable, directly get types
            if (!typeof(IDiscoverable).IsAssignableFrom(typeof(T)))
            {
                // warn
                _logger.Debug<TypeLoader,string>("Running a full, " + (cache ? "" : "non-") + "cached, scan for non-discoverable type {TypeName} (slow).", typeof(T).FullName);

                return GetTypesInternal(
                    typeof(T), null,
                    () => TypeFinder.FindClassesOfType<T>(specificAssemblies ?? AssembliesToScan),
                    "scanning assemblies",
                    cache);
            }

            // get IDiscoverable and always cache
            var discovered = GetTypesInternal(
                typeof(IDiscoverable), null,
                () => TypeFinder.FindClassesOfType<IDiscoverable>(AssembliesToScan),
                "scanning assemblies",
                true);

            // warn
            if (!cache)
                _logger.Debug<TypeLoader, string>("Running a non-cached, filter for discoverable type {TypeName} (slowish).", typeof(T).FullName);

            // filter the cached discovered types (and maybe cache the result)
            return GetTypesInternal(
                typeof(T), null,
                () => discovered
                    .Where(x => typeof(T).IsAssignableFrom(x)),
                "filtering IDiscoverable",
                cache);
        }

        /// <summary>
        /// Gets class types inheriting from or implementing the specified type and marked with the specified attribute.
        /// </summary>
        /// <typeparam name="T">The type to inherit from or implement.</typeparam>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="cache">Indicates whether to use cache for type resolution.</param>
        /// <param name="specificAssemblies">A set of assemblies for type resolution.</param>
        /// <returns>All class types inheriting from or implementing the specified type and marked with the specified attribute.</returns>
        /// <remarks>Caching is disabled when using specific assemblies.</remarks>
        public IEnumerable<Type> GetTypesWithAttribute<T, TAttribute>(bool cache = true, IEnumerable<Assembly> specificAssemblies = null)
            where TAttribute : Attribute
        {
            if (_logger == null)
            {
                throw new InvalidOperationException("Cannot get types from a test/blank type loader.");
            }

            // do not cache anything from specific assemblies
            cache &= specificAssemblies == null;

            // if not IDiscoverable, directly get types
            if (!typeof(IDiscoverable).IsAssignableFrom(typeof(T)))
            {
                _logger.Debug<TypeLoader, string, string>("Running a full, " + (cache ? "" : "non-") + "cached, scan for non-discoverable type {TypeName} / attribute {AttributeName} (slow).", typeof(T).FullName, typeof(TAttribute).FullName);

                return GetTypesInternal(
                    typeof(T), typeof(TAttribute),
                    () => TypeFinder.FindClassesOfTypeWithAttribute<T, TAttribute>(specificAssemblies ?? AssembliesToScan),
                    "scanning assemblies",
                    cache);
            }

            // get IDiscoverable and always cache
            var discovered = GetTypesInternal(
                typeof(IDiscoverable), null,
                () => TypeFinder.FindClassesOfType<IDiscoverable>(AssembliesToScan),
                "scanning assemblies",
                true);

            // warn
            if (!cache)
                _logger.Debug<TypeLoader, string, string>("Running a non-cached, filter for discoverable type {TypeName}  / attribute {AttributeName} (slowish).", typeof(T).FullName, typeof(TAttribute).FullName);

            // filter the cached discovered types (and maybe cache the result)
            return GetTypesInternal(
                typeof(T), typeof(TAttribute),
                () => discovered
                    .Where(x => typeof(T).IsAssignableFrom(x))
                    .Where(x => x.GetCustomAttributes<TAttribute>(false).Any()),
                "filtering IDiscoverable",
                cache);
        }

        /// <summary>
        /// Gets class types marked with the specified attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="cache">Indicates whether to use cache for type resolution.</param>
        /// <param name="specificAssemblies">A set of assemblies for type resolution.</param>
        /// <returns>All class types marked with the specified attribute.</returns>
        /// <remarks>Caching is disabled when using specific assemblies.</remarks>
        public IEnumerable<Type> GetAttributedTypes<TAttribute>(bool cache = true, IEnumerable<Assembly> specificAssemblies = null)
            where TAttribute : Attribute
        {
            if (_logger == null)
            {
                throw new InvalidOperationException("Cannot get types from a test/blank type loader.");
            }

            // do not cache anything from specific assemblies
            cache &= specificAssemblies == null;

            if (!cache)
                _logger.Debug<TypeLoader, string>("Running a full, non-cached, scan for types / attribute {AttributeName} (slow).", typeof(TAttribute).FullName);

            return GetTypesInternal(
                typeof(object), typeof(TAttribute),
                () => TypeFinder.FindClassesWithAttribute<TAttribute>(specificAssemblies ?? AssembliesToScan),
                "scanning assemblies",
                cache);
        }

        private IEnumerable<Type> GetTypesInternal(
            Type baseType, Type attributeType,
            Func<IEnumerable<Type>> finder,
            string action,
            bool cache)
        {
            // using an upgradeable lock makes little sense here as only one thread can enter the upgradeable
            // lock at a time, and we don't have non-upgradeable readers, and quite probably the type
            // loader is mostly not going to be used in any kind of massively multi-threaded scenario - so,
            // a plain lock is enough

            var name = GetName(baseType, attributeType);

            lock (_locko)
            {
                using (_logger.DebugDuration<TypeLoader>(
                "Getting " + name,
                "Got " + name)) // cannot contain typesFound.Count as it's evaluated before the find
                {
                    // get within a lock & timer
                    return GetTypesInternalLocked(baseType, attributeType, finder, action, cache);
                }
            }
        }

        private static string GetName(Type baseType, Type attributeType)
        {
            var s = attributeType == null ? string.Empty : ("[" + attributeType + "]");
            s += baseType;
            return s;
        }

        private IEnumerable<Type> GetTypesInternalLocked(
            Type baseType, Type attributeType,
            Func<IEnumerable<Type>> finder,
            string action,
            bool cache)
        {
            // check if the TypeList already exists, if so return it, if not we'll create it
            var tobject = typeof(object); // CompositeTypeTypeKey does not support null values
            var listKey = new CompositeTypeTypeKey(baseType ?? tobject, attributeType ?? tobject);
            TypeList typeList = null;
            if (cache)
            {
                _types.TryGetValue(listKey, out typeList); // else null
            }

            // if caching and found, return
            if (typeList != null)
            {
                // need to put some logging here to try to figure out why this is happening: http://issues.umbraco.org/issue/U4-3505
                _logger.Debug<TypeLoader, string>("Getting {TypeName}: found a cached type list.", GetName(baseType, attributeType));
                return typeList.Types;
            }

            // else proceed,
            typeList = new TypeList(baseType, attributeType);

            var typesListFilePath = GetTypesListFilePath();
            var scan = RequiresRescanning || File.Exists(typesListFilePath) == false;

            if (scan)
            {
                // either we have to rescan, or we could not find the cache file:
                // report (only once) and scan and update the cache file
                if (_reportedChange == false)
                {
                    _logger.Debug<TypeLoader>("Assemblies changes detected, need to rescan everything.");
                    _reportedChange = true;
                }
            }

            if (scan == false)
            {
                // if we don't have to scan, try the cache
                var cacheResult = TryGetCached(baseType, attributeType);

                // here we need to identify if the CachedTypeNotFoundInFile was the exception, if it was then we need to re-scan
                // in some cases the type will not have been scanned for on application startup, but the assemblies haven't changed
                // so in this instance there will never be a result.
                if (cacheResult.Exception is CachedTypeNotFoundInFileException || cacheResult.Success == false)
                {
                    _logger.Debug<TypeLoader, string>("Getting {TypeName}: failed to load from cache file, must scan assemblies.", GetName(baseType, attributeType));
                    scan = true;
                }
                else
                {
                    // successfully retrieved types from the file cache: load
                    foreach (var type in cacheResult.Result)
                    {
                        try
                        {
                            // we use the build manager to ensure we get all types loaded, this is slightly slower than
                            // Type.GetType but if the types in the assembly aren't loaded yet it would fail whereas
                            // BuildManager will load them - this is how eg MVC loads types, etc - no need to make it
                            // more complicated
                            typeList.Add(BuildManager.GetType(type, true));
                        }
                        catch (Exception ex)
                        {
                            // in case of any exception, we have to exit, and revert to scanning
                            _logger.Error<TypeLoader,string,string>(ex, "Getting {TypeName}: failed to load cache file type {CacheType}, reverting to scanning assemblies.", GetName(baseType, attributeType), type);
                            scan = true;
                            break;
                        }
                    }

                    if (scan == false)
                    {
                        _logger.Debug<TypeLoader, string>("Getting {TypeName}: loaded types from cache file.", GetName(baseType, attributeType));
                    }
                }
            }

            if (scan)
            {
                // either we had to scan, or we could not get the types from the cache file - scan now
                _logger.Debug<TypeLoader, string>("Getting {TypeName}: " + action + ".", GetName(baseType, attributeType));

                foreach (var t in finder())
                {
                    typeList.Add(t);
                }
            }

            // if we are to cache the results, do so
            if (cache)
            {
                var added = _types.ContainsKey(listKey) == false;
                if (added)
                {
                    _types[listKey] = typeList;
                    //if we are scanning then update the cache file
                    if (scan)
                    {
                        UpdateCache();
                    }
                }

                _logger.Debug<TypeLoader, string, string>("Got {TypeName}, caching ({CacheType}).", GetName(baseType, attributeType), added.ToString().ToLowerInvariant());
            }
            else
            {
                _logger.Debug<TypeLoader, string>("Got {TypeName}.", GetName(baseType, attributeType));
            }

            return typeList.Types;
        }

        #endregion

        #region Nested classes and stuff

        /// <summary>
        /// Represents a list of types obtained by looking for types inheriting/implementing a
        /// specified type, and/or marked with a specified attribute type.
        /// </summary>
        internal class TypeList
        {
            private readonly HashSet<Type> _types = new HashSet<Type>();

            public TypeList(Type baseType, Type attributeType)
            {
                BaseType = baseType;
                AttributeType = attributeType;
            }

            public Type BaseType { get; }
            public Type AttributeType { get; }

            /// <summary>
            /// Adds a type.
            /// </summary>
            public void Add(Type type)
            {
                if (BaseType.IsAssignableFrom(type) == false)
                    throw new ArgumentException("Base type " + BaseType + " is not assignable from type " + type + ".", nameof(type));
                _types.Add(type);
            }

            /// <summary>
            /// Gets the types.
            /// </summary>
            public IEnumerable<Type> Types => _types;
        }

        /// <summary>
        /// Represents the error that occurs when a type was not found in the cache type list with the specified TypeResolutionKind.
        /// </summary>
        /// <seealso cref="System.Exception" />
        [Serializable]
        internal class CachedTypeNotFoundInFileException : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CachedTypeNotFoundInFileException" /> class.
            /// </summary>
            public CachedTypeNotFoundInFileException()
            { }

            /// <summary>
            /// Initializes a new instance of the <see cref="CachedTypeNotFoundInFileException" /> class.
            /// </summary>
            /// <param name="message">The message that describes the error.</param>
            public CachedTypeNotFoundInFileException(string message)
                : base(message)
            { }

            /// <summary>
            /// Initializes a new instance of the <see cref="CachedTypeNotFoundInFileException" /> class.
            /// </summary>
            /// <param name="message">The error message that explains the reason for the exception.</param>
            /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
            public CachedTypeNotFoundInFileException(string message, Exception innerException)
                : base(message, innerException)
            { }

            /// <summary>
            /// Initializes a new instance of the <see cref="CachedTypeNotFoundInFileException" /> class.
            /// </summary>
            /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
            /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
            protected CachedTypeNotFoundInFileException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            { }
        }

        #endregion
    }
}
