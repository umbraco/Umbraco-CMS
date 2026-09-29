using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(DocumentDtoConfiguration))]
public class DocumentDto : IPublishableContentDto<DocumentVersionDto>
{
    public const string TableName = Constants.DatabaseSchema.Tables.Document;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.NodeIdName;
    public const string PublishedColumnName = "published";
    public const string EditedColumnName = "edited";

    /// <summary>
    /// Gets or sets the unique identifier for the node associated with this document.
    /// </summary>
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the document is published.
    /// </summary>
    public bool Published { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this document has been modified since it was last published or saved.
    /// </summary>
    public bool Edited { get; set; }

    /// <summary>
    /// Gets or sets the content row. Not a database column — populated by the repository after query.
    /// </summary>
    public ContentDto ContentDto { get; set; } = null!;

    /// <summary>
    /// Gets or sets the current (draft) version row. Not a database column — populated by the repository after query.
    /// </summary>
    public DocumentVersionDto CurrentVersion { get; set; } = null!;

    /// <summary>
    /// Gets or sets the published version row. Not a database column — populated by the repository after query.
    /// </summary>
    public DocumentVersionDto? PublishedVersion { get; set; }
}
