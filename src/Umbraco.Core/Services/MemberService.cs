using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;
using System.Linq;

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

        public IMembershipUser CreateMember(string username, string email, string password, string memberType, int userId = 0)
        {
            var uow = _uowProvider.GetUnitOfWork();

            var member = new Member();

            using (var repository = _repositoryFactory.CreateMemberRepository(uow))
            {
                member.Username = username;
                member.Email = email;
                member.Password = password;

                repository.AddOrUpdate(member);
                uow.Commit();
            }

            return member;
        }

        public IMembershipUser GetByUsername(string userName)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IMembershipUser>.Builder.Where(x => x.Username == userName);
                var membershipUser = repository.GetByQuery(query).FirstOrDefault();

                return membershipUser;
            }
        }

        public IMembershipUser GetByEmail(string email)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IMembershipUser>.Builder.Where(x => x.Email == email);
                var membershipUser = repository.GetByQuery(query).FirstOrDefault();

                return membershipUser;
            }
        }

        public IMembershipUser GetById(object id)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IMembershipUser>.Builder.Where(x => x.Id == id);
                var membershipUser = repository.GetByQuery(query).FirstOrDefault();

                return membershipUser;
            }
        }

        public void Delete(IMembershipUser membershipUser)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                repository.Delete(membershipUser);
            }
        }

        public void Save(IMembershipUser membershipUser)
        {
            throw new System.NotImplementedException();
        }
    }
}