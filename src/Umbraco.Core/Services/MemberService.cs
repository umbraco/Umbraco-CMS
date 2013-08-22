using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the MemberService.
    /// </summary>
    internal class MemberService : IMemberService
    {
        private readonly RepositoryFactory _repositoryFactory;
        private readonly IDatabaseUnitOfWorkProvider _uowProvider;

        public MemberService(RepositoryFactory repositoryFactory)
            : this(new PetaPocoUnitOfWorkProvider(), repositoryFactory)
        {
        }

        public MemberService(IDatabaseUnitOfWorkProvider provider)
            : this(provider, new RepositoryFactory())
        {
        }

        public MemberService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
            _uowProvider = provider;
        }
    }
}