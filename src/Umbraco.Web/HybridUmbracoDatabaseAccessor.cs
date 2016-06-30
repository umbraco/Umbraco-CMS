using Umbraco.Core.Persistence;

namespace Umbraco.Web
{
    internal class HybridUmbracoDatabaseAccessor : HybridAccessorBase<UmbracoDatabase>, IUmbracoDatabaseAccessor
    {
        protected override string HttpContextItemKey => "Umbraco.Core.Persistence.UmbracoDatabase";

        public HybridUmbracoDatabaseAccessor(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        { }

        public UmbracoDatabase UmbracoDatabase
        {
            get { return Value; }
            set { Value = value; }
        }
    }
}
