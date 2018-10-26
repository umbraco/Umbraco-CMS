using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Umbraco.Core.Logging;
using Umbraco.Core.Composing;

namespace Umbraco.Core.IO
{
    public class FileSystems : IFileSystems
    {
        private readonly ConcurrentSet<ShadowWrapper> _wrappers = new ConcurrentSet<ShadowWrapper>();
        private readonly IContainer _container;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, Lazy<IFileSystem>> _filesystems = new ConcurrentDictionary<string, Lazy<IFileSystem>>();

        // wrappers for shadow support
        private ShadowWrapper _macroPartialFileSystem;
        private ShadowWrapper _partialViewsFileSystem;
        private ShadowWrapper _stylesheetsFileSystem;
        private ShadowWrapper _scriptsFileSystem;
        private ShadowWrapper _masterPagesFileSystem;
        private ShadowWrapper _mvcViewsFileSystem;
        
        // well-known file systems lazy initialization
        private object _wkfsLock = new object();
        private bool _wkfsInitialized;
        private object _wkfsObject;

        private MediaFileSystem _mediaFileSystem;

        #region Constructor

        // DI wants a public ctor
        public FileSystems(IContainer container, ILogger logger)
        {
            _container = container;
            _logger = logger;
        }

        // for tests only, totally unsafe
        internal void Reset()
        {
            _wrappers.Clear();
            _filesystems.Clear();
            Volatile.Write(ref _wkfsInitialized, false);
        }

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
        public IFileSystem MasterPagesFileSystem
        {
            get
            {
                if (Volatile.Read(ref _wkfsInitialized) == false) EnsureWellKnownFileSystems();
                return _masterPagesFileSystem;
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

        /// <inheritdoc />
        public IMediaFileSystem MediaFileSystem
        {
            get
            {
                if (Volatile.Read(ref _wkfsInitialized) == false) EnsureWellKnownFileSystems();
                return _mediaFileSystem;
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
            var masterPagesFileSystem = new PhysicalFileSystem(SystemDirectories.Masterpages);
            var mvcViewsFileSystem = new PhysicalFileSystem(SystemDirectories.MvcViews);

            _macroPartialFileSystem = new ShadowWrapper(macroPartialFileSystem, "Views/MacroPartials", () => IsScoped());
            _partialViewsFileSystem = new ShadowWrapper(partialViewsFileSystem, "Views/Partials", () => IsScoped());
            _stylesheetsFileSystem = new ShadowWrapper(stylesheetsFileSystem, "css", () => IsScoped());
            _scriptsFileSystem = new ShadowWrapper(scriptsFileSystem, "scripts", () => IsScoped());
            _masterPagesFileSystem = new ShadowWrapper(masterPagesFileSystem, "masterpages", () => IsScoped());
            _mvcViewsFileSystem = new ShadowWrapper(mvcViewsFileSystem, "Views", () => IsScoped());

            // filesystems obtained from GetFileSystemProvider are already wrapped and do not need to be wrapped again
            _mediaFileSystem = GetFileSystem<MediaFileSystem>();

            return null;
        }

        #endregion

        #region Providers

        /// <summary>
        /// Gets a strongly-typed filesystem.
        /// </summary>
        /// <typeparam name="TFileSystem">The type of the filesystem.</typeparam>
        /// <returns>A strongly-typed filesystem of the specified type.</returns>
        /// <remarks>
        /// <para>Note that any filesystem created by this method *after* shadowing begins, will *not* be
        /// shadowing (and an exception will be thrown by the ShadowWrapper).</para>
        /// </remarks>
        public TFileSystem GetFileSystem<TFileSystem>()
            where TFileSystem : FileSystemWrapper
        {
            var alias = GetFileSystemAlias<TFileSystem>();

            // note: GetOrAdd can run multiple times - and here, since we have side effects
            // (adding to _wrappers) we want to be sure the factory runs only once, hence the
            // additional Lazy.
            return (TFileSystem) _filesystems.GetOrAdd(alias, _ => new Lazy<IFileSystem>(() =>
            {
                var supportingFileSystem = _container.GetInstance<IFileSystem>(alias);
                var shadowWrapper = new ShadowWrapper(supportingFileSystem, "typed/" + alias, () => IsScoped());

                _wrappers.Add(shadowWrapper); // _wrappers is a concurrent set - this is safe

                return _container.CreateInstance<TFileSystem>(new { innerFileSystem = shadowWrapper});
            })).Value;
        }

        private string GetFileSystemAlias<TFileSystem>()
        {
            var fileSystemType = typeof(TFileSystem);

            // find the attribute and get the alias
            var attr = (FileSystemAttribute) fileSystemType.GetCustomAttributes(typeof(FileSystemAttribute), false).SingleOrDefault();
            if (attr == null)
                throw new InvalidOperationException("Type " + fileSystemType.FullName + "is missing the required FileSystemProviderAttribute.");

            return attr.Alias;
        }

        #endregion

        #region Shadow

        // note
        // shadowing is thread-safe, but entering and exiting shadow mode is not, and there is only one
        // global shadow for the entire application, so great care should be taken to ensure that the
        // application is *not* doing anything else when using a shadow.
        // shadow applies to well-known filesystems *only* - at the moment, any other filesystem that would
        // be created directly (via ctor) or via GetFileSystem<T> is *not* shadowed.

        // shadow must be enabled in an app event handler before anything else ie before any filesystem
        // is actually created and used - after, it is too late - enabling shadow has a negligible perfs
        // impact.
        // NO! by the time an app event handler is instantiated it is already too late, see note in ctor.
        //internal void EnableShadow()
        //{
        //    if (_mvcViewsFileSystem != null) // test one of the fs...
        //        throw new InvalidOperationException("Cannot enable shadow once filesystems have been created.");
        //    _shadowEnabled = true;
        //}

        internal ICompletable Shadow(Guid id)
        {
            if (Volatile.Read(ref _wkfsInitialized) == false) EnsureWellKnownFileSystems();

            var typed = _wrappers.ToArray();
            var wrappers = new ShadowWrapper[typed.Length + 6];
            var i = 0;
            while (i < typed.Length) wrappers[i] = typed[i++];
            wrappers[i++] = _macroPartialFileSystem;
            wrappers[i++] = _partialViewsFileSystem;
            wrappers[i++] = _stylesheetsFileSystem;
            wrappers[i++] = _scriptsFileSystem;
            wrappers[i++] = _masterPagesFileSystem;
            wrappers[i] = _mvcViewsFileSystem;

            return new ShadowFileSystems(id, wrappers, _logger);
        }

        #endregion

        private class ConcurrentSet<T>
            where T : class
        {
            private readonly HashSet<T> _set = new HashSet<T>();

            public void Add(T item)
            {
                lock (_set)
                {
                    _set.Add(item);
                }
            }

            public void Clear()
            {
                lock (_set)
                {
                    _set.Clear();
                }
            }

            public T[] ToArray()
            {
                lock (_set)
                {
                    return _set.ToArray();
                }
            }
        }
    }
}
