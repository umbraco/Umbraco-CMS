using Microsoft.AspNet.Identity;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Web.Models.Identity
{
    internal class UserLoginInfoWrapper : IUserLoginInfo
    {
        private readonly UserLoginInfo _info;

        public static IUserLoginInfo Wrap(UserLoginInfo info) => new UserLoginInfoWrapper(info);

        private UserLoginInfoWrapper(UserLoginInfo info)
        {
            _info = info;
        }

        public string LoginProvider
        {
            get => _info.LoginProvider;
            set => _info.LoginProvider = value;
        }

        public string ProviderKey
        {
            get => _info.ProviderKey;
            set => _info.ProviderKey = value;
        }
    }
}
