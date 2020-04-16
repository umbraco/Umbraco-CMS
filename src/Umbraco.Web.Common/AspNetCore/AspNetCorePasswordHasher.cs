using Microsoft.AspNetCore.Identity;
using IPasswordHasher = Umbraco.Core.Security.IPasswordHasher;

namespace Umbraco.Web.Common.AspNetCore
{
    public class AspNetCorePasswordHasher : IPasswordHasher
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
