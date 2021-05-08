// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.IO;
using Examine.Lucene.Directories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Infrastructure.Examine
{
    public class ConfigurationEnabledDirectoryFactory : IDirectoryFactory
    {
        private readonly IServiceProvider _services;
        private readonly ITypeFinder _typeFinder;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILockFactory _lockFactory;
        private readonly IndexCreatorSettings _settings;

        public ConfigurationEnabledDirectoryFactory(
            IServiceProvider services,
            ITypeFinder typeFinder,
            IHostingEnvironment hostingEnvironment,
            ILockFactory lockFactory,
            IOptions<IndexCreatorSettings> settings)
        {
            _services = services;
            _typeFinder = typeFinder;
            _hostingEnvironment = hostingEnvironment;
            _lockFactory = lockFactory;
            _settings = settings.Value;
        }

        public Lucene.Net.Store.Directory CreateDirectory(string indexName) => CreateFileSystemLuceneDirectory(indexName);

        /// <summary>
        /// Creates a file system based Lucene <see cref="Lucene.Net.Store.Directory"/> with the correct locking guidelines for Umbraco
        /// </summary>
        /// <param name="indexName">
        /// The folder name to store the index (single word, not a fully qualified folder) (i.e. Internal)
        /// </param>
        /// <returns></returns>
        public virtual Lucene.Net.Store.Directory CreateFileSystemLuceneDirectory(string indexName)
        {
            var dirInfo = new DirectoryInfo(
                Path.Combine(
                    _hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempData),
                    "ExamineIndexes",
                    indexName));

            if (!dirInfo.Exists)
            {
                Directory.CreateDirectory(dirInfo.FullName);
            }

            //check if there's a configured directory factory, if so create it and use that to create the lucene dir
            var configuredDirectoryFactory = _settings.LuceneDirectoryFactory;

            if (!configuredDirectoryFactory.IsNullOrWhiteSpace())
            {
                //this should be a fully qualified type
                Type factoryType = _typeFinder.GetTypeByName(configuredDirectoryFactory);
                if (factoryType == null)
                {
                    throw new NullReferenceException("No directory type found for value: " + configuredDirectoryFactory);
                }

                var directoryFactory = (IDirectoryFactory)ActivatorUtilities.CreateInstance(_services, factoryType);

                return directoryFactory.CreateDirectory(indexName);
            }

            var fileSystemDirectoryFactory = new FileSystemDirectoryFactory(dirInfo, _lockFactory);
            return fileSystemDirectoryFactory.CreateDirectory(indexName);

        }
    }
}
