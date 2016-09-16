using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;

namespace Umbraco.Core.IO
{	
    public class FileSystemProviderManager
    {
        private readonly FileSystemProvidersSection _config;
        private readonly object _shadowLocker = new object();
        private readonly WeakSet<FileSystemWrapper> _fs = new WeakSet<FileSystemWrapper>();
        private readonly bool _shadowEnabled;
        private Guid _shadow = Guid.Empty;
        private FileSystemWrapper[] _shadowFs;

        // actual well-known filesystems returned by properties
        private readonly IFileSystem2 _macroPartialFileSystem;
        private readonly IFileSystem2 _partialViewsFileSystem;
        private readonly IFileSystem2 _stylesheetsFileSystem;
        private readonly IFileSystem2 _scriptsFileSystem;
        private readonly IFileSystem2 _xsltFileSystem;
        private readonly IFileSystem2 _masterPagesFileSystem;
        private readonly IFileSystem2 _mvcViewsFileSystem;

        // when shadowing is enabled, above filesystems, as wrappers
        private readonly FileSystemWrapper2 _macroPartialFileSystemWrapper;
        private readonly FileSystemWrapper2 _partialViewsFileSystemWrapper;
        private readonly FileSystemWrapper2 _stylesheetsFileSystemWrapper;
        private readonly FileSystemWrapper2 _scriptsFileSystemWrapper;
        private readonly FileSystemWrapper2 _xsltFileSystemWrapper;
        private readonly FileSystemWrapper2 _masterPagesFileSystemWrapper;
        private readonly FileSystemWrapper2 _mvcViewsFileSystemWrapper;

        #region Singleton & Constructor

        private static readonly FileSystemProviderManager Instance = new FileSystemProviderManager();

        public static FileSystemProviderManager Current
        {
            get { return Instance; }
        }

        internal FileSystemProviderManager()
        {
            _config = (FileSystemProvidersSection) ConfigurationManager.GetSection("umbracoConfiguration/FileSystemProviders");

            _macroPartialFileSystem = new PhysicalFileSystem(SystemDirectories.MacroPartials);
            _partialViewsFileSystem = new PhysicalFileSystem(SystemDirectories.PartialViews);
            _stylesheetsFileSystem = new PhysicalFileSystem(SystemDirectories.Css);
            _scriptsFileSystem = new PhysicalFileSystem(SystemDirectories.Scripts);
            _xsltFileSystem = new PhysicalFileSystem(SystemDirectories.Xslt);
            _masterPagesFileSystem = new PhysicalFileSystem(SystemDirectories.Masterpages);
            _mvcViewsFileSystem = new PhysicalFileSystem(SystemDirectories.MvcViews);

            // if shadow is enable we need a mean to replace the filesystem by a shadowed filesystem, however we cannot
            // replace the actual filesystem as we don't know if anything is not holding an app-long reference to them,
            // so we have to force-wrap each of them and work with the wrapped filesystem. if shadow is not enabled,
            // no need to wrap (small perfs improvement).

            // fixme - irks!
            // but cannot be enabled by deploy from an application event handler, because by the time an app event handler
            // is instanciated it is already too late and some filesystems have been referenced by Core. here we force
            // enable for deploy... but maybe it should be some sort of config option?
            _shadowEnabled = AppDomain.CurrentDomain.GetAssemblies().Any(x => x.GetName().Name == "Umbraco.Deploy");

            if (_shadowEnabled)
            {
                _macroPartialFileSystem = _macroPartialFileSystemWrapper = new FileSystemWrapper2(_macroPartialFileSystem);
                _partialViewsFileSystem = _partialViewsFileSystemWrapper = new FileSystemWrapper2(_partialViewsFileSystem);
                _stylesheetsFileSystem = _stylesheetsFileSystemWrapper = new FileSystemWrapper2(_stylesheetsFileSystem);
                _scriptsFileSystem = _scriptsFileSystemWrapper = new FileSystemWrapper2(_scriptsFileSystem);
                _xsltFileSystem = _xsltFileSystemWrapper = new FileSystemWrapper2(_xsltFileSystem);
                _masterPagesFileSystem = _masterPagesFileSystemWrapper = new FileSystemWrapper2(_masterPagesFileSystem);
                _mvcViewsFileSystem = _mvcViewsFileSystemWrapper = new FileSystemWrapper2(_mvcViewsFileSystem);
            }

            // filesystems obtained from GetFileSystemProvider are already wrapped and do not need to be wrapped again,
            // whether shadow is enabled or not

            MediaFileSystem = GetFileSystemProvider<MediaFileSystem>();
        }

        #endregion

        #region Well-Known FileSystems

        public IFileSystem2 MacroPartialsFileSystem { get { return _macroPartialFileSystem; } }
        public IFileSystem2 PartialViewsFileSystem { get { return _partialViewsFileSystem; } }
        public IFileSystem2 StylesheetsFileSystem { get { return _stylesheetsFileSystem; } }
        public IFileSystem2 ScriptsFileSystem { get { return _scriptsFileSystem; } }
        public IFileSystem2 XsltFileSystem { get { return _xsltFileSystem; } }
        public IFileSystem2 MasterPagesFileSystem { get { return _masterPagesFileSystem; } }
        public IFileSystem2 MvcViewsFileSystem { get { return _mvcViewsFileSystem; } }
        public MediaFileSystem MediaFileSystem { get; private set; }

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
        /// <remarks>This method should not be used directly, used <see cref="GetFileSystemProvider{TFileSystem}"/> instead.</remarks>
        public IFileSystem GetUnderlyingFileSystemProvider(string alias)
        {
			// either get the constructor info from cache or create it and add to cache
	        var ctorInfo = _providerLookup.GetOrAdd(alias, s =>
		        {
                    // get config
			        var providerConfig = _config.Providers[s];
			        if (providerConfig == null)
				        throw new ArgumentException(string.Format("No provider found with alias {0}.", s));

                    // get the filesystem type
			        var providerType = Type.GetType(providerConfig.Type);
			        if (providerType == null)
				        throw new InvalidOperationException(string.Format("Could not find type {0}.", providerConfig.Type));

                    // ensure it implements IFileSystem
			        if (providerType.IsAssignableFrom(typeof (IFileSystem)))
				        throw new InvalidOperationException(string.Format("Type {0} does not implement IFileSystem.", providerType.FullName));

                    // find a ctor matching the config parameters
			        var paramCount = providerConfig.Parameters != null ? providerConfig.Parameters.Count : 0;
			        var constructor = providerType.GetConstructors().SingleOrDefault(x 
                        => x.GetParameters().Length == paramCount && x.GetParameters().All(y => providerConfig.Parameters.AllKeys.Contains(y.Name)));
			        if (constructor == null)
				        throw new InvalidOperationException(string.Format("Type {0} has no ctor matching the {1} configuration parameter(s).", providerType.FullName, paramCount));

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
        public TFileSystem GetFileSystemProvider<TFileSystem>()
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
            var innerFs = GetUnderlyingFileSystemProvider(alias);
	        var fs = (TFileSystem) Activator.CreateInstance(typeof (TFileSystem), innerFs);
            if (_shadowEnabled)
                _fs.Add(fs);
	        return fs;
        }

        #endregion

        #region Shadow

        // note
        // shadowing is thread-safe, but entering and exiting shadow mode is not, and there is only one
        // global shadow for the entire application, so great care should be taken to ensure that the
        // application is *not* doing anything else when using a shadow.
        // shadow applies to well-known filesystems *only* - at the moment, any other filesystem that would
        // be created directly (via ctor) or via GetFileSystemProvider<T> is *not* shadowed.

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

        internal void Shadow(Guid id)
        {
            lock (_shadowLocker)
            {
                if (_shadowEnabled == false) throw new InvalidOperationException("Shadowing is not enabled.");
                if (_shadow != Guid.Empty) throw new InvalidOperationException("Already shadowing (" + _shadow + ").");
                _shadow = id;

                LogHelper.Debug<FileSystemProviderManager>("Shadow " + id + ".");

                ShadowFs(id, _macroPartialFileSystemWrapper, "Views/MacroPartials");
                ShadowFs(id, _partialViewsFileSystemWrapper, "Views/Partials");
                ShadowFs(id, _stylesheetsFileSystemWrapper, "css");
                ShadowFs(id, _scriptsFileSystemWrapper, "scripts");
                ShadowFs(id, _xsltFileSystemWrapper, "xslt");
                ShadowFs(id, _masterPagesFileSystemWrapper, "masterpages");
                ShadowFs(id, _mvcViewsFileSystemWrapper, "Views");

                _shadowFs = _fs.ToArray();
                foreach (var fs in _shadowFs)
                    ShadowFs(id, fs, "stfs/" + fs.GetType().FullName);
            }
        }

        private static void ShadowFs(Guid id, FileSystemWrapper filesystem, string path)
        {
            var virt = "~/App_Data/Shadow/" + id + "/" + path;
            var dir = HostingEnvironment.MapPath(virt);
            if (dir == null) throw new InvalidOperationException("Could not map path.");
            Directory.CreateDirectory(dir);

            // shadow filesystem pretends to be IFileSystem2 even though the inner filesystem
            // is not, by invoking the GetSize extension method when needed.
            var shadowFs = new ShadowFileSystem(filesystem.Wrapped, new PhysicalFileSystem(virt));
            filesystem.Wrapped = shadowFs;
        }

        internal void UnShadow(bool complete)
        {
            lock (_shadowLocker)
            {
                if (_shadow == Guid.Empty) return;

                // copy and null before anything else
                var shadow = _shadow;
                var shadowFs = _shadowFs;
                _shadow = Guid.Empty;
                _shadowFs = null;

                LogHelper.Debug<FileSystemProviderManager>("UnShadow " + shadow + (complete?" (complete)":" (abort)") + ".");

                if (complete)
                {
                    ((ShadowFileSystem) _macroPartialFileSystemWrapper.Wrapped).Complete();
                    ((ShadowFileSystem) _partialViewsFileSystemWrapper.Wrapped).Complete();
                    ((ShadowFileSystem) _stylesheetsFileSystemWrapper.Wrapped).Complete();
                    ((ShadowFileSystem) _scriptsFileSystemWrapper.Wrapped).Complete();
                    ((ShadowFileSystem) _xsltFileSystemWrapper.Wrapped).Complete();
                    ((ShadowFileSystem) _masterPagesFileSystemWrapper.Wrapped).Complete();
                    ((ShadowFileSystem) _mvcViewsFileSystemWrapper.Wrapped).Complete();

                    foreach (var fs in shadowFs)
                        ((ShadowFileSystem) fs.Wrapped).Complete();
                }

                UnShadowFs(_macroPartialFileSystemWrapper);
                UnShadowFs(_partialViewsFileSystemWrapper);
                UnShadowFs(_stylesheetsFileSystemWrapper);
                UnShadowFs(_scriptsFileSystemWrapper);
                UnShadowFs(_xsltFileSystemWrapper);
                UnShadowFs(_masterPagesFileSystemWrapper);
                UnShadowFs(_mvcViewsFileSystemWrapper);

                foreach (var fs in shadowFs)
                    UnShadowFs(fs);
            }
        }

        private static void UnShadowFs(FileSystemWrapper filesystem)
        {
            var inner = ((ShadowFileSystem) filesystem.Wrapped).Inner;
            filesystem.Wrapped = inner;
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
