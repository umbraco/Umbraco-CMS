using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Exceptions;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Scoping;
using Umbraco.Core.Security;

namespace Umbraco.Core.Services.Implement
{
    /// <summary>
    /// Represents the MemberService.
    /// </summary>
    public class MemberService : ScopeRepositoryService, IMemberService
    {
        private readonly IMemberRepository _memberRepository;
        private readonly IMemberTypeRepository _memberTypeRepository;
        private readonly IMemberGroupRepository _memberGroupRepository;
        private readonly IAuditRepository _auditRepository;

        private readonly IMemberGroupService _memberGroupService;
        private readonly IMediaFileSystem _mediaFileSystem;

        //only for unit tests!
        internal MembershipProviderBase MembershipProvider { get; set; }

        #region Constructor

        public MemberService(IScopeProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory, IMemberGroupService memberGroupService,  IMediaFileSystem mediaFileSystem,
            IMemberRepository memberRepository, IMemberTypeRepository memberTypeRepository, IMemberGroupRepository memberGroupRepository, IAuditRepository auditRepository)
            : base(provider, logger, eventMessagesFactory)
        {
            _memberRepository = memberRepository;
            _memberTypeRepository = memberTypeRepository;
            _memberGroupRepository = memberGroupRepository;
            _auditRepository = auditRepository;
            _memberGroupService = memberGroupService ?? throw new ArgumentNullException(nameof(memberGroupService));
            _mediaFileSystem = mediaFileSystem ?? throw new ArgumentNullException(nameof(mediaFileSystem));
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
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);

                IQuery<IMember> query;

                switch (countType)
                {
                    case MemberCountType.All:
                        query = Query<IMember>();
                        break;
                    case MemberCountType.Online:
                        var fromDate = DateTime.Now.AddMinutes(-Membership.UserIsOnlineTimeWindow);
                        query = Query<IMember>().Where(x => ((Member) x).PropertyTypeAlias == Constants.Conventions.Member.LastLoginDate && ((Member) x).DateTimePropertyValue > fromDate);
                        break;
                    case MemberCountType.LockedOut:
                        query = Query<IMember>().Where(x => ((Member) x).PropertyTypeAlias == Constants.Conventions.Member.IsLockedOut && ((Member) x).BoolPropertyValue);
                        break;
                    case MemberCountType.Approved:
                        query = Query<IMember>().Where(x => ((Member) x).PropertyTypeAlias == Constants.Conventions.Member.IsApproved && ((Member) x).BoolPropertyValue);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(countType));
                }

                return _memberRepository.GetCountByQuery(query);
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
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                return _memberRepository.Count(memberTypeAlias);
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
            using (var scope = ScopeProvider.CreateScope())
            {
                CreateMember(scope, member, 0, false);
                scope.Complete();
            }

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
            using (var scope = ScopeProvider.CreateScope())
            {
                CreateMember(scope, member, 0, false);
                scope.Complete();
            }

            return member;
        }

        /// <summary>
        /// Creates and persists a new <see cref="IMember"/>
        /// </summary>
        /// <remarks>An <see cref="IMembershipUser"/> can be of type <see cref="IMember"/> or <see cref="IUser"/></remarks>
        /// <param name="username">Username of the <see cref="IMembershipUser"/> to create</param>
        /// <param name="email">Email of the <see cref="IMembershipUser"/> to create</param>
        /// <param name="passwordValue">This value should be the encoded/encrypted/hashed value for the password that will be stored in the database</param>
        /// <param name="memberTypeAlias">Alias of the Type</param>
        /// <param name="isApproved">Is the member approved</param>
        /// <returns><see cref="IMember"/></returns>
        IMember IMembershipMemberService<IMember>.CreateWithIdentity(string username, string email, string passwordValue, string memberTypeAlias)
        {
            return CreateMemberWithIdentity(username, email, username, passwordValue, memberTypeAlias);
        }

        /// <summary>
        /// Creates and persists a new <see cref="IMember"/>
        /// </summary>
        /// <remarks>An <see cref="IMembershipUser"/> can be of type <see cref="IMember"/> or <see cref="IUser"/></remarks>
        /// <param name="username">Username of the <see cref="IMembershipUser"/> to create</param>
        /// <param name="email">Email of the <see cref="IMembershipUser"/> to create</param>
        /// <param name="passwordValue">This value should be the encoded/encrypted/hashed value for the password that will be stored in the database</param>
        /// <param name="memberTypeAlias">Alias of the Type</param>
        /// <param name="isApproved"></param>
        /// <returns><see cref="IMember"/></returns>
        IMember IMembershipMemberService<IMember>.CreateWithIdentity(string username, string email, string passwordValue, string memberTypeAlias, bool isApproved)
        {
            return CreateMemberWithIdentity(username, email, username, passwordValue, memberTypeAlias, isApproved);
        }

        public IMember CreateMemberWithIdentity(string username, string email, string memberTypeAlias)
        {
            return CreateMemberWithIdentity(username, email, username, "", memberTypeAlias);
        }

        public IMember CreateMemberWithIdentity(string username, string email, string memberTypeAlias, bool isApproved)
        {
            return CreateMemberWithIdentity(username, email, username, "", memberTypeAlias, isApproved);
        }

        public IMember CreateMemberWithIdentity(string username, string email, string name, string memberTypeAlias)
        {
            return CreateMemberWithIdentity(username, email, name, "", memberTypeAlias);
        }

        public IMember CreateMemberWithIdentity(string username, string email, string name, string memberTypeAlias, bool isApproved)
        {
            return CreateMemberWithIdentity(username, email, name, "", memberTypeAlias, isApproved);
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
        /// <param name="isApproved">Optional IsApproved of the Member to create</param>
        /// <returns><see cref="IMember"/></returns>
        public IMember CreateMemberWithIdentity(string username, string email, string name, string passwordValue, string memberTypeAlias, bool isApproved = true)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                // locking the member tree secures member types too
                scope.WriteLock(Constants.Locks.MemberTree);

                var memberType = GetMemberType(scope, memberTypeAlias); // + locks // + locks
                if (memberType == null)
                    throw new ArgumentException("No member type with that alias.", nameof(memberTypeAlias)); // causes rollback // causes rollback

                var member = new Member(name, email.ToLower().Trim(), username, passwordValue, memberType, isApproved);
                CreateMember(scope, member, -1, true);

                scope.Complete();
                return member;
            }
        }

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
        /// <param name="memberType">MemberType the Member should be based on</param>
        /// <returns><see cref="IMember"/></returns>
        public IMember CreateMemberWithIdentity(string username, string email, IMemberType memberType, bool isApproved)
        {
            return CreateMemberWithIdentity(username, email, username, "", memberType, isApproved);
        }

        public IMember CreateMemberWithIdentity(string username, string email, string name, IMemberType memberType)
        {
            return CreateMemberWithIdentity(username, email, name, "", memberType);
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
        public IMember CreateMemberWithIdentity(string username, string email, string name, IMemberType memberType, bool isApproved)
        {
            return CreateMemberWithIdentity(username, email, name, "", memberType, isApproved);
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
        private IMember CreateMemberWithIdentity(string username, string email, string name, string passwordValue, IMemberType memberType, bool isApproved = true)
        {
            if (memberType == null) throw new ArgumentNullException(nameof(memberType));

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.MemberTree);

                // ensure it all still make sense
                // ensure it all still make sense
                var vrfy = GetMemberType(scope, memberType.Alias); // + locks

                if (vrfy == null || vrfy.Id != memberType.Id)
                    throw new ArgumentException($"Member type with alias {memberType.Alias} does not exist or is a different member type."); // causes rollback
                var member = new Member(name, email.ToLower().Trim(), username, passwordValue, memberType, isApproved);

                CreateMember(scope, member, -1, true);
                scope.Complete();
                return member;
            }
        }

        private void CreateMember(IScope scope, Member member, int userId, bool withIdentity)
        {
            member.CreatorId = userId;

            if (withIdentity)
            {
                // if saving is cancelled, media remains without an identity
                var saveEventArgs = new SaveEventArgs<IMember>(member);
                if (scope.Events.DispatchCancelable(Saving, this, saveEventArgs))
                    return;

                _memberRepository.Save(member);

                saveEventArgs.CanCancel = false;
                scope.Events.Dispatch(Saved, this, saveEventArgs);
            }

            if (withIdentity == false)
                return;

            Audit(AuditType.New, member.CreatorId, member.Id, $"Member '{member.Name}' was created with Id {member.Id}");
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
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                return _memberRepository.Get(id);
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
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                var query = Query<IMember>().Where(x => x.Key == id);
                return _memberRepository.Get(query).FirstOrDefault();
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
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                return _memberRepository.GetPage(null, pageIndex, pageSize, out totalRecords, null, Ordering.By("LoginName"));
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
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                var query1 = memberTypeAlias == null ? null : Query<IMember>().Where(x => x.ContentTypeAlias == memberTypeAlias);
                var query2 = filter == null ? null : Query<IMember>().Where(x => x.Name.Contains(filter) || x.Username.Contains(filter) || x.Email.Contains(filter));
                return _memberRepository.GetPage(query1, pageIndex, pageSize, out totalRecords, query2, Ordering.By(orderBy, orderDirection, isCustomField: !orderBySystemField));
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
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                var query = Query<IMember>().Where(x => x.Email.Equals(email));
                return _memberRepository.Get(query).FirstOrDefault();
            }
        }

        /// <summary>
        /// Get an <see cref="IMember"/> by username
        /// </summary>
        /// <param name="username">Username to use for retrieval</param>
        /// <returns><see cref="IMember"/></returns>
        public IMember GetByUsername(string username)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);                
                return _memberRepository.GetByUsername(username);
            }
        }

        /// <summary>
        /// Gets all Members for the specified MemberType alias
        /// </summary>
        /// <param name="memberTypeAlias">Alias of the MemberType</param>
        /// <returns><see cref="IEnumerable{IMember}"/></returns>
        public IEnumerable<IMember> GetMembersByMemberType(string memberTypeAlias)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                var query = Query<IMember>().Where(x => x.ContentTypeAlias == memberTypeAlias);
                return _memberRepository.Get(query);
            }
        }

        /// <summary>
        /// Gets all Members for the MemberType id
        /// </summary>
        /// <param name="memberTypeId">Id of the MemberType</param>
        /// <returns><see cref="IEnumerable{IMember}"/></returns>
        public IEnumerable<IMember> GetMembersByMemberType(int memberTypeId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                var query = Query<IMember>().Where(x => x.ContentTypeId == memberTypeId);
                return _memberRepository.Get(query);
            }
        }

        /// <summary>
        /// Gets all Members within the specified MemberGroup name
        /// </summary>
        /// <param name="memberGroupName">Name of the MemberGroup</param>
        /// <returns><see cref="IEnumerable{IMember}"/></returns>
        public IEnumerable<IMember> GetMembersByGroup(string memberGroupName)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                return _memberRepository.GetByMemberGroup(memberGroupName);
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
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                return _memberRepository.GetMany(ids);
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
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                var query = Query<IMember>();

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
                        throw new ArgumentOutOfRangeException(nameof(matchType)); // causes rollback // causes rollback
                }

                return _memberRepository.GetPage(query, pageIndex, pageSize, out totalRecords, null, Ordering.By("Name"));
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
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                var query = Query<IMember>();

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

                return _memberRepository.GetPage(query, pageIndex, pageSize, out totalRecords, null, Ordering.By("Email"));
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
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                var query = Query<IMember>();

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
                        query.Where(member => member.Username.SqlWildcard(login, TextColumnType.NVarchar));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(matchType));
                }

                return _memberRepository.GetPage(query, pageIndex, pageSize, out totalRecords, null, Ordering.By("LoginName"));
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
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                IQuery<IMember> query;

                switch (matchType)
                {
                    case StringPropertyMatchType.Exact:
                        query = Query<IMember>().Where(x => ((Member) x).PropertyTypeAlias == propertyTypeAlias && (((Member) x).LongStringPropertyValue.SqlEquals(value, TextColumnType.NText) || ((Member) x).ShortStringPropertyValue.SqlEquals(value, TextColumnType.NVarchar)));
                        break;
                    case StringPropertyMatchType.Contains:
                        query = Query<IMember>().Where(x => ((Member) x).PropertyTypeAlias == propertyTypeAlias && (((Member) x).LongStringPropertyValue.SqlContains(value, TextColumnType.NText) || ((Member) x).ShortStringPropertyValue.SqlContains(value, TextColumnType.NVarchar)));
                        break;
                    case StringPropertyMatchType.StartsWith:
                        query = Query<IMember>().Where(x => ((Member) x).PropertyTypeAlias == propertyTypeAlias && (((Member) x).LongStringPropertyValue.SqlStartsWith(value, TextColumnType.NText) || ((Member) x).ShortStringPropertyValue.SqlStartsWith(value, TextColumnType.NVarchar)));
                        break;
                    case StringPropertyMatchType.EndsWith:
                        query = Query<IMember>().Where(x => ((Member) x).PropertyTypeAlias == propertyTypeAlias && (((Member) x).LongStringPropertyValue.SqlEndsWith(value, TextColumnType.NText) || ((Member) x).ShortStringPropertyValue.SqlEndsWith(value, TextColumnType.NVarchar)));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(matchType));
                }

                return _memberRepository.Get(query);
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
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                IQuery<IMember> query;

                switch (matchType)
                {
                    case ValuePropertyMatchType.Exact:
                        query = Query<IMember>().Where(x => ((Member) x).PropertyTypeAlias == propertyTypeAlias && ((Member) x).IntegerPropertyValue == value);
                        break;
                    case ValuePropertyMatchType.GreaterThan:
                        query = Query<IMember>().Where(x => ((Member) x).PropertyTypeAlias == propertyTypeAlias && ((Member) x).IntegerPropertyValue > value);
                        break;
                    case ValuePropertyMatchType.LessThan:
                        query = Query<IMember>().Where(x => ((Member) x).PropertyTypeAlias == propertyTypeAlias && ((Member) x).IntegerPropertyValue < value);
                        break;
                    case ValuePropertyMatchType.GreaterThanOrEqualTo:
                        query = Query<IMember>().Where(x => ((Member) x).PropertyTypeAlias == propertyTypeAlias && ((Member) x).IntegerPropertyValue >= value);
                        break;
                    case ValuePropertyMatchType.LessThanOrEqualTo:
                        query = Query<IMember>().Where(x => ((Member) x).PropertyTypeAlias == propertyTypeAlias && ((Member) x).IntegerPropertyValue <= value);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(matchType));
                }

                return _memberRepository.Get(query);
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
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                var query = Query<IMember>().Where(x => ((Member) x).PropertyTypeAlias == propertyTypeAlias && ((Member) x).BoolPropertyValue == value);

                return _memberRepository.Get(query);
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
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                IQuery<IMember> query;

                switch (matchType)
                {
                    case ValuePropertyMatchType.Exact:
                        query = Query<IMember>().Where(x => ((Member) x).PropertyTypeAlias == propertyTypeAlias && ((Member) x).DateTimePropertyValue == value);
                        break;
                    case ValuePropertyMatchType.GreaterThan:
                        query = Query<IMember>().Where(x => ((Member) x).PropertyTypeAlias == propertyTypeAlias && ((Member) x).DateTimePropertyValue > value);
                        break;
                    case ValuePropertyMatchType.LessThan:
                        query = Query<IMember>().Where(x => ((Member) x).PropertyTypeAlias == propertyTypeAlias && ((Member) x).DateTimePropertyValue < value);
                        break;
                    case ValuePropertyMatchType.GreaterThanOrEqualTo:
                        query = Query<IMember>().Where(x => ((Member) x).PropertyTypeAlias == propertyTypeAlias && ((Member) x).DateTimePropertyValue >= value);
                        break;
                    case ValuePropertyMatchType.LessThanOrEqualTo:
                        query = Query<IMember>().Where(x => ((Member) x).PropertyTypeAlias == propertyTypeAlias && ((Member) x).DateTimePropertyValue <= value);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(matchType)); // causes rollback // causes rollback
                }

                // TODO: Since this is by property value, we need a GetByPropertyQuery on the repo!
                // TODO: Since this is by property value, we need a GetByPropertyQuery on the repo!
                return _memberRepository.Get(query);
            }
        }

        /// <summary>
        /// Checks if a Member with the id exists
        /// </summary>
        /// <param name="id">Id of the Member</param>
        /// <returns><c>True</c> if the Member exists otherwise <c>False</c></returns>
        public bool Exists(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                return _memberRepository.Exists(id);
            }
        }

        /// <summary>
        /// Checks if a Member with the username exists
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <returns><c>True</c> if the Member exists otherwise <c>False</c></returns>
        public bool Exists(string username)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                return _memberRepository.Exists(username);
            }
        }

        #endregion

        #region Save

        /// <inheritdoc />
        public void SetLastLogin(string username, DateTime date)
        {
            using (var scope = ScopeProvider.CreateScope())
            {   
                _memberRepository.SetLastLogin(username, date);
                scope.Complete();
            }
        }

        /// <inheritdoc />
        public void Save(IMember member, bool raiseEvents = true)
        {
            //trimming username and email to make sure we have no trailing space
            member.Username = member.Username.Trim();
            member.Email = member.Email.Trim();

            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<IMember>(member);
                if (raiseEvents && scope.Events.DispatchCancelable(Saving, this, saveEventArgs))
                {
                    scope.Complete();
                    return;
                }

                if (string.IsNullOrWhiteSpace(member.Name))
                {
                    throw new ArgumentException("Cannot save member with empty name.");
                }

                scope.WriteLock(Constants.Locks.MemberTree);

                _memberRepository.Save(member);

                if (raiseEvents)
                {
                    saveEventArgs.CanCancel = false;
                    scope.Events.Dispatch(Saved, this, saveEventArgs);
                }
                Audit(AuditType.Save, 0, member.Id);

                scope.Complete();
            }
        }

        /// <inheritdoc />
        public void Save(IEnumerable<IMember> members, bool raiseEvents = true)
        {
            var membersA = members.ToArray();

            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<IMember>(membersA);
                if (raiseEvents && scope.Events.DispatchCancelable(Saving, this, saveEventArgs))
                {
                    scope.Complete();
                    return;
                }

                scope.WriteLock(Constants.Locks.MemberTree);

                foreach (var member in membersA)
                {
                    //trimming username and email to make sure we have no trailing space
                    member.Username = member.Username.Trim();
                    member.Email = member.Email.Trim();

                    _memberRepository.Save(member);
                }

                if (raiseEvents)
                {
                    saveEventArgs.CanCancel = false;
                    scope.Events.Dispatch(Saved, this, saveEventArgs);
                }
                Audit(AuditType.Save, 0, -1, "Save multiple Members");

                scope.Complete();
            }
        }

        #endregion

        #region Delete

        /// <summary>
        /// Deletes an <see cref="IMember"/>
        /// </summary>
        /// <param name="member"><see cref="IMember"/> to Delete</param>
        public void Delete(IMember member)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var deleteEventArgs = new DeleteEventArgs<IMember>(member);
                if (scope.Events.DispatchCancelable(Deleting, this, deleteEventArgs))
                {
                    scope.Complete();
                    return;
                }

                scope.WriteLock(Constants.Locks.MemberTree);
                DeleteLocked(scope, member, deleteEventArgs);

                Audit(AuditType.Delete, 0, member.Id);
                scope.Complete();
            }
        }

        private void DeleteLocked(IScope scope, IMember member, DeleteEventArgs<IMember> args = null)
        {
            // a member has no descendants
            _memberRepository.Delete(member);
            if (args == null)
                args = new DeleteEventArgs<IMember>(member, false); // raise event & get flagged files
            else
                args.CanCancel = false;
            scope.Events.Dispatch(Deleted, this, args);

            // media files deleted by QueuingEventDispatcher
        }

        #endregion

        #region Roles

        public void AddRole(string roleName)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.MemberTree);
                _memberGroupRepository.CreateIfNotExists(roleName);
                scope.Complete();
            }
        }

        public IEnumerable<string> GetAllRoles()
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                return _memberGroupRepository.GetMany().Select(x => x.Name).Distinct();
            }
        }

        public IEnumerable<string> GetAllRoles(int memberId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                var result = _memberGroupRepository.GetMemberGroupsForMember(memberId);
                return result.Select(x => x.Name).Distinct();
            }
        }

        public IEnumerable<string> GetAllRoles(string username)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                var result = _memberGroupRepository.GetMemberGroupsForMember(username);
                return result.Select(x => x.Name).Distinct();
            }
        }

        public IEnumerable<int> GetAllRolesIds()
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                return _memberGroupRepository.GetMany().Select(x => x.Id).Distinct();
            }
        }

        public IEnumerable<int> GetAllRolesIds(int memberId)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                var result = _memberGroupRepository.GetMemberGroupsForMember(memberId);
                return result.Select(x => x.Id).Distinct();
            }
        }

        public IEnumerable<int> GetAllRolesIds(string username)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                var result = _memberGroupRepository.GetMemberGroupsForMember(username);
                return result.Select(x => x.Id).Distinct();
            }
        }
        
        public IEnumerable<IMember> GetMembersInRole(string roleName)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                return _memberRepository.GetByMemberGroup(roleName);
            }
        }

        public IEnumerable<IMember> FindMembersInRole(string roleName, string usernameToMatch, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                return _memberRepository.FindMembersInRole(roleName, usernameToMatch, matchType);
            }
        }

        public bool DeleteRole(string roleName, bool throwIfBeingUsed)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.MemberTree);

                if (throwIfBeingUsed)
                {
                    // get members in role
                    var membersInRole = _memberRepository.GetByMemberGroup(roleName);
                    if (membersInRole.Any())
                        throw new InvalidOperationException("The role " + roleName + " is currently assigned to members");
                }

                var query = Query<IMemberGroup>().Where(g => g.Name == roleName);
                var found = _memberGroupRepository.Get(query).ToArray();

                foreach (var memberGroup in found)
                    _memberGroupService.Delete(memberGroup);

                scope.Complete();
                return found.Length > 0;
            }
        }

        public void AssignRole(string username, string roleName)
        {
            AssignRoles(new[] { username }, new[] { roleName });
        }

        public void AssignRoles(string[] usernames, string[] roleNames)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.MemberTree);
                var ids = _memberGroupRepository.GetMemberIds(usernames);
                _memberGroupRepository.AssignRoles(ids, roleNames);
                scope.Events.Dispatch(AssignedRoles, this, new RolesEventArgs(ids, roleNames), nameof(AssignedRoles));
                scope.Complete();
            }
        }

        public void DissociateRole(string username, string roleName)
        {
            DissociateRoles(new[] { username }, new[] { roleName });
        }

        public void DissociateRoles(string[] usernames, string[] roleNames)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.MemberTree);
                var ids = _memberGroupRepository.GetMemberIds(usernames);
                _memberGroupRepository.DissociateRoles(ids, roleNames);
                scope.Events.Dispatch(RemovedRoles, this, new RolesEventArgs(ids, roleNames), nameof(RemovedRoles));
                scope.Complete();
            }
        }

        public void AssignRole(int memberId, string roleName)
        {
            AssignRoles(new[] { memberId }, new[] { roleName });
        }

        public void AssignRoles(int[] memberIds, string[] roleNames)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.MemberTree);
                _memberGroupRepository.AssignRoles(memberIds, roleNames);
                scope.Events.Dispatch(AssignedRoles, this, new RolesEventArgs(memberIds, roleNames), nameof(AssignedRoles));
                scope.Complete();
            }
        }

        public void DissociateRole(int memberId, string roleName)
        {
            DissociateRoles(new[] { memberId }, new[] { roleName });
        }

        public void DissociateRoles(int[] memberIds, string[] roleNames)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.MemberTree);
                _memberGroupRepository.DissociateRoles(memberIds, roleNames);
                scope.Events.Dispatch(RemovedRoles, this, new RolesEventArgs(memberIds, roleNames), nameof(RemovedRoles));
                scope.Complete();
            }
        }

        #endregion

        #region Private Methods

        private void Audit(AuditType type, int userId, int objectId, string message = null)
        {
            _auditRepository.Save(new AuditItem(objectId, type, userId, ObjectTypes.GetName(UmbracoObjectTypes.Member), message));
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
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IMemberService, SaveEventArgs<IMember>> Saved;

        /// <summary>
        /// Occurs after roles have been assigned.
        /// </summary>
        public static event TypedEventHandler<IMemberService, RolesEventArgs> AssignedRoles;

        /// <summary>
        /// Occurs after roles have been removed.
        /// </summary>
        public static event TypedEventHandler<IMemberService, RolesEventArgs> RemovedRoles;

        /// <summary>
        /// Occurs after members have been exported.
        /// </summary>
        internal static event TypedEventHandler<IMemberService, ExportedMemberEventArgs> Exported;

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
            if (member == null) throw new ArgumentNullException(nameof(member));

            var provider = MembershipProvider ?? MembershipProviderExtensions.GetMembersMembershipProvider();
            if (provider.IsUmbracoMembershipProvider())
                provider.ChangePassword(member.Username, "", password); // this is actually updating the password
            else
                throw new NotSupportedException("When using a non-Umbraco membership provider you must change the member password by using the MembershipProvider.ChangePassword method");

            // go re-fetch the member to update the properties that may have changed
            // check that it still exists (optimistic concurrency somehow)

            // re-fetch and ensure it exists
            var m = GetByUsername(member.Username);
            if (m == null) return; // gone

            // update properties that have changed
            member.RawPasswordValue = m.RawPasswordValue;
            member.LastPasswordChangeDate = m.LastPasswordChangeDate;
            member.UpdateDate = m.UpdateDate;

            // no need to save anything - provider.ChangePassword has done the updates,
            // and then all we do is re-fetch to get the updated values, and update the
            // in-memory member accordingly
        }

        /// <summary>
        /// A helper method that will create a basic/generic member for use with a generic membership provider
        /// </summary>
        /// <returns></returns>
        internal static IMember CreateGenericMembershipProviderMember(string name, string email, string username, string password)
        {
            var identity = int.MaxValue;

            var memType = new MemberType(-1);
            var propGroup = new PropertyGroup(MemberType.SupportsPublishingConst)
            {
                Alias = Constants.Conventions.Member.StandardPropertiesGroupAlias,
                Name = Constants.Conventions.Member.StandardPropertiesGroupName,
                Id = --identity,
                Key = identity.ToGuid()
            };
            propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, Constants.Conventions.Member.Comments)
            {
                Name = Constants.Conventions.Member.CommentsLabel,
                SortOrder = 0,
                Id = --identity,
                Key = identity.ToGuid()
            });
            propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.Aliases.Boolean, ValueStorageType.Integer, Constants.Conventions.Member.IsApproved)
            {
                Name = Constants.Conventions.Member.IsApprovedLabel,
                SortOrder = 3,
                Id = --identity,
                Key = identity.ToGuid()
            });
            propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.Aliases.Boolean, ValueStorageType.Integer, Constants.Conventions.Member.IsLockedOut)
            {
                Name = Constants.Conventions.Member.IsLockedOutLabel,
                SortOrder = 4,
                Id = --identity,
                Key = identity.ToGuid()
            });
            propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.Aliases.Label, ValueStorageType.Date, Constants.Conventions.Member.LastLockoutDate)
            {
                Name = Constants.Conventions.Member.LastLockoutDateLabel,
                SortOrder = 5,
                Id = --identity,
                Key = identity.ToGuid()
            });
            propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.Aliases.Label, ValueStorageType.Date, Constants.Conventions.Member.LastLoginDate)
            {
                Name = Constants.Conventions.Member.LastLoginDateLabel,
                SortOrder = 6,
                Id = --identity,
                Key = identity.ToGuid()
            });
            propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.Aliases.Label, ValueStorageType.Date, Constants.Conventions.Member.LastPasswordChangeDate)
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

        /// <summary>
        /// Exports a member.
        /// </summary>
        /// <remarks>
        /// This is internal for now and is used to export a member in the member editor,
        /// it will raise an event so that auditing logs can be created.
        /// </remarks>
        internal MemberExportModel ExportMember(Guid key)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IMember>().Where(x => x.Key == key);
                var member = _memberRepository.Get(query).FirstOrDefault();

                if (member == null) return null;

                var model = new MemberExportModel
                {
                    Id = member.Id,
                    Key = member.Key,
                    Name = member.Name,
                    Username = member.Username,
                    Email = member.Email,
                    Groups = GetAllRoles(member.Id).ToList(),
                    ContentTypeAlias = member.ContentTypeAlias,
                    CreateDate = member.CreateDate,
                    UpdateDate = member.UpdateDate,
                    Properties = new List<MemberExportProperty>(GetPropertyExportItems(member))
                };

                scope.Events.Dispatch(Exported, this, new ExportedMemberEventArgs(member, model));

                return model;
            }
        }

        private static IEnumerable<MemberExportProperty> GetPropertyExportItems(IMember member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));

            var exportProperties = new List<MemberExportProperty>();

            foreach (var property in member.Properties)
            {
                //ignore list
                switch (property.Alias)
                {
                    case Constants.Conventions.Member.PasswordQuestion:
                        continue;
                }

                var propertyExportModel = new MemberExportProperty
                {
                    Id = property.Id,
                    Alias = property.Alias,
                    Name = property.PropertyType.Name,
                    Value = property.GetValue(), // TODO: ignoring variants
                    CreateDate = property.CreateDate,
                    UpdateDate = property.UpdateDate
                };
                exportProperties.Add(propertyExportModel);
            }

            return exportProperties;
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

            using (var scope = ScopeProvider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.MemberTree);

                // TODO: What about content that has the contenttype as part of its composition?
                // TODO: What about content that has the contenttype as part of its composition?
                var query = Query<IMember>().Where(x => x.ContentTypeId == memberTypeId);

                var members = _memberRepository.Get(query).ToArray();
                var deleteEventArgs = new DeleteEventArgs<IMember>(members);

                if (scope.Events.DispatchCancelable(Deleting, this, deleteEventArgs))
                {
                    scope.Complete();
                    return;
                }

                foreach (var member in members)
                {
                    // delete media
                    // triggers the deleted event (and handles the files)
                    DeleteLocked(scope, member);
                }
                scope.Complete();
            }
        }

        private IMemberType GetMemberType(IScope scope, string memberTypeAlias)
        {
            if (memberTypeAlias == null) throw new ArgumentNullException(nameof(memberTypeAlias));
            if (string.IsNullOrWhiteSpace(memberTypeAlias)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(memberTypeAlias));

            scope.ReadLock(Constants.Locks.MemberTypes);

            var memberType = _memberTypeRepository.Get(memberTypeAlias);

            if (memberType == null)
                throw new Exception($"No MemberType matching the passed in Alias: '{memberTypeAlias}' was found"); // causes rollback

            return memberType;
        }

        private IMemberType GetMemberType(string memberTypeAlias)
        {
            if (memberTypeAlias == null) throw new ArgumentNullException(nameof(memberTypeAlias));
            if (string.IsNullOrWhiteSpace(memberTypeAlias)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(memberTypeAlias));

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return GetMemberType(scope, memberTypeAlias);
            }
        }

        public string GetDefaultMemberType()
        {
            return Current.Services.MemberTypeService.GetDefault();
        }

        #endregion
    }
}
