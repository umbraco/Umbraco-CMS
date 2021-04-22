using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Core.Security
{

    /// <summary>
    /// A password hasher for back office users
    /// </summary>
    /// <remarks>
    /// This allows us to verify passwords in old formats and roll forward to the latest format
    /// </remarks>
    public class BackOfficePasswordHasher : UmbracoPasswordHasher<BackOfficeIdentityUser>
    {
        public BackOfficePasswordHasher(LegacyPasswordSecurity passwordSecurity, IJsonSerializer jsonSerializer)
            : base(passwordSecurity, jsonSerializer)
        {
        }
    }
}
