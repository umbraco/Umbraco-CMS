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
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Profiling;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Cache;
using umbraco.interfaces;
using File = System.IO.File;

namespace Umbraco.Core
{
    /// <summary>
    /// Used to resolve all plugin types and cache them and is also used to instantiate plugin types
    /// </summary>
    /// <remarks>
    /// 
    /// This class should be used to resolve all plugin types, the TypeFinder should not be used directly!
    /// 
    /// This class can expose extension methods to resolve custom plugins
    /// 
    /// Before this class resolves any plugins it checks if the hash has changed for the DLLs in the /bin folder, if it hasn't
    /// it will use the cached resolved plugins that it has already found which means that no assembly scanning is necessary. This leads
    /// to much faster startup times.
    /// </remarks>
    public class PluginManager
    {
        /// <summary>
        /// Creates a new PluginManager with an ApplicationContext instance which ensures that the plugin xml 
        /// file is cached temporarily until app startup completes.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="detectChanges"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="runtimeCache"></param>
        internal PluginManager(IServiceProvider serviceProvider, IRuntimeCacheProvider runtimeCache, ProfilingLogger logger, bool detectChanges = true)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");
            if (runtimeCache == null) throw new ArgumentNullException("runtimeCache");
            if (logger == null) throw new ArgumentNullException("logger");

            _serviceProvider = serviceProvider;
            _runtimeCache = runtimeCache;
            _logger = logger;

            _tempFolder = IOHelper.MapPath("~/App_Data/TEMP/PluginCache");
            //create the folder if it doesn't exist
            if (Directory.Exists(_tempFolder) == false)
            {
                Directory.CreateDirectory(_tempFolder);
            }

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

        private readonly IServiceProvider _serviceProvider;
        private readonly IRuntimeCacheProvider _runtimeCache;
        private readonly ProfilingLogger _logger;
        private const string CacheKey = "umbraco-plugins.list";
        static PluginManager _resolver;
        private readonly string _tempFolder;
        private long _cachedAssembliesHash = -1;
        private long _currentAssembliesHash = -1;
        private static bool _initialized = false;
        private static object _singletonLock = new object();

        /// <summary>
        /// We will ensure that no matter what, only one of these is created, this is to ensure that caching always takes place
        /// </summary>
        /// <remarks>
        /// The setter is generally only used for unit tests
        /// </remarks>
        public static PluginManager Current
        {
            get
            {
                return LazyInitializer.EnsureInitialized(ref _resolver, ref _initialized, ref _singletonLock, () =>
                {
                    if (ApplicationContext.Current == null)
                    {
                        var logger = LoggerResolver.HasCurrent ? LoggerResolver.Current.Logger : new DebugDiagnosticsLogger();
                        var profiler = ProfilerResolver.HasCurrent ? ProfilerResolver.Current.Profiler : new LogProfiler(logger);
                        return new PluginManager(
                            new ActivatorServiceProvider(), 
                            new NullCacheProvider(), 
                            new ProfilingLogger(logger, profiler));
                    }
                    return new PluginManager(
                        new ActivatorServiceProvider(), 
                        ApplicationContext.Current.ApplicationCache.RuntimeCache, 
                        ApplicationContext.Current.ProfilingLogger);
                });
            }
            set
            {
                _initialized = true;
                _resolver = value;
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
                    .SingleOrDefault(x =>
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

        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();
        private readonly HashSet<TypeList> _types = new HashSet<TypeList>();
        private IEnumerable<Assembly> _assemblies;

        /// <summary>
        /// Returns all found property editors (based on the resolved Iparameter editors - this saves a scan)
        /// </summary>
        internal IEnumerable<Type> ResolvePropertyEditors()
        {
            //return all proeprty editor types found except for the base property editor type
            return ResolveTypes<IParameterEditor>()
                .Where(x => x.IsType<PropertyEditor>())
                .Except(new[] { typeof(PropertyEditor) });
        }

        /// <summary>
        /// Returns all found parameter editors (which includes property editors)
        /// </summary>
        internal IEnumerable<Type> ResolveParameterEditors()
        {
            //return all paramter editor types found except for the base property editor type
            return ResolveTypes<IParameterEditor>()
                .Except(new[] { typeof(ParameterEditor), typeof(PropertyEditor) });
        } 

        /// <summary>
        /// Returns all available IApplicationStartupHandler objects
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<Type> ResolveApplicationStartupHandlers()
        {
            return ResolveTypes<IApplicationStartupHandler>();
        }

        /// <summary>
        /// Returns all classes of type ICacheRefresher
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<Type> ResolveCacheRefreshers()
        {
            return ResolveTypes<ICacheRefresher>();
        }

        /// <summary>
        /// Returns all available IPropertyEditorValueConverter
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<Type> ResolvePropertyEditorValueConverters()
        {
            return ResolveTypes<IPropertyEditorValueConverter>();
        }

        /// <summary>
        /// Returns all available IDataType in application
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<Type> ResolveDataTypes()
        {
            return ResolveTypes<IDataType>();
        }

        /// <summary>
        /// Returns all available IMacroGuiRendering in application
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<Type> ResolveMacroRenderings()
        {
            return ResolveTypes<IMacroGuiRendering>();
        }

        /// <summary>
        /// Returns all available IPackageAction in application
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<Type> ResolvePackageActions()
        {
            return ResolveTypes<IPackageAction>();
        }

        /// <summary>
        /// Returns all available IAction in application
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<Type> ResolveActions()
        {
            return ResolveTypes<IAction>();
        }

        /// <summary>
        /// Returns all mapper types that have a MapperFor attribute defined
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<Type> ResolveAssignedMapperTypes()
        {
            return ResolveTypesWithAttribute<BaseMapper, MapperForAttribute>();
        } 
        
        /// <summary>
        /// Returns all SqlSyntaxProviders with the SqlSyntaxProviderAttribute
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<Type> ResolveSqlSyntaxProviders()
        {
            return ResolveTypesWithAttribute<ISqlSyntaxProvider, SqlSyntaxProviderAttribute>();
        }

        /// <summary>
        /// Gets/sets which assemblies to scan when type finding, generally used for unit testing, if not explicitly set
        /// this will search all assemblies known to have plugins and exclude ones known to not have them.
        /// </summary>
        internal IEnumerable<Assembly> AssembliesToScan
        {
            get { return _assemblies ?? (_assemblies = TypeFinder.GetAssembliesWithKnownExclusions()); }
            set { _assemblies = value; }
        }

        /// <summary>
        /// Used to resolve and create instances of the specified type based on the resolved/cached plugin types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="throwException">set to true if an exception is to be thrown if there is an error during instantiation</param>
        /// <param name="cacheResult"></param>
        /// <param name="specificAssemblies"></param>
        /// <returns></returns>
        internal IEnumerable<T> FindAndCreateInstances<T>(bool throwException = false, bool cacheResult = true, IEnumerable<Assembly> specificAssemblies = null)
        {
            var types = ResolveTypes<T>(cacheResult, specificAssemblies);
            return CreateInstances<T>(types, throwException);
        }

        /// <summary>
        /// Used to create instances of the specified type based on the resolved/cached plugin types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="types"></param>
        /// <param name="throwException">set to true if an exception is to be thrown if there is an error during instantiation</param>
        /// <returns></returns>
        internal IEnumerable<T> CreateInstances<T>(IEnumerable<Type> types, bool throwException = false)
        {
            return _serviceProvider.CreateInstances<T>(types, _logger.Logger, throwException);
        }

        /// <summary>
        /// Used to create an instance of the specified type based on the resolved/cached plugin types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="throwException"></param>
        /// <returns></returns>
        internal T CreateInstance<T>(Type type, bool throwException = false)
        {
            var instances = CreateInstances<T>(new[] { type }, throwException);
            return instances.FirstOrDefault();
        }

        private IEnumerable<Type> ResolveTypes<T>(
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
                    var typeList = _types.SingleOrDefault(x => x.IsTypeList<T>(resolutionType));

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
                                            typeList.AddType(type);
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
                    typesFound = typeList.GetTypes().ToList();
                }

                return typesFound;
            }
        }

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
                typeList.AddType(t);
            }
            UpdateCachedPluginsFile<T>(typeList.GetTypes(), resolutionKind);
        }

        #region Public Methods
        /// <summary>
        /// Generic method to find the specified type and cache the result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<Type> ResolveTypes<T>(bool cacheResult = true, IEnumerable<Assembly> specificAssemblies = null)
        {
            return ResolveTypes<T>(
                () => TypeFinder.FindClassesOfType<T>(specificAssemblies ?? AssembliesToScan),
                TypeResolutionKind.FindAllTypes,
                cacheResult);
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
            return ResolveTypes<T>(
                () => TypeFinder.FindClassesOfTypeWithAttribute<T, TAttribute>(specificAssemblies ?? AssembliesToScan),
                TypeResolutionKind.FindTypesWithAttribute,
                cacheResult);
        }

        /// <summary>
        /// Generic method to find any type that has the specified attribute
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <returns></returns>
        public IEnumerable<Type> ResolveAttributedTypes<TAttribute>(bool cacheResult = true, IEnumerable<Assembly> specificAssemblies = null)
            where TAttribute : Attribute
        {
            return ResolveTypes<TAttribute>(
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



        #region Private classes/Enums

        /// <summary>
        /// The type of resolution being invoked
        /// </summary>
        internal enum TypeResolutionKind
        {
            FindAllTypes,
            FindAttributedTypes,
            FindTypesWithAttribute
        }

        internal abstract class TypeList
        {
            public abstract void AddType(Type t);
            public abstract bool IsTypeList<TLookup>(TypeResolutionKind resolutionType);
            public abstract IEnumerable<Type> GetTypes();
        }

        internal class TypeList<T> : TypeList
        {
            private readonly TypeResolutionKind _resolutionType;

            public TypeList(TypeResolutionKind resolutionType)
            {
                _resolutionType = resolutionType;
            }

            private readonly List<Type> _types = new List<Type>();

            public override void AddType(Type t)
            {
                //if the type is an attribute type we won't do the type check because typeof<T> is going to be the 
                //attribute type whereas the 't' type is the object type found with the attribute.
                if (_resolutionType == TypeResolutionKind.FindAttributedTypes || t.IsType<T>())
                {
                    _types.Add(t);
                }
            }

            /// <summary>
            /// Returns true if the current TypeList is of the same lookup type
            /// </summary>
            /// <typeparam name="TLookup"></typeparam>
            /// <param name="resolutionType"></param>
            /// <returns></returns>
            public override bool IsTypeList<TLookup>(TypeResolutionKind resolutionType)
            {
                return _resolutionType == resolutionType && (typeof(T)) == typeof(TLookup);
            }

            public override IEnumerable<Type> GetTypes()
            {
                return _types;
            }
        }

        /// <summary>
        /// This class is used simply to determine that a plugin was not found in the cache plugin list with the specified
        /// TypeResolutionKind.
        /// </summary>
        internal class CachedPluginNotFoundInFileException : Exception
        {

        }

        #endregion
    }
}
