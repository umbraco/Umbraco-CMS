using Umbraco.Core.Cache;
using Umbraco.Core.Security;
using Umbraco.Web;
using Umbraco.Web.Security;

namespace Umbraco.Core
{

    public class HybridWebSecurityAccessor : HybridAccessorBase<IWebSecurity>, IWebSecurityAccessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HybridUmbracoContextAccessor"/> class.
        /// </summary>
        public HybridWebSecurityAccessor(IRequestCache requestCache)
            : base(requestCache)
        { }

        /// <inheritdoc />
        protected override string ItemKey => "Umbraco.Web.HybridWebSecurityAccessor";

        /// <summary>
        /// Gets or sets the <see cref="WebSecurity"/> object.
        /// </summary>
        public IWebSecurity WebSecurity
        {
            get => Value;
            set => Value = value;
        }
    }
}
