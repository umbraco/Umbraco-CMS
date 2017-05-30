using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Plugins;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.IO
{
    public class FileSystems
    {
        private readonly IScopeProviderInternal _scopeProvider;
        private readonly FileSystemProvidersSection _config;
        private readonly ConcurrentSet<ShadowWrapper> _wrappers = new ConcurrentSet<ShadowWrapper>();
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, ProviderConstructionInfo> _providerLookup = new ConcurrentDictionary<string, ProviderConstructionInfo>();
        private readonly ConcurrentDictionary<string, IFileSystem> _filesystems = new ConcurrentDictionary<string, IFileSystem>();

        // wrappers for shadow support
        private ShadowWrapper _macroPartialFileSystem;
        private ShadowWrapper _partialViewsFileSystem;
        private ShadowWrapper _stylesheetsFileSystem;
        private ShadowWrapper _scriptsFileSystem;
        private ShadowWrapper _xsltFileSystem;
        private ShadowWrapper _masterPagesFileSystem;
        private ShadowWrapper _mvcViewsFileSystem;

        #region Constructor

        // fixme - circular dependency on scope provider
        // managed via lazy-injecting FileSystems into ScopeProvider for now,
        // but we must refactor this somehow!

        // DI wants a public ctor
        // but IScopeProviderInternal is not public
        public FileSystems(/*IScopeProviderInternal scopeProvider,*/ ILogger logger)
            : this(Composing.Current.ScopeProvider as IScopeProviderInternal, logger)
        { }

        internal FileSystems(IScopeProviderInternal scopeProvider, ILogger logger)
        {
            _config = (FileSystemProvidersSection)ConfigurationManager.GetSection("umbracoConfiguration/FileSystemProviders");
            _scopeProvider = scopeProvider;
            _logger = logger;
            CreateWellKnownFileSystems();
        }

        // for tests only, totally unsafe
        internal void Reset()
        {
            _wrappers.Clear();
            _providerLookup.Clear();
            _filesystems.Clear();
            CreateWellKnownFileSystems();
        }

        #endregion

        #region Well-Known FileSystems

        public IFileSystem MacroPartialsFileSystem => _macroPartialFileSystem;
        public IFileSystem PartialViewsFileSystem => _partialViewsFileSystem;
        public IFileSystem StylesheetsFileSystem => _stylesheetsFileSystem;
        public IFileSystem ScriptsFileSystem => _scriptsFileSystem;
        public IFileSystem XsltFileSystem => _xsltFileSystem;
        public IFileSystem MasterPagesFileSystem => _masterPagesFileSystem; // fixme - see 7.6?!
        public IFileSystem MvcViewsFileSystem => _mvcViewsFileSystem;
        public MediaFileSystem MediaFileSystem { get; private set; }

        private void CreateWellKnownFileSystems()
        {
            var macroPartialFileSystem = new PhysicalFileSystem(SystemDirectories.MacroPartials);
            var partialViewsFileSystem = new PhysicalFileSystem(SystemDirectories.PartialViews);
            var stylesheetsFileSystem = new PhysicalFileSystem(SystemDirectories.Css);
            var scriptsFileSystem = new PhysicalFileSystem(SystemDirectories.Scripts);
            var xsltFileSystem = new PhysicalFileSystem(SystemDirectories.Xslt);
            var masterPagesFileSystem = new PhysicalFileSystem(SystemDirectories.Masterpages);
            var mvcViewsFileSystem = new PhysicalFileSystem(SystemDirectories.MvcViews);

            _macroPartialFileSystem = new ShadowWrapper(macroPartialFileSystem, "Views/MacroPartials", _scopeProvider);
            _partialViewsFileSystem = new ShadowWrapper(partialViewsFileSystem, "Views/Partials", _scopeProvider);
            _stylesheetsFileSystem = new ShadowWrapper(stylesheetsFileSystem, "css", _scopeProvider);
            _scriptsFileSystem = new ShadowWrapper(scriptsFileSystem, "scripts", _scopeProvider);
            _xsltFileSystem = new ShadowWrapper(xsltFileSystem, "xslt", _scopeProvider);
            _masterPagesFileSystem = new ShadowWrapper(masterPagesFileSystem, "masterpages", _scopeProvider);
            _mvcViewsFileSystem = new ShadowWrapper(mvcViewsFileSystem, "Views", _scopeProvider);

            // filesystems obtained from GetFileSystemProvider are already wrapped and do not need to be wrapped again
            MediaFileSystem = GetFileSystemProvider<MediaFileSystem>();
        }

        #endregion

        #region Providers

        /// <summary>
        /// used to cache the lookup of how to construct this object so we don't have to reflect each time.
        /// </summary>
        private class ProviderConstructionInfo
		{
			public object[] Parameters { get; set; }
			public ConstructorInfo Constructor { get; set; }
			//public string ProviderAlias { get; set; }
		}

        /// <summary>
        /// Gets an underlying (non-typed) filesystem supporting a strongly-typed filesystem.
        /// </summary>
        /// <param name="alias">The alias of the strongly-typed filesystem.</param>
        /// <returns>The non-typed filesystem supporting the strongly-typed filesystem with the specified alias.</returns>
        /// <remarks>This method should not be used directly, used <see cref="GetFileSystemProvider{TFileSystem}()"/> instead.</remarks>
        public IFileSystem GetUnderlyingFileSystemProvider(string alias)
        {
            return GetUnderlyingFileSystemProvider(alias, null);
        }

        /// <summary>
        /// Gets an underlying (non-typed) filesystem supporting a strongly-typed filesystem.
        /// </summary>
        /// <param name="alias">The alias of the strongly-typed filesystem.</param>
        /// <param name="fallback">A fallback creator for the filesystem.</param>
        /// <returns>The non-typed filesystem supporting the strongly-typed filesystem with the specified alias.</returns>
        /// <remarks>This method should not be used directly, used <see cref="GetFileSystem{TFileSystem}"/> instead.</remarks>
        internal IFileSystem GetUnderlyingFileSystemProvider(string alias, Func<IFileSystem> fallback)
        {
            // either get the constructor info from cache or create it and add to cache
            var ctorInfo = _providerLookup.GetOrAdd(alias, _ => GetUnderlyingFileSystemCtor(alias, fallback));
            return ctorInfo == null ? fallback() : (IFileSystem) ctorInfo.Constructor.Invoke(ctorInfo.Parameters);
        }

        private IFileSystem GetUnderlyingFileSystemNoCache(string alias, Func<IFileSystem> fallback)
        {
            var ctorInfo = GetUnderlyingFileSystemCtor(alias, fallback);
            return ctorInfo == null ? fallback() : (IFileSystem) ctorInfo.Constructor.Invoke(ctorInfo.Parameters);
        }

        private ProviderConstructionInfo GetUnderlyingFileSystemCtor(string alias, Func<IFileSystem> fallback)
        {
            // get config
            var providerConfig = _config.Providers[alias];
            if (providerConfig == null)
            {
                if (fallback != null) return null;
                throw new ArgumentException($"No provider found with alias {alias}.");
            }

            // get the filesystem type
            var providerType = Type.GetType(providerConfig.Type);
            if (providerType == null)
                throw new InvalidOperationException($"Could not find type {providerConfig.Type}.");

            // ensure it implements IFileSystem
            if (providerType.IsAssignableFrom(typeof (IFileSystem)))
                throw new InvalidOperationException($"Type {providerType.FullName} does not implement IFileSystem.");

            // find a ctor matching the config parameters
            var paramCount = providerConfig.Parameters?.Count ?? 0;
            var constructor = providerType.GetConstructors().SingleOrDefault(x
                => x.GetParameters().Length == paramCount && x.GetParameters().All(y => providerConfig.Parameters.AllKeys.Contains(y.Name)));
            if (constructor == null)
                throw new InvalidOperationException($"Type {providerType.FullName} has no ctor matching the {paramCount} configuration parameter(s).");

            var parameters = new object[paramCount];
            if (providerConfig.Parameters != null) // keeps ReSharper happy
                for (var i = 0; i < paramCount; i++)
                    parameters[i] = providerConfig.Parameters[providerConfig.Parameters.AllKeys[i]].Value;

            return new ProviderConstructionInfo
            {
                Constructor = constructor,
                Parameters = parameters,
                //ProviderAlias = s
            };
        }

        /// <summary>
        /// Gets a strongly-typed filesystem.
        /// </summary>
        /// <typeparam name="TFileSystem">The type of the filesystem.</typeparam>
        /// <returns>A strongly-typed filesystem of the specified type.</returns>
        /// <remarks>
        /// <para>Ideally, this should cache the instances, but that would break backward compatibility, so we
        /// only do it for our own MediaFileSystem - for everything else, it's the responsibility of the caller
        /// to ensure that they maintain singletons. This is important for singletons, as each filesystem maintains
        /// its own shadow and having multiple instances would lead to inconsistencies.</para>
        /// <para>Note that any filesystem created by this method *after* shadowing begins, will *not* be
        /// shadowing (and an exception will be thrown by the ShadowWrapper).</para>
        /// </remarks>
        // fixme - should it change for v8?
        public TFileSystem GetFileSystemProvider<TFileSystem>()
            where TFileSystem : FileSystemWrapper
        {
            return GetFileSystemProvider<TFileSystem>(null);
        }

        /// <summary>
        /// Gets a strongly-typed filesystem.
        /// </summary>
        /// <typeparam name="TFileSystem">The type of the filesystem.</typeparam>
        /// <param name="fallback">A fallback creator for the inner filesystem.</param>
        /// <returns>A strongly-typed filesystem of the specified type.</returns>
        /// <remarks>
        /// <para>The fallback creator is used only if nothing is configured.</para>
        /// <para>Ideally, this should cache the instances, but that would break backward compatibility, so we
        /// only do it for our own MediaFileSystem - for everything else, it's the responsibility of the caller
        /// to ensure that they maintain singletons. This is important for singletons, as each filesystem maintains
        /// its own shadow and having multiple instances would lead to inconsistencies.</para>
        /// <para>Note that any filesystem created by this method *after* shadowing begins, will *not* be
        /// shadowing (and an exception will be thrown by the ShadowWrapper).</para>
        /// </remarks>
        public TFileSystem GetFileSystemProvider<TFileSystem>(Func<IFileSystem> fallback)
            where TFileSystem : FileSystemWrapper
        {
            var alias = GetFileSystemAlias<TFileSystem>();
            return (TFileSystem)_filesystems.GetOrAdd(alias, _ =>
            {
                // gets the inner fs, create the strongly-typed fs wrapping the inner fs, register & return
                // so we are double-wrapping here
                // could be optimized by having FileSystemWrapper inherit from ShadowWrapper, maybe
                var innerFs = GetUnderlyingFileSystemNoCache(alias, fallback);
                var shadowWrapper = new ShadowWrapper(innerFs, "typed/" + alias, _scopeProvider);
                var fs = (IFileSystem) Activator.CreateInstance(typeof(TFileSystem), shadowWrapper);
                _wrappers.Add(shadowWrapper); // keeping a reference to the wrapper
                return fs;
            });
        }

        private string GetFileSystemAlias<TFileSystem>()
        {
            var fsType = typeof(TFileSystem);

            // validate the ctor
            var constructor = fsType.GetConstructors().SingleOrDefault(x
                => x.GetParameters().Length == 1 && TypeHelper.IsTypeAssignableFrom<IFileSystem>(x.GetParameters().Single().ParameterType));
            if (constructor == null)
                throw new InvalidOperationException("Type " + fsType.FullName + " must inherit from FileSystemWrapper and have a constructor that accepts one parameter of type " + typeof(IFileSystem).FullName + ".");

            // find the attribute and get the alias
            var attr = (FileSystemProviderAttribute)fsType.GetCustomAttributes(typeof(FileSystemProviderAttribute), false).SingleOrDefault();
            if (attr == null)
                throw new InvalidOperationException("Type " + fsType.FullName + "is missing the required FileSystemProviderAttribute.");

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
        // is actually created and used - after, it is too late - enabling shadow has a neglictible perfs
        // impact.
        // NO! by the time an app event handler is instanciated it is already too late, see note in ctor.
        //internal void EnableShadow()
        //{
        //    if (_mvcViewsFileSystem != null) // test one of the fs...
        //        throw new InvalidOperationException("Cannot enable shadow once filesystems have been created.");
        //    _shadowEnabled = true;
        //}

        internal ICompletable Shadow(Guid id)
        {
            var typed = _wrappers.ToArray();
            var wrappers = new ShadowWrapper[typed.Length + 7];
            var i = 0;
            while (i < typed.Length) wrappers[i] = typed[i++];
            wrappers[i++] = _macroPartialFileSystem;
            wrappers[i++] = _partialViewsFileSystem;
            wrappers[i++] = _stylesheetsFileSystem;
            wrappers[i++] = _scriptsFileSystem;
            wrappers[i++] = _xsltFileSystem;
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
