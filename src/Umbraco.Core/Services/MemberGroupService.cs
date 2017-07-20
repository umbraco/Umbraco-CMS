using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    public class MemberGroupService : RepositoryService, IMemberGroupService
    {
        public MemberGroupService(IScopeUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, logger, eventMessagesFactory)
        {
            //Proxy events!
            MemberGroupRepository.SavedMemberGroup += MemberGroupRepository_SavedMemberGroup;
            MemberGroupRepository.SavingMemberGroup += MemberGroupRepository_SavingMemberGroup;
        }

        #region Proxied event handlers

        void MemberGroupRepository_SavingMemberGroup(IMemberGroupRepository sender, SaveEventArgs<IMemberGroup> e)
        {
            // fixme - wtf?
            // why is the repository triggering these events?
            // and, the events are *dispatched* by the repository so it makes no sense dispatching them again!

            // v7.6
            //using (var scope = UowProvider.ScopeProvider.CreateScope())
            //{
            //    scope.Complete(); // always
            //    if (scope.Events.DispatchCancelable(Saving, this, new SaveEventArgs<IMemberGroup>(e.SavedEntities)))
            //        e.Cancel = true;
            //}

            // v8
            if (Saving.IsRaisedEventCancelled(new SaveEventArgs<IMemberGroup>(e.SavedEntities), this))
                e.Cancel = true;
        }

        void MemberGroupRepository_SavedMemberGroup(IMemberGroupRepository sender, SaveEventArgs<IMemberGroup> e)
        {
            // same as above!

            Saved.RaiseEvent(new SaveEventArgs<IMemberGroup>(e.SavedEntities, false), this);
        }

        #endregion

        public IEnumerable<IMemberGroup> GetAll()
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IMemberGroupRepository>();
                return repository.GetAll();
            }
        }

        public IMemberGroup GetById(int id)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IMemberGroupRepository>();
                return repository.Get(id);
            }
        }

        public IMemberGroup GetByName(string name)
        {
            using (var uow = UowProvider.CreateUnitOfWork(readOnly: true))
            {
                var repository = uow.CreateRepository<IMemberGroupRepository>();
                return repository.GetByName(name);
            }
        }

        public void Save(IMemberGroup memberGroup, bool raiseEvents = true)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                if (raiseEvents && uow.Events.DispatchCancelable(Saving, this, new SaveEventArgs<IMemberGroup>(memberGroup)))
                {
                    uow.Complete();
                    return;
                }

                var repository = uow.CreateRepository<IMemberGroupRepository>();
                repository.AddOrUpdate(memberGroup);
                uow.Complete();

                if (raiseEvents)
                    uow.Events.Dispatch(Saved, this, new SaveEventArgs<IMemberGroup>(memberGroup, false));
            }
        }

        public void Delete(IMemberGroup memberGroup)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                if (uow.Events.DispatchCancelable(Deleting, this, new DeleteEventArgs<IMemberGroup>(memberGroup)))
                {
                    uow.Complete();
                    return;
                }

                var repository = uow.CreateRepository<IMemberGroupRepository>();
                repository.Delete(memberGroup);
                uow.Complete();

                uow.Events.Dispatch(Deleted, this, new DeleteEventArgs<IMemberGroup>(memberGroup, false));
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
