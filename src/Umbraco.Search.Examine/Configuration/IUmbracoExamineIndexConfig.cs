using Examine;
using Lucene.Net.Analysis;
using Umbraco.Search.Examine.ValueSetBuilders;

namespace Umbraco.Search.Examine;

public interface IUmbracoExamineIndexConfig
{
    IContentValueSetValidator GetContentValueSetValidator();

    IContentValueSetValidator GetPublishedContentValueSetValidator();

    IValueSetValidator GetMemberValueSetValidator();
    bool PublishedValuesOnly { get; set; }
    bool EnableDefaultEventHandler { get; set; }
    Analyzer Analyzer { get; set; }
}
