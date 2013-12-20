using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
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

        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

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
                    //NOTE What about content that has the contenttype as part of its composition?
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

        /// <summary>
        /// Does a search for members that contain the specified string in their email address
        /// </summary>
        /// <param name="emailStringToMatch"></param>
        /// <param name="matchType"></param>
        /// <returns></returns>
        public IEnumerable<IMember> FindMembersByEmail(string emailStringToMatch, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith)
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

                return repository.GetByQuery(query);
            }
        }

        public IEnumerable<IMember> FindMembersByUsername(string login, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith)
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

                return repository.GetByQuery(query);
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
                                ((Member) x).PropertyTypeAlias == propertyTypeAlias &&
                                (((Member)x).LongStringPropertyValue.SqlEquals(value, TextColumnType.NText) ||
                                 ((Member)x).ShortStringPropertyValue.SqlEquals(value, TextColumnType.NVarchar)));
                        break;
                    case StringPropertyMatchType.Contains:
                        query =
                            Query<IMember>.Builder.Where(
                                x =>
                                ((Member) x).PropertyTypeAlias == propertyTypeAlias &&
                                (((Member)x).LongStringPropertyValue.SqlContains(value, TextColumnType.NText) ||
                                 ((Member)x).ShortStringPropertyValue.SqlContains(value, TextColumnType.NVarchar)));
                        break;
                    case StringPropertyMatchType.StartsWith:
                        query =
                            Query<IMember>.Builder.Where(
                                x =>
                                ((Member) x).PropertyTypeAlias == propertyTypeAlias &&
                                (((Member)x).LongStringPropertyValue.SqlStartsWith(value, TextColumnType.NText) ||
                                 ((Member)x).ShortStringPropertyValue.SqlStartsWith(value, TextColumnType.NVarchar)));
                        break;
                    case StringPropertyMatchType.EndsWith:
                        query =
                            Query<IMember>.Builder.Where(
                                x =>
                                ((Member) x).PropertyTypeAlias == propertyTypeAlias &&
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
        /// <param name="matchType"></param>
        /// <returns></returns>
        public IEnumerable<IMember> GetMembersByPropertyValue(string propertyTypeAlias, DateTime value, ValuePropertyMatchType matchType = ValuePropertyMatchType.Exact)
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
        /// Returns the count of members based on the countType
        /// </summary>
        /// <param name="countType"></param>
        /// <returns></returns>
        /// <remarks>
        /// The way the Online count is done is the same way that it is done in the MS SqlMembershipProvider - We query for any members
        /// that have their last active date within the Membership.UserIsOnlineTimeWindow (which is in minutes). It isn't exact science
        /// but that is how MS have made theirs so we'll follow that principal.
        /// </remarks>
        public int GetMemberCount(MemberCountType countType)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IMember> GetAllMembers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates and persists a new Member
        /// </summary>
        /// <param name="email"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="memberTypeAlias"></param>
        /// <returns></returns>
        public IMember CreateMember(string email, string username, string password, string memberTypeAlias)
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
        /// <param name="member"></param>
        /// <param name="raiseEvents"></param>
        public void Save(IMember member, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IMember>(member), this))
                    return;
            }

            var uow = _uowProvider.GetUnitOfWork();
            using (var repository = _repositoryFactory.CreateMemberRepository(uow))
            {
                repository.AddOrUpdate(member);
                uow.Commit();

                var xml = member.ToXml();
                CreateAndSaveMemberXml(xml, member.Id, uow.Database);
            }

            if (raiseEvents)
                Saved.RaiseEvent(new SaveEventArgs<IMember>(member, false), this);
        }

        public void Save(IEnumerable<IMember> members, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IMember>(members), this))
                    return;
            }
            using (new WriteLock(Locker))
            {                
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateMemberRepository(uow))
                {
                    foreach (var member in members)
                    {
                        repository.AddOrUpdate(member);
                    }

                    //commit the whole lot in one go
                    uow.Commit();

                    foreach (var member in members)
                    {
                        CreateAndSaveMemberXml(member.ToXml(), member.Id, uow.Database);
                    }
                }

                if (raiseEvents)
                    Saved.RaiseEvent(new SaveEventArgs<IMember>(members, false), this);
            }
        }

        #endregion

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
                        //Remove all media records from the cmsContentXml table (DO NOT REMOVE Content/Members!)
                        uow.Database.Execute(@"DELETE FROM cmsContentXml WHERE nodeId IN
                                                (SELECT DISTINCT cmsContentXml.nodeId FROM cmsContentXml 
                                                    INNER JOIN umbracoNode ON cmsContentXml.nodeId = umbracoNode.id
                                                    WHERE nodeObjectType = @nodeObjectType)",
                                             new { nodeObjectType = Constants.ObjectTypes.Member });
                    }
                    else
                    {
                        foreach (var id in memberTypeIds)
                        {
                            //first we'll clear out the data from the cmsContentXml table for this type
                            uow.Database.Execute(@"delete from cmsContentXml where nodeId in 
                                (SELECT DISTINCT cmsContentXml.nodeId FROM cmsContentXml 
                                INNER JOIN umbracoNode ON cmsContentXml.nodeId = umbracoNode.id
                                INNER JOIN cmsContent ON cmsContent.nodeId = umbracoNode.id
                                WHERE nodeObjectType = @nodeObjectType AND cmsContent.contentType = @contentTypeId)",
                                                 new { contentTypeId = id, nodeObjectType = Constants.ObjectTypes.Member });
                        }
                    }

                    //bulk insert it into the database
                    uow.Database.BulkInsertRecords(xmlItems, tr);
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
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IMemberService, SaveEventArgs<IMember>> Saved;

        #endregion

        ///// <summary>
        ///// A helper method that will create a basic/generic member for use with a generic membership provider
        ///// </summary>
        ///// <returns></returns>
        //internal static IMember CreateGenericMembershipProviderMember(string name, string email, string username, string password)
        //{
        //    var identity = int.MaxValue;

        //    var memType = new MemberType(-1);
        //    var propGroup = new PropertyGroup
        //    {
        //        Name = "Membership",
        //        Id = --identity
        //    };
        //    propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext)
        //    {
        //        Alias = Constants.Conventions.Member.Comments,
        //        Name = Constants.Conventions.Member.CommentsLabel,
        //        SortOrder = 0,
        //        Id = --identity
        //    });
        //    propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.TrueFalseAlias, DataTypeDatabaseType.Integer)
        //    {
        //        Alias = Constants.Conventions.Member.IsApproved,
        //        Name = Constants.Conventions.Member.IsApprovedLabel,
        //        SortOrder = 3,
        //        Id = --identity
        //    });
        //    propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.TrueFalseAlias, DataTypeDatabaseType.Integer)
        //    {
        //        Alias = Constants.Conventions.Member.IsLockedOut,
        //        Name = Constants.Conventions.Member.IsLockedOutLabel,
        //        SortOrder = 4,
        //        Id = --identity
        //    });
        //    propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.NoEditAlias, DataTypeDatabaseType.Date)
        //    {
        //        Alias = Constants.Conventions.Member.LastLockoutDate,
        //        Name = Constants.Conventions.Member.LastLockoutDateLabel,
        //        SortOrder = 5,
        //        Id = --identity
        //    });
        //    propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.NoEditAlias, DataTypeDatabaseType.Date)
        //    {
        //        Alias = Constants.Conventions.Member.LastLoginDate,
        //        Name = Constants.Conventions.Member.LastLoginDateLabel,
        //        SortOrder = 6,
        //        Id = --identity
        //    });
        //    propGroup.PropertyTypes.Add(new PropertyType(Constants.PropertyEditors.NoEditAlias, DataTypeDatabaseType.Date)
        //    {
        //        Alias = Constants.Conventions.Member.LastPasswordChangeDate,
        //        Name = Constants.Conventions.Member.LastPasswordChangeDateLabel,
        //        SortOrder = 7,
        //        Id = --identity
        //    });

        //    memType.PropertyGroups.Add(propGroup);

        //    return new Member(name, email, username, password, -1, memType);
        //}
    }
}