using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

// TODO ELEMENTS: split this into two interfaces - like "IEntityDto" and "IPublishedDto"?
public interface IContentVersionDto
{
    internal const string IdColumnName = "id";

    internal const string PublishedColumnName = "published";

    [Column(IdColumnName)]
    int Id { get; }

    [Column(PublishedColumnName)]
    bool Published { get; }
}
