using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

internal interface IPublishableContentDto<TContentVersionDto> : INodeDto
    where TContentVersionDto : class, IContentVersionDto
{
    internal static class Columns
    {
        internal const string NodeId = INodeDto.Columns.NodeId;

        internal const string Published = IContentVersionDto.Columns.Published;
    }

    [Column(Columns.Published)]
    bool Published { get; }

    [ResultColumn]
    ContentDto ContentDto { get; }

    [ResultColumn]
    TContentVersionDto ContentVersionDto { get; }

    [ResultColumn]
    TContentVersionDto? PublishedVersionDto { get; }
}
