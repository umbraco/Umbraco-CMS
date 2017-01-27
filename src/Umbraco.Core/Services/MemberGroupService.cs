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
    public class MemberGroupService : ScopeRepositoryService, IMemberGroupService
    {

        public MemberGroupService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, repositoryFactory, logger, eventMessagesFactory)
        {
            //Proxy events!
            MemberGroupRepository.SavedMemberGroup += MemberGroupRepository_SavedMemberGroup;
            MemberGroupRepository.SavingMemberGroup += MemberGroupRepository_SavingMemberGroup;
        }

        #region Proxied event handlers
        void MemberGroupRepository_SavingMemberGroup(IMemberGroupRepository sender, SaveEventArgs<IMemberGroup> e)
        {
            if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IMemberGroup>(e.SavedEntities), this, UowProvider))
            {
                e.Cancel = true;
            }
        }

        void MemberGroupRepository_SavedMemberGroup(IMemberGroupRepository sender, SaveEventArgs<IMemberGroup> e)
        {
            using (var scope = UowProvider.ScopeProvider.CreateScope())
            {
                scope.Complete(); // always complete
                Saved.RaiseEvent(new SaveEventArgs<IMemberGroup>(e.SavedEntities, false), this, scope.Events);
            }
        } 
        #endregion

        public IEnumerable<IMemberGroup> GetAll()
        {
            using (var uow = UowProvider.GetUnitOfWork(commit: true))
            {
                var repository = RepositoryFactory.CreateMemberGroupRepository(uow);
                return repository.GetAll();
            }
        }

        public IMemberGroup GetById(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork(commit: true))
            {
                var repository = RepositoryFactory.CreateMemberGroupRepository(uow);
                return repository.Get(id);
            }
        }

        public IMemberGroup GetByName(string name)
        {
            using (var uow = UowProvider.GetUnitOfWork(commit: true))
            {
                var repository = RepositoryFactory.CreateMemberGroupRepository(uow);
                return repository.GetByName(name);
            }
        }

        public void Save(IMemberGroup memberGroup, bool raiseEvents = true)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                if (raiseEvents)
                {
                    if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IMemberGroup>(memberGroup), this, uow.Events))
                    {
                        uow.Commit();
                        return;
                    }
                }

                var repository = RepositoryFactory.CreateMemberGroupRepository(uow);
                repository.AddOrUpdate(memberGroup);
                uow.Commit();
                if (raiseEvents)
                    Saved.RaiseEvent(new SaveEventArgs<IMemberGroup>(memberGroup, false), this, uow.Events);
            }

            
        }

        public void Delete(IMemberGroup memberGroup)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                if (Deleting.IsRaisedEventCancelled(new DeleteEventArgs<IMemberGroup>(memberGroup), this, uow.Events))
                    return; // FIXME COMMIT
                var repository = RepositoryFactory.CreateMemberGroupRepository(uow);
                repository.Delete(memberGroup);
                uow.Commit();
                Deleted.RaiseEvent(new DeleteEventArgs<IMemberGroup>(memberGroup, false), this, uow.Events);
            }
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