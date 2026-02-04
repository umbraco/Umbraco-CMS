using NPoco;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

// TODO ELEMENTS: split this into two interfaces - like "IEntityDto" and "IPublishedDto"?
internal interface IContentVersionDto
{
    internal const string IdColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    internal const string PublishedColumnName = "published";

    [Column(IdColumnName)]
    int Id { get; }

    [Column(PublishedColumnName)]
    bool Published { get; }

    [ResultColumn]
    ContentVersionDto ContentVersionDto { get; }
}
