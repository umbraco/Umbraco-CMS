using Examine;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Examine
{
    public class UmbracoIndexConfig : IUmbracoIndexConfig
    {
        public UmbracoIndexConfig(IPublicAccessService publicAccessService)
        {
            PublicAccessService = publicAccessService;
        }

        protected IPublicAccessService PublicAccessService { get; }
        public IContentValueSetValidator GetContentValueSetValidator()
        {
            return new ContentValueSetValidator(false, true, PublicAccessService);
        }

        public IContentValueSetValidator GetPublishedContentValueSetValidator()
        {
            return new ContentValueSetValidator(true, false, PublicAccessService);
        }

        /// <summary>
        /// Returns the <see cref="IValueSetValidator"/> for the member indexer
        /// </summary>
        /// <returns></returns>
        public IValueSetValidator GetMemberValueSetValidator()
        {
            return new MemberValueSetValidator();
        }
    }
}
