using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

internal class MemberGroupService : RepositoryService, IMemberGroupService
{
    private readonly IMemberGroupRepository _memberGroupRepository;

    public MemberGroupService(ICoreScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory, IMemberGroupRepository memberGroupRepository)
        : base(provider, loggerFactory, eventMessagesFactory) =>
        _memberGroupRepository = memberGroupRepository;

    public IEnumerable<IMemberGroup> GetAll()
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
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

        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _memberGroupRepository.GetMany(ids.ToArray());
        }
    }

    public IMemberGroup? GetById(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _memberGroupRepository.Get(id);
        }
    }

    public IMemberGroup? GetById(Guid id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _memberGroupRepository.Get(id);
        }
    }

    public IMemberGroup? GetByName(string? name)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _memberGroupRepository.GetByName(name);
        }
    }

    public void Save(IMemberGroup memberGroup)
    {
        if (string.IsNullOrWhiteSpace(memberGroup.Name))
        {
            throw new InvalidOperationException("The name of a MemberGroup can not be empty");
        }

        EventMessages evtMsgs = EventMessagesFactory.Get();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var savingNotification = new MemberGroupSavingNotification(memberGroup, evtMsgs);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return;
            }

            _memberGroupRepository.Save(memberGroup);
            scope.Complete();

            scope.Notifications.Publish(
                new MemberGroupSavedNotification(memberGroup, evtMsgs).WithStateFrom(savingNotification));
        }
    }

    public void Delete(IMemberGroup memberGroup)
    {
        EventMessages evtMsgs = EventMessagesFactory.Get();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var deletingNotification = new MemberGroupDeletingNotification(memberGroup, evtMsgs);
            if (scope.Notifications.PublishCancelable(deletingNotification))
            {
                scope.Complete();
                return;
            }

            _memberGroupRepository.Delete(memberGroup);
            scope.Complete();

            scope.Notifications.Publish(
                new MemberGroupDeletedNotification(memberGroup, evtMsgs).WithStateFrom(deletingNotification));
        }
    }
}
