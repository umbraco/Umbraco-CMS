using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

        #region IMemberService Implementation

        /// <summary>
        /// Gets a Member by its integer Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IMember GetById(int id)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.Get(id);
            }
        }

        /// <summary>
        /// Gets a Member by its Guid key
        /// </summary>
        /// <remarks>
        /// The guid key corresponds to the unique id in the database
        /// and the user id in the membership provider.
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        public IMember GetByKey(Guid id)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IMember>.Builder.Where(x => x.Key == id);
                var member = repository.GetByQuery(query).FirstOrDefault();
                return member;
            }
        }

        /// <summary>
        /// Gets a list of Members by their MemberType
        /// </summary>
        /// <param name="memberTypeAlias"></param>
        /// <returns></returns>
        public IEnumerable<IMember> GetMembersByMemberType(string memberTypeAlias)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IMember>.Builder.Where(x => x.ContentTypeAlias == memberTypeAlias);
                var members = repository.GetByQuery(query);
                return members;
            }
        }

        /// <summary>
        /// Gets a list of Members by the MemberGroup they are part of
        /// </summary>
        /// <param name="memberGroupName"></param>
        /// <returns></returns>
        public IEnumerable<IMember> GetMembersByGroup(string memberGroupName)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.GetByMemberGroup(memberGroupName);
            }
        }

        /// <summary>
        /// Gets a list of all Members
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public IEnumerable<IMember> GetAllMembers(params int[] ids)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.GetAll(ids);
            }
        }

        /// <summary>
        /// Does a search for members that contain the specified string in their email address
        /// </summary>
        /// <param name="emailStringToMatch"></param>
        /// <returns></returns>
        public IEnumerable<IMember> FindMembersByEmail(string emailStringToMatch)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberRepository(uow))
            {
                var query = new Query<IMember>();


                query.Where(member => member.Email.Contains(emailStringToMatch));

                return repository.GetByQuery(query);
            }
        }

        /// <summary>
        /// Gets a list of Members with a certain string property value
        /// </summary>
        /// <param name="propertyTypeAlias"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IEnumerable<IMember> GetMembersByPropertyValue(string propertyTypeAlias, string value)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                var query =
                    Query<IMember>.Builder.Where(
                        x =>
                            ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                            (((Member)x).LongStringPropertyValue.Contains(value) ||
                             ((Member)x).ShortStringPropertyValue.Contains(value)));

                var members = repository.GetByQuery(query);
                return members;
            }
        }

        /// <summary>
        /// Gets a list of Members with a certain integer property value
        /// </summary>
        /// <param name="propertyTypeAlias"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IEnumerable<IMember> GetMembersByPropertyValue(string propertyTypeAlias, int value)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                var query =
                    Query<IMember>.Builder.Where(
                        x =>
                            ((Member)x).PropertyTypeAlias == propertyTypeAlias && 
                            ((Member)x).IntegerropertyValue == value);

                var members = repository.GetByQuery(query);
                return members;
            }
        }

        /// <summary>
        /// Gets a list of Members with a certain boolean property value
        /// </summary>
        /// <param name="propertyTypeAlias"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IEnumerable<IMember> GetMembersByPropertyValue(string propertyTypeAlias, bool value)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                var query =
                    Query<IMember>.Builder.Where(
                        x =>
                            ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                            ((Member)x).BoolPropertyValue == value);

                var members = repository.GetByQuery(query);
                return members;
            }
        }

        /// <summary>
        /// Gets a list of Members with a certain date time property value
        /// </summary>
        /// <param name="propertyTypeAlias"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IEnumerable<IMember> GetMembersByPropertyValue(string propertyTypeAlias, DateTime value)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                var query =
                    Query<IMember>.Builder.Where(
                        x =>
                            ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                            ((Member)x).DateTimePropertyValue == value);

                var members = repository.GetByQuery(query);
                return members;
            }
        }
        
        #endregion

        #region IMembershipMemberService Implementation

        /// <summary>
        /// Creates a new Member
        /// </summary>
        /// <param name="email"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="memberTypeAlias"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IMember CreateMember(string email, string username, string password, string memberTypeAlias, int userId = 0)
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

            var member = new Member(email, email, username, password, -1, memberType);

            using (var repository = _repositoryFactory.CreateMemberRepository(uow))
            {
                repository.AddOrUpdate(member);
                uow.Commit();
            }

            return member;
        }

        /// <summary>
        /// Gets a Member by its Id
        /// </summary>
        /// <remarks>
        /// The Id should be an integer or Guid.
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets a Member by its Email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public IMember GetByEmail(string email)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IMember>.Builder.Where(x => x.Email == email);
                var member = repository.GetByQuery(query).FirstOrDefault();

                return member;
            }
        }

        /// <summary>
        /// Gets a Member by its Username
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public IMember GetByUsername(string userName)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IMember>.Builder.Where(x => x.Username == userName);
                var member = repository.GetByQuery(query).FirstOrDefault();

                return member;
            }
        }

        /// <summary>
        /// Deletes a Member
        /// </summary>
        /// <param name="member"></param>
        public void Delete(IMember member)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberRepository(uow))
            {
                repository.Delete(member);
                uow.Commit();
            }
        }

        /// <summary>
        /// Saves an updated Member
        /// </summary>
        /// <param name="member"></param>
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