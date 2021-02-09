using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services.Implement
{
    public class MemberTypeService : ContentTypeServiceBase<IMemberTypeRepository, IMemberType, IMemberTypeService>, IMemberTypeService
    {
        private readonly IMemberTypeRepository _memberTypeRepository;

        public MemberTypeService(IScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory, IMemberService memberService,
            IMemberTypeRepository memberTypeRepository, IAuditRepository auditRepository, IEntityRepository entityRepository)
            : base(provider, loggerFactory, eventMessagesFactory, memberTypeRepository, auditRepository, null, entityRepository)
        {
            MemberService = memberService;
            _memberTypeRepository = memberTypeRepository;
        }

        protected override IMemberTypeService This => this;

        // beware! order is important to avoid deadlocks
        protected override int[] ReadLockIds { get; } = { Cms.Core.Constants.Locks.MemberTypes };
        protected override int[] WriteLockIds { get; } = { Cms.Core.Constants.Locks.MemberTree, Cms.Core.Constants.Locks.MemberTypes };

        private IMemberService MemberService { get; }

        protected override Guid ContainedObjectType => Cms.Core.Constants.ObjectTypes.MemberType;

        protected override void DeleteItemsOfTypes(IEnumerable<int> typeIds)
        {
            foreach (var typeId in typeIds)
                MemberService.DeleteMembersOfType(typeId);
        }

        public string GetDefault()
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                scope.ReadLock(ReadLockIds);

                using (var e = _memberTypeRepository.GetMany(new int[0]).GetEnumerator())
                {
                    if (e.MoveNext() == false)
                        throw new InvalidOperationException("No member types could be resolved");
                    var first = e.Current.Alias;
                    var current = true;
                    while (e.Current.Alias.InvariantEquals("Member") == false && (current = e.MoveNext()))
                    { }
                    return current ? e.Current.Alias : first;
                }
            }
        }
    }
}
