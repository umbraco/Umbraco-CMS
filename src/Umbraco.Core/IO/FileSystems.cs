using System.ComponentModel;
using Microsoft.Extensions.Logging;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Core.IO
{
    /// <summary>
    /// Provides the system filesystems.
    /// </summary>
    public sealed class FileSystems
    {
        private readonly ILogger<FileSystems> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IFileSystemFactory _fileSystemFactory;


        // wrappers for shadow support
        private ShadowWrapper? _partialViewsFileSystem;
        private ShadowWrapper? _stylesheetsFileSystem;
        private ShadowWrapper? _scriptsFileSystem;
        private ShadowWrapper? _mvcViewsFileSystem;

        // well-known file systems lazy initialization
        private object _wkfsLock = new();
        private bool _wkfsInitialized;
        private object? _wkfsObject; // unused

        // shadow support
        private readonly List<ShadowWrapper> _shadowWrappers = new();
        private readonly Lock _shadowLocker = new();
        private static string? _shadowCurrentId; // static - unique!!
        #region Constructor

        // DI wants a public ctor
        public FileSystems(
            ILoggerFactory loggerFactory,
            IHostingEnvironment hostingEnvironment,
            IFileSystemFactory fileSystemFactory)
        {
            _logger = loggerFactory.CreateLogger<FileSystems>();
            _loggerFactory = loggerFactory;
            _hostingEnvironment = hostingEnvironment;
            _fileSystemFactory = fileSystemFactory;
        }

        // Ctor for tests, allows you to set the various filesystems
        internal FileSystems(
            ILoggerFactory loggerFactory,
            IHostingEnvironment hostingEnvironment,
            IFileSystem partialViewsFileSystem,
            IFileSystem stylesheetFileSystem,
            IFileSystem scriptsFileSystem,
            IFileSystem mvcViewFileSystem,
            IFileSystemFactory fileSystemFactory)
            : this(loggerFactory, hostingEnvironment, fileSystemFactory)
        {
            _partialViewsFileSystem = CreateShadowWrapperInternal(partialViewsFileSystem, "partials");
            _stylesheetsFileSystem = CreateShadowWrapperInternal(stylesheetFileSystem, "css");
            _scriptsFileSystem = CreateShadowWrapperInternal(scriptsFileSystem, "scripts");
            _mvcViewsFileSystem = CreateShadowWrapperInternal(mvcViewFileSystem, "view");
            // Set initialized to true so the filesystems doesn't get overwritten.
            _wkfsInitialized = true;
        }

        /// <summary>
        /// Used be Scope provider to take control over the filesystems, should never be used for anything else.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Func<bool?>? IsScoped { get; set; } = () => false;

        #endregion

        #region Well-Known FileSystems

        /// <summary>
        /// Gets the partial views filesystem.
        /// </summary>
        public IFileSystem? PartialViewsFileSystem
        {
            get
            {
                if (Volatile.Read(ref _wkfsInitialized) == false)
                {
                    EnsureWellKnownFileSystems();
                }

                return _partialViewsFileSystem;
            }
        }

        /// <summary>
        /// Gets the stylesheets filesystem.
        /// </summary>
        public IFileSystem? StylesheetsFileSystem
        {
            get
            {
                if (Volatile.Read(ref _wkfsInitialized) == false)
                {
                    EnsureWellKnownFileSystems();
                }

                return _stylesheetsFileSystem;
            }
        }

        /// <summary>
        /// Gets the scripts filesystem.
        /// </summary>
        public IFileSystem? ScriptsFileSystem
        {
            get
            {
                if (Volatile.Read(ref _wkfsInitialized) == false)
                {
                    EnsureWellKnownFileSystems();
                }

                return _scriptsFileSystem;
            }
        }

        /// <summary>
        /// Gets the MVC views filesystem.
        /// </summary>
        public IFileSystem? MvcViewsFileSystem
        {
            get
            {
                if (Volatile.Read(ref _wkfsInitialized) == false)
                {
                    EnsureWellKnownFileSystems();
                }

                return _mvcViewsFileSystem;
            }
        }

        /// <summary>
        /// Sets the stylesheet filesystem.
        /// </summary>
        /// <remarks>
        /// Be careful when using this, the root path and root url must be correct for this to work.
        /// </remarks>
        /// <param name="fileSystem">The <see cref="IFileSystem"/>.</param>
        /// <exception cref="ArgumentNullException">If the <paramref name="fileSystem"/> is <c>null</c></exception>
        /// <exception cref="InvalidOperationException">Throws exception if the StylesheetFileSystem has already been initialized.</exception>
        /// <exception cref="InvalidOperationException">Throws exception if full path can't be resolved successfully.</exception>
        public void SetStylesheetFilesystem(IFileSystem fileSystem)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }

            if (_stylesheetsFileSystem != null)
            {
                throw new InvalidOperationException(
                    "The StylesheetFileSystem cannot be changed when it's already been initialized.");
            }

            // Verify that _rootUrl/_rootPath is correct
            // We have to do this because there's a tight coupling
            // to the VirtualPath we get with CodeFileDisplay from the frontend.
            try
            {
                fileSystem.GetFullPath("/css/");
            }
            catch (UnauthorizedAccessException exception)
            {
                throw new UnauthorizedAccessException(
                    "Can't register the stylesheet filesystem, "
                    + "this is most likely caused by using a PhysicalFileSystem with an incorrect "
                    + "rootPath/rootUrl. RootPath must be <installation folder>\\wwwroot\\css"
                    + " and rootUrl must be /css",
                    exception);
            }

            _stylesheetsFileSystem = CreateShadowWrapperInternal(fileSystem, "css");
        }

        private void EnsureWellKnownFileSystems() => LazyInitializer.EnsureInitialized(ref _wkfsObject, ref _wkfsInitialized, ref _wkfsLock, CreateWellKnownFileSystems);

        // need to return something to LazyInitializer.EnsureInitialized
        // but it does not really matter what we return - here, null
        private object? CreateWellKnownFileSystems()
        {
            ILogger<PhysicalFileSystem> logger = _loggerFactory.CreateLogger<PhysicalFileSystem>();

            var partialViewsFileSystem = _fileSystemFactory.CreatePartialViewFileSystem();
            var scriptsFileSystem = _fileSystemFactory.CreateScriptsFileSystem();
            var mvcViewsFileSystem = _fileSystemFactory.CreateMvcViewsFileSystem();

            _partialViewsFileSystem = new ShadowWrapper(partialViewsFileSystem, _hostingEnvironment, _fileSystemFactory, "partials", IsScoped);
            _scriptsFileSystem = new ShadowWrapper(scriptsFileSystem, _hostingEnvironment, _fileSystemFactory, "scripts", IsScoped);
            _mvcViewsFileSystem = new ShadowWrapper(mvcViewsFileSystem, _hostingEnvironment, _fileSystemFactory, "views", IsScoped);

            if (_stylesheetsFileSystem == null)
            {
                var stylesheetsFileSystem = _fileSystemFactory.CreateStylesheetFileSystem();

                _stylesheetsFileSystem = new ShadowWrapper(stylesheetsFileSystem, _hostingEnvironment, _fileSystemFactory, "css", IsScoped);

                _shadowWrappers.Add(_stylesheetsFileSystem);
            }

            // TODO: do we need a lock here?
            _shadowWrappers.Add(_partialViewsFileSystem);
            _shadowWrappers.Add(_scriptsFileSystem);
            _shadowWrappers.Add(_mvcViewsFileSystem);

            return null;
        }

        #endregion

        #region Shadow

        // note
        // shadowing is thread-safe, but entering and exiting shadow mode is not, and there is only one
        // global shadow for the entire application, so great care should be taken to ensure that the
        // application is *not* doing anything else when using a shadow.

        /// <summary>
        /// Shadows the filesystem, should never be used outside the Scope class.
        /// </summary>
        /// <returns></returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ICompletable Shadow()
        {
            if (Volatile.Read(ref _wkfsInitialized) == false)
            {
                EnsureWellKnownFileSystems();
            }

            var id = ShadowWrapper.CreateShadowId(_hostingEnvironment);
            return new ShadowFileSystems(this, id); // will invoke BeginShadow and EndShadow
        }

        internal void BeginShadow(string id)
        {
            lock (_shadowLocker)
            {
                // if we throw here, it means that something very wrong happened.
                if (_shadowCurrentId != null)
                {
                    throw new InvalidOperationException("Already shadowing.");
                }

                _shadowCurrentId = id;
                if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                {
                    _logger.LogDebug("Shadow '{ShadowId}'", _shadowCurrentId);
                }

                foreach (ShadowWrapper wrapper in _shadowWrappers)
                {
                    wrapper.Shadow(_shadowCurrentId);
                }
            }
        }

        internal void EndShadow(string id, bool completed)
        {
            lock (_shadowLocker)
            {
                // if we throw here, it means that something very wrong happened.
                if (_shadowCurrentId == null)
                {
                    throw new InvalidOperationException("Not shadowing.");
                }

                if (id != _shadowCurrentId)
                {
                    throw new InvalidOperationException("Not the current shadow.");
                }
                if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                {
                    _logger.LogDebug("UnShadow '{ShadowId}' {Status}", id, completed ? "complete" : "abort");
                }

                var exceptions = new List<Exception>();
                foreach (ShadowWrapper wrapper in _shadowWrappers)
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
                {
                    throw new AggregateException(completed ? "Failed to apply all changes (see exceptions)." : "Failed to abort (see exceptions).", exceptions);
                }
            }
        }

        /// <summary>
        /// Creates a shadow wrapper for a filesystem, should never be used outside UmbracoBuilder or testing
        /// </summary>
        /// <param name="filesystem"></param>
        /// <param name="shadowPath"></param>
        /// <returns></returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IFileSystem CreateShadowWrapper(IFileSystem filesystem, string shadowPath) => CreateShadowWrapperInternal(filesystem, shadowPath);

        private ShadowWrapper CreateShadowWrapperInternal(IFileSystem filesystem, string shadowPath)
        {
            lock (_shadowLocker)
            {
                var wrapper = new ShadowWrapper(filesystem, _hostingEnvironment, _fileSystemFactory, shadowPath, () => IsScoped?.Invoke());
                if (_shadowCurrentId != null)
                {
                    wrapper.Shadow(_shadowCurrentId);
                }

                _shadowWrappers.Add(wrapper);
                return wrapper;
            }
        }

        #endregion
    }
}
