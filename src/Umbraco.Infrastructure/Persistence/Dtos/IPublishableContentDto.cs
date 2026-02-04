using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

internal interface IPublishableContentDto<TContentVersionDto> : INodeDto
    where TContentVersionDto : class, IContentVersionDto
{
    internal const string PublishedColumnName = "published";

    [Column(PublishedColumnName)]
    bool Published { get; }

    [ResultColumn]
    ContentDto ContentDto { get; }

    [ResultColumn]
    TContentVersionDto ContentVersionDto { get; }

    [ResultColumn]
    TContentVersionDto? PublishedVersionDto { get; }
}
