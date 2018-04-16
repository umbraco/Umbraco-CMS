namespace Umbraco.Web
{
    internal class HybridUmbracoContextAccessor : HybridAccessorBase<UmbracoContext>, IUmbracoContextAccessor
    {
        public HybridUmbracoContextAccessor(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        { }

        protected override string ItemKey => "Umbraco.Web.HybridUmbracoContextAccessor";

        public UmbracoContext UmbracoContext
        {
            get { return Value; }
            set { Value = value; }
        }
    }
}
