using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    public class MemberTypeService : ContentTypeServiceBase, IMemberTypeService
    {
        private readonly IMemberService _memberService;

        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        public MemberTypeService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger, IEventMessagesFactory eventMessagesFactory, IMemberService memberService)
            : base(provider, repositoryFactory, logger, eventMessagesFactory)
        {
            if (memberService == null) throw new ArgumentNullException("memberService");
            _memberService = memberService;
        }

        public IEnumerable<IMemberType> GetAll(params int[] ids)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMemberTypeRepository(uow);
                return repository.GetAll(ids);
            }
        }

        /// <summary>
        /// Gets an <see cref="IMemberType"/> object by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IMemberType"/> to retrieve</param>
        /// <returns><see cref="IMemberType"/></returns>
        public IMemberType Get(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMemberTypeRepository(uow);
                return repository.Get(id);
            }
        }

        /// <summary>
        /// Gets an <see cref="IMemberType"/> object by its Key
        /// </summary>
        /// <param name="key">Key of the <see cref="IMemberType"/> to retrieve</param>
        /// <returns><see cref="IMemberType"/></returns>
        public IMemberType Get(Guid key)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMemberTypeRepository(uow);
                return repository.Get(key);
            }
        }

        /// <summary>
        /// Gets an <see cref="IMemberType"/> object by its Alias
        /// </summary>
        /// <param name="alias">Alias of the <see cref="IMemberType"/> to retrieve</param>
        /// <returns><see cref="IMemberType"/></returns>
        public IMemberType Get(string alias)
        {
            using (var uow = UowProvider.GetUnitOfWork(readOnly: true))
            {
                var repository = RepositoryFactory.CreateMemberTypeRepository(uow);
                return repository.Get(alias);
            }
        }

        public void Save(IMemberType memberType, int userId = 0)
        {
            using (new WriteLock(Locker))
            {
                using (var uow = UowProvider.GetUnitOfWork())
                {
                    var saveEventArgs = new SaveEventArgs<IMemberType>(memberType);
                    if (uow.Events.DispatchCancelable(Saving, this, saveEventArgs))
                    {
                        uow.Commit();
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(memberType.Name))
                    {
                        throw new ArgumentException("Cannot save MemberType with empty name.");
                    }

                    var repository = RepositoryFactory.CreateMemberTypeRepository(uow);
                    memberType.CreatorId = userId;
                    if (memberType.Description == string.Empty)
                        memberType.Description = null;
                    repository.AddOrUpdate(memberType);
                    uow.Commit(); // flush, so that the db contains the saved value

                    UpdateContentXmlStructure(memberType);
                    uow.Commit(); // actually commit uow

                    saveEventArgs.CanCancel = false;
                    uow.Events.Dispatch(Saved, this, saveEventArgs);
                }
            }
        }

        public void Save(IEnumerable<IMemberType> memberTypes, int userId = 0)
        {
            var asArray = memberTypes.ToArray();

            using (new WriteLock(Locker))
            {
                using (var uow = UowProvider.GetUnitOfWork())
                {
                    var saveEventArgs = new SaveEventArgs<IMemberType>(asArray);
                    if (uow.Events.DispatchCancelable(Saving, this, saveEventArgs))
                    {
                        uow.Commit();
                        return;
                    }
                    var repository = RepositoryFactory.CreateMemberTypeRepository(uow);
                    foreach (var memberType in asArray)
                    {
                        memberType.CreatorId = userId;
                        if (memberType.Description == string.Empty)
                            memberType.Description = null;
                        repository.AddOrUpdate(memberType);
                    }
                    uow.Commit(); // flush, so that the db contains the saved values

                    UpdateContentXmlStructure(asArray.Cast<IContentTypeBase>().ToArray());
                    uow.Commit(); // actually commit uow
                    saveEventArgs.CanCancel = false;
                    uow.Events.Dispatch(Saved, this, saveEventArgs);
                }
            }

        }

        public void Delete(IMemberType memberType, int userId = 0)
        {
            using (new WriteLock(Locker))
            {
                var deleteEventArgs = new DeleteEventArgs<IMemberType>(memberType);
                using (var scope = UowProvider.ScopeProvider.CreateScope())
                {
                    scope.Complete(); // always                    
                    if (scope.Events.DispatchCancelable(Deleting, this, deleteEventArgs))
                        return;
                }

                _memberService.DeleteMembersOfType(memberType.Id);

                using (var uow = UowProvider.GetUnitOfWork())
                {
                    var repository = RepositoryFactory.CreateMemberTypeRepository(uow);
                    repository.Delete(memberType);
                    uow.Commit();

                    deleteEventArgs.CanCancel = false;
                    uow.Events.Dispatch(Deleted, this, deleteEventArgs);
                }
            }
        }

        public void Delete(IEnumerable<IMemberType> memberTypes, int userId = 0)
        {
            var asArray = memberTypes.ToArray();

            using (new WriteLock(Locker))
            {
                var deleteEventArgs = new DeleteEventArgs<IMemberType>(asArray);
                using (var scope = UowProvider.ScopeProvider.CreateScope())
                {
                    scope.Complete(); // always                    
                    if (scope.Events.DispatchCancelable(Deleting, this, deleteEventArgs))
                        return;
                }

                foreach (var contentType in asArray)
                {
                    _memberService.DeleteMembersOfType(contentType.Id);
                }

                using (var uow = UowProvider.GetUnitOfWork())
                {
                    var repository = RepositoryFactory.CreateMemberTypeRepository(uow);
                    foreach (var memberType in asArray)
                    {
                        repository.Delete(memberType);
                    }

                    uow.Commit();
                    deleteEventArgs.CanCancel = false;
                    uow.Events.Dispatch(Deleted, this, deleteEventArgs);
                }
            }
        }

        /// <summary>
        /// This is called after an IContentType is saved and is used to update the content xml structures in the database
        /// if they are required to be updated.
        /// </summary>
        /// <param name="contentTypes">A tuple of a content type and a boolean indicating if it is new (HasIdentity was false before committing)</param>
        private void UpdateContentXmlStructure(params IContentTypeBase[] contentTypes)
        {

            var toUpdate = GetContentTypesForXmlUpdates(contentTypes).ToArray();

            if (toUpdate.Any())
            {
                //if it is a media type then call the rebuilding methods for media
                var typedMemberService = _memberService as MemberService;
                if (typedMemberService != null)
                {
                    typedMemberService.RebuildXmlStructures(toUpdate.Select(x => x.Id).ToArray());
                }
            }

        }

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event TypedEventHandler<IMemberTypeService, SaveEventArgs<IMemberType>> Saving;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IMemberTypeService, SaveEventArgs<IMemberType>> Saved;

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IMemberTypeService, DeleteEventArgs<IMemberType>> Deleting;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IMemberTypeService, DeleteEventArgs<IMemberType>> Deleted;
    }
}