using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

public interface IPublishableContentDto<TContentVersionDto> : INodeDto
    where TContentVersionDto : class, IContentVersionDto
{
    [ResultColumn]
    ContentDto ContentDto { get; }

    [ResultColumn]
    TContentVersionDto ContentVersionDto { get; }

    [ResultColumn]
    TContentVersionDto? PublishedVersionDto { get; }
}
