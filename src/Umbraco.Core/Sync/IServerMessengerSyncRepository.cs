using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using File = System.IO.File;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// Persistence of the information about the last read value from the cache instructions
    /// </summary>
    public interface IServerMessengerSyncRepository
    {
        /// <summary>
        ///     Gets the incremental value.
        /// </summary>
        int Value { get; }

        /// <summary>
        ///     Updates the in-memory last-synced id and persists it to file.
        /// </summary>
        /// <param name="value">The incremental value.</param>
        void Save(int value);

        /// <summary>
        ///     Reads the last-synced id from file into memory.
        /// </summary>
        void Read();

        /// <summary>
        ///     Resets the value to its initial value.
        /// </summary>
        void Reset();
    }

    /// <summary>
    /// A version of <see cref="IServerMessengerSyncRepository"/> that uses the umbracoServer table to persist the information in the database.
    /// </summary>
    public class DatabaseServerMessengerSyncRepository : IServerMessengerSyncRepository
    {
        private readonly IServerRegistrationRepository _serverRegistrationRepository;
        private readonly IScopeProvider _scopeProvider;

        private static readonly string ServerIdentity = NetworkHelper.MachineName // eg DOMAIN\SERVER
                                             + "/" + HttpRuntime.AppDomainAppId; // eg /LM/S3SVC/11/ROOT

        public DatabaseServerMessengerSyncRepository(IServerRegistrationRepository serverRegistrationRepository, IScopeProvider scopeProvider)
        {
            _serverRegistrationRepository = serverRegistrationRepository;
            _scopeProvider = scopeProvider;
        }

        /// <inheritdoc />
        public int Value { get; private set; }

        /// <inheritdoc />
        public void Reset()
        {
            Value = -1;
        }

        /// <inheritdoc />
        public void Save(int value)
        {
            using (var scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                var server = GetServer(scope);

                if (server != null)
                {
                    server.LastCacheInstructionId = value;
                    Value = value;
                }

                _serverRegistrationRepository.Save(server);
            }
        }

        /// <inheritdoc />
        public void Read()
        {
            using (var scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                var server = GetServer(scope);

                if (server != null)
                {
                    Value = server.LastCacheInstructionId;
                }
            }
        }

        private IServerRegistration GetServer(IScope scope)
        {
            var serverRegistration = _serverRegistrationRepository.Get(scope.SqlContext.Query<IServerRegistration>().Where(x => x.ServerIdentity == ServerIdentity)).FirstOrDefault();

            return serverRegistration;
        }

    }

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
