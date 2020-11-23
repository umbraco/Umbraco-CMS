using System;
using Umbraco.Core.Members;

namespace Umbraco.Infrastructure.Members
{
    public interface IUmbracoMembersUserManager : IUmbracoMembersUserManager<UmbracoMembersIdentityUser>
    {
    }

    public interface IUmbracoMembersUserManager<TUser> : IDisposable
        where TUser : UmbracoMembersIdentityUser
    {
    }
}
