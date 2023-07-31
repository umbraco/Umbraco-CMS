using Examine;
using Lucene.Net.Analysis;
using Umbraco.Search.Configuration;
using Umbraco.Search.ValueSet.Validators;
using Umbraco.Search.ValueSet.ValueSetBuilders;

namespace Umbraco.Search.Examine.Configuration;

public interface IUmbracoExamineIndexConfig : IUmbracoIndexConfiguration
{
    IContentValueSetValidator GetContentValueSetValidator();

    IContentValueSetValidator? GetPublishedContentValueSetValidator();

    IUmbracoValueSetValidator GetMemberValueSetValidator();
    Analyzer Analyzer { get; set; }

}
