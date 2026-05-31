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

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentNotificationPresentationFactory"/> class.
    /// </summary>
    /// <param name="actionCollection">The <see cref="ActionCollection"/> containing available actions for document notifications.</param>
    /// <param name="notificationService">The <see cref="INotificationService"/> used to manage notifications.</param>
    /// <param name="backOfficeSecurityAccessor">The <see cref="IBackOfficeSecurityAccessor"/> providing access to back office security context.</param>
    public DocumentNotificationPresentationFactory(ActionCollection actionCollection, INotificationService notificationService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _actionCollection = actionCollection;
        _notificationService = notificationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Asynchronously creates notification response models for the specified content item, indicating which notifications the current user is subscribed to.
    /// </summary>
    /// <param name="content">The content item for which to generate notification models.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains a collection of <see cref="DocumentNotificationResponseModel"/> objects, each describing a notification type and whether the current user is subscribed to it for the given content.
    /// </returns>
    public Task<IEnumerable<DocumentNotificationResponseModel>> CreateNotificationModelsAsync(IContent content)
    {
        var subscribedActionIds = _notificationService
            .GetUserNotifications(_backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser, content.Path)?
            .Select(n => n.Action)
            .ToArray() ?? Array.Empty<string>();

        return Task.FromResult<IEnumerable<DocumentNotificationResponseModel>>(_actionCollection
            .Where(action => action.ShowInNotifier)
            .Select(action => new DocumentNotificationResponseModel
            {
                ActionId = action.Letter,
                Alias = action.Alias,
                Subscribed = subscribedActionIds.Contains(action.Letter)
            })
            .ToArray());
    }
}
