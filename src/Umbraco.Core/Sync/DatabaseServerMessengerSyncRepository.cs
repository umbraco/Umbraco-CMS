using System.Linq;
using System.Web;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Sync
{
    /// <summary>
    ///     A version of <see cref="IServerMessengerSyncRepository" /> that uses the umbracoServer table to persist the
    ///     information in the database.
    /// </summary>
    public class DatabaseServerMessengerSyncRepository : IServerMessengerSyncRepository
    {
        private static readonly string ServerIdentity = NetworkHelper.MachineName // eg DOMAIN\SERVER
                                                        + "/" + HttpRuntime.AppDomainAppId; // eg /LM/S3SVC/11/ROOT

        private readonly IScopeProvider _scopeProvider;
        private readonly IServerRegistrationRepository _serverRegistrationRepository;

        public DatabaseServerMessengerSyncRepository(IServerRegistrationRepository serverRegistrationRepository,
            IScopeProvider scopeProvider)
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

                if (server != null) Value = server.LastCacheInstructionId;
            }
        }

        private IServerRegistration GetServer(IScope scope)
        {
            var serverRegistration = _serverRegistrationRepository
                .Get(scope.SqlContext.Query<IServerRegistration>().Where(x => x.ServerIdentity == ServerIdentity))
                .FirstOrDefault();

            return serverRegistration;
        }
    }
}
