using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Umbraco.Core.Auditing;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    public class MemberTypeService : ContentTypeServiceBase, IMemberTypeService
    {
        private readonly IMemberService _memberService;

        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        [Obsolete("Use the constructors that specify all dependencies instead")]
        public MemberTypeService(IMemberService memberService)
            : this(new PetaPocoUnitOfWorkProvider(), new RepositoryFactory(), memberService)
        {}

        [Obsolete("Use the constructors that specify all dependencies instead")]
        public MemberTypeService(RepositoryFactory repositoryFactory, IMemberService memberService)
            : this(new PetaPocoUnitOfWorkProvider(), repositoryFactory, memberService)
        { }

        [Obsolete("Use the constructors that specify all dependencies instead")]
        public MemberTypeService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, IMemberService memberService)
            : this(provider, repositoryFactory, LoggerResolver.Current.Logger, memberService)
        {
        }

        public MemberTypeService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger, IMemberService memberService)
            : base(provider, repositoryFactory, logger)
        {
            if (memberService == null) throw new ArgumentNullException("memberService");
            _memberService = memberService;
        }

        public IEnumerable<IMemberType> GetAll(params int[] ids)
        {
            using (var repository = RepositoryFactory.CreateMemberTypeRepository(UowProvider.GetUnitOfWork()))
            {
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
            using (var repository = RepositoryFactory.CreateMemberTypeRepository(UowProvider.GetUnitOfWork()))
            {
                return repository.Get(id);
            }
        }

        /// <summary>
        /// Gets an <see cref="IMemberType"/> object by its Alias
        /// </summary>
        /// <param name="alias">Alias of the <see cref="IMemberType"/> to retrieve</param>
        /// <returns><see cref="IMemberType"/></returns>
        public IMemberType Get(string alias)
        {
            using (var repository = RepositoryFactory.CreateMemberTypeRepository(UowProvider.GetUnitOfWork()))
            {
                var query = Query<IMemberType>.Builder.Where(x => x.Alias == alias);
                var contentTypes = repository.GetByQuery(query);

                return contentTypes.FirstOrDefault();
            }
        }

        public void Save(IMemberType memberType, int userId = 0)
        {
            if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IMemberType>(memberType), this))
                return;

            using (new WriteLock(Locker))
            {
                var uow = UowProvider.GetUnitOfWork();
                using (var repository = RepositoryFactory.CreateMemberTypeRepository(uow))
                {
                    memberType.CreatorId = userId;
                    repository.AddOrUpdate(memberType);

                    uow.Commit();
                }

                UpdateContentXmlStructure(memberType);
            }
            Saved.RaiseEvent(new SaveEventArgs<IMemberType>(memberType, false), this);
        }

        public void Save(IEnumerable<IMemberType> memberTypes, int userId = 0)
        {
            var asArray = memberTypes.ToArray();

            if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IMemberType>(asArray), this))
                return;

            using (new WriteLock(Locker))
            {
                var uow = UowProvider.GetUnitOfWork();
                using (var repository = RepositoryFactory.CreateMemberTypeRepository(uow))
                {
                    foreach (var memberType in asArray)
                    {
                        memberType.CreatorId = userId;
                        repository.AddOrUpdate(memberType);
                    }

                    //save it all in one go
                    uow.Commit();
                }

                UpdateContentXmlStructure(asArray.Cast<IContentTypeBase>().ToArray());
            }
            Saved.RaiseEvent(new SaveEventArgs<IMemberType>(asArray, false), this);
        }

        public void Delete(IMemberType memberType, int userId = 0)
        {
            if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IMemberType>(memberType), this))
                return;

            using (new WriteLock(Locker))
            {
                _memberService.DeleteMembersOfType(memberType.Id);

                var uow = UowProvider.GetUnitOfWork();
                using (var repository = RepositoryFactory.CreateMemberTypeRepository(uow))
                {
                    repository.Delete(memberType);
                    uow.Commit();

                    Deleted.RaiseEvent(new DeleteEventArgs<IMemberType>(memberType, false), this);
                }
            }
        }

        public void Delete(IEnumerable<IMemberType> memberTypes, int userId = 0)
        {
            var asArray = memberTypes.ToArray();

            if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IMemberType>(asArray), this))
                return;

            using (new WriteLock(Locker))
            {
                foreach (var contentType in asArray)
                {
                    _memberService.DeleteMembersOfType(contentType.Id);
                }

                var uow = UowProvider.GetUnitOfWork();
                using (var repository = RepositoryFactory.CreateMemberTypeRepository(uow))
                {
                    foreach (var memberType in asArray)
                    {
                        repository.Delete(memberType);
                    }

                    uow.Commit();

                    Deleted.RaiseEvent(new DeleteEventArgs<IMemberType>(asArray, false), this);
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