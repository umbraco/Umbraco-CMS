// Copyright (c) Umbraco.
// See LICENSE for more details.

using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Represents a persisted search index document, used by Umbraco Search for change detection.
/// </summary>
[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
public sealed class IndexDocumentDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.IndexDocument;
    public const string PrimaryKeyColumnName = "id";

    /// <summary>
    /// Gets or sets the unique identifier for the index document.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = true)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the key of the content item the document was indexed for.
    /// </summary>
    [Column("key")]
    public required Guid Key { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the document represents the published state of the content item.
    /// </summary>
    [Column("published")]
    public required bool Published { get; set; }

    /// <summary>
    /// Gets or sets the serialized index field data.
    /// </summary>
    [Column("fields")]
    public required byte[] Fields { get; set; }
}
