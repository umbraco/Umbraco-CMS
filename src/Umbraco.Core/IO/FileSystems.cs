using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Umbraco.Core.Logging;
using Umbraco.Core.Composing;

namespace Umbraco.Core.IO
{
    public class FileSystems : IFileSystems
    {
        private readonly IFactory _container;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<Type, Lazy<IFileSystem>> _filesystems = new ConcurrentDictionary<Type, Lazy<IFileSystem>>();

        // wrappers for shadow support
        private ShadowWrapper _macroPartialFileSystem;
        private ShadowWrapper _partialViewsFileSystem;
        private ShadowWrapper _stylesheetsFileSystem;
        private ShadowWrapper _scriptsFileSystem;
        private ShadowWrapper _mvcViewsFileSystem;

        // well-known file systems lazy initialization
        private object _wkfsLock = new object();
        private bool _wkfsInitialized;
        private object _wkfsObject; // unused

        // shadow support
        private readonly List<ShadowWrapper> _shadowWrappers = new List<ShadowWrapper>();
        private readonly object _shadowLocker = new object();
        private static string _shadowCurrentId; // static - unique!!
        #region Constructor

        // DI wants a public ctor
        public FileSystems(IFactory container, ILogger logger)
        {
            _container = container;
            _logger = logger;
        }

        // for tests only, totally unsafe
        internal void Reset()
        {
            _shadowWrappers.Clear();
            _filesystems.Clear();
            Volatile.Write(ref _wkfsInitialized, false);
            _shadowCurrentId = null;
        }

        // for tests only, totally unsafe
        internal static void ResetShadowId()
        {
            _shadowCurrentId = null;
        }

        // set by the scope provider when taking control of filesystems
        internal Func<bool> IsScoped { get; set; } = () => false;

        #endregion

        #region Well-Known FileSystems

        /// <inheritdoc />
        public IFileSystem MacroPartialsFileSystem
        {
            get
            {
                if (Volatile.Read(ref _wkfsInitialized) == false) EnsureWellKnownFileSystems();
                return _macroPartialFileSystem;
            }
        }

        /// <inheritdoc />
        public IFileSystem PartialViewsFileSystem
        {
            get
            {
                if (Volatile.Read(ref _wkfsInitialized) == false) EnsureWellKnownFileSystems();
                return _partialViewsFileSystem;
            }
        }

        /// <inheritdoc />
        public IFileSystem StylesheetsFileSystem
        {
            get
            {
                if (Volatile.Read(ref _wkfsInitialized) == false) EnsureWellKnownFileSystems();
                return _stylesheetsFileSystem;
            }
        }

        /// <inheritdoc />
        public IFileSystem ScriptsFileSystem
        {
            get
            {
                if (Volatile.Read(ref _wkfsInitialized) == false) EnsureWellKnownFileSystems();
                return _scriptsFileSystem;
            }
        }

        /// <inheritdoc />
        public IFileSystem MvcViewsFileSystem
        {
            get
            {
                if (Volatile.Read(ref _wkfsInitialized) == false) EnsureWellKnownFileSystems();
                return _mvcViewsFileSystem;
            }
        }

        private void EnsureWellKnownFileSystems()
        {
            LazyInitializer.EnsureInitialized(ref _wkfsObject, ref _wkfsInitialized, ref _wkfsLock, CreateWellKnownFileSystems);
        }

        // need to return something to LazyInitializer.EnsureInitialized
        // but it does not really matter what we return - here, null
        private object CreateWellKnownFileSystems()
        {
            var macroPartialFileSystem = new PhysicalFileSystem(SystemDirectories.MacroPartials);
            var partialViewsFileSystem = new PhysicalFileSystem(SystemDirectories.PartialViews);
            var stylesheetsFileSystem = new PhysicalFileSystem(SystemDirectories.Css);
            var scriptsFileSystem = new PhysicalFileSystem(SystemDirectories.Scripts);
            var mvcViewsFileSystem = new PhysicalFileSystem(SystemDirectories.MvcViews);

            _macroPartialFileSystem = new ShadowWrapper(macroPartialFileSystem, "macro-partials", IsScoped);
            _partialViewsFileSystem = new ShadowWrapper(partialViewsFileSystem, "partials", IsScoped);
            _stylesheetsFileSystem = new ShadowWrapper(stylesheetsFileSystem, "css", IsScoped);
            _scriptsFileSystem = new ShadowWrapper(scriptsFileSystem, "scripts", IsScoped);
            _mvcViewsFileSystem = new ShadowWrapper(mvcViewsFileSystem, "views", IsScoped);

            // TODO: do we need a lock here?
            _shadowWrappers.Add(_macroPartialFileSystem);
            _shadowWrappers.Add(_partialViewsFileSystem);
            _shadowWrappers.Add(_stylesheetsFileSystem);
            _shadowWrappers.Add(_scriptsFileSystem);
            _shadowWrappers.Add(_mvcViewsFileSystem);

            return null;
        }

        #endregion

        #region Providers

        private readonly Dictionary<Type, string> _paths = new Dictionary<Type, string>();

        // internal for tests
        internal IReadOnlyDictionary<Type, string> Paths => _paths;

        /// <summary>
        /// Gets a strongly-typed filesystem.
        /// </summary>
        /// <typeparam name="TFileSystem">The type of the filesystem.</typeparam>
        /// <returns>A strongly-typed filesystem of the specified type.</returns>
        /// <remarks>
        /// <para>Note that any filesystem created by this method *after* shadowing begins, will *not* be
        /// shadowing (and an exception will be thrown by the ShadowWrapper).</para>
        /// </remarks>
        public TFileSystem GetFileSystem<TFileSystem>(IFileSystem supporting)
            where TFileSystem : FileSystemWrapper
        {
            if (Volatile.Read(ref _wkfsInitialized) == false) EnsureWellKnownFileSystems();

            return (TFileSystem) _filesystems.GetOrAdd(typeof(TFileSystem), _ => new Lazy<IFileSystem>(() =>
            {
                var typeofTFileSystem = typeof(TFileSystem);

                // path must be unique and not collide with paths used in CreateWellKnownFileSystems
                // for our well-known 'media' filesystem we can use the short 'media' path
                // for others, put them under 'x/' and use ... something
                string path;
                if (typeofTFileSystem == typeof(MediaFileSystem))
                {
                    path = "media";
                }
                else
                {
                    lock (_paths)
                    {
                        if (!_paths.TryGetValue(typeofTFileSystem, out path))
                        {
                            path = Guid.NewGuid().ToString("N").Substring(0, 6);
                            while (_paths.ContainsValue(path)) // this can't loop forever, right?
                                path = Guid.NewGuid().ToString("N").Substring(0, 6);
                            _paths[typeofTFileSystem] = path;
                        }
                    }

                    path = "x/" + path;
                }

                var shadowWrapper = CreateShadowWrapper(supporting, path);
                return _container.CreateInstance<TFileSystem>(shadowWrapper);
            })).Value;
        }

        #endregion

        #region Shadow

        // note
        // shadowing is thread-safe, but entering and exiting shadow mode is not, and there is only one
        // global shadow for the entire application, so great care should be taken to ensure that the
        // application is *not* doing anything else when using a shadow.

        internal ICompletable Shadow()
        {
            if (Volatile.Read(ref _wkfsInitialized) == false) EnsureWellKnownFileSystems();

            var id = ShadowWrapper.CreateShadowId();
            return new ShadowFileSystems(this, id); // will invoke BeginShadow and EndShadow
        }

        internal void BeginShadow(string id)
        {
            lock (_shadowLocker)
            {
                // if we throw here, it means that something very wrong happened.
                if (_shadowCurrentId != null)
                    throw new InvalidOperationException("Already shadowing.");

                _shadowCurrentId = id;

                _logger.Debug<ShadowFileSystems, string>("Shadow '{ShadowId}'", _shadowCurrentId);

                foreach (var wrapper in _shadowWrappers)
                    wrapper.Shadow(_shadowCurrentId);
            }
        }

        internal void EndShadow(string id, bool completed)
        {
            lock (_shadowLocker)
            {
                // if we throw here, it means that something very wrong happened.
                if (_shadowCurrentId == null)
                    throw new InvalidOperationException("Not shadowing.");
                if (id != _shadowCurrentId)
                    throw new InvalidOperationException("Not the current shadow.");

                _logger.Debug<ShadowFileSystems, string, string>("UnShadow '{ShadowId}' {Status}", id, completed ? "complete" : "abort");

                var exceptions = new List<Exception>();
                foreach (var wrapper in _shadowWrappers)
                {
                    try
                    {
                        // this may throw an AggregateException if some of the changes could not be applied
                        wrapper.UnShadow(completed);
                    }
                    catch (AggregateException ae)
                    {
                        exceptions.Add(ae);
                    }
                }

                _shadowCurrentId = null;

                if (exceptions.Count > 0)
                    throw new AggregateException(completed ? "Failed to apply all changes (see exceptions)." : "Failed to abort (see exceptions).", exceptions);
            }
        }

        private ShadowWrapper CreateShadowWrapper(IFileSystem filesystem, string shadowPath)
        {
            lock (_shadowLocker)
            {
                var wrapper = new ShadowWrapper(filesystem, shadowPath, IsScoped);
                if (_shadowCurrentId != null)
                    wrapper.Shadow(_shadowCurrentId);
                _shadowWrappers.Add(wrapper);
                return wrapper;
            }
        }

        #endregion
    }
}
