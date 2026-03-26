using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory responsible for creating presentation models for document notifications.
/// </summary>
public interface IDocumentNotificationPresentationFactory
{
    /// <summary>
    /// Creates notification models asynchronously for the specified content.
    /// </summary>
    /// <param name="content">The content to create notification models for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of document notification response models.</returns>
    Task<IEnumerable<DocumentNotificationResponseModel>> CreateNotificationModelsAsync(IContent content);
}
