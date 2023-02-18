using Examine;
using Umbraco.Search.Examine.ValueSetBuilders;

namespace Umbraco.Search.Examine;

public interface IUmbracoExamineIndexConfig
{
    IContentValueSetValidator GetContentValueSetValidator();

    IContentValueSetValidator GetPublishedContentValueSetValidator();

    IValueSetValidator GetMemberValueSetValidator();
}
