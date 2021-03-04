using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Core.Web
{
    /// <summary>
    /// Implements a hybrid <see cref="IUmbracoContextAccessor"/>.
    /// </summary>
    public class HybridUmbracoContextAccessor : HybridAccessorBase<IUmbracoContext>, IUmbracoContextAccessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HybridUmbracoContextAccessor"/> class.
        /// </summary>
        public HybridUmbracoContextAccessor(IRequestCache requestCache)
            : base(requestCache)
        { }

        /// <inheritdoc />
        protected override string ItemKey => "Umbraco.Web.HybridUmbracoContextAccessor";

        /// <summary>
        /// Gets or sets the <see cref="UmbracoContext"/> object.
        /// </summary>
        public IUmbracoContext UmbracoContext
        {
            get => Value;
            set => Value = value;
        }
    }
}
