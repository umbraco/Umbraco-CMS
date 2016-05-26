using System;
using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    internal class MemberTypeService : ContentTypeServiceBase<IMemberTypeRepository, IMemberType, IMemberTypeService>, IMemberTypeService
    {
        private IMemberService _memberService;

        public MemberTypeService(IDatabaseUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory, IMemberService memberService)
            : base(provider, logger, eventMessagesFactory)
        {
            _memberService = memberService;
        }

        protected override IMemberTypeService Instance => this;

        // beware! order is important to avoid deadlocks
        protected override int[] ReadLockIds { get; } = { Constants.Locks.MemberTypes };
        protected override int[] WriteLockIds { get; } = { Constants.Locks.MemberTree, Constants.Locks.MemberTypes };

        // don't remove, will need it later
        private IMemberService MemberService => _memberService;
        //// handle circular dependencies
        //internal IMemberService MemberService
        //{
        //    get
        //    {
        //        if (_memberService == null)
        //            throw new InvalidOperationException("MemberTypeService.MemberService has not been initialized.");
        //        return _memberService;
        //    }
        //    set { _memberService = value; }
        //}

        protected override Guid ContainedObjectType => Constants.ObjectTypes.MemberTypeGuid;

        protected override void DeleteItemsOfTypes(IEnumerable<int> typeIds)
        {
            foreach (var typeId in typeIds)
                MemberService.DeleteMembersOfType(typeId);
        }

        public string GetDefault()
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                uow.ReadLock(ReadLockIds);
                var repo = uow.CreateRepository<IMemberTypeRepository>();
                var e = repo.GetAll(new int[0]).GetEnumerator();
                if (e.MoveNext() == false)
                    throw new InvalidOperationException("No member types could be resolved");
                var first = e.Current.Alias;
                var current = true;
                while (e.Current.Alias.InvariantEquals("Member") == false && (current = e.MoveNext()))
                { }
                uow.Complete();
                return current ? e.Current.Alias : first;
            }
        }
    }
}