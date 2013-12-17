using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Umbraco.Core.Auditing;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    internal class MemberTypeService : ContentTypeServiceBase, IMemberTypeService
    {
        private readonly IDatabaseUnitOfWorkProvider _uowProvider;
        private readonly RepositoryFactory _repositoryFactory;
        private readonly IMemberService _memberService;

        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        public MemberTypeService(IMemberService memberService)
            : this(new PetaPocoUnitOfWorkProvider(), new RepositoryFactory(), memberService)
        {}

        public MemberTypeService(RepositoryFactory repositoryFactory, IMemberService memberService)
            : this(new PetaPocoUnitOfWorkProvider(), repositoryFactory, memberService)
        { }

        public MemberTypeService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, IMemberService memberService)
        {
            if (provider == null) throw new ArgumentNullException("provider");
            if (repositoryFactory == null) throw new ArgumentNullException("repositoryFactory");
            _uowProvider = provider;
            _repositoryFactory = repositoryFactory;
            _memberService = memberService;
        }

        public IEnumerable<IMemberType> GetAllMemberTypes(params int[] ids)
        {
            using (var repository = _repositoryFactory.CreateMemberTypeRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.GetAll(ids);
            }
        }

        /// <summary>
        /// Gets an <see cref="IMemberType"/> object by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IMemberType"/> to retrieve</param>
        /// <returns><see cref="IMemberType"/></returns>
        public IMemberType GetMemberType(int id)
        {
            using (var repository = _repositoryFactory.CreateMemberTypeRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.Get(id);
            }
        }

        /// <summary>
        /// Gets an <see cref="IMemberType"/> object by its Alias
        /// </summary>
        /// <param name="alias">Alias of the <see cref="IMemberType"/> to retrieve</param>
        /// <returns><see cref="IMemberType"/></returns>
        public IMemberType GetMemberType(string alias)
        {
            using (var repository = _repositoryFactory.CreateMemberTypeRepository(_uowProvider.GetUnitOfWork()))
            {
                var query = Query<IMemberType>.Builder.Where(x => x.Alias == alias);
                var contentTypes = repository.GetByQuery(query);

                return contentTypes.FirstOrDefault();
            }
        }

        public void Save(IMemberType memberType, int userId = 0)
        {
            if (SavingMemberType.IsRaisedEventCancelled(new SaveEventArgs<IMemberType>(memberType), this))
                return;

            using (new WriteLock(Locker))
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateMemberTypeRepository(uow))
                {
                    memberType.CreatorId = userId;
                    repository.AddOrUpdate(memberType);

                    uow.Commit();
                }

                UpdateContentXmlStructure(memberType);
            }
            SavedMemberType.RaiseEvent(new SaveEventArgs<IMemberType>(memberType, false), this);
        }

        public void Save(IEnumerable<IMemberType> memberTypes, int userId = 0)
        {
            var asArray = memberTypes.ToArray();

            if (SavingMemberType.IsRaisedEventCancelled(new SaveEventArgs<IMemberType>(asArray), this))
                return;

            using (new WriteLock(Locker))
            {
                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateMemberTypeRepository(uow))
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
            SavedMemberType.RaiseEvent(new SaveEventArgs<IMemberType>(asArray, false), this);
        }

        public void Delete(IMemberType memberType, int userId = 0)
        {
            if (DeletingMemberType.IsRaisedEventCancelled(new DeleteEventArgs<IMemberType>(memberType), this))
                return;

            using (new WriteLock(Locker))
            {
                _memberService.DeleteMembersOfType(memberType.Id);

                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateMemberTypeRepository(uow))
                {
                    repository.Delete(memberType);
                    uow.Commit();

                    DeletedMemberType.RaiseEvent(new DeleteEventArgs<IMemberType>(memberType, false), this);
                }
            }
        }

        public void Delete(IEnumerable<IMemberType> memberTypes, int userId = 0)
        {
            var asArray = memberTypes.ToArray();

            if (DeletingMemberType.IsRaisedEventCancelled(new DeleteEventArgs<IMemberType>(asArray), this))
                return;

            using (new WriteLock(Locker))
            {
                foreach (var contentType in asArray)
                {
                    _memberService.DeleteMembersOfType(contentType.Id);
                }

                var uow = _uowProvider.GetUnitOfWork();
                using (var repository = _repositoryFactory.CreateMemberTypeRepository(uow))
                {
                    foreach (var memberType in asArray)
                    {
                        repository.Delete(memberType);
                    }

                    uow.Commit();

                    DeletedMemberType.RaiseEvent(new DeleteEventArgs<IMemberType>(asArray, false), this);
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
        public static event TypedEventHandler<IMemberTypeService, SaveEventArgs<IMemberType>> SavingMemberType;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event TypedEventHandler<IMemberTypeService, SaveEventArgs<IMemberType>> SavedMemberType;

        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event TypedEventHandler<IMemberTypeService, DeleteEventArgs<IMemberType>> DeletingMemberType;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event TypedEventHandler<IMemberTypeService, DeleteEventArgs<IMemberType>> DeletedMemberType;
    }
}