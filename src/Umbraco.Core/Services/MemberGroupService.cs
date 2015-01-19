using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    public class MemberGroupService : RepositoryService, IMemberGroupService
    {

        [Obsolete("Use the constructors that specify all dependencies instead")]
        public MemberGroupService(RepositoryFactory repositoryFactory)
            : this(new PetaPocoUnitOfWorkProvider(), repositoryFactory)
        {
        }

        [Obsolete("Use the constructors that specify all dependencies instead")]
        public MemberGroupService(IDatabaseUnitOfWorkProvider provider)
            : this(provider, new RepositoryFactory())
        {
        }

        [Obsolete("Use the constructors that specify all dependencies instead")]
        public MemberGroupService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory)
            : this(provider, repositoryFactory, LoggerResolver.Current.Logger)
        {            
        }

        public MemberGroupService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger)
            : base(provider, repositoryFactory, logger)
        {
            //Proxy events!
            MemberGroupRepository.SavedMemberGroup += MemberGroupRepository_SavedMemberGroup;
            MemberGroupRepository.SavingMemberGroup += MemberGroupRepository_SavingMemberGroup;
        }

        #region Proxied event handlers
        void MemberGroupRepository_SavingMemberGroup(IMemberGroupRepository sender, SaveEventArgs<IMemberGroup> e)
        {
            if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IMemberGroup>(e.SavedEntities), this))
            {
                e.Cancel = true;
            }
        }

        void MemberGroupRepository_SavedMemberGroup(IMemberGroupRepository sender, SaveEventArgs<IMemberGroup> e)
        {
            Saved.RaiseEvent(new SaveEventArgs<IMemberGroup>(e.SavedEntities, false), this);
        } 
        #endregion

        public IEnumerable<IMemberGroup> GetAll()
        {
            using (var repository = RepositoryFactory.CreateMemberGroupRepository(UowProvider.GetUnitOfWork()))
            {
                return repository.GetAll();
            }
        }

        public IMemberGroup GetById(int id)
        {
            using (var repository = RepositoryFactory.CreateMemberGroupRepository(UowProvider.GetUnitOfWork()))
            {
                return repository.Get(id);
            }
        }

        public IMemberGroup GetByName(string name)
        {
            using (var repository = RepositoryFactory.CreateMemberGroupRepository(UowProvider.GetUnitOfWork()))
            {
                return repository.GetByName(name);
            }
        }

        public void Save(IMemberGroup memberGroup, bool raiseEvents = true)
        {
            if (raiseEvents)
            {
                if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IMemberGroup>(memberGroup), this))
                {
                    return;
                }
            }

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateMemberGroupRepository(uow))
            {
                repository.AddOrUpdate(memberGroup);
                uow.Commit();
            }

            if (raiseEvents)
                Saved.RaiseEvent(new SaveEventArgs<IMemberGroup>(memberGroup, false), this);
        }

        public void Delete(IMemberGroup memberGroup)
        {
            if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IMemberGroup>(memberGroup), this))
                return;

            var uow = UowProvider.GetUnitOfWork();
            using (var repository = RepositoryFactory.CreateMemberGroupRepository(uow))
            {
                repository.Delete(memberGroup);
                uow.Commit();
            }

            Deleted.RaiseEvent(new DeleteEventArgs<IMemberGroup>(memberGroup, false), this);
        }

        /// <summary>
        /// Occurs before Delete of a member group
        /// </summary>
        public static event TypedEventHandler<IMemberGroupService, DeleteEventArgs<IMemberGroup>> Deleting;

        /// <summary>
        /// Occurs after Delete of a member group
        /// </summary>
        public static event TypedEventHandler<IMemberGroupService, DeleteEventArgs<IMemberGroup>> Deleted;

        /// <summary>
        /// Occurs before Save of a member group
        /// </summary>     
        /// <remarks>
        /// We need to proxy these events because the events need to take place at the repo level
        /// </remarks>
        public static event TypedEventHandler<IMemberGroupService, SaveEventArgs<IMemberGroup>> Saving;

        /// <summary>
        /// Occurs after Save of a member group
        /// </summary>
        /// <remarks>
        /// We need to proxy these events because the events need to take place at the repo level
        /// </remarks>
        public static event TypedEventHandler<IMemberGroupService, SaveEventArgs<IMemberGroup>> Saved;
    }
}