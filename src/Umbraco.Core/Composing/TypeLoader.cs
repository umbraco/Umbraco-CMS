using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Compilation;
using Umbraco.Core.Cache;
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

        private readonly IRuntimeCacheProvider _runtimeCache;
        private readonly IGlobalSettings _globalSettings;
        private readonly IProfilingLogger _logger;

        private readonly object _typesLock = new object();
        private readonly Dictionary<TypeListKey, TypeList> _types = new Dictionary<TypeListKey, TypeList>();

        private string _cachedAssembliesHash;
        private string _currentAssembliesHash;
        private IEnumerable<Assembly> _assemblies;
        private bool _reportedChange;
        private static LocalTempStorage _localTempStorage = LocalTempStorage.Unknown;
        private static string _fileBasePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeLoader"/> class.
        /// </summary>
        /// <param name="runtimeCache">The application runtime cache.</param>
        /// <param name="globalSettings"></param>
        /// <param name="logger">A profiling logger.</param>
        /// <param name="detectChanges">Whether to detect changes using hashes.</param>
        internal TypeLoader(IRuntimeCacheProvider runtimeCache, IGlobalSettings globalSettings, IProfilingLogger logger, bool detectChanges = true)
        {
            _runtimeCache = runtimeCache ?? throw new ArgumentNullException(nameof(runtimeCache));
            _globalSettings = globalSettings ?? throw new ArgumentNullException(nameof(globalSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
                    if (File.Exists(typesListFilePath))
                        File.Delete(typesListFilePath);

                    WriteCacheTypesHash();
                }
            }
            else
            {
                // if the hash has changed, clear out the persisted list no matter what, this will force
                // rescanning of all types including lazy ones.
                // http://issues.umbraco.org/issue/U4-4789
                var typesListFilePath = GetTypesListFilePath();
                if (File.Exists(typesListFilePath))
                    File.Delete(typesListFilePath);

                // always set to true if we're not detecting (generally only for testing)
                RequiresRescanning = true;
            }
        }

        /// <summary>
        /// Gets or sets the set of assemblies to scan.
        /// </summary>
        /// <remarks>
        /// <para>If not explicitely set, defaults to all assemblies except those that are know to not have any of the
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
            _types[new TypeListKey(typeList.BaseType, typeList.AttributeType)] = typeList;
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
                if (!File.Exists(typesHashFilePath)) return string.Empty;

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
                        new Tuple<FileSystemInfo, bool>(new FileInfo(IOHelper.MapPath("~/global.asax")), false),
                        // trees.config - use the contents to create the hash since this gets resaved on every app startup!
                        new Tuple<FileSystemInfo, bool>(new FileInfo(IOHelper.MapPath(SystemDirectories.Config + "/trees.config")), true)
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
            using (logger.TraceDuration<TypeLoader>("Determining hash of code files on disk", "Hash determined"))
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
        internal static string GetFileHash(IEnumerable<FileSystemInfo> filesAndFolders, ProfilingLogger logger)
        {
            using (logger.TraceDuration<TypeLoader>("Determining hash of code files on disk", "Hash determined"))
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

        // internal for tests
        internal Attempt<IEnumerable<string>> TryGetCached(Type baseType, Type attributeType)
        {
            var cache = _runtimeCache.GetCacheItem<Dictionary<Tuple<string, string>, IEnumerable<string>>>(CacheKey, ReadCacheSafe, TimeSpan.FromMinutes(4));

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
                    File.Delete(typesListFilePath);
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
                return cache;

            using (var stream = GetFileStream(typesListFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, ListFileOpenReadTimeout))
            using (var reader = new StreamReader(stream))
            {
                while (true)
                {
                    var baseType = reader.ReadLine();
                    if (baseType == null) return cache; // exit
                    if (baseType.StartsWith("<")) break; // old xml

                    var attributeType = reader.ReadLine();
                    if (attributeType == null) break;

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

                    if (types == null) break;
                }
            }

            cache.Clear();
            return cache;
        }

        // internal for tests
        internal string GetTypesListFilePath() => GetFileBasePath() + ".list";

        private string GetTypesHashFilePath() => GetFileBasePath() + ".hash";

        private string GetFileBasePath()
        {
            var localTempStorage = _globalSettings.LocalTempStorageLocation;
            if (_localTempStorage != localTempStorage)
            {
                string path;
                switch (_globalSettings.LocalTempStorageLocation)
                {
                    case LocalTempStorage.AspNetTemp:
                        path = Path.Combine(HttpRuntime.CodegenDir, "UmbracoData", "umbraco-types");
                        break;
                    case LocalTempStorage.EnvironmentTemp:
                        // include the appdomain hash is just a safety check, for example if a website is moved from worker A to worker B and then back
                        // to worker A again, in theory the %temp%  folder should already be empty but we really want to make sure that its not
                        // utilizing an old path - assuming we cannot have SHA1 collisions on AppDomainAppId
                        var appDomainHash = HttpRuntime.AppDomainAppId.ToSHA1();
                        var cachePath = Path.Combine(Environment.ExpandEnvironmentVariables("%temp%"), "UmbracoData", appDomainHash);
                        path = Path.Combine(cachePath, "umbraco-types");
                        break;
                    case LocalTempStorage.Default:
                    default:
                        var tempFolder = IOHelper.MapPath("~/App_Data/TEMP/TypesCache");
                        path =  Path.Combine(tempFolder, "umbraco-types." + NetworkHelper.FileSafeMachineName);
                        break;
                }

                _fileBasePath = path;
                _localTempStorage = localTempStorage;
            }

            // ensure that the folder exists
            var directory = Path.GetDirectoryName(_fileBasePath);
            if (directory == null)
                throw new InvalidOperationException($"Could not determine folder for path \"{_fileBasePath}\".");
            if (Directory.Exists(directory) == false)
                Directory.CreateDirectory(directory);

            return _fileBasePath;
        }

        //private string GetFilePath(string extension)
        //{
        //    string path;
        //    switch (_globalSettings.LocalTempStorageLocation)
        //    {
        //        case LocalTempStorage.AspNetTemp:
        //            path = Path.Combine(HttpRuntime.CodegenDir, "UmbracoData", "umbraco-types." + extension);
        //            break;
        //        case LocalTempStorage.EnvironmentTemp:
        //            // include the appdomain hash is just a safety check, for example if a website is moved from worker A to worker B and then back
        //            // to worker A again, in theory the %temp%  folder should already be empty but we really want to make sure that its not
        //            // utilizing an old path - assuming we cannot have SHA1 collisions on AppDomainAppId
        //            var appDomainHash = HttpRuntime.AppDomainAppId.ToSHA1();
        //            var cachePath = Path.Combine(Environment.ExpandEnvironmentVariables("%temp%"), "UmbracoData", appDomainHash);
        //            path = Path.Combine(cachePath, "umbraco-types." + extension);
        //            break;
        //        case LocalTempStorage.Default:
        //        default:
        //            var tempFolder = IOHelper.MapPath("~/App_Data/TEMP/TypesCache");
        //            path =  Path.Combine(tempFolder, "umbraco-types." + NetworkHelper.FileSafeMachineName + "." + extension);
        //            break;
        //    }

        //    // ensure that the folder exists
        //    var directory = Path.GetDirectoryName(path);
        //    if (directory == null)
        //        throw new InvalidOperationException($"Could not determine folder for file \"{path}\".");
        //    if (Directory.Exists(directory) == false)
        //        Directory.CreateDirectory(directory);

        //    return path;
        //}

        // internal for tests
        internal void WriteCache()
        {
            var typesListFilePath = GetTypesListFilePath();
            using (var stream = GetFileStream(typesListFilePath, FileMode.Create, FileAccess.Write, FileShare.None, ListFileOpenWriteTimeout))
            using (var writer = new StreamWriter(stream))
            {
                foreach (var typeList in _types.Values)
                {
                    writer.WriteLine(typeList.BaseType == null ? string.Empty : typeList.BaseType.FullName);
                    writer.WriteLine(typeList.AttributeType == null ? string.Empty : typeList.AttributeType.FullName);
                    foreach (var type in typeList.Types)
                        writer.WriteLine(type.AssemblyQualifiedName);
                    writer.WriteLine();
                }
            }
        }

        // internal for tests
        internal void UpdateCache()
        {
            // note
            // at the moment we write the cache to disk every time we update it. ideally we defer the writing
            // since all the updates are going to happen in a row when Umbraco starts. that being said, the
            // file is small enough, so it is not a priority.
            WriteCache();
        }

        /// <summary>
        /// Removes cache files and internal cache.
        /// </summary>
        /// <remarks>Generally only used for resetting cache, for example during the install process.</remarks>
        public void ClearTypesCache()
        {
            var typesListFilePath = GetTypesListFilePath();
            if (File.Exists(typesListFilePath))
                File.Delete(typesListFilePath);

            var typesHashFilePath = GetTypesHashFilePath();
            if (File.Exists(typesHashFilePath))
                File.Delete(typesHashFilePath);

            _runtimeCache.ClearCacheItem(CacheKey);
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
                        throw;

                    _logger.Debug<TypeLoader>("Attempted to get filestream for file {Path} failed, {NumberOfAttempts} attempts left, pausing for {PauseMilliseconds} milliseconds", path, attempts, pauseMilliseconds);
                    Thread.Sleep(pauseMilliseconds);
                }
            }
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
            // do not cache anything from specific assemblies
            cache &= specificAssemblies == null;

            // if not caching, or not IDiscoverable, directly get types
            if (cache == false || typeof(IDiscoverable).IsAssignableFrom(typeof(T)) == false)
            {
                return GetTypesInternal(
                    typeof (T), null,
                    () => TypeFinder.FindClassesOfType<T>(specificAssemblies ?? AssembliesToScan),
                    cache);
            }

            // if caching and IDiscoverable
            // filter the cached discovered types (and cache the result)

            var discovered = GetTypesInternal(
                typeof (IDiscoverable), null,
                () => TypeFinder.FindClassesOfType<IDiscoverable>(AssembliesToScan),
                true);

            return GetTypesInternal(
                typeof (T), null,
                () => discovered
                    .Where(x => typeof (T).IsAssignableFrom(x)),
                true);
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
            // do not cache anything from specific assemblies
            cache &= specificAssemblies == null;

            // if not caching, or not IDiscoverable, directly get types
            if (cache == false || typeof(IDiscoverable).IsAssignableFrom(typeof(T)) == false)
            {
                return GetTypesInternal(
                    typeof (T), typeof (TAttribute),
                    () => TypeFinder.FindClassesOfTypeWithAttribute<T, TAttribute>(specificAssemblies ?? AssembliesToScan),
                    cache);
            }

            // if caching and IDiscoverable
            // filter the cached discovered types (and cache the result)

            var discovered = GetTypesInternal(
                typeof (IDiscoverable), null,
                () => TypeFinder.FindClassesOfType<IDiscoverable>(AssembliesToScan),
                true);

            return GetTypesInternal(
                typeof (T), typeof (TAttribute),
                () => discovered
                    .Where(x => typeof(T).IsAssignableFrom(x))
                    .Where(x => x.GetCustomAttributes<TAttribute>(false).Any()),
                true);
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
            // do not cache anything from specific assemblies
            cache &= specificAssemblies == null;

            return GetTypesInternal(
                typeof (object), typeof (TAttribute),
                () => TypeFinder.FindClassesWithAttribute<TAttribute>(specificAssemblies ?? AssembliesToScan),
                cache);
        }

        private IEnumerable<Type> GetTypesInternal(
            Type baseType, Type attributeType,
            Func<IEnumerable<Type>> finder,
            bool cache)
        {
            // using an upgradeable lock makes little sense here as only one thread can enter the upgradeable
            // lock at a time, and we don't have non-upgradeable readers, and quite probably the type
            // loader is mostly not going to be used in any kind of massively multi-threaded scenario - so,
            // a plain lock is enough

            var name = GetName(baseType, attributeType);

            lock (_typesLock)
            using (_logger.TraceDuration<TypeLoader>(
                "Getting " + name,
                "Got " + name)) // cannot contain typesFound.Count as it's evaluated before the find
            {
                // get within a lock & timer
                return GetTypesInternalLocked(baseType, attributeType, finder, cache);
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
            bool cache)
        {
            // check if the TypeList already exists, if so return it, if not we'll create it
            var listKey = new TypeListKey(baseType, attributeType);
            TypeList typeList = null;
            if (cache)
                _types.TryGetValue(listKey, out typeList); // else null

            // if caching and found, return
            if (typeList != null)
            {
                // need to put some logging here to try to figure out why this is happening: http://issues.umbraco.org/issue/U4-3505
                _logger.Debug<TypeLoader>("Getting {TypeName}: found a cached type list.", GetName(baseType, attributeType));
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
                    _logger.Debug<TypeLoader>("Getting {TypeName}: failed to load from cache file, must scan assemblies.", GetName(baseType, attributeType));
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
                            _logger.Error<TypeLoader>(ex, "Getting {TypeName}: failed to load cache file type {CacheType}, reverting to scanning assemblies.", GetName(baseType, attributeType), type);
                            scan = true;
                            break;
                        }
                    }

                    if (scan == false)
                    {
                        _logger.Debug<TypeLoader>("Getting {TypeName}: loaded types from cache file.", GetName(baseType, attributeType));
                    }
                }
            }

            if (scan)
            {
                // either we had to scan, or we could not get the types from the cache file - scan now
                _logger.Debug<TypeLoader>("Getting {TypeName}: scanning assemblies.", GetName(baseType, attributeType));

                foreach (var t in finder())
                    typeList.Add(t);
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
                        UpdateCache();
                }

                _logger.Debug<TypeLoader>("Got {TypeName}, caching ({CacheType}).", GetName(baseType, attributeType), added.ToString().ToLowerInvariant());
            }
            else
            {
                _logger.Debug<TypeLoader>("Got {TypeName}.", GetName(baseType, attributeType));
            }

            return typeList.Types;
        }

        #endregion

        #region Nested classes and stuff

        /// <summary>
        /// Groups a type and a resolution kind into a key.
        /// </summary>
        private struct TypeListKey
        {
            // ReSharper disable MemberCanBePrivate.Local
            public readonly Type BaseType;
            public readonly Type AttributeType;
            // ReSharper restore MemberCanBePrivate.Local

            public TypeListKey(Type baseType, Type attributeType)
            {
                BaseType = baseType ?? typeof (object);
                AttributeType = attributeType;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || obj is TypeListKey == false) return false;
                var o = (TypeListKey)obj;
                return BaseType == o.BaseType && AttributeType == o.AttributeType;
            }

            public override int GetHashCode()
            {
                // in case AttributeType is null we need something else, using typeof (TypeListKey)
                // which does not really "mean" anything, it's just a value...

                var hash = 5381;
                hash = ((hash << 5) + hash) ^ BaseType.GetHashCode();
                hash = ((hash << 5) + hash) ^ (AttributeType ?? typeof (TypeListKey)).GetHashCode();
                return hash;
            }
        }

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
        /// Represents the error that occurs when a type was not found in the cache type
        /// list with the specified TypeResolutionKind.
        /// </summary>
        internal class CachedTypeNotFoundInFileException : Exception
        { }

        #endregion
    }
}
