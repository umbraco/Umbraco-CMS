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
using Umbraco.Core.PropertyEditors;
using Umbraco.Core._Legacy.PackageActions;
using File = System.IO.File;

namespace Umbraco.Core.Plugins
{
    /// <summary>Resolves and caches all plugin types, and instanciates them.
    /// </summary>
    /// <remarks>
    /// <para>This class should be used to resolve all plugin types, the TypeFinder should not be used directly!.</para>
    /// <para>Before this class resolves any plugins it checks if the hash has changed for the DLLs in the /bin folder, if it hasn't
    /// it will use the cached resolved plugins that it has already found which means that no assembly scanning is necessary. This leads
    /// to much faster startup times.</para>
    /// </remarks>
    public class PluginManager
    {
        private const string CacheKey = "umbraco-plugins.list";
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        private readonly IRuntimeCacheProvider _runtimeCache;
        private readonly ProfilingLogger _logger;
        private readonly string _tempFolder;
        private readonly HashSet<TypeList> _types = new HashSet<TypeList>();
        private long _cachedAssembliesHash = -1;
        private long _currentAssembliesHash = -1;
        private IEnumerable<Assembly> _assemblies;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginManager"/> class.
        /// </summary>
        /// <param name="runtimeCache">A runtime cache.</param>
        /// <param name="logger">A logger</param>
        /// <param name="detectChanges">A value indicating whether to detect changes.</param>
        public PluginManager(IRuntimeCacheProvider runtimeCache, ProfilingLogger logger, bool detectChanges = true)
        {
            if (runtimeCache == null) throw new ArgumentNullException(nameof(runtimeCache));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _runtimeCache = runtimeCache;
            _logger = logger;

            // create the temp folder if it doesn't exist
            _tempFolder = IOHelper.MapPath("~/App_Data/TEMP/PluginCache");
            if (Directory.Exists(_tempFolder) == false)
                Directory.CreateDirectory(_tempFolder);

            var pluginListFile = GetPluginListFilePath();

            // check for legacy changes: before, we didn't store the TypeResolutionKind in the file, which was a mistake,
            // so we need to detect if the old file is there without this attribute, if it is then we delete it
            if (DetectLegacyPluginListFile())
                File.Delete(pluginListFile);

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

        public static PluginManager Current => DependencyInjection.Current.PluginManager;

        internal static PluginManager Default
        {
            get
            {
                var appctx = ApplicationContext.Current;
                var cacheProvider = appctx == null // fixme - should Current have an ApplicationCache?
                    ? new NullCacheProvider()
                    : appctx.ApplicationCache.RuntimeCache;
                return new PluginManager(cacheProvider, DependencyInjection.Current.ProfilingLogger);
            }
        }

        #region Hash checking methods

        /// <summary>
        /// Gets a value indicating whether the assemblies in the /bin, app_code, global.asax, etc... have changed since they were last hashed.
        /// </summary>
        internal bool RequiresRescanning { get; }

        /// <summary>
        /// Gets the currently cached hash value of the scanned assemblies in the /bin folder.
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
        /// Gets the current assemblies hash based on creating a hash from the assemblies in the /bin folder.
        /// </summary>
        /// <value>The current hash.</value>
        internal long CurrentAssembliesHash
        {
            get
            {
                if (_currentAssembliesHash != -1)
                    return _currentAssembliesHash;

                _currentAssembliesHash = GetFileHash(
                    new List<Tuple<FileSystemInfo, bool>>
						{
							// add the bin folder and everything in it
							new Tuple<FileSystemInfo, bool>(new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Bin)), false),
							// add the app code folder and everything in it
							new Tuple<FileSystemInfo, bool>(new DirectoryInfo(IOHelper.MapPath("~/App_Code")), false),
							// add the global.asax (the app domain also monitors this, if it changes will do a full restart)
							new Tuple<FileSystemInfo, bool>(new FileInfo(IOHelper.MapPath("~/global.asax")), false),
                            // add the trees.config - use the contents to create the has since this gets resaved on every app startup!
                            new Tuple<FileSystemInfo, bool>(new FileInfo(IOHelper.MapPath(SystemDirectories.Config + "/trees.config")), true)
						}, _logger
                    );

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
        /// <param name="filesAndFolders">
        /// A collection of files and whether or not to use their file contents to determine the hash or the file's properties
        /// (true will make a hash based on it's contents)
        /// </param>
        /// <param name="logger">A profiling logger.</param>
        /// <returns>The hash.</returns>
        internal static long GetFileHash(IEnumerable<Tuple<FileSystemInfo, bool>> filesAndFolders, ProfilingLogger logger)
        {
            var ffA = filesAndFolders.ToArray();

            using (logger.TraceDuration<PluginManager>("Determining hash of code files on disk", "Hash determined"))
            {
                var hashCombiner = new HashCodeCombiner();

                // get the file info's to check
                var fileInfos = ffA.Where(x => x.Item2 == false).ToArray();
                var fileContents = ffA.Except(fileInfos);

                // add each unique folder/file to the hash
                foreach (var i in fileInfos.Select(x => x.Item1).DistinctBy(x => x.FullName))
                    hashCombiner.AddFileSystemItem(i);

                // add each unique file's contents to the hash
                foreach (var i in fileContents.Select(x => x.Item1)
                    .DistinctBy(x => x.FullName)
                    .Where(x => File.Exists(x.FullName)))
                {
                    var content = File.ReadAllText(i.FullName).Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
                    hashCombiner.AddCaseInsensitiveString(content);
                }

                return ConvertPluginsHashFromHex(hashCombiner.GetCombinedHashCode());
            }
        }

        internal static long GetFileHash(IEnumerable<FileSystemInfo> filesAndFolders, ProfilingLogger logger)
        {
            using (logger.TraceDuration<PluginManager>("Determining hash of code files on disk", "Hash determined"))
            {
                var hashCombiner = new HashCodeCombiner();

                // add each unique folder/file to the hash
                foreach (var i in filesAndFolders.DistinctBy(x => x.FullName))
                    hashCombiner.AddFileSystemItem(i);

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
            return long.TryParse(val, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out outVal) ? outVal : 0;
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
            if (File.Exists(filePath) == false)
                return Attempt<IEnumerable<string>>.Fail();

            try
            {
                // we will load the xml document, if the app context exist, we will load it from the cache (which is only around for 5 minutes)
                // while the app boots up, this should save some IO time on app startup when the app context is there (which is always unless in unit tests)
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

                // return false but specify this exception type so we can detect it
                if (typeElement == null)
                    return Attempt<IEnumerable<string>>.Fail(new CachedPluginNotFoundInFileException());

                // return success
                return Attempt.Succeed(typeElement.Elements("add")
                        .Select(x => (string)x.Attribute("type")));
            }
            catch (Exception ex)
            {
                // if the file is corrupted, etc... return false
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
            return Path.Combine(_tempFolder, $"umbraco-plugins.{NetworkHelper.FileSafeMachineName}.list");
        }

        private string GetPluginHashFilePath()
        {
            return Path.Combine(_tempFolder, $"umbraco-plugins.{NetworkHelper.FileSafeMachineName}.hash");
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
            if (File.Exists(filePath) == false)
                return false;

            try
            {
                var xml = XDocument.Load(filePath);

                var typeElement = xml.Root?.Elements()
                    .FirstOrDefault(x => x.Name.LocalName == "baseType");

                //now check if the typeElement is missing the resolutionType attribute
                return typeElement != null && typeElement.Attributes().All(x => x.Name.LocalName != "resolutionType");
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
                // if there's an exception loading then this is somehow corrupt, we'll just replace it.
                File.Delete(filePath);
                // create the document and the root
                xml = new XDocument(new XElement("plugins"));
            }
            if (xml.Root == null)
            {
                // if for some reason there is no root, create it
                xml.Add(new XElement("plugins"));
            }
            // find the type 'T' element to add or update
            var typeElement = xml.Root.Elements()
                .SingleOrDefault(x =>
                                 x.Name.LocalName == "baseType"
                                 && ((string)x.Attribute("type")) == typeof(T).FullName
                                 && ((string)x.Attribute("resolutionType")) == resolutionType.ToString());

            if (typeElement == null)
            {
                // create the type element
                typeElement = new XElement("baseType",
                    new XAttribute("type", typeof(T).FullName),
                    new XAttribute("resolutionType", resolutionType.ToString()));
                // then add it to the root
                xml.Root.Add(typeElement);
            }


            // now we have the type element, we need to clear any previous types as children and add/update it with new ones
            typeElement.ReplaceNodes(typesFound.Select(x => new XElement("add", new XAttribute("type", x.AssemblyQualifiedName))));
            // save the xml file
            xml.Save(filePath);
        }

        #endregion

        /// <summary>
        /// Gets or sets which assemblies to scan when type finding, generally used for unit testing, if not explicitly set
        /// this will search all assemblies known to have plugins and exclude ones known to not have them.
        /// </summary>
        internal IEnumerable<Assembly> AssembliesToScan
        {
            get { return _assemblies ?? (_assemblies = TypeFinder.GetAssembliesWithKnownExclusions()); }
            set { _assemblies = value; }
        }

        #region Resolve Types

        private bool _reportedChange;

        private IEnumerable<Type> ResolveTypes<T>(Func<IEnumerable<Type>> finder, TypeResolutionKind resolutionType, bool cacheResult)
        {
            using (var rlock = new UpgradeableReadLock(Locker))
            {
                using (_logger.DebugDuration<PluginManager>(
                    $"Resolving {typeof(T).FullName}",
                    $"Resolved {typeof(T).FullName}", // cannot contain typesFound.Count as it's evaluated before the find!
                    50))
                {
                    // resolve within a lock & timer
                    return ResolveTypes2<T>(finder, resolutionType, cacheResult, rlock);
                }
            }

        }

        private IEnumerable<Type> ResolveTypes2<T>(Func<IEnumerable<Type>> finder, TypeResolutionKind resolutionType, bool cacheResult, UpgradeableReadLock rlock)
        {
            // check if the TypeList already exists, if so return it, if not we'll create it
            var typeList = _types.SingleOrDefault(x => x.IsTypeList<T>(resolutionType));

            //need to put some logging here to try to figure out why this is happening: http://issues.umbraco.org/issue/U4-3505
            if (cacheResult && typeList != null)
            {
                _logger.Logger.Debug<PluginManager>($"Resolving {typeof(T).FullName} ({resolutionType}): found a cached type list.");
            }

            //if we're not caching the result then proceed, or if the type list doesn't exist then proceed
            if (cacheResult == false || typeList == null)
            {
                // upgrade to a write lock since we're adding to the collection
                rlock.UpgradeToWriteLock();

                typeList = new TypeList<T>(resolutionType);

                var scan = RequiresRescanning || File.Exists(GetPluginListFilePath()) == false;

                if (scan)
                {
                    // either we have to rescan, or we could not find the cache file:
                    // report (only once) and scan and update the cache file
                    if (_reportedChange == false)
                    {
                        _logger.Logger.Debug<PluginManager>("Assemblies changes detected, need to rescan everything.");
                        _reportedChange = true;
                    }
                }

                if (scan == false)
                {
                    // if we don't have to scan, try the cache file
                    var fileCacheResult = TryGetCachedPluginsFromFile<T>(resolutionType);

                    // here we need to identify if the CachedPluginNotFoundInFile was the exception, if it was then we need to re-scan
                    // in some cases the plugin will not have been scanned for on application startup, but the assemblies haven't changed
                    // so in this instance there will never be a result.
                    if (fileCacheResult.Exception is CachedPluginNotFoundInFileException || fileCacheResult.Success == false)
                    {
                        _logger.Logger.Debug<PluginManager>($"Resolving {typeof(T).FullName} ({resolutionType}): failed to load from cache file, must scan assemblies.");
                        scan = true;
                    }
                    else
                    {
                        // successfully retrieved types from the file cache: load
                        foreach (var type in fileCacheResult.Result)
                        {
                            try
                            {
                                // we use the build manager to ensure we get all types loaded, this is slightly slower than
                                // Type.GetType but if the types in the assembly aren't loaded yet it would fail whereas
                                // BuildManager will load them
                                typeList.AddType(BuildManager.GetType(type, true));
                            }
                            catch (Exception ex)
                            {
                                // in case of any exception, we have to exit, and revert to scanning
                                _logger.Logger.Error<PluginManager>($"Resolving {typeof(T).FullName} ({resolutionType}): failed to load cache file type {type}, reverting to scanning assemblies.", ex);
                                scan = true;
                                break;
                            }
                        }
                        if (scan == false)
                        {
                            _logger.Logger.Debug<PluginManager>($"Resolving {typeof(T).FullName} ({resolutionType}): loaded types from cache file.");
                        }
                    }
                }

                if (scan)
                {
                    // either we had to scan, or we could not resolve the types from the cache file - scan now
                    _logger.Logger.Debug<PluginManager>($"Resolving {typeof(T).FullName} ({resolutionType}): scanning assemblies.");
                    LoadViaScanningAndUpdateCacheFile<T>(typeList, resolutionType, finder);
                }

                if (scan && cacheResult)
                {
                    // if we are to cache the results, add the list to the collection
                    var added = _types.Add(typeList);
                    _logger.Logger.Debug<PluginManager>($"Resolved {typeof(T).FullName} ({resolutionType}), caching (added = {added.ToString().ToLowerInvariant()}).");
                }
                else
                {
                    _logger.Logger.Debug<PluginManager>($"Resolved {typeof(T).FullName} ({resolutionType}).");
                }
            }

            return typeList.GetTypes().ToList();
        }

        /// <summary>
        /// Invokes the finder which scans the assemblies for the types and then loads the result into the type finder,
        /// then updates the cached type xml file.
        /// </summary>
        /// <param name="typeList"></param>
        /// <param name="resolutionKind"> </param>
        /// <param name="finder"></param>
        /// <remarks>This method is not thread safe.</remarks>
        private void LoadViaScanningAndUpdateCacheFile<T>(TypeList typeList, TypeResolutionKind resolutionKind, Func<IEnumerable<Type>> finder)
        {
            // we don't have a cache for this so proceed to look them up by scanning
            foreach (var t in finder())
                typeList.AddType(t);

            UpdateCachedPluginsFile<T>(typeList.GetTypes(), resolutionKind);
        }

        /// <summary>
        /// Resolves specified types.
        /// </summary>
        /// <typeparam name="T">The type to find.</typeparam>
        /// <returns>The types.</returns>
        public IEnumerable<Type> ResolveTypes<T>(bool cacheResult = true, IEnumerable<Assembly> specificAssemblies = null)
        {
            return ResolveTypes<T>(
                () => TypeFinder.FindClassesOfType<T>(specificAssemblies ?? AssembliesToScan),
                TypeResolutionKind.FindAllTypes,
                cacheResult);
        }

        /// <summary>
        /// Resolves specified, attributed types.
        /// </summary>
        /// <typeparam name="T">The type to find.</typeparam>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <returns>The corresponding types.</returns>
        public IEnumerable<Type> ResolveTypesWithAttribute<T, TAttribute>(bool cacheResult = true, IEnumerable<Assembly> specificAssemblies = null)
            where TAttribute : Attribute
        {
            return ResolveTypes<T>(
                () => TypeFinder.FindClassesOfTypeWithAttribute<T, TAttribute>(specificAssemblies ?? AssembliesToScan),
                TypeResolutionKind.FindTypesWithAttribute,
                cacheResult);
        }

        /// <summary>
        /// Resolves attributed types.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <returns>The corresopnding types.</returns>
        public IEnumerable<Type> ResolveAttributedTypes<TAttribute>(bool cacheResult = true, IEnumerable<Assembly> specificAssemblies = null)
            where TAttribute : Attribute
        {
            return ResolveTypes<TAttribute>(
                () => TypeFinder.FindClassesWithAttribute<TAttribute>(specificAssemblies ?? AssembliesToScan),
                TypeResolutionKind.FindAttributedTypes,
                cacheResult);
        }

        #endregion

        #region Private

        /// <summary>
        /// Gets the list of types.
        /// </summary>
        /// <returns>The list of types.</returns>
        /// <remarks>For unit tests only.</remarks>
        internal HashSet<TypeList> GetTypeLists()
        {
            return _types;
        }

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
        /// Represents the error that occurs when a plugin was not found in the cache plugin list with the specified
        /// TypeResolutionKind.
        /// </summary>
        internal class CachedPluginNotFoundInFileException : Exception
        { }

        #endregion
    }

    internal static class PluginManagerExtensions
    {
        /// <summary>
        /// Resolves property editors (based on the resolved Iparameter editors - this saves a scan).
        /// </summary>
        public static IEnumerable<Type> ResolvePropertyEditors(this PluginManager mgr)
        {
            //return all proeprty editor types found except for the base property editor type
            return mgr.ResolveTypes<IParameterEditor>()
                .Where(x => x.IsType<PropertyEditor>())
                .Except(new[] { typeof(PropertyEditor) });
        }

        /// <summary>
        /// Resolves parameter editors (which includes property editors)
        /// </summary>
        internal static IEnumerable<Type> ResolveParameterEditors(this PluginManager mgr)
        {
            //return all paramter editor types found except for the base property editor type
            return mgr.ResolveTypes<IParameterEditor>()
                .Except(new[] { typeof(ParameterEditor), typeof(PropertyEditor) });
        }

        /// <summary>
        /// Resolves IApplicationStartupHandler objects.
        /// </summary>
        internal static IEnumerable<Type> ResolveApplicationStartupHandlers(this PluginManager mgr)
        {
            return mgr.ResolveTypes<IApplicationEventHandler>();
        }

        /// <summary>
        /// Resolves ICacheRefresher objects.
        /// </summary>
        internal static IEnumerable<Type> ResolveCacheRefreshers(this PluginManager mgr)
        {
            return mgr.ResolveTypes<ICacheRefresher>();
        }

        /// <summary>
        /// Resolves IPackageAction objects.
        /// </summary>
        internal static IEnumerable<Type> ResolvePackageActions(this PluginManager mgr)
        {
            return mgr.ResolveTypes<IPackageAction>();
        }

        /// <summary>
        /// Resolves mapper types that have a MapperFor attribute defined.
        /// </summary>
        internal static IEnumerable<Type> ResolveAssignedMapperTypes(this PluginManager mgr)
        {
            return mgr.ResolveTypesWithAttribute<BaseMapper, MapperForAttribute>();
        }
    }
}
