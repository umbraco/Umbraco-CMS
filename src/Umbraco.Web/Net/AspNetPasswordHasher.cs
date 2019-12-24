using Microsoft.AspNet.Identity;
using IPasswordHasher = Umbraco.Core.Security.IPasswordHasher;

namespace Umbraco.Web
{
    public class AspNetPasswordHasher : IPasswordHasher
    {
        private PasswordHasher _underlyingHasher;

        public AspNetPasswordHasher()
        {
            _underlyingHasher = new PasswordHasher();
        }

        public string HashPassword(string password)
        {
            return _underlyingHasher.HashPassword(password);
        }
    }
}
