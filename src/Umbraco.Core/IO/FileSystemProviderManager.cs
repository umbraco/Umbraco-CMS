﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Configuration;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.IO
{
    public class FileSystemProviderManager
    {
        private readonly FileSystemProvidersSection _config;
        private readonly ConcurrentSet<ShadowWrapper> _wrappers = new ConcurrentSet<ShadowWrapper>();

        private readonly ConcurrentDictionary<string, ProviderConstructionInfo> _providerLookup = new ConcurrentDictionary<string, ProviderConstructionInfo>();
        private readonly ConcurrentDictionary<string, IFileSystem2> _filesystems = new ConcurrentDictionary<string, IFileSystem2>();

        private ShadowWrapper _macroPartialFileSystem;
        private ShadowWrapper _partialViewsFileSystem;
        private ShadowWrapper _stylesheetsFileSystem;
        private ShadowWrapper _scriptsFileSystem;
        private ShadowWrapper _xsltFileSystem;
        private ShadowWrapper _masterPagesFileSystem;
        private ShadowWrapper _mvcViewsFileSystem;

        #region Singleton & Constructor

        private static readonly FileSystemProviderManager Instance = new FileSystemProviderManager();

        public static FileSystemProviderManager Current
        {
            get { return Instance; }
        }

        // for tests only, totally unsafe
        internal void Reset()
        {
            _wrappers.Clear();
            _providerLookup.Clear();
            _filesystems.Clear();
            CreateWellKnownFileSystems();
        }

        private IScopeProviderInternal ScopeProvider
        {
            // this is bad, but enough for now, and we'll refactor
            // in v8 when we'll get rid of this class' singleton
            // beware: means that we capture the "current" scope provider - take care in tests!
            get { return ApplicationContext.Current == null ? null : ApplicationContext.Current.ScopeProvider as IScopeProviderInternal; }
        }

        internal FileSystemProviderManager()
        {
            _config = (FileSystemProvidersSection) ConfigurationManager.GetSection("umbracoConfiguration/FileSystemProviders");
            CreateWellKnownFileSystems();
        }

        private void CreateWellKnownFileSystems()
        {
            var macroPartialFileSystem = new PhysicalFileSystem(SystemDirectories.MacroPartials);
            var partialViewsFileSystem = new PhysicalFileSystem(SystemDirectories.PartialViews);
            var stylesheetsFileSystem = new PhysicalFileSystem(SystemDirectories.Css);
            var scriptsFileSystem = new PhysicalFileSystem(SystemDirectories.Scripts);
            var xsltFileSystem = new PhysicalFileSystem(SystemDirectories.Xslt);
            var masterPagesFileSystem = new PhysicalFileSystem(SystemDirectories.Masterpages);
            var mvcViewsFileSystem = new PhysicalFileSystem(SystemDirectories.MvcViews);

            _macroPartialFileSystem = new ShadowWrapper(macroPartialFileSystem, "Views/MacroPartials", ScopeProvider);
            _partialViewsFileSystem = new ShadowWrapper(partialViewsFileSystem, "Views/Partials", ScopeProvider);
            _stylesheetsFileSystem = new ShadowWrapper(stylesheetsFileSystem, "css", ScopeProvider);
            _scriptsFileSystem = new ShadowWrapper(scriptsFileSystem, "scripts", ScopeProvider);
            _xsltFileSystem = new ShadowWrapper(xsltFileSystem, "xslt", ScopeProvider);
            _masterPagesFileSystem = new ShadowWrapper(masterPagesFileSystem, "masterpages", ScopeProvider);
            _mvcViewsFileSystem = new ShadowWrapper(mvcViewsFileSystem, "Views", ScopeProvider);

            // filesystems obtained from GetFileSystemProvider are already wrapped and do not need to be wrapped again
            MediaFileSystem = GetFileSystemProvider<MediaFileSystem>();
        }

        #endregion

        #region Well-Known FileSystems

        public IFileSystem2 MacroPartialsFileSystem { get { return _macroPartialFileSystem; } }
        public IFileSystem2 PartialViewsFileSystem { get { return _partialViewsFileSystem; } }
        public IFileSystem2 StylesheetsFileSystem { get { return _stylesheetsFileSystem; } }
        public IFileSystem2 ScriptsFileSystem { get { return _scriptsFileSystem; } }
        public IFileSystem2 XsltFileSystem { get { return _xsltFileSystem; } }
        public IFileSystem2 MasterPagesFileSystem { get { return _mvcViewsFileSystem; } }
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
        /// /// <param name="fallback">A fallback creator for the filesystem.</param>
        /// <returns>The non-typed filesystem supporting the strongly-typed filesystem with the specified alias.</returns>
        /// <remarks>This method should not be used directly, used <see cref="GetFileSystemProvider{TFileSystem}()"/> instead.</remarks>
        private IFileSystem GetUnderlyingFileSystemProvider(string alias, Func<IFileSystem> fallback)
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
                throw new ArgumentException(string.Format("No provider found with alias {0}.", alias));
            }

            // get the filesystem type
            var providerType = Type.GetType(providerConfig.Type);
            if (providerType == null)
                throw new InvalidOperationException(string.Format("Could not find type {0}.", providerConfig.Type));

            // ensure it implements IFileSystem
            if (providerType.IsAssignableFrom(typeof(IFileSystem)))
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
            return (TFileSystem) _filesystems.GetOrAdd(alias, _ =>
            {
                // gets the inner fs, create the strongly-typed fs wrapping the inner fs, register & return
                // so we are double-wrapping here
                // could be optimized by having FileSystemWrapper inherit from ShadowWrapper, maybe
                var innerFs = GetUnderlyingFileSystemNoCache(alias, fallback);
                var shadowWrapper = new ShadowWrapper(innerFs, "typed/" + alias, ScopeProvider);
                var fs = (IFileSystem2) Activator.CreateInstance(typeof (TFileSystem), shadowWrapper);
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

            return new ShadowFileSystems(id, wrappers);
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
