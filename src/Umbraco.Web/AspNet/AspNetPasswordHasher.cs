using System;
using Microsoft.AspNet.Identity;
using IPasswordHasher = Umbraco.Cms.Core.Security.IPasswordHasher;

namespace Umbraco.Web
{
    [Obsolete("Should be removed")]
    public class AspNetPasswordHasher : Cms.Core.Security.IPasswordHasher
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
