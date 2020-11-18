using Umbraco.Core.Cache;
using Umbraco.Core.Security;
using Umbraco.Web;
using Umbraco.Web.Security;

namespace Umbraco.Core
{

    public class HybridBackofficeSecurityAccessor : HybridAccessorBase<IBackOfficeSecurity>, IBackOfficeSecurityAccessor
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
        /// Gets or sets the <see cref="BackOfficeSecurity"/> object.
        /// </summary>
        public IBackOfficeSecurity BackOfficeSecurity
        {
            get => Value;
            set => Value = value;
        }
    }
}
