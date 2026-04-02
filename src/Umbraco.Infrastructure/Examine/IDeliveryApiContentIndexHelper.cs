using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Examine;

internal interface IDeliveryApiContentIndexHelper
{
    /// <summary>
    /// Enumerates all applicable descendant content items of the specified root content ID, invoking the provided action for each batch of descendants.
    /// </summary>
    /// <param name="rootContentId">The ID of the root content item from which to start enumerating descendants.</param>
    /// <param name="actionToPerform">An action to perform on each batch (array) of descendant <see cref="IContent"/> items.</param>
    void EnumerateApplicableDescendantsForContentIndex(int rootContentId, Action<IContent[]> actionToPerform);
}
