using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class DocumentNotificationPresentationFactory : IDocumentNotificationPresentationFactory
{
    private readonly ActionCollection _actionCollection;
    private readonly INotificationService _notificationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public DocumentNotificationPresentationFactory(ActionCollection actionCollection, INotificationService notificationService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _actionCollection = actionCollection;
        _notificationService = notificationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    public async Task<IEnumerable<DocumentNotificationResponseModel>> CreateNotificationModelsAsync(IContent content)
    {
        var subscribedActionIds = _notificationService
                                          .GetUserNotifications(_backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser, content.Path)?
                                          .Select(n => n.Action)
                                          .ToArray()
                                      ?? Array.Empty<string>();

        var availableActionIds = _actionCollection.Where(a => a.ShowInNotifier).Select(a => a.Letter.ToString()).ToArray();

        return await Task.FromResult(
            availableActionIds.Select(actionId => new DocumentNotificationResponseModel
            {
                ActionId = actionId,
                Subscribed = subscribedActionIds.Contains(actionId)
            }).ToArray());
    }
}
