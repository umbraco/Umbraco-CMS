using Examine;

namespace Umbraco.Examine
{
    public interface IUmbracoIndexConfig
    {
        IContentValueSetValidator GetContentValueSetValidator();
        IContentValueSetValidator GetPublishedContentValueSetValidator();
        IValueSetValidator GetMemberValueSetValidator();

    }
}
