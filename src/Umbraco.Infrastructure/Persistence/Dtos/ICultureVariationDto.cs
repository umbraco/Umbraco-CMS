using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

internal interface ICultureVariationDto : INodeDto
{
    internal static class Columns
    {
        internal const string NodeId = INodeDto.Columns.NodeId;

        internal const string LanguageId = "languageId";

        internal const string Edited = "edited";

        internal const string Name = "name";

        internal const string Available = "available";

        internal const string Published = IContentVersionDto.Columns.Published;
    }

    [Column(Columns.LanguageId)]
    int LanguageId { get; set; }

    [Column(Columns.Edited)]
    bool Edited { get; set; }

    [Ignore]
    string? Culture { get; set; }

    [Column(Columns.Available)]
    bool Available { get; set; }

    // de-normalized for perfs
    // (means there is a published content version culture variation for the language)
    [Column(Columns.Published)]
    bool Published { get; set; }

    [Column(Columns.Name)]
    string? Name { get; set; }
}
