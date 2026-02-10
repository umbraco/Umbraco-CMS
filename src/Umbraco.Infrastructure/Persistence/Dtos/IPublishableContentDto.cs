using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

internal interface IPublishableContentDto<TContentVersionDto> : INodeDto
    where TContentVersionDto : class, IContentVersionDto
{
    internal static class Columns
    {
        internal const string NodeId = INodeDto.Columns.NodeId;

        internal const string Published = IContentVersionDto.Columns.Published;

        internal const string Edited = ICultureVariationDto.Columns.Edited;
    }

    [Column(Columns.Published)]
    bool Published { get; set;  }

    [Column(Columns.Edited)]
    bool Edited { get; set; }

    [ResultColumn]
    ContentDto ContentDto { get; }

    [ResultColumn]
    TContentVersionDto ContentVersionDto { get; }

    [ResultColumn]
    TContentVersionDto? PublishedVersionDto { get; }
}
