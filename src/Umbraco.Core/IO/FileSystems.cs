using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Plugins;

namespace Umbraco.Core.IO
{	
    public class FileSystems
    {
        private readonly FileSystemProvidersSection _config;
        private readonly WeakSet<ShadowWrapper> _wrappers = new WeakSet<ShadowWrapper>();
        private readonly ILogger _logger;

        // wrappers for shadow support
        private readonly ShadowWrapper _macroPartialFileSystemWrapper;
        private readonly ShadowWrapper _partialViewsFileSystemWrapper;
        private readonly ShadowWrapper _stylesheetsFileSystemWrapper;
        private readonly ShadowWrapper _scriptsFileSystemWrapper;
        private readonly ShadowWrapper _xsltFileSystemWrapper;
        private readonly ShadowWrapper _masterPagesFileSystemWrapper;
        private readonly ShadowWrapper _mvcViewsFileSystemWrapper;

        #region Singleton & Constructor

        public FileSystems(ILogger logger)
        {
            _config = (FileSystemProvidersSection) ConfigurationManager.GetSection("umbracoConfiguration/FileSystemProviders");
            _logger = logger;

            // create the filesystems
            MacroPartialsFileSystem = new PhysicalFileSystem(SystemDirectories.MacroPartials);
            PartialViewsFileSystem = new PhysicalFileSystem(SystemDirectories.PartialViews);
            StylesheetsFileSystem = new PhysicalFileSystem(SystemDirectories.Css);
            ScriptsFileSystem = new PhysicalFileSystem(SystemDirectories.Scripts);
            XsltFileSystem = new PhysicalFileSystem(SystemDirectories.Xslt);
            MasterPagesFileSystem = new PhysicalFileSystem(SystemDirectories.Masterpages);
            MvcViewsFileSystem = new PhysicalFileSystem(SystemDirectories.MvcViews);

            // wrap the filesystems for shadow support
            MacroPartialsFileSystem = _macroPartialFileSystemWrapper = new ShadowWrapper(MacroPartialsFileSystem, "Views/MacroPartials");
            PartialViewsFileSystem = _partialViewsFileSystemWrapper = new ShadowWrapper(PartialViewsFileSystem, "Views/Partials");
            StylesheetsFileSystem = _stylesheetsFileSystemWrapper = new ShadowWrapper(StylesheetsFileSystem, "css");
            ScriptsFileSystem = _scriptsFileSystemWrapper = new ShadowWrapper(ScriptsFileSystem, "scripts");
            XsltFileSystem = _xsltFileSystemWrapper = new ShadowWrapper(XsltFileSystem, "xslt");
            MasterPagesFileSystem = _masterPagesFileSystemWrapper = new ShadowWrapper(MasterPagesFileSystem, "masterpages");
            MvcViewsFileSystem = _mvcViewsFileSystemWrapper = new ShadowWrapper(MvcViewsFileSystem, "Views");

            // obtain filesystems from GetFileSystem 
            // these are already wrapped and do not need to be wrapped again
            MediaFileSystem = GetFileSystem<MediaFileSystem>();
        }

        #endregion

        #region Well-Known FileSystems

        public IFileSystem MacroPartialsFileSystem { get; }
        public IFileSystem PartialViewsFileSystem { get; }
        public IFileSystem StylesheetsFileSystem { get; }
        public IFileSystem ScriptsFileSystem { get; }
        public IFileSystem XsltFileSystem { get; }
        public IFileSystem MasterPagesFileSystem { get; }
        public IFileSystem MvcViewsFileSystem { get; }
        public MediaFileSystem MediaFileSystem { get; }

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

		private readonly ConcurrentDictionary<string, ProviderConstructionInfo> _providerLookup = new ConcurrentDictionary<string, ProviderConstructionInfo>();
		private readonly ConcurrentDictionary<Type, string> _aliases = new ConcurrentDictionary<Type, string>(); 

        /// <summary>
        /// Gets an underlying (non-typed) filesystem supporting a strongly-typed filesystem.
        /// </summary>
        /// <param name="alias">The alias of the strongly-typed filesystem.</param>
        /// <returns>The non-typed filesystem supporting the strongly-typed filesystem with the specified alias.</returns>
        /// <remarks>This method should not be used directly, used <see cref="GetFileSystem{TFileSystem}"/> instead.</remarks>
        internal IFileSystem GetUnderlyingFileSystemProvider(string alias)
        {
			// either get the constructor info from cache or create it and add to cache
	        var ctorInfo = _providerLookup.GetOrAdd(alias, s =>
		        {
                    // get config
			        var providerConfig = _config.Providers[s];
			        if (providerConfig == null)
				        throw new ArgumentException($"No provider found with alias {s}.");

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
		        });

            // create the fs and return
			return (IFileSystem) ctorInfo.Constructor.Invoke(ctorInfo.Parameters);
        }

        /// <summary>
        /// Gets a strongly-typed filesystem.
        /// </summary>
        /// <typeparam name="TFileSystem">The type of the filesystem.</typeparam>
        /// <returns>A strongly-typed filesystem of the specified type.</returns>
        public TFileSystem GetFileSystem<TFileSystem>()
			where TFileSystem : FileSystemWrapper
        {
            // deal with known types - avoid infinite loops!
            if (typeof(TFileSystem) == typeof(MediaFileSystem) && MediaFileSystem != null)
                return MediaFileSystem as TFileSystem; // else create and return

			// get/cache the alias for the filesystem type
	        var alias = _aliases.GetOrAdd(typeof (TFileSystem), fsType =>
		        {
					// validate the ctor
					var constructor = fsType.GetConstructors().SingleOrDefault(x 
                        => x.GetParameters().Length == 1 && TypeHelper.IsTypeAssignableFrom<IFileSystem>(x.GetParameters().Single().ParameterType));
					if (constructor == null)
						throw new InvalidOperationException("Type " + fsType.FullName + " must inherit from FileSystemWrapper and have a constructor that accepts one parameter of type " + typeof(IFileSystem).FullName + ".");

                    // find the attribute and get the alias
					var attr = (FileSystemProviderAttribute) fsType.GetCustomAttributes(typeof(FileSystemProviderAttribute), false).SingleOrDefault();
					if (attr == null)
						throw new InvalidOperationException("Type " + fsType.FullName + "is missing the required FileSystemProviderAttribute.");

			        return attr.Alias;
		        });

            // gets the inner fs, create the strongly-typed fs wrapping the inner fs, register & return
            // so we are double-wrapping here
            // could be optimized by having FileSystemWrapper inherit from ShadowWrapper, maybe
            var innerFs = GetUnderlyingFileSystemProvider(alias);
            var shadowWrapper = new ShadowWrapper(innerFs, "typed/" + alias);
	        var fs = (TFileSystem) Activator.CreateInstance(typeof (TFileSystem), innerFs);
            _wrappers.Add(shadowWrapper); // keeping a weak reference to the wrapper
	        return fs;
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

        public ICompletable Shadow(Guid id)
        {
            var typed = _wrappers.ToArray();
            var wrappers = new ShadowWrapper[typed.Length + 7];
            var i = 0;
            while (i < typed.Length) wrappers[i] = typed[i++];
            wrappers[i++] = _macroPartialFileSystemWrapper;
            wrappers[i++] = _partialViewsFileSystemWrapper;
            wrappers[i++] = _stylesheetsFileSystemWrapper;
            wrappers[i++] = _scriptsFileSystemWrapper;
            wrappers[i++] = _xsltFileSystemWrapper;
            wrappers[i++] = _masterPagesFileSystemWrapper;
            wrappers[i] = _mvcViewsFileSystemWrapper;

            return ShadowFileSystemsScope.CreateScope(id, wrappers, _logger);
        }

        #endregion

        private class WeakSet<T>
            where T : class
        {
            private readonly HashSet<WeakReference<T>> _set = new HashSet<WeakReference<T>>();

            public void Add(T item)
            {
                lock (_set)
                {
                    _set.Add(new WeakReference<T>(item));
                    CollectLocked();
                }
            }

            public T[] ToArray()
            {
                lock (_set)
                {
                    CollectLocked();
                    return _set.Select(x =>
                    {
                        T target;
                        return x.TryGetTarget(out target) ? target : null;
                    }).WhereNotNull().ToArray();
                }
            }

            private void CollectLocked()
            {
                _set.RemoveWhere(x =>
                {
                    T target;
                    return x.TryGetTarget(out target) == false;
                });
            }
        }
    }
}
