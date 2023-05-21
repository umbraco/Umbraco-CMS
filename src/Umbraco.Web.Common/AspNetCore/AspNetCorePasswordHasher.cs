using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Web.Common.AspNetCore;

public class AspNetCorePasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _underlyingHasher;

    public AspNetCorePasswordHasher() => _underlyingHasher = new PasswordHasher<object>();

    public string HashPassword(string password) => _underlyingHasher.HashPassword(null!, password);
}
