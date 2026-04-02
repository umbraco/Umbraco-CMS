using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
internal sealed class RelationDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Relation;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    private const string RelationTypeColumnName = "relType";
    private const string ParentIdColumnName = "parentId";
    private const string ChildIdColumnName = "childId";

    /// <summary>
    /// Gets or sets the unique identifier for the relation.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the parent node in this relation.
    /// </summary>
    [Column(ParentIdColumnName)]
    [ForeignKey(typeof(NodeDto), Name = "FK_umbracoRelation_umbracoNode")]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoRelation_parentChildType", ForColumns = $"{ParentIdColumnName},{ChildIdColumnName},{RelationTypeColumnName}")]
    public int ParentId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the child node in this relation.
    /// </summary>
    [Column(ChildIdColumnName)]
    [ForeignKey(typeof(NodeDto), Name = "FK_umbracoRelation_umbracoNode1")]
    public int ChildId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the relation type associated with this relation.
    /// This is a foreign key to the <see cref="RelationTypeDto"/>.
    /// </summary>
    [Column(RelationTypeColumnName)]
    [ForeignKey(typeof(RelationTypeDto))]
    public int RelationType { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the relation was created.
    /// </summary>
    [Column("datetime")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime Datetime { get; set; }

    /// <summary>
    /// Gets or sets the comment associated with the relation.
    /// </summary>
    [Column("comment")]
    [Length(1000)]
    public string? Comment { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier (GUID) of the parent object's type in the relation.
    /// </summary>
    [ResultColumn]
    [Column("parentObjectType")]
    public Guid ParentObjectType { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier (GUID) of the child object's type in this relation.
    /// </summary>
    [ResultColumn]
    [Column("childObjectType")]
    public Guid ChildObjectType { get; set; }
}
