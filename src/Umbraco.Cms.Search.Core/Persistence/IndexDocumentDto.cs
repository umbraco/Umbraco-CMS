using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Search.Core.Persistence;

[TableName(Constants.Persistence.IndexDocumentTableName)]
[PrimaryKey("id")]
[ExplicitColumns]
public class IndexDocumentDto
{
    [Column("id")]
    [PrimaryKeyColumn(AutoIncrement = true)]
    public int Id { get; set; }

    [Column("key")]
    public required Guid Key { get; set; }

    [Column("published")]
    public required bool Published { get; set; }

    [Column("fields")]
    public required byte[] Fields { get; set; }
}
