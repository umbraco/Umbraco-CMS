using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;
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

        public void Initialize(NameValueCollection config, string configuredPath, Lucene.Net.Store.Directory baseLuceneDirectory, Analyzer analyzer)
        {
            var codegenPath = HttpRuntime.CodegenDir;

            _tempPath = Path.Combine(codegenPath, configuredPath.TrimStart('~', '/').Replace("/", "\\"));

            var success = InitializeLocalIndexAndDirectory(baseLuceneDirectory, analyzer, configuredPath);

            //create the custom lucene directory which will keep the main and temp FS's in sync
            LuceneDirectory = LocalTempStorageDirectoryTracker.Current.GetDirectory(
                new DirectoryInfo(_tempPath),
                baseLuceneDirectory,
                //flag to disable the mirrored folder if not successful
                success == false);
        }

        private bool InitializeLocalIndexAndDirectory(Lucene.Net.Store.Directory baseLuceneDirectory, Analyzer analyzer, string configuredPath)
        {
            lock (_locker)
            {
                if (Directory.Exists(_tempPath) == false)
                {
                    Directory.CreateDirectory(_tempPath);
                }

                //copy index

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
                        var allSnapshotFiles = commit.GetFileNames().Concat(new[] { commit.GetSegmentsFileName() })
                            .Distinct()
                            .ToArray();

                        var tempDir = new DirectoryInfo(_tempPath);

                        //Get all files in the temp storage that don't exist in the snapshot collection, we want to remove these
                        var toRemove = tempDir.GetFiles()
                            .Select(x => x.Name)
                            .Except(allSnapshotFiles);

                        using (var tempDirectory = new SimpleFSDirectory(tempDir))
                        {
                            if (IndexWriter.IsLocked(tempDirectory) == false)
                            {
                                foreach (var file in toRemove)
                                {
                                    try
                                    {
                                        File.Delete(Path.Combine(_tempPath, file));
                                    }
                                    catch (IOException ex)
                                    {
                                        LogHelper.Error<LocalTempStorageIndexer>("Could not delete index file, could not sync from main storage", ex);
                                        //quit here
                                        return false;
                                    }
                                }
                            }
                            else
                            {
                                LogHelper.Warn<LocalTempStorageIndexer>("Cannot sync index files from main storage, the index is currently locked");
                                //quit here
                                return false;
                            }
                        }

                        foreach (var fileName in allSnapshotFiles.Where(f => f.IsNullOrWhiteSpace() == false))
                        {
                            try
                            {
                                File.Copy(
                                    Path.Combine(basePath, "Index", fileName),
                                    Path.Combine(_tempPath, Path.GetFileName(fileName)), true);
                            }
                            catch (IOException ex)
                            {
                                LogHelper.Error<LocalTempStorageIndexer>("Could not copy index file, could not sync from main storage", ex);

                                //quit here
                                return false;
                            }
                        }

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