using Umbraco.Core;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.Routing
{
    public sealed class SecureRequestResolver : SingleObjectResolverBase<SecureRequestResolver, ISecureRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecureRequestResolver"/> class with an <see cref="ISecureRequest"/> implementation.
        /// </summary>
        /// <param name="helper">The <see cref="ISecureRequest"/> implementation.</param>
        public SecureRequestResolver(ISecureRequest secureRequest)
			: base(secureRequest)
		{ }

        /// <summary>
        /// Can be used by developers at startup to set their <see cref="ISecureRequest"/> at app startup
		/// </summary>
        /// <param name="helper"></param>
		public void SetHelper(ISecureRequest secureRequest)
        {
            Value = secureRequest;
        }

        /// <summary>
        /// Gets the <see cref="ISecureRequest"/> implementation.
		/// </summary>
        public ISecureRequest SecureRequest
        {
            get { return Value; }
        }
    }
}