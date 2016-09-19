namespace Umbraco.Web
{
    internal class HybridUmbracoContextAccessor : HybridAccessorBase<UmbracoContext>, IUmbracoContextAccessor
    {
        private const string ItemKeyConst = "Umbraco.Web.HybridUmbracoContextAccessor";

        protected override string ItemKey => ItemKeyConst;

        static HybridUmbracoContextAccessor()
        {
            SafeCallContextRegister(ItemKeyConst);
        }

        public HybridUmbracoContextAccessor(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        { }

        public UmbracoContext UmbracoContext
        {
            get { return Value; }
            set { Value = value; }
        }
    }
}
