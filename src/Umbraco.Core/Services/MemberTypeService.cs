using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    internal class MemberTypeService : ContentTypeServiceBase<IMemberTypeRepository, IMemberType, IMemberTypeService>, IMemberTypeService
    {
        public MemberTypeService(IDatabaseUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory, IMemberService memberService)
            : base(provider, logger, eventMessagesFactory)
        {
            MemberService = memberService;
        }

        // beware! order is important to avoid deadlocks
        protected override int[] ReadLockIds { get; } = { Constants.Locks.MemberTypes };
        protected override int[] WriteLockIds { get; } = { Constants.Locks.MemberTree, Constants.Locks.MemberTypes };

        private IMemberService MemberService { get; }

        protected override Guid ContainedObjectType => Constants.ObjectTypes.MemberTypeGuid;

        protected override void DeleteItemsOfTypes(IEnumerable<int> typeIds)
        {
            foreach (var typeId in typeIds)
                MemberService.DeleteMembersOfType(typeId);
        }

        protected override void UpdateContentXmlStructure(params IContentTypeBase[] contentTypes)
        {

            var toUpdate = GetContentTypesForXmlUpdates(contentTypes).ToArray();
            if (toUpdate.Any() == false) return;

            var memberService = MemberService as MemberService;
            memberService?.RebuildXmlStructures(toUpdate.Select(x => x.Id).ToArray());
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