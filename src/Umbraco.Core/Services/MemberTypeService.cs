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
        private IMemberService _memberService;

        public MemberTypeService(IDatabaseUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory, IMemberService memberService)
            : base(provider, logger, eventMessagesFactory)
        {
            _memberService = memberService;
        }

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

        protected override void UpdateContentXmlStructure(params IContentTypeBase[] contentTypes)
        {

            var toUpdate = GetContentTypesForXmlUpdates(contentTypes).ToArray();
            if (toUpdate.Any() == false) return;

            var memberService = _memberService as MemberService;
            memberService?.RebuildXmlStructures(toUpdate.Select(x => x.Id).ToArray());
        }
    }
}