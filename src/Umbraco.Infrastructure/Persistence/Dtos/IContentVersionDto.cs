using NPoco;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

// TODO ELEMENTS: split this into two interfaces - like "IEntityDto" and "IPublishedDto"?
internal interface IContentVersionDto
{
    internal static class Columns
    {
        internal const string Id = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

        internal const string Published = "published";
    }

    [Column(Columns.Id)]
    int Id { get; }

    [Column(Columns.Published)]
    bool Published { get; }

    [ResultColumn]
    ContentVersionDto ContentVersionDto { get; }
}
