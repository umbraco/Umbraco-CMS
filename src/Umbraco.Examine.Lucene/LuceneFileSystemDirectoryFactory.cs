// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.IO;
using Examine.LuceneEngine.Directories;
using Lucene.Net.Store;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Infrastructure.Examine
{
    public class LuceneFileSystemDirectoryFactory : ILuceneDirectoryFactory
    {
        private readonly ITypeFinder _typeFinder;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IndexCreatorSettings _settings;

        public LuceneFileSystemDirectoryFactory(ITypeFinder typeFinder, IHostingEnvironment hostingEnvironment, IOptions<IndexCreatorSettings> settings)
        {
            _typeFinder = typeFinder;
            _hostingEnvironment = hostingEnvironment;
            _settings = settings.Value;
        }

        public Lucene.Net.Store.Directory CreateDirectory(string indexName) => CreateFileSystemLuceneDirectory(indexName);

        /// <summary>
        /// Creates a file system based Lucene <see cref="Lucene.Net.Store.Directory"/> with the correct locking guidelines for Umbraco
        /// </summary>
        /// <param name="folderName">
        /// The folder name to store the index (single word, not a fully qualified folder) (i.e. Internal)
        /// </param>
        /// <returns></returns>
        public virtual Lucene.Net.Store.Directory CreateFileSystemLuceneDirectory(string folderName)
        {

            var dirInfo = new DirectoryInfo(Path.Combine(_hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempData), "ExamineIndexes", folderName));
            if (!dirInfo.Exists)
                System.IO.Directory.CreateDirectory(dirInfo.FullName);

            //check if there's a configured directory factory, if so create it and use that to create the lucene dir
            var configuredDirectoryFactory = _settings.LuceneDirectoryFactory;

            if (!configuredDirectoryFactory.IsNullOrWhiteSpace())
            {
                //this should be a fully qualified type
                var factoryType = _typeFinder.GetTypeByName(configuredDirectoryFactory);
                if (factoryType == null) throw new NullReferenceException("No directory type found for value: " + configuredDirectoryFactory);
                var directoryFactory = (IDirectoryFactory)Activator.CreateInstance(factoryType);
                return directoryFactory.CreateDirectory(dirInfo);
            }

            //no dir factory, just create a normal fs directory

            var luceneDir = new SimpleFSDirectory(dirInfo);

            //we want to tell examine to use a different fs lock instead of the default NativeFSFileLock which could cause problems if the appdomain
            //terminates and in some rare cases would only allow unlocking of the file if IIS is forcefully terminated. Instead we'll rely on the simplefslock
            //which simply checks the existence of the lock file
            // The full syntax of this is: new NoPrefixSimpleFsLockFactory(dirInfo)
            // however, we are setting the DefaultLockFactory in startup so we'll use that instead since it can be managed globally.
            luceneDir.SetLockFactory(DirectoryFactory.DefaultLockFactory(dirInfo));
            return luceneDir;


        }
    }
}
