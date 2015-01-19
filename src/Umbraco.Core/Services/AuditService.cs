using System;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    public sealed class AuditService : IAuditService
    {
        private readonly RepositoryFactory _repositoryFactory;
        private readonly IDatabaseUnitOfWorkProvider _uowProvider;

        public AuditService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory)
        {
            if (provider == null) throw new ArgumentNullException("provider");
            if (repositoryFactory == null) throw new ArgumentNullException("repositoryFactory");
            _uowProvider = provider;
            _repositoryFactory = repositoryFactory;
        }

        public void Add(AuditType type, string comment, int userId, int objectId)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repo = _repositoryFactory.CreateAuditRepository(uow))
            {
                repo.AddOrUpdate(new AuditItem(objectId, comment, type, userId));
                uow.Commit();
            }
        }
    }
}