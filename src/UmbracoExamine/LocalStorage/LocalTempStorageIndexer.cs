using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using Examine.LuceneEngine;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Directory = System.IO.Directory;

namespace UmbracoExamine.LocalStorage
{
    internal enum InitializeDirectoryFlags
    {
        Success = 0,
        SuccessNoIndexExists = 1,

        FailedCorrupt = 100,
        FailedLocked = 101,
        FailedFileSync = 102
    }

    internal class LocalTempStorageIndexer
    {
        public Lucene.Net.Store.Directory LuceneDirectory { get; private set; }
        private readonly object _locker = new object();
        public SnapshotDeletionPolicy Snapshotter { get; private set; }

        public string TempPath { get; private set; }


        public LocalTempStorageIndexer()
        {
            IndexDeletionPolicy policy = new KeepOnlyLastCommitDeletionPolicy();
            Snapshotter = new SnapshotDeletionPolicy(policy);
        }

        public void Initialize(NameValueCollection config, string configuredPath, FSDirectory baseLuceneDirectory, Analyzer analyzer, LocalStorageType localStorageType)
        {
            var codegenPath = HttpRuntime.CodegenDir;

            TempPath = Path.Combine(codegenPath, configuredPath.TrimStart('~', '/').Replace("/", "\\"));

            switch (localStorageType)
            {
                case LocalStorageType.Sync:
                    var success = InitializeLocalIndexAndDirectory(baseLuceneDirectory, analyzer, configuredPath);

                    //create the custom lucene directory which will keep the main and temp FS's in sync
                    LuceneDirectory = LocalTempStorageDirectoryTracker.Current.GetDirectory(
                        new DirectoryInfo(TempPath),
                        baseLuceneDirectory,
                        //flag to disable the mirrored folder if not successful
                        (int)success >= 100);

                    //If the master index simply doesn't exist, we don't continue to try to open anything since there will
                    // actually be nothing there.
                    if (success == InitializeDirectoryFlags.SuccessNoIndexExists)
                    {
                        return;
                    }

                    //Try to open the reader, this will fail if the index is corrupt and we'll need to handle that
                    var result = DelegateExtensions.RetryUntilSuccessOrMaxAttempts(i =>
                    {
                        try
                        {
                            using (IndexReader.Open(
                                LuceneDirectory,
                                DeletePolicyTracker.Current.GetPolicy(LuceneDirectory),
                                true))
                            {
                            }

                            return Attempt.Succeed(true);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.WarnWithException<LocalTempStorageIndexer>(
                                string.Format("Could not open an index reader, local temp storage index is empty or corrupt... retrying... {0}", configuredPath),
                                ex);
                        }
                        return Attempt.Fail(false);
                    }, 5, TimeSpan.FromSeconds(1));

                    if (result.Success == false)
                    {
                        LogHelper.Warn<LocalTempStorageIndexer>(
                                string.Format("Could not open an index reader, local temp storage index is empty or corrupt... attempting to clear index files in local temp storage, will operate from main storage only {0}", configuredPath));

                        ClearFilesInPath(TempPath);

                        //create the custom lucene directory which will keep the main and temp FS's in sync
                        LuceneDirectory = LocalTempStorageDirectoryTracker.Current.GetDirectory(
                            new DirectoryInfo(TempPath),
                            baseLuceneDirectory,
                            //Disable mirrored index, we're kind of screwed here only use master index
                            true);
                    }                   

                    break;
                case LocalStorageType.LocalOnly:
                    if (Directory.Exists(TempPath) == false)
                    {
                        Directory.CreateDirectory(TempPath);
                    }
                    LuceneDirectory = DirectoryTracker.Current.GetDirectory(new DirectoryInfo(TempPath));
                    break;
                default:
                    throw new ArgumentOutOfRangeException("localStorageType");
            }
        }

        private void ClearFilesInPath(string path)
        {
            if (Directory.Exists(path))
            {
                foreach (var file in Directory.GetFiles(path))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception exInner)
                    {
                        LogHelper.Error<LocalTempStorageIndexer>("Could not delete local temp storage index file", exInner);
                    }
                }
            }
        }

        private bool ClearLuceneDirFiles(Lucene.Net.Store.Directory baseLuceneDirectory)
        {
            try
            {
                //unlock it!
                IndexWriter.Unlock(baseLuceneDirectory);

                var fileLuceneDirectory = baseLuceneDirectory as FSDirectory;
                if (fileLuceneDirectory != null)
                {
                    foreach (var file in fileLuceneDirectory.ListAll())
                    {
                        try
                        {
                            fileLuceneDirectory.DeleteFile(file);
                        }
                        catch (IOException)
                        {
                            if (file.InvariantEquals("write.lock"))
                            {
                                LogHelper.Warn<LocalTempStorageIndexer>("The lock file could not be deleted but should be removed when the writer is disposed");
                            }

                        }
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                LogHelper.Error<LocalTempStorageIndexer>("Could not clear corrupt index from main index folder, the index cannot be used", ex);
                return false;
            }
        }

        private Attempt<IndexWriter> TryCreateWriter(Lucene.Net.Store.Directory baseLuceneDirectory, Analyzer analyzer)
        {
            try
            {
                var w = new IndexWriter(
                    //read from the underlying/default directory, not the temp codegen dir
                    baseLuceneDirectory,
                    analyzer,
                    Snapshotter,
                    IndexWriter.MaxFieldLength.UNLIMITED);

                //Done!
                return Attempt.Succeed(w);
            }
            catch (Exception ex)
            {
                LogHelper.WarnWithException<LocalTempStorageIndexer>("Could not create index writer with snapshot policy for copying... retrying...", ex);
                return Attempt<IndexWriter>.Fail(ex);
            }
        }

        /// <summary>
        /// Attempts to create an index writer, it will retry on failure 5 times and on the last time will try to forcefully unlock the index files
        /// </summary>
        /// <param name="baseLuceneDirectory"></param>
        /// <param name="analyzer"></param>
        /// <returns></returns>
        private Attempt<IndexWriter> TryCreateWriterWithRetry(Lucene.Net.Store.Directory baseLuceneDirectory, Analyzer analyzer)
        {
            var maxTries = 5;

            var result = DelegateExtensions.RetryUntilSuccessOrMaxAttempts((currentTry) =>
            {
                //last try...
                if (currentTry == maxTries)
                {
                    LogHelper.Info<LocalTempStorageIndexer>("Could not acquire index lock, attempting to force unlock it...");
                    //unlock it!
                    IndexWriter.Unlock(baseLuceneDirectory);
                }

                var writerAttempt = TryCreateWriter(baseLuceneDirectory, analyzer);
                if (writerAttempt) return writerAttempt;
                LogHelper.Info<LocalTempStorageIndexer>("Could not create writer on {0}, retrying ....", baseLuceneDirectory.ToString);
                return Attempt<IndexWriter>.Fail();
            }, 5, TimeSpan.FromSeconds(1));

            return result;
        }

        private bool TryWaitForDirectoryUnlock(Lucene.Net.Store.Directory dir)
        {
            var maxTries = 5;

            var result = DelegateExtensions.RetryUntilSuccessOrMaxAttempts((currentTry) =>
            {
                //last try...
                if (currentTry == maxTries)
                {
                    LogHelper.Info<LocalTempStorageIndexer>("Could not acquire directory lock, attempting to force unlock it...");
                    //unlock it!
                    IndexWriter.Unlock(dir);
                }

                if (IndexWriter.IsLocked(dir) == false) return Attempt.Succeed(true);                
                LogHelper.Info<LocalTempStorageIndexer>("Could not acquire directory lock for {0} writer, retrying ....", dir.ToString);
                return Attempt<bool>.Fail();
            }, 5, TimeSpan.FromSeconds(1));

            return result;
        }

        private InitializeDirectoryFlags InitializeLocalIndexAndDirectory(Lucene.Net.Store.Directory baseLuceneDirectory, Analyzer analyzer, string configuredPath)
        {
            lock (_locker)
            {
                if (Directory.Exists(TempPath) == false)
                {
                    Directory.CreateDirectory(TempPath);
                }

                //copy index if it exists, don't do anything if it's not there
                if (IndexReader.IndexExists(baseLuceneDirectory) == false) return InitializeDirectoryFlags.SuccessNoIndexExists;

                var writerAttempt = TryCreateWriterWithRetry(baseLuceneDirectory, analyzer);

                if (writerAttempt.Success == false)
                {
                    LogHelper.Error<LocalTempStorageIndexer>("Could not create index writer with snapshot policy for copying, the index cannot be used", writerAttempt.Exception);
                    return InitializeDirectoryFlags.FailedLocked;
                }

                //Try to open the reader from the source, this will fail if the index is corrupt and we'll need to handle that
                try
                {
                    //NOTE: To date I've not seen this error occur
                    using (writerAttempt.Result.GetReader())
                    {                        
                    }
                }
                catch (Exception ex)
                {
                    writerAttempt.Result.Dispose();

                    LogHelper.Error<LocalTempStorageIndexer>(
                        string.Format("Could not open an index reader, {0} is empty or corrupt... attempting to clear index files in master folder", configuredPath), 
                        ex);

                    if (ClearLuceneDirFiles(baseLuceneDirectory) == false)
                    {
                        //hrm, not much we can do in this situation, but this shouldn't happen
                        LogHelper.Error<LocalTempStorageIndexer>("Could not open an index reader, index is corrupt.", ex);                        
                        return InitializeDirectoryFlags.FailedCorrupt;                        
                    }

                    //the main index is now blank, we'll proceed as normal with a new empty index...
                    writerAttempt = TryCreateWriter(baseLuceneDirectory, analyzer);
                    if (writerAttempt.Success == false)
                    {
                        //ultra fail...
                        LogHelper.Error<LocalTempStorageIndexer>("Could not create index writer with snapshot policy for copying, the index cannot be used", writerAttempt.Exception);
                        return InitializeDirectoryFlags.FailedLocked;
                    }
                }

                using (writerAttempt.Result)
                {
                    try
                    {
                        var basePath = IOHelper.MapPath(configuredPath);

                        var commit = Snapshotter.Snapshot();
                        var allSnapshotFiles = commit.GetFileNames()
                            .Concat(new[]
                            {
                                commit.GetSegmentsFileName(), 
                                //we need to manually include the segments.gen file
                                "segments.gen"
                            })
                            .Distinct()
                            .ToArray();

                        var tempDir = new DirectoryInfo(TempPath);

                        //Get all files in the temp storage that don't exist in the snapshot collection, we want to remove these
                        var toRemove = tempDir.GetFiles()
                            .Select(x => x.Name)
                            .Except(allSnapshotFiles);

                        using (var tempDirectory = new SimpleFSDirectory(tempDir))
                        {
                            if (TryWaitForDirectoryUnlock(tempDirectory))
                            {
                                foreach (var file in toRemove)
                                {
                                    try
                                    {
                                        File.Delete(Path.Combine(TempPath, file));
                                    }
                                    catch (IOException ex)
                                    {
                                        if (file.InvariantEquals("write.lock"))
                                        {
                                            //This might happen if the writer is open
                                            LogHelper.Warn<LocalTempStorageIndexer>("The lock file could not be deleted but should be removed when the writer is disposed");
                                        }

                                        LogHelper.Debug<LocalTempStorageIndexer>("Could not delete non synced index file file, index sync will continue but old index files will remain - this shouldn't affect indexing/searching operations. {0}", () => ex.ToString());
                                        
                                    }
                                }
                            }
                            else
                            {
                                //quit here, this shouldn't happen with all the checks above.
                                LogHelper.Warn<LocalTempStorageIndexer>("Cannot sync index files from main storage, the temp file index is currently locked");
                                return InitializeDirectoryFlags.FailedLocked;
                            }


                            foreach (var fileName in allSnapshotFiles.Where(f => f.IsNullOrWhiteSpace() == false))
                            {
                                var destination = Path.Combine(TempPath, Path.GetFileName(fileName));

                                //don't copy if it's already there, lucene is 'write once' so this file is meant to be there already
                                if (File.Exists(destination)) continue;

                                try
                                {
                                    File.Copy(
                                        Path.Combine(basePath, "Index", fileName),
                                        destination);
                                }
                                catch (IOException ex)
                                {
                                    LogHelper.Error<LocalTempStorageIndexer>("Could not copy index file, could not sync from main storage", ex);

                                    //quit here
                                    return InitializeDirectoryFlags.FailedFileSync;
                                }
                            }
                        }
                    }
                    finally
                    {
                        Snapshotter.Release();
                    }
                }

                LogHelper.Info<LocalTempStorageIndexer>("Successfully sync'd main index to local temp storage for index: {0}", () => configuredPath);
                return InitializeDirectoryFlags.Success;
            }
        }
    }
}