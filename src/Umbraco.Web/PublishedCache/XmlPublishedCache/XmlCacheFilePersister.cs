using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web.Scheduling;

namespace Umbraco.Web.PublishedCache.XmlPublishedCache
{
    /// <summary>
    /// This is the background task runner that persists the xml file to the file system
    /// </summary>
    /// <remarks>
    /// This is used so that all file saving is done on a web aware worker background thread and all logic is performed async so this
    /// process will not interfere with any web requests threads. This is also done as to not require any global locks and to ensure that
    /// if multiple threads are performing publishing tasks that the file will be persisted in accordance with the final resulting
    /// xml structure since the file writes are queued.
    /// </remarks>
    internal class XmlCacheFilePersister : DisposableObject, IBackgroundTask
    {
        private readonly XmlDocument _xDoc;
        private readonly string _xmlFileName;
        private readonly ProfilingLogger _logger;

        public XmlCacheFilePersister(XmlDocument xDoc, string xmlFileName, ProfilingLogger logger)
        {
            _xDoc = xDoc;
            _xmlFileName = xmlFileName;
            _logger = logger;
        }

        public async Task RunAsync()
        {
            await PersistXmlToFileAsync(_xDoc);
        }

        public bool IsAsync
        {
            get { return true; }
        }

        /// <summary>
        /// Persist a XmlDocument to the Disk Cache
        /// </summary>
        /// <param name="xmlDoc"></param>
        internal async Task PersistXmlToFileAsync(XmlDocument xmlDoc)
        {
            if (xmlDoc != null)
            {
                using (_logger.DebugDuration<XmlCacheFilePersister>(
                    string.Format("Saving content to disk on thread '{0}' (Threadpool? {1})", Thread.CurrentThread.Name, Thread.CurrentThread.IsThreadPoolThread),
                    string.Format("Saved content to disk on thread '{0}' (Threadpool? {1})", Thread.CurrentThread.Name, Thread.CurrentThread.IsThreadPoolThread)))
                {
                    try
                    {
                        // Try to create directory for cache path if it doesn't yet exist
                        var directoryName = Path.GetDirectoryName(_xmlFileName);
                        // create dir if it is not there, if it's there, this will proceed as normal
                        Directory.CreateDirectory(directoryName);

                        await xmlDoc.SaveAsync(_xmlFileName);
                    }
                    catch (Exception ee)
                    {
                        // If for whatever reason something goes wrong here, invalidate disk cache
                        DeleteXmlCache();

                        LogHelper.Error<XmlCacheFilePersister>("Error saving content to disk", ee);
                    }
                }

                
            }
        }

        private void DeleteXmlCache()
        {
            if (File.Exists(_xmlFileName) == false) return;

            // Reset file attributes, to make sure we can delete file
            try
            {
                File.SetAttributes(_xmlFileName, FileAttributes.Normal);
            }
            finally
            {
                File.Delete(_xmlFileName);
            }
        }

        protected override void DisposeResources()
        {
        }

        public void Run()
        {
            throw new NotImplementedException();
        }

    }
}