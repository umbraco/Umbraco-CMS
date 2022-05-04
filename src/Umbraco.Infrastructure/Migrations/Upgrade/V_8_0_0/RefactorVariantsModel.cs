using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

public class RefactorVariantsModel : MigrationBase
{
    public RefactorVariantsModel(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        if (ColumnExists(Constants.DatabaseSchema.Tables.ContentVersionCultureVariation, "edited"))
        {
            Delete.Column("edited").FromTable(Constants.DatabaseSchema.Tables.ContentVersionCultureVariation).Do();
        }

        // add available column
        AddColumn<DocumentCultureVariationDto>("available", out IEnumerable<string> sqls);

        // so far, only those cultures that were available had records in the table
        Update.Table(DocumentCultureVariationDto.TableName).Set(new { available = true }).AllRows().Do();

        foreach (var sql in sqls)
        {
            Execute.Sql(sql).Do();
        }

        // add published column
        AddColumn<DocumentCultureVariationDto>("published", out sqls);

        // make it false by default
        Update.Table(DocumentCultureVariationDto.TableName).Set(new { published = false }).AllRows().Do();

        // now figure out whether these available cultures are published, too
        Sql<ISqlContext> getPublished = Sql()
            .Select<NodeDto>(x => x.NodeId)
            .AndSelect<ContentVersionCultureVariationDto>(x => x.LanguageId)
            .From<NodeDto>()
            .InnerJoin<ContentVersionDto>().On<NodeDto, ContentVersionDto>((node, cv) => node.NodeId == cv.NodeId)
            .InnerJoin<DocumentVersionDto>()
            .On<ContentVersionDto, DocumentVersionDto>((cv, dv) => cv.Id == dv.Id && dv.Published)
            .InnerJoin<ContentVersionCultureVariationDto>()
            .On<ContentVersionDto, ContentVersionCultureVariationDto>((cv, ccv) => cv.Id == ccv.VersionId);

        foreach (TempDto? dto in Database.Fetch<TempDto>(getPublished))
        {
            Database.Execute(Sql()
                .Update<DocumentCultureVariationDto>(u => u.Set(x => x.Published, true))
                .Where<DocumentCultureVariationDto>(x => x.NodeId == dto.NodeId && x.LanguageId == dto.LanguageId));
        }

        foreach (var sql in sqls)
        {
            Execute.Sql(sql).Do();
        }

        // so far, it was kinda impossible to make a culture unavailable again,
        // so we *should* not have anything published but not available - ignore

        // add name column
        AddColumn<DocumentCultureVariationDto>("name");

        // so far, every record in the table mapped to an available culture
        Sql<ISqlContext> getNames = Sql()
            .Select<NodeDto>(x => x.NodeId)
            .AndSelect<ContentVersionCultureVariationDto>(x => x.LanguageId, x => x.Name)
            .From<NodeDto>()
            .InnerJoin<ContentVersionDto>()
            .On<NodeDto, ContentVersionDto>((node, cv) => node.NodeId == cv.NodeId && cv.Current)
            .InnerJoin<ContentVersionCultureVariationDto>()
            .On<ContentVersionDto, ContentVersionCultureVariationDto>((cv, ccv) => cv.Id == ccv.VersionId);

        foreach (TempDto? dto in Database.Fetch<TempDto>(getNames))
        {
            Database.Execute(Sql()
                .Update<DocumentCultureVariationDto>(u => u.Set(x => x.Name, dto.Name))
                .Where<DocumentCultureVariationDto>(x => x.NodeId == dto.NodeId && x.LanguageId == dto.LanguageId));
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    // ReSharper disable UnusedAutoPropertyAccessor.Local
    private class TempDto
    {
        public int NodeId { get; set; }

        public int LanguageId { get; set; }

        public string? Name { get; set; }
    }

    // ReSharper restore UnusedAutoPropertyAccessor.Local
}
