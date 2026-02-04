using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

internal interface ICultureVariationDto : INodeDto
{
    internal const string LanguageIdColumnName = "languageId";

    internal const string EditedColumnName = "edited";

    [Column(LanguageIdColumnName)]
    int LanguageId { get; }

    [Column(EditedColumnName)]
    bool Edited { get; }
}
