﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Compilation;
using Umbraco.Core.Cache;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Profiling;
using Umbraco.Core.PropertyEditors;
using umbraco.interfaces;
using File = System.IO.File;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides methods to find and instanciate types.
    /// </summary>
    /// <remarks>
    /// <para>This class should be used to resolve all types, the <see cref="TypeFinder"/> class should never be used directly.</para>
    /// <para>In most cases this class is not used directly but through extension methods that retrieve specific types.</para>
    /// <para>This class caches the types it knows to avoid excessive assembly scanning and shorten startup times, relying
    /// on a hash of the DLLs in the ~/bin folder to check for cache expiration.</para>
    /// </remarks>
    public class PluginManager
    {
        private const string CacheKey = "umbraco-plugins.list";

        private static PluginManager _current;
        private static bool _hasCurrent;
        private static object _currentLock = new object();

        private readonly IServiceProvider _serviceProvider;
        private readonly IRuntimeCacheProvider _runtimeCache;
        private readonly ProfilingLogger _logger;
        private readonly string _tempFolder;

        private readonly object _typesLock = new object();
        private readonly Dictionary<TypeListKey, TypeList> _types = new Dictionary<TypeListKey, TypeList>();

        private long _cachedAssembliesHash = -1;
        private long _currentAssembliesHash = -1;
        private IEnumerable<Assembly> _assemblies;
        private bool _reportedChange;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginManager"/> class.
        /// </summary>
        /// <param name="serviceProvider">A mechanism for retrieving service objects.</param>
        /// <param name="runtimeCache">The application runtime cache.</param>
        /// <param name="logger">A profiling logger.</param>
        /// <param name="detectChanges">Whether to detect changes using hashes.</param>
        internal PluginManager(IServiceProvider serviceProvider, IRuntimeCacheProvider runtimeCache, ProfilingLogger logger, bool detectChanges = true)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");
            if (runtimeCache == null) throw new ArgumentNullException("runtimeCache");
            if (logger == null) throw new ArgumentNullException("logger");

            _serviceProvider = serviceProvider;
            _runtimeCache = runtimeCache;
            _logger = logger;

            // the temp folder where the cache file lives
            _tempFolder = IOHelper.MapPath("~/App_Data/TEMP/PluginCache");
            if (Directory.Exists(_tempFolder) == false)
                Directory.CreateDirectory(_tempFolder);

            var pluginListFile = GetPluginListFilePath();

            if (detectChanges)
            {
                //first check if the cached hash is 0, if it is then we ne
                //do the check if they've changed
                RequiresRescanning = (CachedAssembliesHash != CurrentAssembliesHash) || CachedAssembliesHash == 0;
                //if they have changed, we need to write the new file
                if (RequiresRescanning)
                {
                    // if the hash has changed, clear out the persisted list no matter what, this will force
                    // rescanning of all plugin types including lazy ones.
                    // http://issues.umbraco.org/issue/U4-4789
                    File.Delete(pluginListFile);

                    WriteCachePluginsHash();
                }
            }
            else
            {
                // if the hash has changed, clear out the persisted list no matter what, this will force
                // rescanning of all plugin types including lazy ones.
                // http://issues.umbraco.org/issue/U4-4789
                File.Delete(pluginListFile);

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
        internal IEnumerable<Assembly> AssembliesToScan
        {
            get { return _assemblies ?? (_assemblies = TypeFinder.GetAssembliesWithKnownExclusions()); }
            set { _assemblies = value; }
        }

        /// <summary>
        /// Gets the type lists.
        /// </summary>
        /// <remarks>For unit tests.</remarks>
        internal IEnumerable<TypeList> TypeLists
        {
            get { return _types.Values; }
        }

        /// <summary>
        /// Sets a type list.
        /// </summary>
        /// <remarks>For unit tests.</remarks>
        internal void AddTypeList(TypeList typeList)
        {
            _types[new TypeListKey(typeList.BaseType, typeList.AttributeType)] = typeList;
        }

        /// <summary>
        /// Gets or sets the singleton instance.
        /// </summary>
        /// <remarks>The setter exists for unit tests.</remarks>
        public static PluginManager Current
        {
            get
            {
                return LazyInitializer.EnsureInitialized(ref _current, ref _hasCurrent, ref _currentLock, () =>
                {
                    IRuntimeCacheProvider runtimeCache;
                    ProfilingLogger profilingLogger;

                    if (ApplicationContext.Current == null)
                    {
                        runtimeCache = new NullCacheProvider();
                        var logger = LoggerResolver.HasCurrent ? LoggerResolver.Current.Logger : new DebugDiagnosticsLogger();
                        var profiler = ProfilerResolver.HasCurrent ? ProfilerResolver.Current.Profiler : new LogProfiler(logger);
                        profilingLogger = new ProfilingLogger(logger, profiler);
                    }
                    else
                    {
                        runtimeCache = ApplicationContext.Current.ApplicationCache.RuntimeCache;
                        profilingLogger = ApplicationContext.Current.ProfilingLogger;
                    }

                    return new PluginManager(new ActivatorServiceProvider(), runtimeCache, profilingLogger);
                });
            }
            set
            {
                _hasCurrent = true;
                _current = value;
            }
        }

        #region Hashing

        /// <summary>
        /// Gets a value indicating whether the assemblies in bin, app_code, global.asax, etc... have changed since they were last hashed.
        /// </summary>
        internal bool RequiresRescanning { get; private set; }

        /// <summary>
        /// Gets the currently cached hash value of the scanned assemblies.
        /// </summary>
        /// <value>The cached hash value, or 0 if no cache is found.</value>
        internal long CachedAssembliesHash
        {
            get
            {
                if (_cachedAssembliesHash != -1)
                    return _cachedAssembliesHash;

                var filePath = GetPluginHashFilePath();
                if (File.Exists(filePath) == false) return 0;

                var hash = File.ReadAllText(filePath, Encoding.UTF8);

                long val;
                if (long.TryParse(hash, out val) == false) return 0;

                _cachedAssembliesHash = val;
                return _cachedAssembliesHash;
            }
        }

        /// <summary>
        /// Gets the current assemblies hash based on creating a hash from the assemblies in various places.
        /// </summary>
        /// <value>The current hash.</value>
        internal long CurrentAssembliesHash
        {
            get
            {
                if (_currentAssembliesHash != -1)
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
        private void WriteCachePluginsHash()
        {
            var filePath = GetPluginHashFilePath();
            File.WriteAllText(filePath, CurrentAssembliesHash.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Returns a unique hash for a combination of FileInfo objects.
        /// </summary>
        /// <param name="filesAndFolders">A collection of files.</param>
        /// <param name="logger">A profiling logger.</param>
        /// <returns>The hash.</returns>
        /// <remarks>Each file is a tuple containing the FileInfo object and a boolean which indicates whether to hash the
        /// file properties (false) or the file contents (true).</remarks>
        internal static long GetFileHash(IEnumerable<Tuple<FileSystemInfo, bool>> filesAndFolders, ProfilingLogger logger)
        {
            using (logger.TraceDuration<PluginManager>("Determining hash of code files on disk", "Hash determined"))
            {
                var hashCombiner = new HashCodeCombiner();

                // get the distinct file infos to hash
                var uniqInfos = new HashSet<string>();
                var uniqContent = new HashSet<string>();

                foreach (var fileOrFolder in filesAndFolders)
                {
                    var info = fileOrFolder.Item1;
                    if (fileOrFolder.Item2)
                    {
                        // add each unique file's contents to the hash
                        // normalize the content for cr/lf and case-sensitivity

                        if (uniqContent.Contains(info.FullName)) continue;
                        uniqContent.Add(info.FullName);
                        if (File.Exists(info.FullName) == false) continue;
                        var content = RemoveCrLf(File.ReadAllText(info.FullName));
                        hashCombiner.AddCaseInsensitiveString(content);
                    }
                    else
                    {
                        // add each unique folder/file to the hash

                        if (uniqInfos.Contains(info.FullName)) continue;
                        uniqInfos.Add(info.FullName);
                        hashCombiner.AddFileSystemItem(info);
                    }
                } 

                return ConvertHashToInt64(hashCombiner.GetCombinedHashCode());
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
        internal static long GetFileHash(IEnumerable<FileSystemInfo> filesAndFolders, ProfilingLogger logger)
        {
            using (logger.TraceDuration<PluginManager>("Determining hash of code files on disk", "Hash determined"))
            {
                var hashCombiner = new HashCodeCombiner();

                // get the distinct file infos to hash
                var uniqInfos = new HashSet<string>();

                foreach (var fileOrFolder in filesAndFolders)
                {
                    if (uniqInfos.Contains(fileOrFolder.FullName)) continue;
                    uniqInfos.Add(fileOrFolder.FullName);
                    hashCombiner.AddFileSystemItem(fileOrFolder);
                }

                return ConvertHashToInt64(hashCombiner.GetCombinedHashCode());
            }
        }

        /// <summary>
        /// Converts a string hash value into an Int64.
        /// </summary>
        internal static long ConvertHashToInt64(string val)
        {
            long outVal;
            return long.TryParse(val, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out outVal) ? outVal : 0;
        }

        #endregion

        #region Cache

        /// <summary>
        /// Attemps to retrieve the list of types from the cache.
        /// </summary>
        /// <remarks>Fails if the cache is missing or corrupt in any way.</remarks>
        internal Attempt<IEnumerable<string>> TryGetCached(Type baseType, Type attributeType)
        {
            var cache = _runtimeCache.GetCacheItem<Dictionary<Tuple<string, string>, IEnumerable<string>>>(CacheKey, ReadCacheSafe, TimeSpan.FromMinutes(4));

            IEnumerable<string> types;
            cache.TryGetValue(Tuple.Create(baseType == null ? string.Empty : baseType.FullName, attributeType == null ? string.Empty : attributeType.FullName), out types);
            return types == null
                ? Attempt<IEnumerable<string>>.Fail()
                : Attempt.Succeed(types);
        }

        internal Dictionary<Tuple<string, string>, IEnumerable<string>> ReadCacheSafe()
        {
            try
            {
                return ReadCache();
            }
            catch
            {
                try
                {
                    var filePath = GetPluginListFilePath();
                    File.Delete(filePath);
                }
                catch
                {
                    // on-purpose, does not matter
                }
            }

            return new Dictionary<Tuple<string, string>, IEnumerable<string>>();
        }

        internal Dictionary<Tuple<string, string>, IEnumerable<string>> ReadCache()
        {
            var cache = new Dictionary<Tuple<string, string>, IEnumerable<string>>();

            var filePath = GetPluginListFilePath();
            if (File.Exists(filePath) == false)
                return cache;

            using (var stream = File.OpenRead(filePath))
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

        /// <summary>
        /// Removes cache files and internal cache.
        /// </summary>
        /// <remarks>Generally only used for resetting cache, for example during the install process.</remarks>
        public void ClearPluginCache()
        {
            var path = GetPluginListFilePath();
            if (File.Exists(path))
                File.Delete(path);

            path = GetPluginHashFilePath();
            if (File.Exists(path))
                File.Delete(path);

            _runtimeCache.ClearCacheItem(CacheKey);
        }

        private string GetPluginListFilePath()
        {
            var filename = "umbraco-plugins." + NetworkHelper.FileSafeMachineName + ".list";
            return Path.Combine(_tempFolder, filename);
        }

        private string GetPluginHashFilePath()
        {
            var filename = "umbraco-plugins." + NetworkHelper.FileSafeMachineName + ".hash";
            return Path.Combine(_tempFolder, filename);
        }

        internal void WriteCache()
        {
            var filePath = GetPluginListFilePath();

            using (var stream = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite))
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

        internal void UpdateCache()
        {
            // note
            // at the moment we write the cache to disk every time we update it. ideally we defer the writing
            // since all the updates are going to happen in a row when Umbraco starts. that being said, the
            // file is small enough, so it is not a priority.
            WriteCache();            
        }

        #endregion

        #region Create Instances

        /// <summary>
        /// Resolves and creates instances.
        /// </summary>
        /// <typeparam name="T">The type to use for resolution.</typeparam>
        /// <param name="throwException">Indicates whether to throw if an instance cannot be created.</param>
        /// <param name="cache">Indicates whether to use cache for type resolution.</param>
        /// <param name="specificAssemblies">A set of assemblies for type resolution.</param>
        /// <returns>The created instances.</returns>
        /// <remarks>
        /// <para>By default <paramref name="throwException"/> is false and instances that cannot be created are just skipped.</para>
        /// <para>By default <paramref name="cache"/> is true and cache is used for type resolution.</para>
        /// <para>By default <paramref name="specificAssemblies"/> is null and <see cref="AssembliesToScan"/> is used.</para>
        /// <para>Caching is disabled when using specific assemblies.</para>
        /// </remarks>
        internal IEnumerable<T> FindAndCreateInstances<T>(bool throwException = false, bool cache = true, IEnumerable<Assembly> specificAssemblies = null)
        {
            var types = ResolveTypes<T>(cache, specificAssemblies);
            return CreateInstances<T>(types, throwException);
        }

        /// <summary>
        /// Creates instances of the specified types.
        /// </summary>
        /// <typeparam name="T">The base type for all instances.</typeparam>
        /// <param name="types">The instance types.</param>
        /// <param name="throwException">Indicates whether to throw if an instance cannot be created.</param>
        /// <returns>The created instances.</returns>
        /// <remarks>By default <paramref name="throwException"/> is false and instances that cannot be created are just skipped.</remarks>
        internal IEnumerable<T> CreateInstances<T>(IEnumerable<Type> types, bool throwException = false)
        {
            return _serviceProvider.CreateInstances<T>(types, _logger.Logger, throwException);
        }

        /// <summary>
        /// Creates an instance of the specified type.
        /// </summary>
        /// <typeparam name="T">The base type of the instance.</typeparam>
        /// <param name="type">The type of the instance.</param>
        /// <param name="throwException"></param>
        /// <returns>The created instance.</returns>
        internal T CreateInstance<T>(Type type, bool throwException = false)
        {
            var instances = CreateInstances<T>(new[] { type }, throwException);
            return instances.FirstOrDefault();
        }

        #endregion

        #region Resolve Types

        /// <summary>
        /// Resolves class types inheriting from or implementing the specified type
        /// </summary>
        /// <typeparam name="T">The type to inherit from or implement.</typeparam>
        /// <param name="cache">Indicates whether to use cache for type resolution.</param>
        /// <param name="specificAssemblies">A set of assemblies for type resolution.</param>
        /// <returns>All class types inheriting from or implementing the specified type.</returns>
        /// <remarks>Caching is disabled when using specific assemblies.</remarks>
        public IEnumerable<Type> ResolveTypes<T>(bool cache = true, IEnumerable<Assembly> specificAssemblies = null)
        {
            // do not cache anything from specific assemblies
            cache &= specificAssemblies == null;

            // if not caching, or not IDiscoverable, directly resolve types
            if (cache == false || typeof(IDiscoverable).IsAssignableFrom(typeof(T)) == false)
            {
                return ResolveTypesInternal(
                    typeof (T), null,
                    () => TypeFinder.FindClassesOfType<T>(specificAssemblies ?? AssembliesToScan),
                    cache);
            }

            // if caching and IDiscoverable
            // filter the cached discovered types (and cache the result)

            var discovered = ResolveTypesInternal(
                typeof (IDiscoverable), null,
                () => TypeFinder.FindClassesOfType<IDiscoverable>(AssembliesToScan),
                true);

            return ResolveTypesInternal(
                typeof (T), null,
                () => discovered
                    .Where(x => typeof (T).IsAssignableFrom(x)),
                true);
        }

        /// <summary>
        /// Resolves class types inheriting from or implementing the specified type and marked with the specified attribute.
        /// </summary>
        /// <typeparam name="T">The type to inherit from or implement.</typeparam>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="cache">Indicates whether to use cache for type resolution.</param>
        /// <param name="specificAssemblies">A set of assemblies for type resolution.</param>
        /// <returns>All class types inheriting from or implementing the specified type and marked with the specified attribute.</returns>
        /// <remarks>Caching is disabled when using specific assemblies.</remarks>
        public IEnumerable<Type> ResolveTypesWithAttribute<T, TAttribute>(bool cache = true, IEnumerable<Assembly> specificAssemblies = null)
            where TAttribute : Attribute
        {
            // do not cache anything from specific assemblies
            cache &= specificAssemblies == null;

            // if not caching, or not IDiscoverable, directly resolve types
            if (cache == false || typeof(IDiscoverable).IsAssignableFrom(typeof(T)) == false)
            {
                return ResolveTypesInternal(
                    typeof (T), typeof (TAttribute),
                    () => TypeFinder.FindClassesOfTypeWithAttribute<T, TAttribute>(specificAssemblies ?? AssembliesToScan),
                    cache);
            }

            // if caching and IDiscoverable
            // filter the cached discovered types (and cache the result)

            var discovered = ResolveTypesInternal(
                typeof (IDiscoverable), null,
                () => TypeFinder.FindClassesOfType<IDiscoverable>(AssembliesToScan),
                true);

            return ResolveTypesInternal(
                typeof (T), typeof (TAttribute),
                () => discovered
                    .Where(x => typeof(T).IsAssignableFrom(x))
                    .Where(x => x.GetCustomAttributes<TAttribute>(false).Any()),
                true);
        }

        /// <summary>
        /// Resolves class types marked with the specified attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="cache">Indicates whether to use cache for type resolution.</param>
        /// <param name="specificAssemblies">A set of assemblies for type resolution.</param>
        /// <returns>All class types marked with the specified attribute.</returns>
        /// <remarks>Caching is disabled when using specific assemblies.</remarks>
        public IEnumerable<Type> ResolveAttributedTypes<TAttribute>(bool cache = true, IEnumerable<Assembly> specificAssemblies = null)
            where TAttribute : Attribute
        {
            // do not cache anything from specific assemblies
            cache &= specificAssemblies == null;

            return ResolveTypesInternal(
                typeof (object), typeof (TAttribute),
                () => TypeFinder.FindClassesWithAttribute<TAttribute>(specificAssemblies ?? AssembliesToScan),
                cache);
        }

        private IEnumerable<Type> ResolveTypesInternal(
            Type baseType, Type attributeType,
            Func<IEnumerable<Type>> finder,
            bool cache)
        {
            // using an upgradeable lock makes little sense here as only one thread can enter the upgradeable
            // lock at a time, and we don't have non-upgradeable readers, and quite probably the plugin
            // manager is mostly not going to be used in any kind of massively multi-threaded scenario - so,
            // a plain lock is enough

            var name = ResolvedName(baseType, attributeType);

            lock (_typesLock)
            using (_logger.TraceDuration<PluginManager>(
                "Resolving " + name,
                "Resolved " + name)) // cannot contain typesFound.Count as it's evaluated before the find
            {
                // resolve within a lock & timer
                return ResolveTypesInternalLocked(baseType, attributeType, finder, cache);
            }
        }

        private static string ResolvedName(Type baseType, Type attributeType)
        {
            var s = attributeType == null ? string.Empty : ("[" + attributeType + "]");
            s += baseType;
            return s;
        }

        private IEnumerable<Type> ResolveTypesInternalLocked(
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
                _logger.Logger.Debug<PluginManager>("Resolving {0}: found a cached type list.", () => ResolvedName(baseType, attributeType));
                return typeList.Types;
            }

            // else proceed,
            typeList = new TypeList(baseType, attributeType);

            var scan = RequiresRescanning || File.Exists(GetPluginListFilePath()) == false;

            if (scan)
            {
                // either we have to rescan, or we could not find the cache file:
                // report (only once) and scan and update the cache file - this variable is purely used to check if we need to log
                if (_reportedChange == false)
                {
                    _logger.Logger.Debug<PluginManager>("Assembly changes detected, need to rescan everything.");
                    _reportedChange = true;
                }
            }

            if (scan == false)
            {
                // if we don't have to scan, try the cache
                var cacheResult = TryGetCached(baseType, attributeType);

                // here we need to identify if the CachedPluginNotFoundInFile was the exception, if it was then we need to re-scan
                // in some cases the plugin will not have been scanned for on application startup, but the assemblies haven't changed
                // so in this instance there will never be a result.
                if (cacheResult.Exception is CachedPluginNotFoundInFileException || cacheResult.Success == false)
                {
                    _logger.Logger.Debug<PluginManager>("Resolving {0}: failed to load from cache file, must scan assemblies.", () => ResolvedName(baseType, attributeType));
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
                            _logger.Logger.Error<PluginManager>("Resolving " + ResolvedName(baseType, attributeType) + ": failed to load cache file type " + type + ", reverting to scanning assemblies.", ex);
                            scan = true;
                            break;
                        }
                    }

                    if (scan == false)
                    {
                        _logger.Logger.Debug<PluginManager>("Resolving {0}: loaded types from cache file.", () => ResolvedName(baseType, attributeType));
                    }
                }
            }

            if (scan)
            {
                // either we had to scan, or we could not resolve the types from the cache file - scan now
                _logger.Logger.Debug<PluginManager>("Resolving {0}: scanning assemblies.", () => ResolvedName(baseType, attributeType));

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
                    {
                        UpdateCache();
                    }
                }

                _logger.Logger.Debug<PluginManager>("Resolved {0}, caching ({1}).", () => ResolvedName(baseType, attributeType), () => added.ToString().ToLowerInvariant());
            }
            else
            {
                _logger.Logger.Debug<PluginManager>("Resolved {0}.", () => ResolvedName(baseType, attributeType));
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

            public Type BaseType { get; private set; }
            public Type AttributeType { get; private set; }

            /// <summary>
            /// Adds a type.
            /// </summary>
            public void Add(Type type)
            {
                if (BaseType.IsAssignableFrom(type) == false)
                    throw new ArgumentException("Base type " + BaseType + " is not assignable from type " + type + ".", "type");
                _types.Add(type);
            }

            /// <summary>
            /// Gets the types.
            /// </summary>
            public IEnumerable<Type> Types
            {
                get { return _types; }
            }
        }

        /// <summary>
        /// Represents the error that occurs when a plugin was not found in the cache plugin
        /// list with the specified TypeResolutionKind.
        /// </summary>
        internal class CachedPluginNotFoundInFileException : Exception
        { }

        #endregion
    }

    internal static class PluginManagerExtensions
    {
        /// <summary>
        /// Gets all classes inheriting from PropertyEditor.
        /// </summary>
        /// <remarks>
        /// <para>Excludes the actual PropertyEditor base type.</para>
        /// </remarks>
        public static IEnumerable<Type> ResolvePropertyEditors(this PluginManager mgr)
        {
            // look for IParameterEditor (fast, IDiscoverable) then filter

            var propertyEditor = typeof (PropertyEditor);

            return mgr.ResolveTypes<IParameterEditor>()
                .Where(x => propertyEditor.IsAssignableFrom(x) && x != propertyEditor);
        }

        /// <summary>
        /// Gets all classes implementing IParameterEditor.
        /// </summary>
        /// <remarks>
        /// <para>Includes property editors.</para>
        /// <para>Excludes the actual ParameterEditor and PropertyEditor base types.</para>
        /// </remarks>
        public static IEnumerable<Type> ResolveParameterEditors(this PluginManager mgr)
        {
            var propertyEditor = typeof (PropertyEditor);
            var parameterEditor = typeof (ParameterEditor);

            return mgr.ResolveTypes<IParameterEditor>()
                .Where(x => x != propertyEditor && x != parameterEditor);
        }

        /// <summary>
        /// Gets all classes implementing IApplicationStartupHandler.
        /// </summary>
        [Obsolete("IApplicationStartupHandler is obsolete.")]
        public static IEnumerable<Type> ResolveApplicationStartupHandlers(this PluginManager mgr)
        {
            return mgr.ResolveTypes<IApplicationStartupHandler>();
        }

        /// <summary>
        /// Gets all classes implementing ICacheRefresher.
        /// </summary>
        public static IEnumerable<Type> ResolveCacheRefreshers(this PluginManager mgr)
        {
            return mgr.ResolveTypes<ICacheRefresher>();
        }

        /// <summary>
        /// Gets all classes implementing IPropertyEditorValueConverter.
        /// </summary>
        [Obsolete("IPropertyEditorValueConverter is obsolete.")]
        public static IEnumerable<Type> ResolvePropertyEditorValueConverters(this PluginManager mgr)
        {
            return mgr.ResolveTypes<IPropertyEditorValueConverter>();
        }

        /// <summary>
        /// Gets all classes implementing IDataType.
        /// </summary>
        [Obsolete("IDataType is obsolete.")]
        public static IEnumerable<Type> ResolveDataTypes(this PluginManager mgr)
        {
            return mgr.ResolveTypes<IDataType>();
        }

        /// <summary>
        /// Gets all classes implementing IMacroGuiRendering.
        /// </summary>
        [Obsolete("IMacroGuiRendering is obsolete.")]
        public static IEnumerable<Type> ResolveMacroRenderings(this PluginManager mgr)
        {
            return mgr.ResolveTypes<IMacroGuiRendering>();
        }

        /// <summary>
        /// Gets all classes implementing IPackageAction.
        /// </summary>
        public static IEnumerable<Type> ResolvePackageActions(this PluginManager mgr)
        {
            return mgr.ResolveTypes<IPackageAction>();
        }

        /// <summary>
        /// Gets all classes implementing IAction.
        /// </summary>
        public static IEnumerable<Type> ResolveActions(this PluginManager mgr)
        {
            return mgr.ResolveTypes<IAction>();
        }

        /// <summary>
        /// Gets all classes inheriting from BaseMapper and marked with the MapperForAttribute.
        /// </summary>
        public static IEnumerable<Type> ResolveAssignedMapperTypes(this PluginManager mgr)
        {
            return mgr.ResolveTypesWithAttribute<BaseMapper, MapperForAttribute>();
        }

        /// <summary>
        /// Gets all classes implementing ISqlSyntaxProvider and marked with the SqlSyntaxProviderAttribute.
        /// </summary>
        public static IEnumerable<Type> ResolveSqlSyntaxProviders(this PluginManager mgr)
        {
            return mgr.ResolveTypesWithAttribute<ISqlSyntaxProvider, SqlSyntaxProviderAttribute>();
        }
    }
}
