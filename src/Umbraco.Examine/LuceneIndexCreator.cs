using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Examine;
using Examine.LuceneEngine.Directories;
using Lucene.Net.Store;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;

namespace Umbraco.Examine
{
    /// <inheritdoc />
    /// <summary>
    /// Abstract class for creating Lucene based Indexes
    /// </summary>
    public abstract class LuceneIndexCreator : IIndexCreator
    {
        private readonly IDirectoryFactory _directoryFactory;

        [Obsolete("Use  LuceneIndexCreator(IDirectoryFactory factory) instead")]
        public LuceneIndexCreator() : this((IDirectoryFactory)Current.Factory.GetInstance(TypeFinder.GetTypeByName(ConfigurationManager.AppSettings["Umbraco.Examine.LuceneDirectoryFactory"].ToString())))
        {
        }
        public LuceneIndexCreator(IDirectoryFactory directoryFactory)
        {
            _directoryFactory = directoryFactory;
        }
        public abstract IEnumerable<IIndex> Create();

        /// <summary>
        /// Creates a file system based Lucene <see cref="Lucene.Net.Store.Directory"/> with the correct locking guidelines for Umbraco
        /// </summary>
        /// <param name="folderName">
        /// The folder name to store the index (single word, not a fully qualified folder) (i.e. Internal)
        /// </param>
        /// <returns></returns>
        public virtual Lucene.Net.Store.Directory CreateFileSystemLuceneDirectory(string folderName)
        {
            var dirInfo = new DirectoryInfo(Path.Combine(IOHelper.MapPath(SystemDirectories.TempData), "ExamineIndexes", folderName));
            if (!dirInfo.Exists)
                System.IO.Directory.CreateDirectory(dirInfo.FullName);

            //check if there's a configured directory factory, if so create it and use that to create the lucene dir
            if (_directoryFactory != null)
            {
                return _directoryFactory.CreateDirectory(dirInfo);
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
