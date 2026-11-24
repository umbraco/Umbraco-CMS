namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
/// Defines repository methods for querying property type usage.
/// </summary>
public interface IPropertyTypeUsageRepository
{
    /// <summary>
    /// Determines whether there are any saved property values for the specified content type and property alias.
    /// </summary>
    /// <param name="contentTypeKey">The key of the content type that declares the property.</param>
    /// <param name="propertyAlias">The alias of the property.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is <c>true</c> if there are saved values;
    /// otherwise, <c>false</c>.
    /// </returns>
    Task<bool> HasSavedPropertyValuesAsync(Guid contentTypeKey, string propertyAlias, CancellationToken cancellationToken);

    /// <summary>
    /// Determines whether a content type with the specified unique identifier exists.
    /// </summary>
    /// <param name="contentTypeKey">The key of the content type.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is <c>true</c> if a content type with the
    /// key exists; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> ContentTypeExistAsync(Guid contentTypeKey, CancellationToken cancellationToken);
}
