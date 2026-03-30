namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents the result of publishing a document along with its descendant documents.
/// </summary>
public class PublishWithDescendantsResultModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the publish task.
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the publish operation with descendants is complete.
    /// </summary>
    public bool IsComplete { get; set; }
}
