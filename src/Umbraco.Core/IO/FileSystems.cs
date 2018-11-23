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
        private object _wkfsObject; // unused

        // shadow support
        private readonly List<ShadowWrapper> _shadowWrappers = new List<ShadowWrapper>();
        private readonly object _shadowLocker = new object();
        private static Guid _shadowCurrentId = Guid.Empty; // static - unique!!
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
            _shadowWrappers.Clear();
            _filesystems.Clear();
            Volatile.Write(ref _wkfsInitialized, false);
            _shadowCurrentId = Guid.Empty;
        }

        // for tests only, totally unsafe
        internal static void ResetShadowId()
        {
            _shadowCurrentId = Guid.Empty;
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

            _macroPartialFileSystem = new ShadowWrapper(macroPartialFileSystem, "Views/MacroPartials", IsScoped);
            _partialViewsFileSystem = new ShadowWrapper(partialViewsFileSystem, "Views/Partials", IsScoped);
            _stylesheetsFileSystem = new ShadowWrapper(stylesheetsFileSystem, "css", IsScoped);
            _scriptsFileSystem = new ShadowWrapper(scriptsFileSystem, "scripts", IsScoped);
            _masterPagesFileSystem = new ShadowWrapper(masterPagesFileSystem, "masterpages", IsScoped);
            _mvcViewsFileSystem = new ShadowWrapper(mvcViewsFileSystem, "Views", IsScoped);

            // fixme locking?
            _shadowWrappers.Add(_macroPartialFileSystem);
            _shadowWrappers.Add(_partialViewsFileSystem);
            _shadowWrappers.Add(_stylesheetsFileSystem);
            _shadowWrappers.Add(_scriptsFileSystem);
            _shadowWrappers.Add(_masterPagesFileSystem);
            _shadowWrappers.Add(_mvcViewsFileSystem);

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
        public TFileSystem GetFileSystem<TFileSystem>(Func<IFileSystem> innerFileSystemFactory)
            where TFileSystem : FileSystemWrapper
        {
            if (Volatile.Read(ref _wkfsInitialized) == false) EnsureWellKnownFileSystems();

            var name = typeof(TFileSystem).FullName;
            if (name == null) throw new Exception("panic!");

            return (TFileSystem) _filesystems.GetOrAdd(name, _ => new Lazy<IFileSystem>(() =>
            {
                var innerFileSystem = innerFileSystemFactory();
                var shadowWrapper = CreateShadowWrapper(innerFileSystem, "typed/" + name);
                return _container.CreateInstance<TFileSystem>(new { innerFileSystem = shadowWrapper });
            })).Value;
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

            return new ShadowFileSystems(this, id); // will invoke BeginShadow and EndShadow
        }

        internal void BeginShadow(Guid id)
        {
            lock (_shadowLocker)
            {
                // if we throw here, it means that something very wrong happened.
                if (_shadowCurrentId != Guid.Empty)
                    throw new InvalidOperationException("Already shadowing.");
                _shadowCurrentId = id;

                _logger.Debug<ShadowFileSystems>("Shadow '{ShadowId}'", id);

                foreach (var wrapper in _shadowWrappers)
                    wrapper.Shadow(id);
            }
        }

        internal void EndShadow(Guid id, bool completed)
        {
            lock (_shadowLocker)
            {
                // if we throw here, it means that something very wrong happened.
                if (_shadowCurrentId == Guid.Empty)
                    throw new InvalidOperationException("Not shadowing.");
                if (id != _shadowCurrentId)
                    throw new InvalidOperationException("Not the current shadow.");

                _logger.Debug<ShadowFileSystems>("UnShadow '{ShadowId}' {Status}", id, completed ? "complete" : "abort");

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

                _shadowCurrentId = Guid.Empty;

                if (exceptions.Count > 0)
                    throw new AggregateException(completed ? "Failed to apply all changes (see exceptions)." : "Failed to abort (see exceptions).", exceptions);
            }
        }

        private ShadowWrapper CreateShadowWrapper(IFileSystem filesystem, string shadowPath)
        {
            lock (_shadowLocker)
            {
                var wrapper = new ShadowWrapper(filesystem, shadowPath, IsScoped);
                if (_shadowCurrentId != Guid.Empty)
                    wrapper.Shadow(_shadowCurrentId);
                _shadowWrappers.Add(wrapper);
                return wrapper;
            }
        }

        #endregion
    }
}
