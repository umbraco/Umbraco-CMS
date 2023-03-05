using Examine;
using Lucene.Net.Analysis;
using Umbraco.Search.Configuration;
using Umbraco.Search.Examine.ValueSetBuilders;

namespace Umbraco.Search.Examine.Configuration;

public interface IUmbracoExamineIndexConfig : IUmbracoIndexConfiguration
{
    IContentValueSetValidator GetContentValueSetValidator();

    IContentValueSetValidator GetPublishedContentValueSetValidator();

    IValueSetValidator GetMemberValueSetValidator();
    Analyzer Analyzer { get; set; }


}
