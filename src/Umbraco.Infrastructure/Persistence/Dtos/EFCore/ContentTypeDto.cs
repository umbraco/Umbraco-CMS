using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(ContentTypeDtoConfiguration))]
public class ContentTypeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ContentType;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNamePk;
    public const string NodeIdColumnName = Constants.DatabaseSchema.Columns.NodeIdName;

    /// <summary>Gets or sets the primary key of the content type.</summary>
    public int PrimaryKey { get; set; }

    /// <summary>Gets or sets the unique identifier of the node associated with this content type.</summary>
    public int NodeId { get; set; }

    /// <summary>Gets or sets the alias that uniquely identifies the content type.</summary>
    public string? Alias { get; set; }

    /// <summary>Gets or sets the icon associated with the content type.</summary>
    public string? Icon { get; set; }

    /// <summary>Gets or sets the thumbnail image file name associated with the content type.</summary>
    public string? Thumbnail { get; set; }

    /// <summary>Gets or sets the description of the content type.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the unique identifier of the list view configuration associated with this content type.</summary>
    public Guid? ListView { get; set; }

    /// <summary>Gets or sets a value indicating whether this content type is an element.</summary>
    public bool IsElement { get; set; }

    /// <summary>Gets or sets a value indicating whether this content type is allowed in the library.</summary>
    public bool AllowedInLibrary { get; set; }

    /// <summary>Gets or sets a value indicating whether this content type is allowed at the root level.</summary>
    public bool AllowAtRoot { get; set; }

    /// <summary>Gets or sets the variation flags for the content type.</summary>
    public byte Variations { get; set; }
}
