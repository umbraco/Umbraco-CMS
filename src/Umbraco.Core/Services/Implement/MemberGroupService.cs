using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services.Implement
{
    public class MemberGroupService : RepositoryService, IMemberGroupService
    {
        private readonly IMemberGroupRepository _memberGroupRepository;

        public MemberGroupService(IScopeProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory,
            IMemberGroupRepository memberGroupRepository)
            : base(provider, logger, eventMessagesFactory)
        {
            _memberGroupRepository = memberGroupRepository;
            //Proxy events!
            MemberGroupRepository.SavedMemberGroup += MemberGroupRepository_SavedMemberGroup;
            MemberGroupRepository.SavingMemberGroup += MemberGroupRepository_SavingMemberGroup;
        }

        #region Proxy event handlers

        void MemberGroupRepository_SavingMemberGroup(IMemberGroupRepository sender, SaveEventArgs<IMemberGroup> e)
        {
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
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _memberGroupRepository.GetMany();
            }
        }

        public IEnumerable<IMemberGroup> GetByIds(IEnumerable<int> ids)
        {
            if (ids == null || ids.Any() == false)
            {
                return new IMemberGroup[0];
            }

            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _memberGroupRepository.GetMany(ids.ToArray());
            }
        }

        public IMemberGroup GetById(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _memberGroupRepository.Get(id);
            }
        }

        public IMemberGroup GetById(Guid id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _memberGroupRepository.Get(id);
            }
        }

        public IMemberGroup GetByName(string name)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _memberGroupRepository.GetByName(name);
            }
        }

        public void Save(IMemberGroup memberGroup, bool raiseEvents = true)
        {
            if (string.IsNullOrWhiteSpace(memberGroup.Name))
            {
                throw new InvalidOperationException("The name of a MemberGroup can not be empty");
            }
            using (var scope = ScopeProvider.CreateScope())
            {
                var saveEventArgs = new SaveEventArgs<IMemberGroup>(memberGroup);
                if (raiseEvents && scope.Events.DispatchCancelable(Saving, this, saveEventArgs))
                {
                    scope.Complete();
                    return;
                }

                _memberGroupRepository.Save(memberGroup);
                scope.Complete();

                if (raiseEvents)
                {
                    saveEventArgs.CanCancel = false;
                    scope.Events.Dispatch(Saved, this, saveEventArgs);
                }
            }
        }

        public void Delete(IMemberGroup memberGroup)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var deleteEventArgs = new DeleteEventArgs<IMemberGroup>(memberGroup);
                if (scope.Events.DispatchCancelable(Deleting, this, deleteEventArgs))
                {
                    scope.Complete();
                    return;
                }

                _memberGroupRepository.Delete(memberGroup);
                scope.Complete();
                deleteEventArgs.CanCancel = false;
                scope.Events.Dispatch(Deleted, this, deleteEventArgs);
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
