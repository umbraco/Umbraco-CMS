using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.IO
{
    internal class FileSystemProviderManager
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

        public FileSystemProviderManager()
        {
            _config = (FileSystemProvidersSection)ConfigurationManager.GetSection("FileSystemProviders");
        }

        #endregion

        public IFileSystem GetFileSystemProvider(string alias)
        {
            var providerConfig = _config.Providers[alias];
            if(providerConfig == null)
                throw new ArgumentException(string.Format("No provider found with the alias '{0}'", alias));

            var providerType = Type.GetType(providerConfig.Type);
            if(providerType == null)
                throw new InvalidOperationException(string.Format("Could not find type '{0}'", providerConfig.Type));

            if (providerType.IsAssignableFrom(typeof(IFileSystem)))
                throw new InvalidOperationException(string.Format("The type '{0}' does not implement IFileSystem", providerConfig.Type));

            var paramCount = providerConfig.Parameters != null ? providerConfig.Parameters.Count : 0;
            var constructor = providerType.GetConstructors()
                .SingleOrDefault(x => x.GetParameters().Count() == paramCount 
                    && x.GetParameters().All(y => providerConfig.Parameters.AllKeys.Contains(y.Name)));
            if(constructor == null)
                throw new InvalidOperationException(string.Format("Could not find constructor for type '{0}' which accepts {1} parameters", providerConfig.Type, paramCount));

            var parameters = new object[paramCount];
            for(var i = 0; i < paramCount; i++)
                parameters[i] = providerConfig.Parameters[providerConfig.Parameters.AllKeys[i]].Value;

            return (IFileSystem) constructor.Invoke(parameters);
        }

        public TProviderTypeFilter GetFileSystemProvider<TProviderTypeFilter>()
            where TProviderTypeFilter : class, IFileSystem
        {
            var attr =
                (FileSystemProviderAttribute)typeof(TProviderTypeFilter).GetCustomAttributes(typeof(FileSystemProviderAttribute), false).
                    SingleOrDefault();

            if (attr == null)
                throw new InvalidOperationException(string.Format("The provider type filter '{0}' is missing the required FileSystemProviderAttribute", typeof(FileSystemProviderAttribute).FullName));

            return GetFileSystemProvider(attr.Alias).As<TProviderTypeFilter>();
        }
    }
}
