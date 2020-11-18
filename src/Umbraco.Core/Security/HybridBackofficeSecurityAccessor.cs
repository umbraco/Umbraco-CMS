using Umbraco.Core.Cache;
using Umbraco.Web;

namespace Umbraco.Core.Security
{
    public class HybridBackofficeSecurityAccessor : HybridAccessorBase<IBackOfficeSecurity>, IBackOfficeSecurityAccessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HybridBackofficeSecurityAccessor"/> class.
        /// </summary>
        public HybridBackofficeSecurityAccessor(IRequestCache requestCache)
            : base(requestCache)
        { }

        /// <inheritdoc />
        protected override string ItemKey => "Umbraco.Web.HybridBackofficeSecurityAccessor";

        /// <summary>
        /// Gets or sets the <see cref="IBackOfficeSecurity"/> object.
        /// </summary>
        public IBackOfficeSecurity BackOfficeSecurity
        {
            get => Value;
            set => Value = value;
        }
    }
}
