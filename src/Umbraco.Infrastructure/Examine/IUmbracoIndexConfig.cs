using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;

public interface IUmbracoIndexConfig
{
    IContentValueSetValidator GetContentValueSetValidator();

    IContentValueSetValidator GetPublishedContentValueSetValidator();

    IValueSetValidator GetMemberValueSetValidator();
}
