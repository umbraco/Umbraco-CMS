using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Web.Security;
using System.Xml.Linq;
using System.Xml.Linq;
using Umbraco.Core.Auditing;
using Umbraco.Core.Events;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using System.Linq;
using Umbraco.Core.Security;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the MemberService.
    /// </summary>
    public class MemberService : IMemberService
    {
        private readonly RepositoryFactory _repositoryFactory;
        private readonly IMemberGroupService _memberGroupService;
        private readonly IDatabaseUnitOfWorkProvider _uowProvider;

        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        public MemberService(RepositoryFactory repositoryFactory, IMemberGroupService memberGroupService)
            : this(new PetaPocoUnitOfWorkProvider(), repositoryFactory, memberGroupService)
        {
        }

        public MemberService(IDatabaseUnitOfWorkProvider provider, IMemberGroupService memberGroupService)
            : this(provider, new RepositoryFactory(), memberGroupService)
        {
        }

        public MemberService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, IMemberGroupService memberGroupService)
        {
            if (provider == null) throw new ArgumentNullException("provider");
            if (repositoryFactory == null) throw new ArgumentNullException("repositoryFactory");
            if (memberGroupService == null) throw new ArgumentNullException("memberGroupService");
            _repositoryFactory = repositoryFactory;
            _memberGroupService = memberGroupService;
            _uowProvider = provider;
        }

        #region IMemberService Implementation

        /// <summary>
        /// Get the default member type from the database - first check if the type "Member" is there, if not choose the first one found
        /// </summary>
        /// <returns></returns>
        public string GetDefaultMemberType()
        {
            using (var repository = _repositoryFactory.CreateMemberTypeRepository(_uowProvider.GetUnitOfWork()))
            {
                var types = repository.GetAll().Select(x => x.Alias).ToArray();

                if (types.Any() == false)
                {
                    throw new InvalidOperationException("No member types could be resolved");
                }

                if (types.InvariantContains("Member"))
                {
                    return types.First(x => x.InvariantEquals("Member"));
                }

                return types.First();
            }
        }

        /// <summary>
        /// Checks if a member with the username exists
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public bool Exists(string username)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.Exists(username);
            }
        }

        /// <summary>
        /// This is simply a helper method which essentially just wraps the MembershipProvider's ChangePassword method
        /// </summary>
        /// <param name="member">The member to save the password for</param>
        /// <param name="password"></param>
        /// <remarks>
        /// This method exists so that Umbraco developers can use one entry point to create/update members if they choose to.
        /// </remarks>
        public void SavePassword(IMember member, string password)
        {
            if (member == null) throw new ArgumentNullException("member");

            var provider = MembershipProviderExtensions.GetMembersMembershipProvider();
            if (provider.IsUmbracoMembershipProvider())
            {
                provider.ChangePassword(member.Username, "", password);
            }

            //go re-fetch the member and update the properties that may have changed
            var result = GetByUsername(member.Username);
            if (result != null)
            {
                //should never be null but it could have been deleted by another thread.
                member.RawPasswordValue = result.RawPasswordValue;
                member.LastPasswordChangeDate = result.LastPasswordChangeDate;
                member.UpdateDate = member.UpdateDate;             
            }

            throw new NotSupportedException("When using a non-Umbraco membership provider you must change the member password by using the MembershipProvider.ChangePassword method");
        }

        /// <summary>
        /// Checks if a member with the id exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Exists(int id)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.Exists(id);
            }
        }

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
        /// Gets a list of Members by their MemberType
        /// </summary>
        /// <param name="memberTypeId"></param>
        /// <returns></returns>
        public IEnumerable<IMember> GetMembersByMemberType(int memberTypeId)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                repository.Get(memberTypeId);
                var query = Query<IMember>.Builder.Where(x => x.ContentTypeId == memberTypeId);
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

        public void DeleteMembersOfType(int memberTypeId)
        {
            using (new WriteLock(Locker))
            {
                using (var uow = _uowProvider.GetUnitOfWork())
                {
                    var repository = _repositoryFactory.CreateMemberRepository(uow);
                    //TODO: What about content that has the contenttype as part of its composition?
                    var query = Query<IMember>.Builder.Where(x => x.ContentTypeId == memberTypeId);
                    var members = repository.GetByQuery(query).ToArray();

                    if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IMember>(members), this))
                        return;

                    foreach (var member in members)
                    {
                        //Permantly delete the member
                        Delete(member);
                    }
                }
            }
        }

        public IEnumerable<IMember> FindMembersByDisplayName(string displayNameToMatch, int pageIndex, int pageSize, out int totalRecords, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberRepository(uow))
            {
                //var query = new Query<IMember>();
                var sql = new Sql()
                    .Select("*")
                    .From<NodeDto>()
                    .Where<NodeDto>(dto => dto.NodeObjectType == new Guid(Constants.ObjectTypes.Member));
                
                switch (matchType)
                {
                    case StringPropertyMatchType.Exact:
                        sql.Where<NodeDto>(dto => dto.Text.Equals(displayNameToMatch));

                        //query.Where(member => member.Name.Equals(displayNameToMatch));
                        break;
                    case StringPropertyMatchType.Contains:
                        sql.Where<NodeDto>(dto => dto.Text.Contains(displayNameToMatch));
                        
                        //query.Where(member => member.Name.Contains(displayNameToMatch));
                        break;
                    case StringPropertyMatchType.StartsWith:
                        sql.Where<NodeDto>(dto => dto.Text.StartsWith(displayNameToMatch));

                        //query.Where(member => member.Name.StartsWith(displayNameToMatch));
                        break;
                    case StringPropertyMatchType.EndsWith:
                        sql.Where<NodeDto>(dto => dto.Text.EndsWith(displayNameToMatch));

                        //query.Where(member => member.Name.EndsWith(displayNameToMatch));
                        break;
                    case StringPropertyMatchType.Wildcard:
                        sql.Where<NodeDto>(dto => dto.Text.SqlWildcard(displayNameToMatch, TextColumnType.NVarchar));
                        
                        //query.Where(member => member.Name.SqlWildcard(displayNameToMatch, TextColumnType.NVarchar));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("matchType");
                }

                var result = repository.GetPagedResultsByQuery<NodeDto>(sql, pageIndex, pageSize, out totalRecords,
                    dtos => dtos.Select(x => x.NodeId).ToArray());

                //ensure this result is sorted correct just in case
                return result.OrderBy(x => x.Name);
            }
        }

        /// <summary>
        /// Does a search for members that contain the specified string in their email address
        /// </summary>
        /// <param name="emailStringToMatch"></param>
        /// <param name="totalRecords"></param>
        /// <param name="matchType"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IEnumerable<IMember> FindByEmail(string emailStringToMatch, int pageIndex, int pageSize, out int totalRecords, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberRepository(uow))
            {
                var query = new Query<IMember>();

                switch (matchType)
                {
                    case StringPropertyMatchType.Exact:
                        query.Where(member => member.Email.Equals(emailStringToMatch));
                        break;
                    case StringPropertyMatchType.Contains:
                        query.Where(member => member.Email.Contains(emailStringToMatch));
                        break;
                    case StringPropertyMatchType.StartsWith:
                        query.Where(member => member.Email.StartsWith(emailStringToMatch));
                        break;
                    case StringPropertyMatchType.EndsWith:
                        query.Where(member => member.Email.EndsWith(emailStringToMatch));
                        break;
                    case StringPropertyMatchType.Wildcard:
                        query.Where(member => member.Email.SqlWildcard(emailStringToMatch, TextColumnType.NVarchar));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("matchType");
                }

                return repository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalRecords, dto => dto.Email);
            }
        }

        public IEnumerable<IMember> FindByUsername(string login, int pageIndex, int pageSize, out int totalRecords, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberRepository(uow))
            {
                var query = new Query<IMember>();

                switch (matchType)
                {
                    case StringPropertyMatchType.Exact:
                        query.Where(member => member.Username.Equals(login));
                        break;
                    case StringPropertyMatchType.Contains:
                        query.Where(member => member.Username.Contains(login));
                        break;
                    case StringPropertyMatchType.StartsWith:
                        query.Where(member => member.Username.StartsWith(login));
                        break;
                    case StringPropertyMatchType.EndsWith:
                        query.Where(member => member.Username.EndsWith(login));
                        break;
                    case StringPropertyMatchType.Wildcard:
                        query.Where(member => member.Email.SqlWildcard(login, TextColumnType.NVarchar));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("matchType");
                }

                return repository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalRecords, dto => dto.Username);
            }
        }

        /// <summary>
        /// Gets a list of Members with a certain string property value
        /// </summary>
        /// <param name="propertyTypeAlias"></param>
        /// <param name="value"></param>
        /// <param name="matchType"></param>
        /// <returns></returns>
        public IEnumerable<IMember> GetMembersByPropertyValue(string propertyTypeAlias, string value, StringPropertyMatchType matchType = StringPropertyMatchType.Exact)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                IQuery<IMember> query;

                switch (matchType)
                {
                    case StringPropertyMatchType.Exact:
                        query =
                            Query<IMember>.Builder.Where(
                                x =>
                                ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                                (((Member)x).LongStringPropertyValue.SqlEquals(value, TextColumnType.NText) ||
                                 ((Member)x).ShortStringPropertyValue.SqlEquals(value, TextColumnType.NVarchar)));
                        break;
                    case StringPropertyMatchType.Contains:
                        query =
                            Query<IMember>.Builder.Where(
                                x =>
                                ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                                (((Member)x).LongStringPropertyValue.SqlContains(value, TextColumnType.NText) ||
                                 ((Member)x).ShortStringPropertyValue.SqlContains(value, TextColumnType.NVarchar)));
                        break;
                    case StringPropertyMatchType.StartsWith:
                        query =
                            Query<IMember>.Builder.Where(
                                x =>
                                ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                                (((Member)x).LongStringPropertyValue.SqlStartsWith(value, TextColumnType.NText) ||
                                 ((Member)x).ShortStringPropertyValue.SqlStartsWith(value, TextColumnType.NVarchar)));
                        break;
                    case StringPropertyMatchType.EndsWith:
                        query =
                            Query<IMember>.Builder.Where(
                                x =>
                                ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                                (((Member)x).LongStringPropertyValue.SqlEndsWith(value, TextColumnType.NText) ||
                                 ((Member)x).ShortStringPropertyValue.SqlEndsWith(value, TextColumnType.NVarchar)));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("matchType");
                }

                var members = repository.GetByQuery(query);
                return members;
            }
        }

        /// <summary>
        /// Gets a list of Members with a certain integer property value
        /// </summary>
        /// <param name="propertyTypeAlias"></param>
        /// <param name="value"></param>
        /// <param name="matchType"></param>
        /// <returns></returns>
        public IEnumerable<IMember> GetMembersByPropertyValue(string propertyTypeAlias, int value, ValuePropertyMatchType matchType = ValuePropertyMatchType.Exact)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                IQuery<IMember> query;

                switch (matchType)
                {
                    case ValuePropertyMatchType.Exact:
                        query =
                            Query<IMember>.Builder.Where(
                                x =>
                                ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                                ((Member)x).IntegerropertyValue == value);
                        break;
                    case ValuePropertyMatchType.GreaterThan:
                        query =
                            Query<IMember>.Builder.Where(
                                x =>
                                ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                                ((Member)x).IntegerropertyValue > value);
                        break;
                    case ValuePropertyMatchType.LessThan:
                        query =
                            Query<IMember>.Builder.Where(
                                x =>
                                ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                                ((Member)x).IntegerropertyValue < value);
                        break;
                    case ValuePropertyMatchType.GreaterThanOrEqualTo:
                        query =
                            Query<IMember>.Builder.Where(
                                x =>
                                ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                                ((Member)x).IntegerropertyValue >= value);
                        break;
                    case ValuePropertyMatchType.LessThanOrEqualTo:
                        query =
                            Query<IMember>.Builder.Where(
                                x =>
                                ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                                ((Member)x).IntegerropertyValue <= value);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("matchType");
                }

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
        /// <param name="matchType"></param>
        /// <returns></returns>
        public IEnumerable<IMember> GetMembersByPropertyValue(string propertyTypeAlias, DateTime value, ValuePropertyMatchType matchType = ValuePropertyMatchType.Exact)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                IQuery<IMember> query;

                switch (matchType)
                {
                    case ValuePropertyMatchType.Exact:
                        query =
                            Query<IMember>.Builder.Where(
                                x =>
                                ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                                ((Member)x).DateTimePropertyValue == value);
                        break;
                    case ValuePropertyMatchType.GreaterThan:
                        query =
                            Query<IMember>.Builder.Where(
                                x =>
                                ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                                ((Member)x).DateTimePropertyValue > value);
                        break;
                    case ValuePropertyMatchType.LessThan:
                        query =
                            Query<IMember>.Builder.Where(
                                x =>
                                ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                                ((Member)x).DateTimePropertyValue < value);
                        break;
                    case ValuePropertyMatchType.GreaterThanOrEqualTo:
                        query =
                            Query<IMember>.Builder.Where(
                                x =>
                                ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                                ((Member)x).DateTimePropertyValue >= value);
                        break;
                    case ValuePropertyMatchType.LessThanOrEqualTo:
                        query =
                            Query<IMember>.Builder.Where(
                                x =>
                                ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                                ((Member)x).DateTimePropertyValue <= value);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("matchType");
                }

                var members = repository.GetByQuery(query);
                return members;
            }
        }

        #endregion

        #region IMembershipMemberService Implementation

        /// <summary>
        /// Returns the count of members based on the countType
        /// </summary>
        /// <param name="countType"></param>
        /// <returns></returns>
        /// <remarks>
        /// The way the Online count is done is the same way that it is done in the MS SqlMembershipProvider - We query for any members
        /// that have their last active date within the Membership.UserIsOnlineTimeWindow (which is in minutes). It isn't exact science
        /// but that is how MS have made theirs so we'll follow that principal.
        /// </remarks>
        public int GetCount(MemberCountType countType)
        {
            using (var repository = _repositoryFactory.CreateMemberRepository(_uowProvider.GetUnitOfWork()))
            {
                IQuery<IMember> query;

                switch (countType)
                {
                    case MemberCountType.All:
                        query = new Query<IMember>();
                        return repository.Count(query);
                    case MemberCountType.Online:
                        var fromDate = DateTime.Now.AddMinutes(-Membership.UserIsOnlineTimeWindow);
                        query =
                            Query<IMember>.Builder.Where(
                                x =>
                                ((Member)x).PropertyTypeAlias == Constants.Conventions.Member.LastLoginDate &&
                                ((Member)x).DateTimePropertyValue > fromDate);
                        return repository.GetCountByQuery(query);
                    case MemberCountType.LockedOut:
                        query =
                            Query<IMember>.Builder.Where(
                                x =>
                                ((Member)x).PropertyTypeAlias == Constants.Conventions.Member.IsLockedOut &&
                                ((Member)x).BoolPropertyValue == true);
                        return repository.GetCountByQuery(query);
                    case MemberCountType.Approved:
                        query =
                            Query<IMember>.Builder.Where(
                                x =>
                                ((Member)x).PropertyTypeAlias == Constants.Conventions.Member.IsApproved &&
                                ((Member)x).BoolPropertyValue == true);
                        return repository.GetCountByQuery(query);
                    default:
                        throw new ArgumentOutOfRangeException("countType");
                }
            }

        }

        public IEnumerable<IMember> GetAll(int pageIndex, int pageSize, out int totalRecords)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberRepository(uow))
            {
                return repository.GetPagedResultsByQuery(null, pageIndex, pageSize, out totalRecords, member => member.Username);
            }
        }

        /// <summary>
        /// Creates a member object
        /// </summary>
        /// <param name="username"></param>
        /// <param name="email"></param>
        /// <param name="name"></param>
        /// <param name="memberTypeAlias"></param>
        /// <returns></returns>
        public IMember CreateMember(string username, string email, string name, string memberTypeAlias)
        {
            var memberType = FindMemberTypeByAlias(memberTypeAlias);
            return CreateMember(username, email, name, memberType);
        }

        /// <summary>
        /// Creates a new member object
        /// </summary>
        /// <param name="username"></param>
        /// <param name="email"></param>
        /// <param name="name"></param>
        /// <param name="memberType"></param>
        /// <returns></returns>
        public IMember CreateMember(string username, string email, string name, IMemberType memberType)
        {
            var member = new Member(name, email.ToLower().Trim(), username, memberType);

            Created.RaiseEvent(new NewEventArgs<IMember>(member, false, memberType.Alias, -1), this);

            return member;
        }

        /// <summary>
        /// Creates a member with an Id
        /// </summary>
        /// <param name="username"></param>
        /// <param name="email"></param>
        /// <param name="name"></param>
        /// <param name="memberTypeAlias"></param>
        /// <returns></returns>
        public IMember CreateMemberWithIdentity(string username, string email, string name, string memberTypeAlias)
        {
            var memberType = FindMemberTypeByAlias(memberTypeAlias);
            return CreateMemberWithIdentity(username, email, name, memberType);
        }

        /// <summary>
        /// Creates a member with an Id, the username will be used as their name
        /// </summary>
        /// <param name="username"></param>
        /// <param name="email"></param>
        /// <param name="memberType"></param>
        /// <returns></returns>
        public IMember CreateMemberWithIdentity(string username, string email, IMemberType memberType)
        {
            return CreateMemberWithIdentity(username, email, username, memberType);
        }

        /// <summary>
        /// Creates a member with an Id
        /// </summary>
        /// <param name="username"></param>
        /// <param name="email"></param>
        /// <param name="name"></param>
        /// <param name="memberType"></param>
        /// <returns></returns>
        public IMember CreateMemberWithIdentity(string username, string email, string name, IMemberType memberType)
        {
            return CreateMemberWithIdentity(username, email, name, "", memberType);
        }

        /// <summary>
        /// Creates and persists a new Member
        /// </summary>
        /// <param name="email"></param>
        /// <param name="username"></param>
        /// <param name="rawPasswordValue"></param>
        /// <param name="memberTypeAlias"></param>
        /// <returns></returns>
        IMember IMembershipMemberService<IMember>.CreateWithIdentity(string username, string email, string rawPasswordValue, string memberTypeAlias)
        {
            var memberType = FindMemberTypeByAlias(memberTypeAlias);
            return CreateMemberWithIdentity(username, email, memberType);
        }

        private IMember CreateMemberWithIdentity(string username, string email, string name, string rawPasswordValue, IMemberType memberType)
        {
            if (memberType == null) throw new ArgumentNullException("memberType");

            var member = new Member(name, email.ToLower().Trim(), username, rawPasswordValue, memberType);

            if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IMember>(member), this))
            {
                member.WasCancelled = true;
                return member;
            }

            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberRepository(uow))
            {
                repository.AddOrUpdate(member);
                uow.Commit();

                //insert the xml
                var xml = member.ToXml();
                CreateAndSaveMemberXml(xml, member.Id, uow.Database);
            }

            Saved.RaiseEvent(new SaveEventArgs<IMember>(member, false), this);
            Created.RaiseEvent(new NewEventArgs<IMember>(member, false, memberType.Alias, -1), this);

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
        public IMember GetByProviderKey(object id)
        {
            var asGuid = id.TryConvertTo<Guid>();
            if (asGuid.Success)
            {
                return GetByKey((Guid)id);
            }
            var asInt = id.TryConvertTo<int>();
            if (asInt.Success)
            {
                return GetById((int)id);
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
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberRepository(uow))
            {
                var query = Query<IMember>.Builder.Where(x => x.Email.Equals(email));
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
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberRepository(uow))
            {
                var query = Query<IMember>.Builder.Where(x => x.Username.Equals(userName));
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
            if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IMember>(member), this))
                return;

            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberRepository(uow))
            {
                repository.Delete(member);
                uow.Commit();
            }

            Deleted.RaiseEvent(new DeleteEventArgs<IMember>(member, false), this);
        }
        
        /// <summary>
        /// Saves an updated Member
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="raiseEvents"></param>
        public void Save(IMember entity, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IMember>(entity), this))
                {
                    return;
                }

            }

            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberRepository(uow))
            {
                repository.AddOrUpdate(entity);
                uow.Commit();

                var xml = entity.ToXml();
                CreateAndSaveMemberXml(xml, entity.Id, uow.Database);
            }

            if (raiseEvents)
                Saved.RaiseEvent(new SaveEventArgs<IMember>(entity, false), this);
        }

        public void Save(IEnumerable<IMember> entities, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IMember>(entities), this))
                    return;
            }
            using (new WriteLock(Locker))
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateMemberRepository(uow))
                {
                    foreach (var member in entities)
                    {
                        repository.AddOrUpdate(member);
                    }

                    //commit the whole lot in one go
                    uow.Commit();

                    foreach (var member in entities)
                    {
                        CreateAndSaveMemberXml(member.ToXml(), member.Id, uow.Database);
                    }
                }

                if (raiseEvents)
                    Saved.RaiseEvent(new SaveEventArgs<IMember>(entities, false), this);
            }
        }

        #endregion

        #region IMembershipRoleService Implementation

        public void AddRole(string roleName)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberGroupRepository(uow))
            {
                repository.CreateIfNotExists(roleName);
            }
        }

        public IEnumerable<string> GetAllRoles()
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberGroupRepository(uow))
            {
                var result = repository.GetAll();
                return result.Select(x => x.Name).Distinct();
            }
        }

        public IEnumerable<string> GetAllRoles(int memberId)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberGroupRepository(uow))
            {
                var result = repository.GetMemberGroupsForMember(memberId);
                return result.Select(x => x.Name).Distinct();
            }
        }

        public IEnumerable<string> GetAllRoles(string username)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberGroupRepository(uow))
            {
                var result = repository.GetMemberGroupsForMember(username);
                return result.Select(x => x.Name).Distinct();
            }
        }

        public IEnumerable<IMember> GetMembersInRole(string roleName)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberRepository(uow))
            {
                return repository.GetByMemberGroup(roleName);
            }
        }

        public IEnumerable<IMember> FindMembersInRole(string roleName, string usernameToMatch, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberRepository(uow))
            {
                return repository.FindMembersInRole(roleName, usernameToMatch, matchType);
            }
        }

        public bool DeleteRole(string roleName, bool throwIfBeingUsed)
        {
            using (new WriteLock(Locker))
            {
                if (throwIfBeingUsed)
                {
                    var inRole = GetMembersInRole(roleName);
                    if (inRole.Any())
                    {
                        throw new InvalidOperationException("The role " + roleName + " is currently assigned to members");
                    }
                }

                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateMemberGroupRepository(uow))
                {
                    var qry = new Query<IMemberGroup>().Where(g => g.Name == roleName);
                    var found = repository.GetByQuery(qry).ToArray();

                    foreach (var memberGroup in found)
                    {
                        _memberGroupService.Delete(memberGroup);
                    }
                    return found.Any();
                }
            }
        }

        public void AssignRoles(string[] usernames, string[] roleNames)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberGroupRepository(uow))
            {
                repository.AssignRoles(usernames, roleNames);
            }
        }

        public void DissociateRoles(string[] usernames, string[] roleNames)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberGroupRepository(uow))
            {
                repository.DissociateRoles(usernames, roleNames);
            }
        }

        public void AssignRoles(int[] memberIds, string[] roleNames)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberGroupRepository(uow))
            {
                repository.AssignRoles(memberIds, roleNames);
            }
        }

        public void DissociateRoles(int[] memberIds, string[] roleNames)
        {
            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberGroupRepository(uow))
            {
                repository.DissociateRoles(memberIds, roleNames);
            }
        }

        

        #endregion

        private IMemberType FindMemberTypeByAlias(string memberTypeAlias)
        {
            using (var repository = _repositoryFactory.CreateMemberTypeRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IMemberType>.Builder.Where(x => x.Alias == memberTypeAlias);
                var types = repository.GetByQuery(query);

                if (types.Any() == false)
                    throw new Exception(
                        string.Format("No MemberType matching the passed in Alias: '{0}' was found",
                                      memberTypeAlias));

                var contentType = types.First();

                if (contentType == null)
                    throw new Exception(string.Format("MemberType matching the passed in Alias: '{0}' was null",
                                                      memberTypeAlias));

                return contentType;
            }
        }

        /// <summary>
        /// Rebuilds all xml content in the cmsContentXml table for all media
        /// </summary>
        /// <param name="memberTypeIds">
        /// Only rebuild the xml structures for the content type ids passed in, if none then rebuilds the structures
        /// for all members = USE WITH CARE!
        /// </param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        internal void RebuildXmlStructures(params int[] memberTypeIds)
        {
            using (new WriteLock(Locker))
            {
                var list = new List<IMember>();

                var uow = _uowProvider.GetUnitOfWork();

                //First we're going to get the data that needs to be inserted before clearing anything, this 
                //ensures that we don't accidentally leave the content xml table empty if something happens
                //during the lookup process.

                if (memberTypeIds.Any() == false)
                {
                    list.AddRange(GetAllMembers());
                }
                else
                {
                    list.AddRange(memberTypeIds.SelectMany(GetMembersByMemberType));
                }

                var xmlItems = new List<ContentXmlDto>();
                foreach (var c in list)
                {
                    var xml = c.ToXml();
                    xmlItems.Add(new ContentXmlDto { NodeId = c.Id, Xml = xml.ToString(SaveOptions.None) });
                }

                //Ok, now we need to remove the data and re-insert it, we'll do this all in one transaction too.
                using (var tr = uow.Database.GetTransaction())
                {
                    if (memberTypeIds.Any() == false)
                    {
                        //Remove all member records from the cmsContentXml table (DO NOT REMOVE Content/Media!)
                        var memberObjectType = Guid.Parse(Constants.ObjectTypes.Member);
                        var subQuery = new Sql()
                            .Select("DISTINCT cmsContentXml.nodeId")
                            .From<ContentXmlDto>()
                            .InnerJoin<NodeDto>()
                            .On<ContentXmlDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                            .Where<NodeDto>(dto => dto.NodeObjectType == memberObjectType);

                        var deleteSql = SqlSyntaxContext.SqlSyntaxProvider.GetDeleteSubquery("cmsContentXml", "nodeId", subQuery);
                        uow.Database.Execute(deleteSql);
                    }
                    else
                    {
                        foreach (var id in memberTypeIds)
                        {
                            var id1 = id;
                            var memberObjectType = Guid.Parse(Constants.ObjectTypes.Member);
                            var subQuery = new Sql()
                                .Select("DISTINCT cmsContentXml.nodeId")
                                .From<ContentXmlDto>()
                                .InnerJoin<NodeDto>()
                                .On<ContentXmlDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                                .InnerJoin<ContentDto>()
                                .On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                                .Where<NodeDto>(dto => dto.NodeObjectType == memberObjectType)
                                .Where<ContentDto>(dto => dto.ContentTypeId == id1);

                            var deleteSql = SqlSyntaxContext.SqlSyntaxProvider.GetDeleteSubquery("cmsContentXml", "nodeId", subQuery);
                            uow.Database.Execute(deleteSql);
                        }
                    }

                    //bulk insert it into the database
                    uow.Database.BulkInsertRecords(xmlItems, tr);

                    tr.Complete();
                }
            }
        }

        private void CreateAndSaveMemberXml(XElement xml, int id, UmbracoDatabase db)
        {
            var poco = new ContentXmlDto { NodeId = id, Xml = xml.ToString(SaveOptions.None) };
            var exists = db.FirstOrDefault<ContentXmlDto>("WHERE nodeId = @Id", new { Id = id }) != null;
            int result = exists ? db.Update(poco) : Convert.ToInt32(db.Insert(poco));
        }

        #region Event Handlers

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IMemberService, DeleteEventArgs<IMember>> Deleting;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IMemberService, DeleteEventArgs<IMember>> Deleted;

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IMemberService, SaveEventArgs<IMember>> Saving;
        
        /// <summary>
        /// Occurs after Create
        /// </summary>
        /// <remarks>
        /// Please note that the Member object has been created, but might not have been saved
        /// so it does not have an identity yet (meaning no Id has been set).
        /// </remarks>
        public static event TypedEventHandler<IMemberService, NewEventArgs<IMember>> Created;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IMemberService, SaveEventArgs<IMember>> Saved;

        #endregion

        /// <summary>
        /// A helper method that will create a basic/generic member for use with a generic membership provider
        /// </summary>
        /// <returns></returns>
        internal static IMember CreateGenericMembershipProviderMember(string name, string email, string username, string password)
        {
            var identity = int.MaxValue;

            var memType = new MemberType(-1);
            var propGroup = new PropertyGroup
                {
                    Name = "Membership",
                    Id = --identity
                };
            propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext)
                {
                    Alias = Constants.Conventions.Member.Comments,
                    Name = Constants.Conventions.Member.CommentsLabel,
                    SortOrder = 0,
                    Id = --identity
                });
            propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.TrueFalseAlias, DataTypeDatabaseType.Integer)
                {
                    Alias = Constants.Conventions.Member.IsApproved,
                    Name = Constants.Conventions.Member.IsApprovedLabel,
                    SortOrder = 3,
                    Id = --identity
                });
            propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.TrueFalseAlias, DataTypeDatabaseType.Integer)
                {
                    Alias = Constants.Conventions.Member.IsLockedOut,
                    Name = Constants.Conventions.Member.IsLockedOutLabel,
                    SortOrder = 4,
                    Id = --identity
                });
            propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.NoEditAlias, DataTypeDatabaseType.Date)
                {
                    Alias = Constants.Conventions.Member.LastLockoutDate,
                    Name = Constants.Conventions.Member.LastLockoutDateLabel,
                    SortOrder = 5,
                    Id = --identity
                });
            propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.NoEditAlias, DataTypeDatabaseType.Date)
                {
                    Alias = Constants.Conventions.Member.LastLoginDate,
                    Name = Constants.Conventions.Member.LastLoginDateLabel,
                    SortOrder = 6,
                    Id = --identity
                });
            propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.NoEditAlias, DataTypeDatabaseType.Date)
                {
                    Alias = Constants.Conventions.Member.LastPasswordChangeDate,
                    Name = Constants.Conventions.Member.LastPasswordChangeDateLabel,
                    SortOrder = 7,
                    Id = --identity
                });

            memType.PropertyGroups.Add(propGroup);

            return new Member(name, email, username, password, memType);
        }


    }
}