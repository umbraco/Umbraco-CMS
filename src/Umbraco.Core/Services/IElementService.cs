using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines the ElementService, which is an easy access to operations involving <see cref="IElement" />
/// </summary>
public interface IElementService : IPublishableContentService<IElement>
{
    /// <summary>
    ///     Saves and publishes an element in a single scope.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         For invariant content types, <paramref name="culturesToPublish" /> must be empty; the element is
    ///         saved and the invariant culture is published.
    ///     </para>
    ///     <para>
    ///         For variant content types, only the cultures listed in <paramref name="culturesToPublish" /> are
    ///         published. Wildcards (<c>"*"</c>), nulls, whitespace and duplicate entries are not accepted. Passing
    ///         an empty array saves the element without publishing any culture.
    ///     </para>
    ///     <para>When a culture is being published, it includes all varying values along with all invariant values.</para>
    ///     <para>
    ///         The save and publish run in the same scope. If publishing fails for a business reason (for example,
    ///         invalid content or an expired schedule) the save still takes effect; both are skipped only when a
    ///         saving notification handler cancels the operation.
    ///     </para>
    /// </remarks>
    /// <param name="content">The element to publish.</param>
    /// <param name="culturesToPublish">The cultures to publish, or an empty array for invariant content.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    // TODO (V19): Remove the default implementation when the method is no longer new.
    PublishResult SaveAndPublish(IElement content, string[] culturesToPublish, int userId = Constants.Security.SuperUserId)
    {
        OperationResult saveResult = Save(content, userId);
        if (saveResult.Success)
        {
            return Publish(content, culturesToPublish, userId);
        }

        PublishResultType resultType = saveResult.Result == OperationResultType.FailedCancelledByEvent
            ? PublishResultType.FailedPublishCancelledByEvent
            : PublishResultType.FailedPublish;
        return new PublishResult(resultType, saveResult.EventMessages, content);
    }
}
