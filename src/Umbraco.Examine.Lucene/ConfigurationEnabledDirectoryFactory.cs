// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.IO;
using System.Threading;
using Examine;
using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Lucene.Net.Index;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Infrastructure.Examine
{
    /// <summary>
    /// An Examine directory factory implementation based on configured values
    /// </summary>
    public class ConfigurationEnabledDirectoryFactory : IDirectoryFactory
    {
        private readonly IServiceProvider _services;
        private readonly ITypeFinder _typeFinder;
        private readonly ILockFactory _lockFactory;
        private readonly IApplicationRoot _applicationRoot;
        private readonly IndexCreatorSettings _settings;
        private bool _disposedValue;
        private IDirectoryFactory _directoryFactory;
        private Lucene.Net.Store.Directory _directory;

        public ConfigurationEnabledDirectoryFactory(
            IServiceProvider services,
            ITypeFinder typeFinder,
            ILockFactory lockFactory,
            IOptions<IndexCreatorSettings> settings,
            IApplicationRoot applicationRoot)
        {
            _services = services;
            _typeFinder = typeFinder;
            _lockFactory = lockFactory;
            _applicationRoot = applicationRoot;
            _settings = settings.Value;
        }

        public Lucene.Net.Store.Directory CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock)
            => LazyInitializer.EnsureInitialized(ref _directory, () =>
            {
                _directoryFactory = CreateFileSystemLuceneDirectory();

                return _directoryFactory.CreateDirectory(luceneIndex, forceUnlock);
            });

        /// <summary>
        /// Creates a directory factory based on the configured value and ensures that 
        /// </summary>        
        private IDirectoryFactory CreateFileSystemLuceneDirectory()
        {
            DirectoryInfo dirInfo = _applicationRoot.ApplicationRoot;

            if (!dirInfo.Exists)
            {
                Directory.CreateDirectory(dirInfo.FullName);
            }

            Type factoryType = _settings.GetRequiredLuceneDirectoryFactoryType(_typeFinder);
            if (factoryType == null)
            {
                return new FileSystemDirectoryFactory(dirInfo, _lockFactory);
            }
            else
            {
                // All directory factories should be in DI
                return (IDirectoryFactory)_services.GetRequiredService(factoryType);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _directoryFactory?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
        }
    }
}
