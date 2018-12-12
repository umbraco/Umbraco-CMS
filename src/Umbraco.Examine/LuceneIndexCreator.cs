using System.Collections.Generic;
using System.IO;
using Examine;
using Lucene.Net.Store;
using Umbraco.Core.IO;

namespace Umbraco.Examine
{
    /// <inheritdoc />
    /// <summary>
    /// Abstract class for creating Lucene based Indexes
    /// </summary>
    public abstract class LuceneIndexCreator : IIndexCreator
    {
        public abstract IEnumerable<IIndex> Create();

        /// <summary>
        /// Creates a file system based Lucene <see cref="Lucene.Net.Store.Directory"/> with the correct locking guidelines for Umbraco
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual Lucene.Net.Store.Directory CreateFileSystemLuceneDirectory(string name)
        {
            //TODO: We should have a single AppSetting to be able to specify a default DirectoryFactory so we can have a single
            //setting to configure all indexes that use this to easily swap the directory to Sync/%temp%/blog, etc...

            var dirInfo = new DirectoryInfo(Path.Combine(IOHelper.MapPath(SystemDirectories.Data), "TEMP", "ExamineIndexes", name));
            if (!dirInfo.Exists)
                System.IO.Directory.CreateDirectory(dirInfo.FullName);

            var luceneDir = new SimpleFSDirectory(dirInfo);
            //we want to tell examine to use a different fs lock instead of the default NativeFSFileLock which could cause problems if the appdomain
            //terminates and in some rare cases would only allow unlocking of the file if IIS is forcefully terminated. Instead we'll rely on the simplefslock
            //which simply checks the existence of the lock file
            luceneDir.SetLockFactory(new NoPrefixSimpleFsLockFactory(dirInfo));
            return luceneDir;
        }
    }
}
