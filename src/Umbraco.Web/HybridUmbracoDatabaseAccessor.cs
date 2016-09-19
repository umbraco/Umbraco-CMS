using Umbraco.Core.Persistence;

namespace Umbraco.Web
{
    internal class HybridUmbracoDatabaseAccessor : HybridAccessorBase<UmbracoDatabase>, IUmbracoDatabaseAccessor
    {
        private const string ItemKeyConst = "Umbraco.Core.Persistence.HybridUmbracoDatabaseAccessor";

        protected override string ItemKey => ItemKeyConst;

        static HybridUmbracoDatabaseAccessor()
        {
            SafeCallContextRegister(ItemKeyConst);
        }

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
