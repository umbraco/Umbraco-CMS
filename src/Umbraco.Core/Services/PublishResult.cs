using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Represents the result of publishing a document.
/// </summary>
public class PublishResult : OperationResult<PublishResultType, IContent>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishResult" /> class.
    /// </summary>
    public PublishResult(PublishResultType resultType, EventMessages? eventMessages, IContent? content)
        : base(resultType, eventMessages, content)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishResult" /> class.
    /// </summary>
    public PublishResult(EventMessages eventMessages, IContent content)
        : base(PublishResultType.SuccessPublish, eventMessages, content)
    {
    }

    /// <summary>
    ///     Gets the document.
    /// </summary>
    public IContent? Content => Entity;

    /// <summary>
    ///     Gets or sets the invalid properties, if the status failed due to validation.
    /// </summary>
    public IEnumerable<IProperty>? InvalidProperties { get; set; }
}
