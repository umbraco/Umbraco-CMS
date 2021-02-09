using Microsoft.AspNetCore.Identity;
using IPasswordHasher = Umbraco.Cms.Core.Security.IPasswordHasher;

namespace Umbraco.Web.Common.AspNetCore
{
    public class AspNetCorePasswordHasher : Cms.Core.Security.IPasswordHasher
    {
        private PasswordHasher<object> _underlyingHasher;

        public AspNetCorePasswordHasher()
        {
            _underlyingHasher = new PasswordHasher<object>();
        }

        public string HashPassword(string password)
        {
            return _underlyingHasher.HashPassword(null, password);
        }
    }
}
