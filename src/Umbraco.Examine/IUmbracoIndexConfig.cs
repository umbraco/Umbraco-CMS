using Examine;

namespace Umbraco.Examine
{
    public abstract class IUmbracoIndexConfig
    {
        public abstract IContentValueSetValidator GetContentValueSetValidator();
        public abstract IContentValueSetValidator GetPublishedContentValueSetValidator();
        public abstract IValueSetValidator GetMemberValueSetValidator();

    }
}
