using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Owin.Security.DataProtection;
using Umbraco.Core.Security;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// Adapted from Microsoft.AspNet.Identity.Owin.DataProtectorTokenProvider
    /// </summary>
    public class OwinDataProtectorTokenProvider<TUser> : IUserTwoFactorTokenProvider<TUser> where TUser : BackOfficeIdentityUser
    {
        public TimeSpan TokenLifespan { get; set; }
        private static readonly Encoding _defaultEncoding = new UTF8Encoding(false, true);
        private readonly IDataProtector _protector;

        public OwinDataProtectorTokenProvider(IDataProtector protector)
        {
            _protector = protector ?? throw new ArgumentNullException(nameof(protector));
            TokenLifespan = TimeSpan.FromDays(1);
        }

        public async Task<string> GenerateAsync(string purpose, UserManager<TUser> manager, TUser user)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            if (user == null) throw new ArgumentNullException(nameof(user));

            var ms = new MemoryStream();
            using (var writer = new BinaryWriter(ms, _defaultEncoding, true))
            {
                writer.Write(DateTimeOffset.UtcNow.UtcTicks);
                writer.Write(Convert.ToString(user.Id, CultureInfo.InvariantCulture));
                writer.Write(purpose ?? "");

                string stamp = null;
                if (manager.SupportsUserSecurityStamp)
                {
                    stamp = await manager.GetSecurityStampAsync(user);
                }
                writer.Write(stamp ?? "");
            }

            var protectedBytes = _protector.Protect(ms.ToArray());
            return Convert.ToBase64String(protectedBytes);
        }

        public async Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
        {
            if (string.IsNullOrWhiteSpace(token)) throw new ArgumentNullException(nameof(token));
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            if (user == null) throw new ArgumentNullException(nameof(user));

            try
            {
                var unprotectedData = _protector.Unprotect(Convert.FromBase64String(token));
                var ms = new MemoryStream(unprotectedData);
                using (var reader = new BinaryReader(ms, _defaultEncoding, true))
                {
                    var creationTime = new DateTimeOffset(reader.ReadInt64(), TimeSpan.Zero);
                    var expirationTime = creationTime + TokenLifespan;
                    if (expirationTime < DateTimeOffset.UtcNow)
                    {
                        return false;
                    }

                    var userId = reader.ReadString();
                    if (!string.Equals(userId, Convert.ToString(user.Id, CultureInfo.InvariantCulture)))
                    {
                        return false;
                    }

                    var purp = reader.ReadString();
                    if (!string.Equals(purp, purpose))
                    {
                        return false;
                    }

                    var stamp = reader.ReadString();
                    if (reader.PeekChar() != -1)
                    {
                        return false;
                    }

                    if (manager.SupportsUserSecurityStamp)
                    {
                        var expectedStamp = await manager.GetSecurityStampAsync(user);
                        return stamp == expectedStamp;
                    }

                    return stamp == "";
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
                // Do not leak exception
            }

            return false;
        }

        public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
        {
            // This token provider is designed for flows such as password reset and account confirmation
            return Task.FromResult(false);
        }
    }
}
