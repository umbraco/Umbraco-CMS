using System;
using System.Threading.Tasks;

namespace Umbraco.Cms.Core.Security
{
    public interface ITwoFactorSetupGenerator
    {
        string ProviderName { get; }

        Task<string> GetSetupQrCodeUrlAsync(Guid userOrMemberKey, string secret);
        bool ValidateTwoFactorPIN(string secret, string token);
    }
}
