using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.IO
{	
    public class FileSystemProviderManager
    {
        private readonly FileSystemProvidersSection _config;

        #region Singleton

        private static readonly FileSystemProviderManager Instance = new FileSystemProviderManager();

        public static FileSystemProviderManager Current
        {
            get { return Instance; }
        }

        #endregion

        #region Constructors

        internal FileSystemProviderManager()
        {
            _config = (FileSystemProvidersSection)ConfigurationManager.GetSection("umbracoConfiguration/FileSystemProviders");
        }

        #endregion

		/// <summary>
		/// used to cache the lookup of how to construct this object so we don't have to reflect each time.
		/// </summary>
		private class ProviderConstructionInfo
		{
			public object[] Parameters { get; set; }
			public ConstructorInfo Constructor { get; set; }
			public string ProviderAlias { get; set; }
		}

		private readonly ConcurrentDictionary<string, ProviderConstructionInfo> _providerLookup = new ConcurrentDictionary<string, ProviderConstructionInfo>();
		private readonly ConcurrentDictionary<Type, string> _wrappedProviderLookup = new ConcurrentDictionary<Type, string>(); 

        /// <summary>
        /// Returns the underlying (non-typed) file system provider for the alias specified
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        /// <remarks>
        /// It is recommended to use the typed GetFileSystemProvider method instead to get a strongly typed provider instance.
        /// </remarks>
        public IFileSystem GetUnderlyingFileSystemProvider(string alias)
        {
			//either get the constructor info from cache or create it and add to cache
	        var ctorInfo = _providerLookup.GetOrAdd(alias, s =>
		        {
			        var providerConfig = _config.Providers[s];
			        if (providerConfig == null)
				        throw new ArgumentException(string.Format("No provider found with the alias '{0}'", s));

			        var providerType = Type.GetType(providerConfig.Type);
			        if (providerType == null)
				        throw new InvalidOperationException(string.Format("Could not find type '{0}'", providerConfig.Type));

			        if (providerType.IsAssignableFrom(typeof (IFileSystem)))
				        throw new InvalidOperationException(string.Format("The type '{0}' does not implement IFileSystem", providerConfig.Type));

			        var paramCount = providerConfig.Parameters != null ? providerConfig.Parameters.Count : 0;
			        var constructor = providerType.GetConstructors()
				        .SingleOrDefault(x => x.GetParameters().Count() == paramCount
				                              && x.GetParameters().All(y => providerConfig.Parameters.AllKeys.Contains(y.Name)));
			        if (constructor == null)
				        throw new InvalidOperationException(string.Format("Could not find constructor for type '{0}' which accepts {1} parameters", providerConfig.Type, paramCount));

			        var parameters = new object[paramCount];
			        for (var i = 0; i < paramCount; i++)
				        parameters[i] = providerConfig.Parameters[providerConfig.Parameters.AllKeys[i]].Value;			

			        //return the new constructor info class to cache so we don't have to do this again.
			        return new ProviderConstructionInfo()
				        {
					        Constructor = constructor,
					        Parameters = parameters,
					        ProviderAlias = s
				        };
		        });

			var fs = (IFileSystem)ctorInfo.Constructor.Invoke(ctorInfo.Parameters);
	        return fs;
        }

        /// <summary>
        /// Returns the strongly typed file system provider
        /// </summary>
        /// <typeparam name="TProviderTypeFilter"></typeparam>
        /// <returns></returns>
        public TProviderTypeFilter GetFileSystemProvider<TProviderTypeFilter>()
			where TProviderTypeFilter : FileSystemWrapper
        {
			//get the alias for the type from cache or look it up and add it to the cache, then we don't have to reflect each time
	        var alias = _wrappedProviderLookup.GetOrAdd(typeof (TProviderTypeFilter), fsType =>
		        {
					//validate the ctor
					var constructor = fsType.GetConstructors()
						.SingleOrDefault(x =>
										 x.GetParameters().Count() == 1 && TypeHelper.IsTypeAssignableFrom<IFileSystem>(x.GetParameters().Single().ParameterType));
					if (constructor == null)
						throw new InvalidOperationException("The type of " + fsType + " must inherit from FileSystemWrapper and must have a constructor that accepts one parameter of type " + typeof(IFileSystem));

					var attr =
						(FileSystemProviderAttribute)fsType.GetCustomAttributes(typeof(FileSystemProviderAttribute), false).
							SingleOrDefault();

					if (attr == null)
						throw new InvalidOperationException(string.Format("The provider type filter '{0}' is missing the required FileSystemProviderAttribute", typeof(FileSystemProviderAttribute).FullName));

			        return attr.Alias;
		        });
			
            var innerFs = GetUnderlyingFileSystemProvider(alias);
	        var outputFs = Activator.CreateInstance(typeof (TProviderTypeFilter), innerFs);
	        return (TProviderTypeFilter)outputFs;
        }
    }
}
