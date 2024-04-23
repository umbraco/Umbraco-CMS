using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

public class AddListViewKeysToDocumentTypes : UnscopedMigrationBase
{
    private readonly IScopeProvider _scopeProvider;
    private readonly IDataTypeService _dataTypeService;
    private const string NewColumnName = "listView";

    public AddListViewKeysToDocumentTypes(IMigrationContext context, IScopeProvider scopeProvider, IDataTypeService dataTypeService) : base(context)
    {
        _scopeProvider = scopeProvider;
        _dataTypeService = dataTypeService;
    }

    protected override void Migrate()
    {
        using IScope scope = _scopeProvider.CreateScope();
        using IDisposable notificationSuppression = scope.Notifications.Suppress();
        ScopeDatabase(scope);

        if (DatabaseType != DatabaseType.SQLite)
        {
            MigrateSqlServer();
            Context.Complete();
            scope.Complete();
            return;
        }

        MigrateSqlite();
        Context.Complete();
        scope.Complete();
    }

    private void MigrateSqlServer()
    {
        if (ColumnExists(Constants.DatabaseSchema.Tables.ContentType, NewColumnName) is false)
        {
            IEnumerable<ContentTypeDto> contentTypeDtos = GetContentTypeDtos().Where(x => x.ListView is not null);
            Delete.DefaultConstraint().OnTable(Constants.DatabaseSchema.Tables.ContentType).OnColumn("isContainer").Do();
            Delete.Column("isContainer").FromTable(Constants.DatabaseSchema.Tables.ContentType).Do();
            Create.Column(NewColumnName)
                .OnTable(Constants.DatabaseSchema.Tables.ContentType)
                .AsGuid()
                .Nullable()
                .Do();

            foreach (ContentTypeDto contentTypeDto in contentTypeDtos)
            {
                Database.Update(contentTypeDto);
            }
        }
    }

    private void MigrateSqlite()
    {
        if (ColumnExists(Constants.DatabaseSchema.Tables.ContentType, NewColumnName))
        {
            return;
        }

        /*
         * We commit the initial transaction started by the scope. This is required in order to disable the foreign keys.
         * We then begin a new transaction, this transaction will be committed or rolled back by the scope, like normal.
         * We don't have to worry about re-enabling the foreign keys, since these are enabled by default every time a connection is established.
         *
         * Ideally we'd want to do this with the unscoped database we get, however, this cannot be done,
         * since our scoped database cannot share a connection with the unscoped database, so a new one will be created, which enables the foreign keys.
         * Similarly we cannot use Database.CompleteTransaction(); since this also closes the connection,
         * so starting a new transaction would re-enable foreign keys.
         */
        Database.Execute("COMMIT;");
        Database.Execute("PRAGMA foreign_keys=off;");
        Database.Execute("BEGIN TRANSACTION;");

        IEnumerable<ContentTypeDto> contentTypeDtos = GetContentTypeDtos();

        Delete.Table(Constants.DatabaseSchema.Tables.ContentType).Do();
        Create.Table<ContentTypeDto>().Do();

        foreach (ContentTypeDto contentTypeDto in contentTypeDtos)
        {
            Database.Insert(Constants.DatabaseSchema.Tables.ContentType, "pk", false, contentTypeDto);
        }
    }

    private Guid? CalculateListView(ContentTypeDtoOld dto)
    {
        if (dto.IsContainer is false)
        {
            return null;
        }

        var name = Constants.Conventions.DataTypes.ListViewPrefix + dto.Alias;
        IDataType? listview = _dataTypeService.GetAsync(name).GetAwaiter().GetResult();
        if (listview is not null)
        {
            return listview.Key;
        }

        if (dto.NodeDto.NodeObjectType == Constants.ObjectTypes.DocumentType)
        {
            return Constants.DataTypes.Guids.ListViewContentGuid;
        }

        if (dto.NodeDto.NodeObjectType == Constants.ObjectTypes.MediaType)
        {
            return Constants.DataTypes.Guids.ListViewMediaGuid;
        }

        // No custom list view was found, and not one of the default types either. Therefore we cannot find it.
        return null;
    }

    private IEnumerable<ContentTypeDto> GetContentTypeDtos()
    {
        Sql<ISqlContext> sql = Sql()
            .SelectAll()
            .From<ContentTypeDtoOld>()
            .InnerJoin<NodeDto>()
            .On<ContentTypeDtoOld, NodeDto>(dto => dto.NodeId, dto => dto.NodeId);

        List<ContentTypeDtoOld>? contentTypeDtoOld = Database.Fetch<ContentTypeDtoOld>(sql);

        var contentTypeDtos = contentTypeDtoOld.Select(x => new ContentTypeDto
        {
            PrimaryKey = x.PrimaryKey,
            NodeId = x.NodeId,
            Alias = x.Alias,
            Icon = x.Icon,
            Thumbnail = x.Thumbnail,
            Description = x.Description,
            ListView = CalculateListView(x),
            IsElement = x.IsElement,
            AllowAtRoot = x.AllowAtRoot,
            Variations = x.Variations,
            NodeDto = x.NodeDto,
        }).ToList();

        return contentTypeDtos;
    }


    [TableName(Constants.DatabaseSchema.Tables.ContentType)]
    [PrimaryKey("pk")]
    [ExplicitColumns]
    private class ContentTypeDtoOld
    {
        public const string TableName = Constants.DatabaseSchema.Tables.ContentType;
        private string? _alias;

        [Column("pk")]
        [PrimaryKeyColumn(IdentitySeed = 700)]
        public int PrimaryKey { get; set; }

        [Column("nodeId")]
        [ForeignKey(typeof(NodeDto))]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsContentType")]
        public int NodeId { get; set; }

        [Column("alias")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string? Alias { get => _alias; set => _alias = value == null ? null : string.Intern(value); }

        [Column("icon")]
        [Index(IndexTypes.NonClustered)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string? Icon { get; set; }

        [Column("thumbnail")]
        [Constraint(Default = "folder.png")]
        public string? Thumbnail { get; set; }

        [Column("description")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(1500)]
        public string? Description { get; set; }

        [Column("isContainer")]
        [Constraint(Default = "0")]
        public bool IsContainer { get; set; }

        [Column("isElement")]
        [Constraint(Default = "0")]
        public bool IsElement { get; set; }

        [Column("allowAtRoot")]
        [Constraint(Default = "0")]
        public bool AllowAtRoot { get; set; }

        [Column("variations")]
        [Constraint(Default = "1" /*ContentVariation.InvariantNeutral*/)]
        public byte Variations { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.OneToOne, ColumnName = "NodeId")]
        public NodeDto NodeDto { get; set; } = null!;
    }
}
