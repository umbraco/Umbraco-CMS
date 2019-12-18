namespace Umbraco.Web
{
    /// <summary>
    /// Implements a hybrid <see cref="IUmbracoContextAccessor"/>.
    /// </summary>
    internal class HybridUmbracoContextAccessor : HybridAccessorBase<UmbracoContext>, IUmbracoContextAccessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HybridUmbracoContextAccessor"/> class.
        /// </summary>
        public HybridUmbracoContextAccessor(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        { }

        /// <inheritdoc />
        protected override string ItemKey => "Umbraco.Web.HybridUmbracoContextAccessor";

        /// <summary>
        /// Gets or sets the <see cref="UmbracoContext"/> object.
        /// </summary>
        public UmbracoContext UmbracoContext
        {
            get => Value;
            set => Value = value;
        }
    }
}
