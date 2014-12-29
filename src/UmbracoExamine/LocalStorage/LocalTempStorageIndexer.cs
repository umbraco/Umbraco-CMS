using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
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
    internal class LocalTempStorageIndexer
    {
        private string _tempPath;
        public Lucene.Net.Store.Directory LuceneDirectory { get; private set; }
        private readonly object _locker = new object();
        public SnapshotDeletionPolicy Snapshotter { get; private set; }

        public LocalTempStorageIndexer()
        {
            IndexDeletionPolicy policy = new KeepOnlyLastCommitDeletionPolicy();
            Snapshotter = new SnapshotDeletionPolicy(policy);
        }

        public void Initialize(NameValueCollection config, string configuredPath, Lucene.Net.Store.Directory baseLuceneDirectory, Analyzer analyzer, LocalStorageType localStorageType)
        {
            var codegenPath = HttpRuntime.CodegenDir;

            _tempPath = Path.Combine(codegenPath, configuredPath.TrimStart('~', '/').Replace("/", "\\"));

            switch (localStorageType)
            {
                case LocalStorageType.Sync:
                    var success = InitializeLocalIndexAndDirectory(baseLuceneDirectory, analyzer, configuredPath);

                    //create the custom lucene directory which will keep the main and temp FS's in sync
                    LuceneDirectory = LocalTempStorageDirectoryTracker.Current.GetDirectory(
                        new DirectoryInfo(_tempPath),
                        baseLuceneDirectory,
                        //flag to disable the mirrored folder if not successful
                        success == false);
                    break;
                case LocalStorageType.LocalOnly:
                    if (Directory.Exists(_tempPath) == false)
                    {
                        Directory.CreateDirectory(_tempPath);
                    }

                    LuceneDirectory = DirectoryTracker.Current.GetDirectory(new DirectoryInfo(_tempPath));
                    break;
                default:
                    throw new ArgumentOutOfRangeException("localStorageType");
            }

        }

        private bool InitializeLocalIndexAndDirectory(Lucene.Net.Store.Directory baseLuceneDirectory, Analyzer analyzer, string configuredPath)
        {
            lock (_locker)
            {
                if (Directory.Exists(_tempPath) == false)
                {
                    Directory.CreateDirectory(_tempPath);
                }

                //copy index if it exists, don't do anything if it's not there
                if (IndexReader.IndexExists(baseLuceneDirectory) == false) return true;

                using (new IndexWriter(
                    //read from the underlying/default directory, not the temp codegen dir
                    baseLuceneDirectory,
                    analyzer,
                    Snapshotter,
                    IndexWriter.MaxFieldLength.UNLIMITED))
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

                        var tempDir = new DirectoryInfo(_tempPath);

                        //Get all files in the temp storage that don't exist in the snapshot collection, we want to remove these
                        var toRemove = tempDir.GetFiles()
                            .Select(x => x.Name)
                            .Except(allSnapshotFiles);

                        //using (var tempDirectory = new SimpleFSDirectory(tempDir))
                        //{
                        //TODO: We're ignoring if it is locked right now, it shouldn't be unless for some strange reason the 
                        // last process hasn't fully shut down, in that case we're not going to worry about it.

                        //if (IndexWriter.IsLocked(tempDirectory) == false)
                        //{
                        foreach (var file in toRemove)
                        {
                            try
                            {
                                File.Delete(Path.Combine(_tempPath, file));
                            }
                            catch (IOException ex)
                            {
                                LogHelper.WarnWithException<LocalTempStorageIndexer>("Could not delete non synced index file file, index sync will continue but old index files will remain - this shouldn't affect indexing/searching operations", ex);

                                //TODO: we're ignoring this, as old files shouldn't affect the index/search operations, lucene files are 'write once'
                                //quit here
                                //return false;
                            }
                        }
                        //}
                        //else
                        //{
                        //    LogHelper.Warn<LocalTempStorageIndexer>("Cannot sync index files from main storage, the index is currently locked");
                        //    //quit here
                        //    return false;
                        //}

                        foreach (var fileName in allSnapshotFiles.Where(f => f.IsNullOrWhiteSpace() == false))
                        {
                            var destination = Path.Combine(_tempPath, Path.GetFileName(fileName));

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
                                return false;
                            }
                        }

                        //}



                    }
                    finally
                    {
                        Snapshotter.Release();
                    }


                }

                return true;
            }
        }
    }
}