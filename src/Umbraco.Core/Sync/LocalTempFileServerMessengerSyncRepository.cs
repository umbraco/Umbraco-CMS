using System;
using System.Globalization;
using System.IO;
using System.Web;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// A version of <see cref="IServerMessengerSyncRepository"/> that uses a file in the local temp folder to persist the information.
    /// </summary>
    public class LocalTempFileServerMessengerSyncRepository : IServerMessengerSyncRepository
    {
        private readonly Lazy<string> _distCacheFilePath;

        public LocalTempFileServerMessengerSyncRepository(IGlobalSettings globalSettings)
        {
            _distCacheFilePath = new Lazy<string>(() => GetDistCacheFilePath(globalSettings));
        }

        private string DistCacheFilePath => _distCacheFilePath.Value;

        /// <inheritdoc />
        public int Value { get; private set; }

        /// <inheritdoc />
        public void Reset()
        {
            Value = -1;
        }

        /// <inheritdoc />
        /// <remarks>
        ///     Thread safety: this is NOT thread safe. Because it is NOT meant to run multi-threaded.
        /// </remarks>
        public void Read()
        {
            if (File.Exists(DistCacheFilePath) == false) return;

            var content = File.ReadAllText(DistCacheFilePath);
            if (int.TryParse(content, out var last)) Value = last;
        }

        /// <inheritdoc />
        /// <remarks>
        ///     Thread safety: this is NOT thread safe. Because it is NOT meant to run multi-threaded.
        /// </remarks>
        public void Save(int value)
        {
            File.WriteAllText(DistCacheFilePath, value.ToString(CultureInfo.InvariantCulture));
            Value = value;
        }

        private string GetDistCacheFilePath(IGlobalSettings globalSettings)
        {
            var fileName = HttpRuntime.AppDomainAppId.ReplaceNonAlphanumericChars(string.Empty) + "-lastsynced.txt";

            var distCacheFilePath = Path.Combine(globalSettings.LocalTempPath, "DistCache", fileName);

            //ensure the folder exists
            var folder = Path.GetDirectoryName(distCacheFilePath);
            if (folder == null)
                throw new InvalidOperationException("The folder could not be determined for the file " +
                                                    distCacheFilePath);
            if (Directory.Exists(folder) == false)
                Directory.CreateDirectory(folder);

            return distCacheFilePath;
        }
    }
}