using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Compilation;
using System.Xml.Linq;
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

        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        private static PluginManager _current;
        private static bool _hasCurrent;
        private static object _currentLock = new object();

        private readonly IServiceProvider _serviceProvider;
        private readonly IRuntimeCacheProvider _runtimeCache;
        private readonly ProfilingLogger _logger;
        private readonly string _tempFolder;
        private readonly HashSet<TypeList> _types = new HashSet<TypeList>();

        private long _cachedAssembliesHash = -1;
        private long _currentAssembliesHash = -1;
        private IEnumerable<Assembly> _assemblies;
        private HashSet<Type> _extensions;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginManager"/> class.
        /// Creates a new PluginManager with an ApplicationContext instance which ensures that the plugin xml
        /// file is cached temporarily until app startup completes. fixme?
        /// </summary>
        /// <param name="serviceProvider">A mechanism for retrieving service objects.</param>
        /// <param name="runtimeCache">The application runtime cache.</param>
        /// <param name="logger">A profiling logger.</param>
        /// <param name="detectChanges">fixme</param>
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

            //this is a check for legacy changes, before we didn't store the TypeResolutionKind in the file which was a mistake,
            //so we need to detect if the old file is there without this attribute, if it is then we delete it
            if (DetectLegacyPluginListFile())
            {
                File.Delete(pluginListFile);
            }

            if (detectChanges)
            {
                //first check if the cached hash is 0, if it is then we ne
                //do the check if they've changed
                RequiresRescanning = (CachedAssembliesHash != CurrentAssembliesHash) || CachedAssembliesHash == 0;
                //if they have changed, we need to write the new file
                if (RequiresRescanning)
                {
                    //if the hash has changed, clear out the persisted list no matter what, this will force
                    // rescanning of all plugin types including lazy ones.
                    // http://issues.umbraco.org/issue/U4-4789
                    File.Delete(pluginListFile);

                    WriteCachePluginsHash();
                }
            }
            else
            {

                //if the hash has changed, clear out the persisted list no matter what, this will force
                // rescanning of all plugin types including lazy ones.
                // http://issues.umbraco.org/issue/U4-4789
                File.Delete(pluginListFile);

                //always set to true if we're not detecting (generally only for testing)
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

        #region Hash checking methods


        /// <summary>
        /// Returns a bool if the assemblies in the /bin, app_code, global.asax, etc... have changed since they were last hashed.
        /// </summary>
        internal bool RequiresRescanning { get; private set; }

        /// <summary>
        /// Returns the currently cached hash value of the scanned assemblies in the /bin folder. Returns 0
        /// if no cache is found.
        /// </summary>
        /// <value> </value>
        internal long CachedAssembliesHash
        {
            get
            {
                if (_cachedAssembliesHash != -1)
                    return _cachedAssembliesHash;

                var filePath = GetPluginHashFilePath();
                if (!File.Exists(filePath))
                    return 0;
                var hash = File.ReadAllText(filePath, Encoding.UTF8);
                Int64 val;
                if (Int64.TryParse(hash, out val))
                {
                    _cachedAssembliesHash = val;
                    return _cachedAssembliesHash;
                }
                //it could not parse for some reason so we'll return 0.
                return 0;
            }
        }

        /// <summary>
        /// Returns the current assemblies hash based on creating a hash from the assemblies in the /bin
        /// </summary>
        /// <value> </value>
        internal long CurrentAssembliesHash
        {
            get
            {
                if (_currentAssembliesHash != -1)
                    return _currentAssembliesHash;

                _currentAssembliesHash = GetFileHash(
                    new List<Tuple<FileSystemInfo, bool>>
						{
							//add the bin folder and everything in it
							new Tuple<FileSystemInfo, bool>(new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Bin)), false),
							//add the app code folder and everything in it
							new Tuple<FileSystemInfo, bool>(new DirectoryInfo(IOHelper.MapPath("~/App_Code")), false),
							//add the global.asax (the app domain also monitors this, if it changes will do a full restart)
							new Tuple<FileSystemInfo, bool>(new FileInfo(IOHelper.MapPath("~/global.asax")), false),

                            //add the trees.config - use the contents to create the has since this gets resaved on every app startup!
                            new Tuple<FileSystemInfo, bool>(new FileInfo(IOHelper.MapPath(SystemDirectories.Config + "/trees.config")), true)
						}, _logger
                    );
                return _currentAssembliesHash;
            }
        }

        /// <summary>
        /// Writes the assembly hash file
        /// </summary>
        private void WriteCachePluginsHash()
        {
            var filePath = GetPluginHashFilePath();
            File.WriteAllText(filePath, CurrentAssembliesHash.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Returns a unique hash for the combination of FileInfo objects passed in
        /// </summary>
        /// <param name="filesAndFolders">
        /// A collection of files and whether or not to use their file contents to determine the hash or the file's properties
        /// (true will make a hash based on it's contents)
        /// </param>
        /// <returns></returns>
        internal static long GetFileHash(IEnumerable<Tuple<FileSystemInfo, bool>> filesAndFolders, ProfilingLogger logger)
        {
            using (logger.TraceDuration<PluginManager>("Determining hash of code files on disk", "Hash determined"))
            {
                var hashCombiner = new HashCodeCombiner();

                //get the file info's to check
                var fileInfos = filesAndFolders.Where(x => x.Item2 == false).ToArray();
                var fileContents = filesAndFolders.Except(fileInfos);

                //add each unique folder/file to the hash
                foreach (var i in fileInfos.Select(x => x.Item1).DistinctBy(x => x.FullName))
                {
                    hashCombiner.AddFileSystemItem(i);
                }

                //add each unique file's contents to the hash
                foreach (var i in fileContents.Select(x => x.Item1).DistinctBy(x => x.FullName))
                {
                    if (File.Exists(i.FullName))
                    {
                        var content = File.ReadAllText(i.FullName).Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
                        hashCombiner.AddCaseInsensitiveString(content);
                    }

                }

                return ConvertPluginsHashFromHex(hashCombiner.GetCombinedHashCode());
            }
        }

        internal static long GetFileHash(IEnumerable<FileSystemInfo> filesAndFolders, ProfilingLogger logger)
        {
            using (logger.TraceDuration<PluginManager>("Determining hash of code files on disk", "Hash determined"))
            {
                var hashCombiner = new HashCodeCombiner();

                //add each unique folder/file to the hash
                foreach (var i in filesAndFolders.DistinctBy(x => x.FullName))
                {
                    hashCombiner.AddFileSystemItem(i);
                }
                return ConvertPluginsHashFromHex(hashCombiner.GetCombinedHashCode());
            }
        }

        /// <summary>
        /// Converts the hash value of current plugins to long from string
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        internal static long ConvertPluginsHashFromHex(string val)
        {
            long outVal;
            if (Int64.TryParse(val, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out outVal))
            {
                return outVal;
            }
            return 0;
        }

        /// <summary>
        /// Attempts to resolve the list of plugin + assemblies found in the runtime for the base type 'T' passed in.
        /// If the cache file doesn't exist, fails to load, is corrupt or the type 'T' element is not found then
        /// a false attempt is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal Attempt<IEnumerable<string>> TryGetCachedPluginsFromFile<T>(TypeResolutionKind resolutionType)
        {
            var filePath = GetPluginListFilePath();
            if (!File.Exists(filePath))
                return Attempt<IEnumerable<string>>.Fail();

            try
            {
                //we will load the xml document, if the app context exist, we will load it from the cache (which is only around for 5 minutes)
                //while the app boots up, this should save some IO time on app startup when the app context is there (which is always unless in unit tests)
                var xml = _runtimeCache.GetCacheItem<XDocument>(CacheKey,
                    () => XDocument.Load(filePath),
                    new TimeSpan(0, 0, 5, 0));

                if (xml.Root == null)
                    return Attempt<IEnumerable<string>>.Fail();

                var typeElement = xml.Root.Elements()
                    .FirstOrDefault(x =>
                                     x.Name.LocalName == "baseType"
                                     && ((string)x.Attribute("type")) == typeof(T).FullName
                                     && ((string)x.Attribute("resolutionType")) == resolutionType.ToString());

                //return false but specify this exception type so we can detect it
                if (typeElement == null)
                    return Attempt<IEnumerable<string>>.Fail(new CachedPluginNotFoundInFileException());

                //return success
                return Attempt.Succeed(typeElement.Elements("add")
                        .Select(x => (string)x.Attribute("type")));
            }
            catch (Exception ex)
            {
                //if the file is corrupted, etc... return false
                return Attempt<IEnumerable<string>>.Fail(ex);
            }
        }

        /// <summary>
        /// Removes cache files and internal cache as well
        /// </summary>
        /// <remarks>
        /// Generally only used for resetting cache, for example during the install process
        /// </remarks>
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
            return Path.Combine(_tempFolder, string.Format("umbraco-plugins.{0}.list", NetworkHelper.FileSafeMachineName));
        }

        private string GetPluginHashFilePath()
        {
            return Path.Combine(_tempFolder, string.Format("umbraco-plugins.{0}.hash", NetworkHelper.FileSafeMachineName));
        }

        /// <summary>
        /// This will return true if the plugin list file is a legacy one
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This method exists purely due to an error in 4.11. We were writing the plugin list file without the
        /// type resolution kind which will have caused some problems. Now we detect this legacy file and if it is detected
        /// we remove it so it can be recreated properly.
        /// </remarks>
        internal bool DetectLegacyPluginListFile()
        {
            var filePath = GetPluginListFilePath();
            if (!File.Exists(filePath))
                return false;

            try
            {
                var xml = XDocument.Load(filePath);
                if (xml.Root == null)
                    return false;

                var typeElement = xml.Root.Elements()
                    .FirstOrDefault(x => x.Name.LocalName == "baseType");

                if (typeElement == null)
                    return false;

                //now check if the typeElement is missing the resolutionType attribute
                return typeElement.Attributes().All(x => x.Name.LocalName != "resolutionType");
            }
            catch (Exception)
            {
                //if the file is corrupted, etc... return true so it is removed
                return true;
            }
        }

        /// <summary>
        /// Adds/Updates the type list for the base type 'T' in the cached file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="typesFound"></param>
        ///<param name="resolutionType"> </param>
        ///<remarks>
        /// THIS METHOD IS NOT THREAD SAFE
        /// </remarks>
        /// <example>
        /// <![CDATA[
        /// <plugins>
        ///		<baseType type="Test.Testing.Tester">
        ///			<add type="My.Assembly.MyTester" assembly="My.Assembly" />
        ///			<add type="Your.Assembly.YourTester" assembly="Your.Assembly" />
        ///		</baseType>
        /// </plugins>
        /// ]]>
        /// </example>
        internal void UpdateCachedPluginsFile<T>(IEnumerable<Type> typesFound, TypeResolutionKind resolutionType)
        {
            var filePath = GetPluginListFilePath();
            XDocument xml;
            try
            {
                xml = XDocument.Load(filePath);
            }
            catch
            {
                //if there's an exception loading then this is somehow corrupt, we'll just replace it.
                File.Delete(filePath);
                //create the document and the root
                xml = new XDocument(new XElement("plugins"));
            }
            if (xml.Root == null)
            {
                //if for some reason there is no root, create it
                xml.Add(new XElement("plugins"));
            }
            //find the type 'T' element to add or update
            var typeElement = xml.Root.Elements()
                .SingleOrDefault(x =>
                                 x.Name.LocalName == "baseType"
                                 && ((string)x.Attribute("type")) == typeof(T).FullName
                                 && ((string)x.Attribute("resolutionType")) == resolutionType.ToString());

            if (typeElement == null)
            {
                //create the type element
                typeElement = new XElement("baseType",
                    new XAttribute("type", typeof(T).FullName),
                    new XAttribute("resolutionType", resolutionType.ToString()));
                //then add it to the root
                xml.Root.Add(typeElement);
            }


            //now we have the type element, we need to clear any previous types as children and add/update it with new ones
            typeElement.ReplaceNodes(typesFound.Select(x => new XElement("add", new XAttribute("type", x.AssemblyQualifiedName))));
            //save the xml file
            xml.Save(filePath);
        }

        #endregion

        #region Create Instances

        /// <summary>
        /// Resolves and creates instances.
        /// </summary>
        /// <typeparam name="T">The type to use for resolution.</typeparam>
        /// <param name="throwException">Indicates whether to throw if an instance cannot be created.</param>
        /// <param name="cacheResult">Indicates whether to use cache for type resolution.</param>
        /// <param name="specificAssemblies">A set of assemblies for type resolution.</param>
        /// <returns>The created instances.</returns>
        /// <remarks>
        /// <para>By default <paramref name="throwException"/> is false and instances that cannot be created are just skipped.</para>
        /// <para>By default <paramref name="cacheResult"/> is true and cache is used for type resolution.</para>
        /// <para>By default <paramref name="specificAssemblies"/> is null and <see cref="AssembliesToScan"/> is used.</para>
        //fixme if we specify assemblies we should not cache?
        /// </remarks>
        internal IEnumerable<T> FindAndCreateInstances<T>(bool throwException = false, bool cacheResult = true, IEnumerable<Assembly> specificAssemblies = null)
        {
            var types = ResolveTypes<T>(cacheResult, specificAssemblies);
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

        private IEnumerable<Type> ResolveTypesInternal<T>(
            Func<IEnumerable<Type>> finder,
            TypeResolutionKind resolutionType,
            bool cacheResult)
        {
            using (var readLock = new UpgradeableReadLock(Locker))
            {
                var typesFound = new List<Type>();

                using (_logger.TraceDuration<PluginManager>(
                    String.Format("Starting resolution types of {0}", typeof(T).FullName),
                    String.Format("Completed resolution of types of {0}, found {1}", typeof(T).FullName, typesFound.Count)))
                {
                    //check if the TypeList already exists, if so return it, if not we'll create it
                    var typeList = _types.FirstOrDefault(x => x.IsList<T>(resolutionType));

                    //need to put some logging here to try to figure out why this is happening: http://issues.umbraco.org/issue/U4-3505
                    if (cacheResult && typeList != null)
                    {
                        _logger.Logger.Debug<PluginManager>("Existing typeList found for {0} with resolution type {1}", () => typeof(T), () => resolutionType);
                    }

                    //if we're not caching the result then proceed, or if the type list doesn't exist then proceed
                    if (cacheResult == false || typeList == null)
                    {
                        //upgrade to a write lock since we're adding to the collection
                        readLock.UpgradeToWriteLock();

                        typeList = new TypeList<T>(resolutionType);

                        //we first need to look into our cache file (this has nothing to do with the 'cacheResult' parameter which caches in memory).
                        //if assemblies have not changed and the cache file actually exists, then proceed to try to lookup by the cache file.
                        if (RequiresRescanning == false && File.Exists(GetPluginListFilePath()))
                        {
                            var fileCacheResult = TryGetCachedPluginsFromFile<T>(resolutionType);

                            //here we need to identify if the CachedPluginNotFoundInFile was the exception, if it was then we need to re-scan
                            //in some cases the plugin will not have been scanned for on application startup, but the assemblies haven't changed
                            //so in this instance there will never be a result.
                            if (fileCacheResult.Exception != null && fileCacheResult.Exception is CachedPluginNotFoundInFileException)
                            {
                                _logger.Logger.Debug<PluginManager>("Tried to find typelist for type {0} and resolution {1} in file cache but the type was not found so loading types by assembly scan ", () => typeof(T), () => resolutionType);

                                //we don't have a cache for this so proceed to look them up by scanning
                                LoadViaScanningAndUpdateCacheFile<T>(typeList, resolutionType, finder);
                            }
                            else
                            {
                                if (fileCacheResult.Success)
                                {
                                    var successfullyLoadedFromCache = true;
                                    //we have a previous cache for this so we don't need to scan we just load what has been found in the file
                                    foreach (var t in fileCacheResult.Result)
                                    {
                                        try
                                        {
                                            //we use the build manager to ensure we get all types loaded, this is slightly slower than
                                            //Type.GetType but if the types in the assembly aren't loaded yet then we have problems with that.
                                            var type = BuildManager.GetType(t, true);
                                            typeList.Add(type);
                                        }
                                        catch (Exception ex)
                                        {
                                            //if there are any exceptions loading types, we have to exist, this should never happen so
                                            //we will need to revert to scanning for types.
                                            successfullyLoadedFromCache = false;
                                            _logger.Logger.Error<PluginManager>("Could not load a cached plugin type: " + t + " now reverting to re-scanning assemblies for the base type: " + typeof(T).FullName, ex);
                                            break;
                                        }
                                    }
                                    if (successfullyLoadedFromCache == false)
                                    {
                                        //we need to manually load by scanning if loading from the file was not successful.
                                        LoadViaScanningAndUpdateCacheFile<T>(typeList, resolutionType, finder);
                                    }
                                    else
                                    {
                                        _logger.Logger.Debug<PluginManager>("Loaded plugin types {0} with resolution {1} from persisted cache", () => typeof(T), () => resolutionType);
                                    }
                                }
                            }
                        }
                        else
                        {
                            _logger.Logger.Debug<PluginManager>("Assembly changes detected, loading types {0} for resolution {1} by assembly scan", () => typeof(T), () => resolutionType);

                            //we don't have a cache for this so proceed to look them up by scanning
                            LoadViaScanningAndUpdateCacheFile<T>(typeList, resolutionType, finder);
                        }

                        //only add the cache if we are to cache the results
                        if (cacheResult)
                        {
                            //add the type list to the collection
                            var added = _types.Add(typeList);

                            _logger.Logger.Debug<PluginManager>("Caching of typelist for type {0} and resolution {1} was successful = {2}", () => typeof(T), () => resolutionType, () => added);

                        }
                    }
                    typesFound = typeList.Types.ToList();
                }

                return typesFound;
            }
        }

        #endregion

        /// <summary>
        /// This method invokes the finder which scans the assemblies for the types and then loads the result into the type finder.
        /// Once the results are loaded, we update the cached type xml file
        /// </summary>
        /// <param name="typeList"></param>
        /// <param name="resolutionKind"> </param>
        /// <param name="finder"></param>
        /// <remarks>
        /// THIS METHODS IS NOT THREAD SAFE
        /// </remarks>
        private void LoadViaScanningAndUpdateCacheFile<T>(TypeList typeList, TypeResolutionKind resolutionKind, Func<IEnumerable<Type>> finder)
        {
            //we don't have a cache for this so proceed to look them up by scanning
            foreach (var t in finder())
            {
                typeList.Add(t);
            }
            UpdateCachedPluginsFile<T>(typeList.Types, resolutionKind);
        }

        #region Public Methods

        /// <summary>
        /// Generic method to find the specified type and cache the result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<Type> ResolveTypes<T>(bool cacheResult = true, IEnumerable<Assembly> specificAssemblies = null)
        {
            if (specificAssemblies != null || cacheResult == false || typeof(IDiscoverable).IsAssignableFrom(typeof(T)) == false)
            {
                var extensions = ResolveTypesInternal<T>(
                    () => TypeFinder.FindClassesOfType<T>(specificAssemblies ?? AssembliesToScan),
                    TypeResolutionKind.FindAllTypes,
                    cacheResult);

                return extensions.Where(x => typeof(T).IsAssignableFrom(x));
            }
            else
            {
                //Use the cache if all assemblies

                var extensions = _extensions ?? (_extensions = new HashSet<Type>(ResolveTypesInternal<IDiscoverable>(
                    () => TypeFinder.FindClassesOfType<IDiscoverable>(AssembliesToScan),
                    TypeResolutionKind.FindAllTypes, true)));

                return extensions.Where(x => typeof(T).IsAssignableFrom(x));
            }
        }

        /// <summary>
        /// Generic method to find the specified type that has an attribute and cache the result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TAttribute"></typeparam>
        /// <returns></returns>
        public IEnumerable<Type> ResolveTypesWithAttribute<T, TAttribute>(bool cacheResult = true, IEnumerable<Assembly> specificAssemblies = null)
            where TAttribute : Attribute
        {
            return ResolveTypes<T>(specificAssemblies: specificAssemblies)
                .Where(x => x.GetCustomAttributes<TAttribute>(false).Any());
        }

        /// <summary>
        /// Resolves class types marked with the specified attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="cacheResult">Indicates whether to use cache for type resolution.</param>
        /// <param name="specificAssemblies">A set of assemblies for type resolution.</param>
        /// <returns>All class types marked with the specified attribute.</returns>
        public IEnumerable<Type> ResolveAttributedTypes<TAttribute>(bool cacheResult = true, IEnumerable<Assembly> specificAssemblies = null)
            where TAttribute : Attribute
        {
            return ResolveTypesInternal<TAttribute>(
                () => TypeFinder.FindClassesWithAttribute<TAttribute>(specificAssemblies ?? AssembliesToScan),
                TypeResolutionKind.FindAttributedTypes,
                cacheResult);
        }

        #endregion

        /// <summary>
        /// Used for unit tests
        /// </summary>
        /// <returns></returns>
        internal HashSet<TypeList> GetTypeLists()
        {
            return _types;
        }



        #region Nested classes and stuff

        /// <summary>
        /// The type of resolution being invoked
        /// </summary>
        internal enum TypeResolutionKind
        {
            FindAllTypes,
            FindAttributedTypes,
            FindTypesWithAttribute
        }

        /// <summary>
        /// Represents a list of types obtained by looking for types inheriting/implementing a
        /// specified type, and/or marked with a specified attribute type.
        /// </summary>
        internal abstract class TypeList
        {
            /// <summary>
            /// Adds a type.
            /// </summary>
            public abstract void Add(Type t);

            /// <summary>
            /// Gets the types.
            /// </summary>
            public abstract IEnumerable<Type> Types { get; }

            /// <summary>
            /// Gets a value indicating whether this instance is a type list for a specified type and resolution type.
            /// </summary>
            public abstract bool IsList<TLookup>(TypeResolutionKind resolutionType);
        }

        /// <summary>
        /// Represents a list of types obtained by looking for types inheriting/implementing a
        /// specified type, and/or marked with a specified attribute type.
        /// </summary>
        internal class TypeList<T> : TypeList
        {
            private readonly TypeResolutionKind _resolutionType;
            private readonly HashSet<Type> _types = new HashSet<Type>();

            /// <summary>
            /// Initializes a new instance of the <see cref="TypeList{T}"/> class.
            /// </summary>
            public TypeList(TypeResolutionKind resolutionType)
            {
                _resolutionType = resolutionType;
            }

            /// <inheritdoc />
            public override void Add(Type type)
            {
                // only add the type if it inherits/implements T
                // skip the check for FindAttributedTypes as in this case T is the attribute type
                if (_resolutionType == TypeResolutionKind.FindAttributedTypes || typeof(T).IsAssignableFrom(type))
                    _types.Add(type);
            }

            /// <inheritdoc />
            public override IEnumerable<Type> Types
            {
                get { return _types; }
            }

            /// <inheritdoc />
            public override bool IsList<TLookup>(TypeResolutionKind resolutionType)
            {
                return _resolutionType == resolutionType && typeof (T) == typeof (TLookup);
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
