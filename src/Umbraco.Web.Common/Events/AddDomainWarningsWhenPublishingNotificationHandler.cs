using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Events;

public class AddDomainWarningsWhenPublishingNotificationHandler : INotificationHandler<ContentPublishedNotification>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptions<ContentSettings> _contentSettings;
    private readonly IContentService _contentService;
    private readonly IDomainService _domainService;
    private readonly IEventMessagesFactory _eventMessagesFactory;
    private readonly ILogger<AddDomainWarningsWhenPublishingNotificationHandler> _logger;

    public AddDomainWarningsWhenPublishingNotificationHandler(
        IHttpContextAccessor httpContextAccessor,
        IOptions<ContentSettings> contentSettings,
        IContentService contentService,
        IDomainService domainService,
        IEventMessagesFactory eventMessagesFactory,
        ILogger<AddDomainWarningsWhenPublishingNotificationHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _contentSettings = contentSettings;
        _contentService = contentService;
        _domainService = domainService;
        _eventMessagesFactory = eventMessagesFactory;
        _logger = logger;
    }

    public void Handle(ContentPublishedNotification notification)
    {
        if (_contentSettings.Value.ShowDomainWarnings is false)
        {
            return;
        }

        // Exit if not running in an HTTP context such as scheduled publishing, where there will be no editor to see the warnings.
        if (_httpContextAccessor.HttpContext is null)
        {
            return;
        }

        foreach (IContent content in notification.PublishedEntities)
        {
            var publishedCultures = GetPublishedCulturesFromAncestors(content).ToList();
            // If only a single culture is published we shouldn't have any routing issues
            if (publishedCultures.Count < 2)
            {
                return;
            }

            // If more than a single culture is published we need to verify that there's a domain registered for each published culture
            HashSet<IDomain>? assignedDomains = content is null
                ? null
                : _domainService.GetAssignedDomains(content.Id, true)?.ToHashSet();

            IEnumerable<int>? ancestorIds = content?.GetAncestorIds();
            if (ancestorIds is not null && assignedDomains is not null)
            {
                // We also have to check all of the ancestors, if any of those has the appropriate culture assigned we don't need to warn
                foreach (var ancestorID in ancestorIds)
                {
                    assignedDomains.UnionWith(_domainService.GetAssignedDomains(ancestorID, true) ??
                                              Enumerable.Empty<IDomain>());
                }
            }

            var eventMessages = _eventMessagesFactory.Get();
            // No domains at all, add a warning, to add domains.
            if (assignedDomains is null || assignedDomains.Count == 0)
            {

                eventMessages.Add(new EventMessage("Content published", $"Domains are not configured for multilingual site, please contact an administrator, see log for more information", EventMessageType.Warning));

                _logger.LogWarning(
                    "The root node {RootNodeName} was published with multiple cultures, but no domains are configured, this will cause routing and caching issues, please register domains for: {Cultures}",
                    content?.Name,
                    string.Join(", ", publishedCultures));
                return;

            }

            // If there is some domains, verify that there's a domain for each of the published cultures
            foreach (var culture in publishedCultures
                         .Where(culture => assignedDomains.Any(x =>
                             x.LanguageIsoCode?.Equals(culture, StringComparison.OrdinalIgnoreCase) ?? false) is false))
            {
                eventMessages.Add(new EventMessage("Content published", $"There is no domain configured for '{culture}', please contact an administrator, see\\n      log for more information", EventMessageType.Warning));

                _logger.LogWarning(
                    "The root node {RootNodeName} was published in culture {Culture}, but there's no domain configured for it, this will cause routing and caching issues, please register a domain for it",
                    content?.Name,
                    culture);
            }
        }
    }

    private IEnumerable<string> GetPublishedCulturesFromAncestors(IContent? content)
    {
        if (content?.ParentId is not -1 && content?.HasIdentity is false)
        {
            content = _contentService.GetById(content.ParentId);
        }

        if (content?.ParentId == -1)
        {
            return content.PublishedCultures;
        }

        HashSet<string> publishedCultures = new();
        publishedCultures.UnionWith(content?.PublishedCultures ?? Enumerable.Empty<string>());

        IEnumerable<int>? ancestorIds = content?.GetAncestorIds();

        if (ancestorIds is not null)
        {
            foreach (var id in ancestorIds)
            {
                IEnumerable<string>? cultures = _contentService.GetById(id)?.PublishedCultures;
                publishedCultures.UnionWith(cultures ?? Enumerable.Empty<string>());
            }
        }

        return publishedCultures;
    }
}
