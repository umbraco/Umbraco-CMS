using Umbraco.Core.Cache;
using Umbraco.Core.Security;
using Umbraco.Web;
using Umbraco.Web.Security;

namespace Umbraco.Core
{

    public class HybridBackofficeSecurityAccessor : HybridAccessorBase<IBackofficeSecurity>, IBackofficeSecurityAccessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HybridUmbracoContextAccessor"/> class.
        /// </summary>
        public HybridBackofficeSecurityAccessor(IRequestCache requestCache)
            : base(requestCache)
        { }

        /// <inheritdoc />
        protected override string ItemKey => "Umbraco.Web.HybridBackofficeSecurityAccessor";

        /// <summary>
        /// Gets or sets the <see cref="BackofficeSecurity"/> object.
        /// </summary>
        public IBackofficeSecurity BackofficeSecurity
        {
            get => Value;
            set => Value = value;
        }
    }
}
