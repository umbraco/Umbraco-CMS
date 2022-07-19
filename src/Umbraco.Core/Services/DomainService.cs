using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

public class DomainService : RepositoryService, IDomainService
{
    private readonly IDomainRepository _domainRepository;

    public DomainService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IDomainRepository domainRepository)
        : base(provider, loggerFactory, eventMessagesFactory) =>
        _domainRepository = domainRepository;

    public bool Exists(string domainName)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _domainRepository.Exists(domainName);
        }
    }

    public Attempt<OperationResult?> Delete(IDomain domain)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var deletingNotification = new DomainDeletingNotification(domain, eventMessages);
            if (scope.Notifications.PublishCancelable(deletingNotification))
            {
                scope.Complete();
                return OperationResult.Attempt.Cancel(eventMessages);
            }

            _domainRepository.Delete(domain);
            scope.Complete();

            scope.Notifications.Publish(
                new DomainDeletedNotification(domain, eventMessages).WithStateFrom(deletingNotification));
        }

        return OperationResult.Attempt.Succeed(eventMessages);
    }

    public IDomain? GetByName(string name)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _domainRepository.GetByName(name);
        }
    }

    public IDomain? GetById(int id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _domainRepository.Get(id);
        }
    }

    public IEnumerable<IDomain> GetAll(bool includeWildcards)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _domainRepository.GetAll(includeWildcards);
        }
    }

    public IEnumerable<IDomain> GetAssignedDomains(int contentId, bool includeWildcards)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _domainRepository.GetAssignedDomains(contentId, includeWildcards);
        }
    }

    public Attempt<OperationResult?> Save(IDomain domainEntity)
    {
        EventMessages eventMessages = EventMessagesFactory.Get();

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            var savingNotification = new DomainSavingNotification(domainEntity, eventMessages);
            if (scope.Notifications.PublishCancelable(savingNotification))
            {
                scope.Complete();
                return OperationResult.Attempt.Cancel(eventMessages);
            }

            _domainRepository.Save(domainEntity);
            scope.Complete();
            scope.Notifications.Publish(
                new DomainSavedNotification(domainEntity, eventMessages).WithStateFrom(savingNotification));
        }

        return OperationResult.Attempt.Succeed(eventMessages);
    }
}
