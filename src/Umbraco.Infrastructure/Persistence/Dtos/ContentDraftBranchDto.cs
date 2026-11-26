using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Represents a data transfer object (DTO) for a named draft branch — an independently editable, in-progress draft
/// that exists alongside a content item's default draft, created so multiple future states of the same node can be
/// prepared in parallel (e.g. a "Black Friday" draft and a "Summer Sale" draft on the same node).
/// </summary>
[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
public class ContentDraftBranchDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ContentDraftBranch;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    private const string KeyColumnName = "key";
    private const string NodeIdColumnName = Constants.DatabaseSchema.Columns.NodeIdName;
    private const string ContentKeyColumnName = "contentKey";
    private const string NameColumnName = "name";
    private const string CreatedByUserIdColumnName = "createdByUserId";
    private const string CreateDateColumnName = "createDate";
    private const string UpdateDateColumnName = "updateDate";
    private const string BasedOnVersionIdColumnName = "basedOnVersionId";
    private const string ScheduledPublishDateColumnName = "scheduledPublishDate";
    private const string ScheduledCulturesColumnName = "scheduledCultures";

    /// <summary>
    /// Gets or sets the unique identifier for the draft branch.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the external (API-facing) unique identifier for the draft branch.
    /// </summary>
    [Column(KeyColumnName)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_" + KeyColumnName)]
    public Guid Key { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the node this draft branch belongs to.
    /// </summary>
    [Column(NodeIdColumnName)]
    [ForeignKey(typeof(ContentDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_" + NodeIdColumnName)]
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the content item's own key (as distinct from <see cref="Key"/>, the branch's own key).
    /// Captured at creation time so the branch can be resolved back to its content via the generic,
    /// entity-agnostic <c>GetById(Guid)</c> service surface shared by Documents, Elements, etc. - there is
    /// no generic node-id-based lookup, only key-based, across those content-type-specific services.
    /// </summary>
    [Column(ContentKeyColumnName)]
    public Guid ContentKey { get; set; }

    /// <summary>
    /// Gets or sets the editor-facing name of the draft branch (e.g. "Black Friday").
    /// </summary>
    [Column(NameColumnName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the id of the user who created this draft branch.
    /// </summary>
    [Column(CreatedByUserIdColumnName)]
    [ForeignKey(typeof(UserDto))]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? CreatedByUserId { get; set; }

    /// <summary>
    /// Gets or sets the date and time this draft branch was created.
    /// </summary>
    [Column(CreateDateColumnName)]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time this draft branch was last updated.
    /// </summary>
    [Column(UpdateDateColumnName)]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime UpdateDate { get; set; }

    /// <summary>
    /// Gets or sets the id of the content version that was current on the node's default draft at the moment this
    /// branch was forked.
    /// </summary>
    /// <remarks>
    /// Deliberately not declared as a database foreign key: <see cref="ContentVersionDto"/> itself references
    /// this table via <see cref="ContentVersionDto.BranchId"/>, and a FK in both directions is not something a
    /// relational schema can express (nor insert into, even where it could be declared). Referential integrity
    /// here is trusted to the code, same as the "current" version convention this table extends.
    /// </remarks>
    [Column(BasedOnVersionIdColumnName)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? BasedOnVersionId { get; set; }

    /// <summary>
    /// Gets or sets the date and time (UTC) at which this branch should automatically be published, or
    /// <c>null</c> if it has no scheduled publish date.
    /// </summary>
    [Column(ScheduledPublishDateColumnName)]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_" + ScheduledPublishDateColumnName)]
    public DateTime? ScheduledPublishDate { get; set; }

    /// <summary>
    /// Gets or sets the comma-delimited set of cultures to promote/publish when <see cref="ScheduledPublishDate"/>
    /// fires - empty string for invariant content, <c>null</c> only when no schedule has ever been set (kept in
    /// lock-step with <see cref="ScheduledPublishDate"/>, both always written together).
    /// </summary>
    [Column(ScheduledCulturesColumnName)]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(500)]
    public string? ScheduledCultures { get; set; }
}
