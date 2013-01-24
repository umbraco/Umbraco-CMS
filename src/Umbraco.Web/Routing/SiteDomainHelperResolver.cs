using System;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Resolves the <see cref="ISiteDomainHelper"/> implementation.
	/// </summary>
	public sealed class SiteDomainHelperResolver : SingleObjectResolverBase<SiteDomainHelperResolver, ISiteDomainHelper>
	{
		
		/// <summary>
        /// Initializes a new instance of the <see cref="SiteDomainHelperResolver"/> class with an <see cref="ISiteDomainHelper"/> implementation.
		/// </summary>
        /// <param name="helper">The <see cref="ISiteDomainHelper"/> implementation.</param>
        internal SiteDomainHelperResolver(ISiteDomainHelper helper)
			: base(helper)
		{ }


		/// <summary>
        /// Can be used by developers at runtime to set their IDomainHelper at app startup
		/// </summary>
        /// <param name="helper"></param>
		public void SetHelper(ISiteDomainHelper helper)
		{
			Value = helper;
		}

		/// <summary>
        /// Gets or sets the <see cref="ISiteDomainHelper"/> implementation.
		/// </summary>
        public ISiteDomainHelper Helper
		{
			get { return Value; }
		}
	}
}