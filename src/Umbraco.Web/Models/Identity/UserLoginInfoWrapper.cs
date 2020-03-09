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

    internal class UserLoginInfoWrapper2 : IUserLoginInfo
    {
        private readonly Microsoft.AspNetCore.Identity.UserLoginInfo _info;

        public static IUserLoginInfo Wrap(Microsoft.AspNetCore.Identity.UserLoginInfo info) => new UserLoginInfoWrapper2(info);

        private UserLoginInfoWrapper2(Microsoft.AspNetCore.Identity.UserLoginInfo info)
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
