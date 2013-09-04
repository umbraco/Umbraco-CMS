using System;
using Umbraco.Core.Models;
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

        public IMember GetById(int id)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.Get(id);
            }
        }

        public IMember GetByKey(Guid id)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IMember>.Builder.Where(x => x.Key == id);
                var member = repository.GetByQuery(query).FirstOrDefault();
                return member;
            }
        }

        #region IMembershipMemberService Implementation

        public IMember CreateMember(string username, string email, string password, string memberTypeAlias, int userId = 0)
        {
            var uow = _uowProvider.GetUnitOfWork();
            IMemberType memberType;

            using (var repository = _repositoryFactory.CreateMemberTypeRepository(uow))
            {
                var query = Query<IMemberType>.Builder.Where(x => x.Alias == memberTypeAlias);
                memberType = repository.GetByQuery(query).FirstOrDefault();
            }

            if (memberType == null)
                throw new Exception(string.Format("No MemberType matching the passed in Alias: '{0}' was found", memberTypeAlias));

            var member = new Member(email, -1, memberType, new PropertyCollection());

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

        public IMember GetByUsername(string userName)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IMember>.Builder.Where(x => x.Username == userName);
                var member = repository.GetByQuery(query).FirstOrDefault();

                return member;
            }
        }

        public IMember GetByEmail(string email)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IMember>.Builder.Where(x => x.Email == email);
                var member = repository.GetByQuery(query).FirstOrDefault();

                return member;
            }
        }

        public IMember GetById(object id)
        {
            if (id is int)
            {
                return GetById((int)id);
            }

            if (id is Guid)
            {
                return GetByKey((Guid)id);
            }

            return null;
        }

        public void Delete(IMember member)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberRepository(uow))
            {
                repository.Delete(member);
                uow.Commit();
            }
        }

        public void Save(IMember member)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberRepository(uow))
            {
                repository.AddOrUpdate(member);
                uow.Commit();
            }
        }

        #endregion
    }
}