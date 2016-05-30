using System;
using System.Collections.Generic;
using System.Web.Security;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;
using System.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Security;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the MemberService.
    /// </summary>
    public class MemberService : RepositoryService, IMemberService
    {
        private readonly EntityXmlSerializer _entitySerializer = new EntityXmlSerializer();
        private readonly IDataTypeService _dataTypeService;
        private readonly IMemberGroupService _memberGroupService;

        #region Constructor

        public MemberService(
            IDatabaseUnitOfWorkProvider provider,
            ILogger logger,
            IEventMessagesFactory eventMessagesFactory,
            IMemberGroupService memberGroupService,
            IDataTypeService dataTypeService)
            : base(provider, logger, eventMessagesFactory)
        {
            if (memberGroupService == null) throw new ArgumentNullException(nameof(memberGroupService));
            if (dataTypeService == null) throw new ArgumentNullException(nameof(dataTypeService));
            _memberGroupService = memberGroupService;
            _dataTypeService = dataTypeService;
        }

        #endregion

        #region Count

        /// <summary>
        /// Gets the total number of Members based on the count type
        /// </summary>
        /// <remarks>
        /// The way the Online count is done is the same way that it is done in the MS SqlMembershipProvider - We query for any members
        /// that have their last active date within the Membership.UserIsOnlineTimeWindow (which is in minutes). It isn't exact science
        /// but that is how MS have made theirs so we'll follow that principal.
        /// </remarks>
        /// <param name="countType"><see cref="MemberCountType"/> to count by</param>
        /// <returns><see cref="System.int"/> with number of Members for passed in type</returns>
        public int GetCount(MemberCountType countType)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                IQuery<IMember> query;

                switch (countType)
                {
                    case MemberCountType.All:
                        query = repository.Query;
                        break;
                    case MemberCountType.Online:
                        var fromDate = DateTime.Now.AddMinutes(-Membership.UserIsOnlineTimeWindow);
                        query = repository.Query.Where(x =>
                            ((Member)x).PropertyTypeAlias == Constants.Conventions.Member.LastLoginDate &&
                            ((Member)x).DateTimePropertyValue > fromDate);
                        break;
                    case MemberCountType.LockedOut:
                        query = repository.Query.Where(x =>
                            ((Member)x).PropertyTypeAlias == Constants.Conventions.Member.IsLockedOut &&
                            ((Member)x).BoolPropertyValue);
                        break;
                    case MemberCountType.Approved:
                        query = repository.Query.Where(x =>
                            ((Member)x).PropertyTypeAlias == Constants.Conventions.Member.IsApproved &&
                            ((Member)x).BoolPropertyValue);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(countType)); // causes rollback;
                }

                var count = repository.GetCountByQuery(query);
                uow.Complete();
                return count;
            }
        }

        /// <summary>
        /// Gets the count of Members by an optional MemberType alias
        /// </summary>
        /// <remarks>If no alias is supplied then the count for all Member will be returned</remarks>
        /// <param name="memberTypeAlias">Optional alias for the MemberType when counting number of Members</param>
        /// <returns><see cref="System.int"/> with number of Members</returns>
        public int Count(string memberTypeAlias = null)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                var count = repository.Count(memberTypeAlias);
                uow.Complete();
                return count;
            }
        }

        #endregion

        #region Create

        /// <summary>
        /// Creates an <see cref="IMember"/> object without persisting it
        /// </summary>
        /// <remarks>This method is convenient for when you need to add properties to a new Member
        /// before persisting it in order to limit the amount of times its saved.
        /// Also note that the returned <see cref="IMember"/> will not have an Id until its saved.</remarks>
        /// <param name="username">Username of the Member to create</param>
        /// <param name="email">Email of the Member to create</param>
        /// <param name="name">Name of the Member to create</param>
        /// <param name="memberTypeAlias">Alias of the MemberType the Member should be based on</param>
        /// <returns><see cref="IMember"/></returns>
        public IMember CreateMember(string username, string email, string name, string memberTypeAlias)
        {
            var memberType = GetMemberType(memberTypeAlias);
            if (memberType == null)
                throw new ArgumentException("No member type with that alias.", nameof(memberTypeAlias));

            var member = new Member(name, email.ToLower().Trim(), username, memberType);
            CreateMember(null, member, 0, false);

            return member;
        }

        /// <summary>
        /// Creates an <see cref="IMember"/> object without persisting it
        /// </summary>
        /// <remarks>This method is convenient for when you need to add properties to a new Member
        /// before persisting it in order to limit the amount of times its saved.
        /// Also note that the returned <see cref="IMember"/> will not have an Id until its saved.</remarks>
        /// <param name="username">Username of the Member to create</param>
        /// <param name="email">Email of the Member to create</param>
        /// <param name="name">Name of the Member to create</param>
        /// <param name="memberType">MemberType the Member should be based on</param>
        /// <returns><see cref="IMember"/></returns>
        public IMember CreateMember(string username, string email, string name, IMemberType memberType)
        {
            if (memberType == null) throw new ArgumentNullException(nameof(memberType));

            var member = new Member(name, email.ToLower().Trim(), username, memberType);
            CreateMember(null, member, 0, false);

            return member;
        }

        /// <summary>
        /// Creates and persists a Member
        /// </summary>
        /// <remarks>Using this method will persist the Member object before its returned
        /// meaning that it will have an Id available (unlike the CreateMember method)</remarks>
        /// <param name="username">Username of the Member to create</param>
        /// <param name="email">Email of the Member to create</param>
        /// <param name="name">Name of the Member to create</param>
        /// <param name="memberTypeAlias">Alias of the MemberType the Member should be based on</param>
        /// <returns><see cref="IMember"/></returns>
        public IMember CreateMemberWithIdentity(string username, string email, string name, string memberTypeAlias)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                // locking the member tree secures member types too
                uow.WriteLock(Constants.Locks.MemberTree);

                var memberType = GetMemberType(memberTypeAlias); // + locks
                if (memberType == null)
                    throw new ArgumentException("No member type with that alias.", nameof(memberTypeAlias)); // causes rollback

                var member = new Member(name, email.ToLower().Trim(), username, memberType);
                CreateMember(uow, member, 0, true);

                uow.Complete();
                return member;
            }
        }

        /// <summary>
        /// Creates and persists a Member
        /// </summary>
        /// <remarks>Using this method will persist the Member object before its returned
        /// meaning that it will have an Id available (unlike the CreateMember method)</remarks>
        /// <param name="username">Username of the Member to create</param>
        /// <param name="email">Email of the Member to create</param>
        /// <param name="memberType">MemberType the Member should be based on</param>
        /// <returns><see cref="IMember"/></returns>
        public IMember CreateMemberWithIdentity(string username, string email, IMemberType memberType)
        {
            return CreateMemberWithIdentity(username, email, username, "", memberType);
        }

        /// <summary>
        /// Creates and persists a Member
        /// </summary>
        /// <remarks>Using this method will persist the Member object before its returned
        /// meaning that it will have an Id available (unlike the CreateMember method)</remarks>
        /// <param name="username">Username of the Member to create</param>
        /// <param name="email">Email of the Member to create</param>
        /// <param name="name">Name of the Member to create</param>
        /// <param name="memberType">MemberType the Member should be based on</param>
        /// <returns><see cref="IMember"/></returns>
        public IMember CreateMemberWithIdentity(string username, string email, string name, IMemberType memberType)
        {
            return CreateMemberWithIdentity(username, email, name, "", memberType);
        }

        /// <summary>
        /// Creates and persists a new <see cref="IMember"/>
        /// </summary>
        /// <remarks>An <see cref="IMembershipUser"/> can be of type <see cref="IMember"/> or <see cref="IUser"/></remarks>
        /// <param name="username">Username of the <see cref="IMembershipUser"/> to create</param>
        /// <param name="email">Email of the <see cref="IMembershipUser"/> to create</param>
        /// <param name="passwordValue">This value should be the encoded/encrypted/hashed value for the password that will be stored in the database</param>
        /// <param name="memberTypeAlias">Alias of the Type</param>
        /// <returns><see cref="IMember"/></returns>
        IMember IMembershipMemberService<IMember>.CreateWithIdentity(string username, string email, string passwordValue, string memberTypeAlias)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.MemberTree);

                // ensure it all still make sense
                var memberType = GetMemberType(memberTypeAlias); // + locks
                if (memberType == null)
                    throw new ArgumentException("No member type with that alias.", nameof(memberTypeAlias)); // causes rollback

                var member = new Member(username, email.ToLower().Trim(), username, passwordValue, memberType);
                CreateMember(uow, member, -1, true);

                uow.Complete();
                return member;
            }
        }

        /// <summary>
        /// Creates and persists a Member
        /// </summary>
        /// <remarks>Using this method will persist the Member object before its returned
        /// meaning that it will have an Id available (unlike the CreateMember method)</remarks>
        /// <param name="username">Username of the Member to create</param>
        /// <param name="email">Email of the Member to create</param>
        /// <param name="name">Name of the Member to create</param>
        /// <param name="passwordValue">This value should be the encoded/encrypted/hashed value for the password that will be stored in the database</param>
        /// <param name="memberType">MemberType the Member should be based on</param>
        /// <returns><see cref="IMember"/></returns>
        private IMember CreateMemberWithIdentity(string username, string email, string name, string passwordValue, IMemberType memberType)
        {
            if (memberType == null) throw new ArgumentNullException(nameof(memberType));

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.MemberTree);

                // ensure it all still make sense
                var vrfy = GetMemberType(memberType.Alias); // + locks
                if (vrfy == null || vrfy.Id != memberType.Id)
                    throw new ArgumentException($"Member type with alias {memberType.Alias} does not exist or is a different member type."); // causes rollback

                var member = new Member(name, email.ToLower().Trim(), username, passwordValue, memberType);
                CreateMember(uow, member, -1, true);

                uow.Complete();
                return member;
            }
        }

        private void CreateMember(IDatabaseUnitOfWork uow, Member member, int userId, bool withIdentity)
        {
            // there's no Creating event for members

            if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IMember>(member), this))
            {
                member.WasCancelled = true;
                return;
            }

            member.CreatorId = userId;

            if (withIdentity)
            {
                if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IMember>(member), this))
                {
                    member.WasCancelled = true;
                    return;
                }

                var repository = uow.CreateRepository<IMemberRepository>();
                repository.AddOrUpdate(member);

                // fixme kill
                uow.Flush(); // need everything so we can serialize
                repository.AddOrUpdateContentXml(member, m => _entitySerializer.Serialize(_dataTypeService, m));
                // generate preview for blame history?
                if (UmbracoConfig.For.UmbracoSettings().Content.GlobalPreviewStorageEnabled)
                    repository.AddOrUpdatePreviewXml(member, m => _entitySerializer.Serialize(_dataTypeService, m));

                Saved.RaiseEvent(new SaveEventArgs<IMember>(member, false), this);
            }

            Created.RaiseEvent(new NewEventArgs<IMember>(member, false, member.ContentType.Alias, -1), this);

            var msg = withIdentity
                ? "Member '{0}' was created with Id {1}"
                : "Member '{0}' was created";
            Audit(AuditType.New, string.Format(msg, member.Name, member.Id), member.CreatorId, member.Id);
        }

        #endregion

        #region Get, Has, Is, Exists...

        /// <summary>
        /// Gets a Member by its integer id
        /// </summary>
        /// <param name="id"><see cref="System.int"/> Id</param>
        /// <returns><see cref="IMember"/></returns>
        public IMember GetById(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                var member = repository.Get(id);
                uow.Complete();
                return member;
            }
        }

        /// <summary>
        /// Gets a Member by the unique key
        /// </summary>
        /// <remarks>The guid key corresponds to the unique id in the database
        /// and the user id in the membership provider.</remarks>
        /// <param name="id"><see cref="Guid"/> Id</param>
        /// <returns><see cref="IMember"/></returns>
        public IMember GetByKey(Guid id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                var query = repository.Query.Where(x => x.Key == id);
                var member = repository.GetByQuery(query).FirstOrDefault();
                uow.Complete();
                return member;
            }
        }

        /// <summary>
        /// Gets a list of paged <see cref="IMember"/> objects
        /// </summary>
        /// <param name="pageIndex">Current page index</param>
        /// <param name="pageSize">Size of the page</param>
        /// <param name="totalRecords">Total number of records found (out)</param>
        /// <returns><see cref="IEnumerable{IMember}"/></returns>
        public IEnumerable<IMember> GetAll(long pageIndex, int pageSize, out long totalRecords)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                var members = repository.GetPagedResultsByQuery(null, pageIndex, pageSize, out totalRecords, "LoginName", Direction.Ascending, true);
                uow.Complete();
                return members;
            }
        }

        public IEnumerable<IMember> GetAll(long pageIndex, int pageSize, out long totalRecords,
            string orderBy, Direction orderDirection, string memberTypeAlias = null, string filter = "")
        {
            return GetAll(pageIndex, pageSize, out totalRecords, orderBy, orderDirection, true, memberTypeAlias, filter);
        }

        public IEnumerable<IMember> GetAll(long pageIndex, int pageSize, out long totalRecords,
            string orderBy, Direction orderDirection, bool orderBySystemField, string memberTypeAlias, string filter)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                var query = memberTypeAlias == null ? null : repository.Query.Where(x => x.ContentTypeAlias == memberTypeAlias);
                var members = repository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalRecords, orderBy, orderDirection, orderBySystemField, filter);
                uow.Complete();
                return members;
            }
        }

        /// <summary>
        /// Gets an <see cref="IMember"/> by its provider key
        /// </summary>
        /// <param name="id">Id to use for retrieval</param>
        /// <returns><see cref="IMember"/></returns>
        public IMember GetByProviderKey(object id)
        {
            var asGuid = id.TryConvertTo<Guid>();
            if (asGuid.Success)
                return GetByKey(asGuid.Result);

            var asInt = id.TryConvertTo<int>();
            if (asInt.Success)
                return GetById(asInt.Result);

            return null;
        }

        /// <summary>
        /// Get an <see cref="IMember"/> by email
        /// </summary>
        /// <param name="email">Email to use for retrieval</param>
        /// <returns><see cref="IMember"/></returns>
        public IMember GetByEmail(string email)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                var query = repository.Query.Where(x => x.Email.Equals(email));
                var member = repository.GetByQuery(query).FirstOrDefault();
                uow.Complete();
                return member;
            }
        }

        /// <summary>
        /// Get an <see cref="IMember"/> by username
        /// </summary>
        /// <param name="username">Username to use for retrieval</param>
        /// <returns><see cref="IMember"/></returns>
        public IMember GetByUsername(string username)
        {
            //TODO: Somewhere in here, whether at this level or the repository level, we need to add
            // a caching mechanism since this method is used by all the membership providers and could be
            // called quite a bit when dealing with members.

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                var query = repository.Query.Where(x => x.Username.Equals(username));
                var member = repository.GetByQuery(query).FirstOrDefault();
                uow.Complete();
                return member;
            }
        }

        /// <summary>
        /// Gets all Members for the specified MemberType alias
        /// </summary>
        /// <param name="memberTypeAlias">Alias of the MemberType</param>
        /// <returns><see cref="IEnumerable{IMember}"/></returns>
        public IEnumerable<IMember> GetMembersByMemberType(string memberTypeAlias)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                var query = repository.Query.Where(x => x.ContentTypeAlias == memberTypeAlias);
                var members = repository.GetByQuery(query);
                uow.Complete();
                return members;
            }
        }

        /// <summary>
        /// Gets all Members for the MemberType id
        /// </summary>
        /// <param name="memberTypeId">Id of the MemberType</param>
        /// <returns><see cref="IEnumerable{IMember}"/></returns>
        public IEnumerable<IMember> GetMembersByMemberType(int memberTypeId)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                repository.Get(memberTypeId);
                var query = repository.Query.Where(x => x.ContentTypeId == memberTypeId);
                var members = repository.GetByQuery(query);
                uow.Complete();
                return members;
            }
        }

        /// <summary>
        /// Gets all Members within the specified MemberGroup name
        /// </summary>
        /// <param name="memberGroupName">Name of the MemberGroup</param>
        /// <returns><see cref="IEnumerable{IMember}"/></returns>
        public IEnumerable<IMember> GetMembersByGroup(string memberGroupName)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                var members = repository.GetByMemberGroup(memberGroupName);
                uow.Complete();
                return members;
            }
        }

        /// <summary>
        /// Gets all Members with the ids specified
        /// </summary>
        /// <remarks>If no Ids are specified all Members will be retrieved</remarks>
        /// <param name="ids">Optional list of Member Ids</param>
        /// <returns><see cref="IEnumerable{IMember}"/></returns>
        public IEnumerable<IMember> GetAllMembers(params int[] ids)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                var members = repository.GetAll(ids);
                uow.Complete();
                return members;
            }
        }

        /// <summary>
        /// Finds Members based on their display name
        /// </summary>
        /// <param name="displayNameToMatch">Display name to match</param>
        /// <param name="pageIndex">Current page index</param>
        /// <param name="pageSize">Size of the page</param>
        /// <param name="totalRecords">Total number of records found (out)</param>
        /// <param name="matchType">The type of match to make as <see cref="StringPropertyMatchType"/>. Default is <see cref="StringPropertyMatchType.StartsWith"/></param>
        /// <returns><see cref="IEnumerable{IMember}"/></returns>
        public IEnumerable<IMember> FindMembersByDisplayName(string displayNameToMatch, long pageIndex, int pageSize, out long totalRecords, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                var query = repository.Query;

                switch (matchType)
                {
                    case StringPropertyMatchType.Exact:
                        query.Where(member => member.Name.Equals(displayNameToMatch));
                        break;
                    case StringPropertyMatchType.Contains:
                        query.Where(member => member.Name.Contains(displayNameToMatch));
                        break;
                    case StringPropertyMatchType.StartsWith:
                        query.Where(member => member.Name.StartsWith(displayNameToMatch));
                        break;
                    case StringPropertyMatchType.EndsWith:
                        query.Where(member => member.Name.EndsWith(displayNameToMatch));
                        break;
                    case StringPropertyMatchType.Wildcard:
                        query.Where(member => member.Name.SqlWildcard(displayNameToMatch, TextColumnType.NVarchar));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(matchType)); // causes rollback
                }

                var members = repository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalRecords, "Name", Direction.Ascending, true);
                uow.Complete();
                return members;
            }
        }

        /// <summary>
        /// Finds a list of <see cref="IMember"/> objects by a partial email string
        /// </summary>
        /// <param name="emailStringToMatch">Partial email string to match</param>
        /// <param name="pageIndex">Current page index</param>
        /// <param name="pageSize">Size of the page</param>
        /// <param name="totalRecords">Total number of records found (out)</param>
        /// <param name="matchType">The type of match to make as <see cref="StringPropertyMatchType"/>. Default is <see cref="StringPropertyMatchType.StartsWith"/></param>
        /// <returns><see cref="IEnumerable{IMember}"/></returns>
        public IEnumerable<IMember> FindByEmail(string emailStringToMatch, long pageIndex, int pageSize, out long totalRecords, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                var query = repository.Query;

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
                        throw new ArgumentOutOfRangeException(nameof(matchType));
                }

                var members = repository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalRecords, "Email", Direction.Ascending, true);
                uow.Complete();
                return members;
            }
        }

        /// <summary>
        /// Finds a list of <see cref="IMember"/> objects by a partial username
        /// </summary>
        /// <param name="login">Partial username to match</param>
        /// <param name="pageIndex">Current page index</param>
        /// <param name="pageSize">Size of the page</param>
        /// <param name="totalRecords">Total number of records found (out)</param>
        /// <param name="matchType">The type of match to make as <see cref="StringPropertyMatchType"/>. Default is <see cref="StringPropertyMatchType.StartsWith"/></param>
        /// <returns><see cref="IEnumerable{IMember}"/></returns>
        public IEnumerable<IMember> FindByUsername(string login, long pageIndex, int pageSize, out long totalRecords, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                var query = repository.Query;

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
                        throw new ArgumentOutOfRangeException(nameof(matchType));
                }

                var members = repository.GetPagedResultsByQuery(query, pageIndex, pageSize, out totalRecords, "LoginName", Direction.Ascending, true);
                uow.Complete();
                return members;
            }
        }

        /// <summary>
        /// Gets a list of Members based on a property search
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType to search for</param>
        /// <param name="value"><see cref="System.string"/> Value to match</param>
        /// <param name="matchType">The type of match to make as <see cref="StringPropertyMatchType"/>. Default is <see cref="StringPropertyMatchType.Exact"/></param>
        /// <returns><see cref="IEnumerable{IMember}"/></returns>
        public IEnumerable<IMember> GetMembersByPropertyValue(string propertyTypeAlias, string value, StringPropertyMatchType matchType = StringPropertyMatchType.Exact)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                IQuery<IMember> query;

                switch (matchType)
                {
                    case StringPropertyMatchType.Exact:
                        query = repository.Query.Where(x =>
                            ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                            (((Member)x).LongStringPropertyValue.SqlEquals(value, TextColumnType.NText) ||
                                ((Member)x).ShortStringPropertyValue.SqlEquals(value, TextColumnType.NVarchar)));
                        break;
                    case StringPropertyMatchType.Contains:
                        query = repository.Query.Where(x =>
                            ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                            (((Member)x).LongStringPropertyValue.SqlContains(value, TextColumnType.NText) ||
                                ((Member)x).ShortStringPropertyValue.SqlContains(value, TextColumnType.NVarchar)));
                        break;
                    case StringPropertyMatchType.StartsWith:
                        query = repository.Query.Where(x =>
                            ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                            (((Member)x).LongStringPropertyValue.SqlStartsWith(value, TextColumnType.NText) ||
                                ((Member)x).ShortStringPropertyValue.SqlStartsWith(value, TextColumnType.NVarchar)));
                        break;
                    case StringPropertyMatchType.EndsWith:
                        query = repository.Query.Where(x =>
                            ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                            (((Member)x).LongStringPropertyValue.SqlEndsWith(value, TextColumnType.NText) ||
                                ((Member)x).ShortStringPropertyValue.SqlEndsWith(value, TextColumnType.NVarchar)));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(matchType)); // causes rollback
                }

                var members = repository.GetByQuery(query);
                uow.Complete();
                return members;
            }
        }

        /// <summary>
        /// Gets a list of Members based on a property search
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType to search for</param>
        /// <param name="value"><see cref="System.int"/> Value to match</param>
        /// <param name="matchType">The type of match to make as <see cref="StringPropertyMatchType"/>. Default is <see cref="StringPropertyMatchType.Exact"/></param>
        /// <returns><see cref="IEnumerable{IMember}"/></returns>
        public IEnumerable<IMember> GetMembersByPropertyValue(string propertyTypeAlias, int value, ValuePropertyMatchType matchType = ValuePropertyMatchType.Exact)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                IQuery<IMember> query;

                switch (matchType)
                {
                    case ValuePropertyMatchType.Exact:
                        query = repository.Query.Where(x =>
                            ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                            ((Member)x).IntegerPropertyValue == value);
                        break;
                    case ValuePropertyMatchType.GreaterThan:
                        query = repository.Query.Where(x =>
                            ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                            ((Member)x).IntegerPropertyValue > value);
                        break;
                    case ValuePropertyMatchType.LessThan:
                        query = repository.Query.Where(x =>
                            ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                            ((Member)x).IntegerPropertyValue < value);
                        break;
                    case ValuePropertyMatchType.GreaterThanOrEqualTo:
                        query = repository.Query.Where(x =>
                            ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                            ((Member)x).IntegerPropertyValue >= value);
                        break;
                    case ValuePropertyMatchType.LessThanOrEqualTo:
                        query = repository.Query.Where(x =>
                            ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                            ((Member)x).IntegerPropertyValue <= value);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(matchType)); // causes rollback
                }

                var members = repository.GetByQuery(query);
                uow.Complete();
                return members;
            }
        }

        /// <summary>
        /// Gets a list of Members based on a property search
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType to search for</param>
        /// <param name="value"><see cref="System.bool"/> Value to match</param>
        /// <returns><see cref="IEnumerable{IMember}"/></returns>
        public IEnumerable<IMember> GetMembersByPropertyValue(string propertyTypeAlias, bool value)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                var query = repository.Query.Where(x =>
                    ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                    ((Member)x).BoolPropertyValue == value);

                var members = repository.GetByQuery(query);
                uow.Complete();
                return members;
            }
        }

        /// <summary>
        /// Gets a list of Members based on a property search
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType to search for</param>
        /// <param name="value"><see cref="System.DateTime"/> Value to match</param>
        /// <param name="matchType">The type of match to make as <see cref="StringPropertyMatchType"/>. Default is <see cref="StringPropertyMatchType.Exact"/></param>
        /// <returns><see cref="IEnumerable{IMember}"/></returns>
        public IEnumerable<IMember> GetMembersByPropertyValue(string propertyTypeAlias, DateTime value, ValuePropertyMatchType matchType = ValuePropertyMatchType.Exact)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                IQuery<IMember> query;

                switch (matchType)
                {
                    case ValuePropertyMatchType.Exact:
                        query = repository.Query.Where( x =>
                            ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                            ((Member)x).DateTimePropertyValue == value);
                        break;
                    case ValuePropertyMatchType.GreaterThan:
                        query = repository.Query.Where(x =>
                            ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                            ((Member)x).DateTimePropertyValue > value);
                        break;
                    case ValuePropertyMatchType.LessThan:
                        query = repository.Query.Where(x =>
                            ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                            ((Member)x).DateTimePropertyValue < value);
                        break;
                    case ValuePropertyMatchType.GreaterThanOrEqualTo:
                        query = repository.Query.Where(x =>
                            ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                            ((Member)x).DateTimePropertyValue >= value);
                        break;
                    case ValuePropertyMatchType.LessThanOrEqualTo:
                        query = repository.Query.Where(x =>
                            ((Member)x).PropertyTypeAlias == propertyTypeAlias &&
                            ((Member)x).DateTimePropertyValue <= value);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(matchType)); // causes rollback
                }

                //TODO: Since this is by property value, we need a GetByPropertyQuery on the repo!
                var members = repository.GetByQuery(query);
                uow.Complete();
                return members;
            }
        }

        /// <summary>
        /// Checks if a Member with the id exists
        /// </summary>
        /// <param name="id">Id of the Member</param>
        /// <returns><c>True</c> if the Member exists otherwise <c>False</c></returns>
        public bool Exists(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                var exists = repository.Exists(id);
                uow.Complete();
                return exists;
            }
        }

        /// <summary>
        /// Checks if a Member with the username exists
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <returns><c>True</c> if the Member exists otherwise <c>False</c></returns>
        public bool Exists(string username)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                var exists = repository.Exists(username);
                uow.Complete();
                return exists;
            }
        }

        #endregion

        #region Save

        /// <summary>
        /// Saves an <see cref="IMember"/>
        /// </summary>
        /// <param name="member"><see cref="IMember"/> to Save</param>
        /// <param name="raiseEvents">Optional parameter to raise events.
        /// Default is <c>True</c> otherwise set to <c>False</c> to not raise events</param>
        public void Save(IMember member, bool raiseEvents = true)
        {
            if (raiseEvents && Saving.IsRaisedEventCancelled(new SaveEventArgs<IMember>(member), this))
                    return;

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();

                repository.AddOrUpdate(member);

                // fixme get rid of xml
                repository.AddOrUpdateContentXml(member, m => _entitySerializer.Serialize(_dataTypeService, m));

                // generate preview for blame history?
                if (UmbracoConfig.For.UmbracoSettings().Content.GlobalPreviewStorageEnabled)
                    repository.AddOrUpdatePreviewXml(member, m => _entitySerializer.Serialize(_dataTypeService, m));

                uow.Complete();
            }

            if (raiseEvents)
                Saved.RaiseEvent(new SaveEventArgs<IMember>(member, false), this);
            Audit(AuditType.Save, "Save Member performed by user", 0, member.Id);
        }

        /// <summary>
        /// Saves a list of <see cref="IMember"/> objects
        /// </summary>
        /// <param name="members"><see cref="IEnumerable{IMember}"/> to save</param>
        /// <param name="raiseEvents">Optional parameter to raise events.
        /// Default is <c>True</c> otherwise set to <c>False</c> to not raise events</param>
        public void Save(IEnumerable<IMember> members, bool raiseEvents = true)
        {
            var membersA = members.ToArray();

            if (raiseEvents && Saving.IsRaisedEventCancelled(new SaveEventArgs<IMember>(membersA), this))
                    return;

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                foreach (var member in membersA)
                {
                    repository.AddOrUpdate(member);

                    // fixme get rid of xml stuff
                    repository.AddOrUpdateContentXml(member, m => _entitySerializer.Serialize(_dataTypeService, m));

                    // generate preview for blame history?
                    if (UmbracoConfig.For.UmbracoSettings().Content.GlobalPreviewStorageEnabled)
                        repository.AddOrUpdatePreviewXml(member, m => _entitySerializer.Serialize(_dataTypeService, m));
                }

                uow.Complete();
            }

            if (raiseEvents)
                Saved.RaiseEvent(new SaveEventArgs<IMember>(membersA, false), this);
            Audit(AuditType.Save, "Save Member items performed by user", 0, -1);
        }

        #endregion

        #region Delete

        /// <summary>
        /// Deletes an <see cref="IMember"/>
        /// </summary>
        /// <param name="member"><see cref="IMember"/> to Delete</param>
        public void Delete(IMember member)
        {
            if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IMember>(member), this))
                return;

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                DeleteLocked(repository, member);
                uow.Complete();

            }

            Audit(AuditType.Delete, "Delete Member performed by user", 0, member.Id);
        }

        private void DeleteLocked(IMemberRepository repository, IMember member)
        {
            // a member has no descendants
            repository.Delete(member);
            var args = new DeleteEventArgs<IMember>(member, false); // raise event & get flagged files
            Deleted.RaiseEvent(args, this);
            IOHelper.DeleteFiles(args.MediaFilesToDelete, // remove flagged files
                (file, e) => Logger.Error<MemberService>("An error occurred while deleting file attached to nodes: " + file, e));
        }

        #endregion

        #region Roles

        public void AddRole(string roleName)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberGroupRepository>();
                repository.CreateIfNotExists(roleName);
                uow.Complete();
            }
        }

        public IEnumerable<string> GetAllRoles()
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberGroupRepository>();
                var result = repository.GetAll().Select(x => x.Name).Distinct();
                uow.Complete();
                return result;
            }
        }

        public IEnumerable<string> GetAllRoles(int memberId)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberGroupRepository>();
                var result = repository.GetMemberGroupsForMember(memberId);
                var roles = result.Select(x => x.Name).Distinct();
                uow.Complete();
                return roles;
            }
        }

        public IEnumerable<string> GetAllRoles(string username)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberGroupRepository>();
                var result = repository.GetMemberGroupsForMember(username);
                var roles = result.Select(x => x.Name).Distinct();
                uow.Complete();
                return roles;
            }
        }

        public IEnumerable<IMember> GetMembersInRole(string roleName)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                var members = repository.GetByMemberGroup(roleName);
                uow.Complete();
                return members;
            }
        }

        public IEnumerable<IMember> FindMembersInRole(string roleName, string usernameToMatch, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();
                var members = repository.FindMembersInRole(roleName, usernameToMatch, matchType);
                uow.Complete();
                return members;
            }
        }

        // FIXME CURRENT WIP

        public bool DeleteRole(string roleName, bool throwIfBeingUsed)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberGroupRepository>();

                if (throwIfBeingUsed)
                {
                    // get members in role
                    var memberRepository = uow.CreateRepository<IMemberRepository>();
                    var membersInRole = memberRepository.GetByMemberGroup(roleName);
                    if (membersInRole.Any())
                        throw new InvalidOperationException("The role " + roleName + " is currently assigned to members");
                }

                var query = repository.QueryFactory.Create<IMemberGroup>().Where(g => g.Name == roleName);
                var found = repository.GetByQuery(query).ToArray();

                foreach (var memberGroup in found)
                    _memberGroupService.Delete(memberGroup); // FIXME BAD BAD BAD!

                uow.Complete();
                return found.Length > 0;
            }
        }
        public void AssignRole(string username, string roleName)
        {
            AssignRoles(new[] { username }, new[] { roleName });
        }

        public void AssignRoles(string[] usernames, string[] roleNames)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberGroupRepository>();
                repository.AssignRoles(usernames, roleNames);
                uow.Complete();
            }
        }

        public void DissociateRole(string username, string roleName)
        {
            DissociateRoles(new[] { username }, new[] { roleName });
        }

        public void DissociateRoles(string[] usernames, string[] roleNames)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberGroupRepository>();
                repository.DissociateRoles(usernames, roleNames);
                uow.Complete();
            }
        }

        public void AssignRole(int memberId, string roleName)
        {
            AssignRoles(new[] { memberId }, new[] { roleName });
        }

        public void AssignRoles(int[] memberIds, string[] roleNames)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberGroupRepository>();
                repository.AssignRoles(memberIds, roleNames);
                uow.Complete();
            }
        }

        public void DissociateRole(int memberId, string roleName)
        {
            DissociateRoles(new[] { memberId }, new[] { roleName });
        }

        public void DissociateRoles(int[] memberIds, string[] roleNames)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberGroupRepository>();
                repository.DissociateRoles(memberIds, roleNames);
                uow.Complete();
            }
        }

        #endregion

        #region Private Methods

        private void Audit(AuditType type, string message, int userId, int objectId)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repo = uow.CreateRepository<IAuditRepository>();
                repo.AddOrUpdate(new AuditItem(objectId, message, type, userId));
                uow.Complete();
            }
        }

        #endregion

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

        #region Membership

        /// <summary>
        /// This is simply a helper method which essentially just wraps the MembershipProvider's ChangePassword method
        /// </summary>
        /// <remarks>This method exists so that Umbraco developers can use one entry point to create/update
        /// Members if they choose to. </remarks>
        /// <param name="member">The Member to save the password for</param>
        /// <param name="password">The password to encrypt and save</param>
        public void SavePassword(IMember member, string password)
        {
            if (member == null) throw new ArgumentNullException("member");

            var provider = MembershipProviderExtensions.GetMembersMembershipProvider();
            if (provider.IsUmbracoMembershipProvider())
                provider.ChangePassword(member.Username, "", password);
            else
                throw new NotSupportedException("When using a non-Umbraco membership provider you must change the member password by using the MembershipProvider.ChangePassword method");

            //go re-fetch the member and update the properties that may have changed
            var result = GetByUsername(member.Username);

            //should never be null but it could have been deleted by another thread.
            // fixme - should LOCK! instead
            if (result == null)
                return;

            member.RawPasswordValue = result.RawPasswordValue;
            member.LastPasswordChangeDate = result.LastPasswordChangeDate;
            member.UpdateDate = result.UpdateDate;

            // fixme - not saving?
        }

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
            propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, Constants.Conventions.Member.Comments)
            {
                Name = Constants.Conventions.Member.CommentsLabel,
                SortOrder = 0,
                Id = --identity,
                Key = identity.ToGuid()
            });
            propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.TrueFalseAlias, DataTypeDatabaseType.Integer, Constants.Conventions.Member.IsApproved)
            {
                Name = Constants.Conventions.Member.IsApprovedLabel,
                SortOrder = 3,
                Id = --identity,
                Key = identity.ToGuid()
            });
            propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.TrueFalseAlias, DataTypeDatabaseType.Integer, Constants.Conventions.Member.IsLockedOut)
            {
                Name = Constants.Conventions.Member.IsLockedOutLabel,
                SortOrder = 4,
                Id = --identity,
                Key = identity.ToGuid()
            });
            propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.NoEditAlias, DataTypeDatabaseType.Date, Constants.Conventions.Member.LastLockoutDate)
            {
                Name = Constants.Conventions.Member.LastLockoutDateLabel,
                SortOrder = 5,
                Id = --identity,
                Key = identity.ToGuid()
            });
            propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.NoEditAlias, DataTypeDatabaseType.Date, Constants.Conventions.Member.LastLoginDate)
            {
                Name = Constants.Conventions.Member.LastLoginDateLabel,
                SortOrder = 6,
                Id = --identity,
                Key = identity.ToGuid()
            });
            propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.NoEditAlias, DataTypeDatabaseType.Date, Constants.Conventions.Member.LastPasswordChangeDate)
            {
                Name = Constants.Conventions.Member.LastPasswordChangeDateLabel,
                SortOrder = 7,
                Id = --identity,
                Key = identity.ToGuid()
            });

            memType.PropertyGroups.Add(propGroup);

            // should we "create member"?
            var member = new Member(name, email, username, password, memType);

            //we've assigned ids to the property types and groups but we also need to assign fake ids to the properties themselves.
            foreach (var property in member.Properties)
            {
                property.Id = --identity;
            }

            return member;
        }

        #endregion

        #region Content Types

        /// <summary>
        /// Delete Members of the specified MemberType id
        /// </summary>
        /// <param name="memberTypeId">Id of the MemberType</param>
        public void DeleteMembersOfType(int memberTypeId)
        {
            // note: no tree to manage here

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.MemberTree);
                var repository = uow.CreateRepository<IMemberRepository>();

                //TODO: What about content that has the contenttype as part of its composition?
                var query = repository.Query.Where(x => x.ContentTypeId == memberTypeId);
                var members = repository.GetByQuery(query).ToArray();

                if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IMember>(members), this))
                    return; // causes rollback

                foreach (var member in members)
                {
                    // delete media
                    // triggers the deleted event (and handles the files)
                    DeleteLocked(repository, member);
                }

                uow.Complete();
            }
        }

        private IMemberType GetMemberType(string memberTypeAlias)
        {
            Mandate.ParameterNotNullOrEmpty(memberTypeAlias, nameof(memberTypeAlias));

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.MemberTypes);

                var repository = uow.CreateRepository<IMemberTypeRepository>();
                var memberType = repository.Get(memberTypeAlias);

                if (memberType == null)
                    throw new Exception($"No MemberType matching the passed in Alias: '{memberTypeAlias}' was found"); // causes rollback

                uow.Complete();
                return memberType;
            }
        }

        #endregion

        #region Xml - Should Move!

        /// <summary>
        /// Rebuilds all xml content in the cmsContentXml table for all members
        /// </summary>
        /// <param name="memberTypeIds">
        /// Only rebuild the xml structures for the content type ids passed in, if none then rebuilds the structures
        /// for all members = USE WITH CARE!
        /// </param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        public void RebuildXmlStructures(params int[] memberTypeIds)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repository = uow.CreateRepository<IMemberRepository>();
                repository.RebuildXmlStructures(
                    member => _entitySerializer.Serialize(_dataTypeService, member),
                    contentTypeIds: memberTypeIds.Length == 0 ? null : memberTypeIds);
                uow.Complete();
            }

            Audit(AuditType.Publish, "MemberService.RebuildXmlStructures completed, the xml has been regenerated in the database", 0, -1);
        }

        #endregion
    }
}