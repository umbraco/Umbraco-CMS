using System;
using Microsoft.Owin;
using Umbraco.Web.Models.Identity;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// This class is only here due to the fact that IOwinContext Get / Set only work in generics, if they worked
    /// with regular 'object' then we wouldn't have to use this work around but because of that we have to use this
    /// class to resolve the 'real' type of the registered user manager
    /// </summary>
    /// <typeparam name="TManager"></typeparam>
    /// <typeparam name="TUser"></typeparam>
    internal class BackOfficeUserManagerMarker2<TManager, TUser> : IBackOfficeUserManagerMarker2
        where TManager : BackOfficeUserManager2<TUser>
        where TUser : BackOfficeIdentityUser
    {
        public BackOfficeUserManager2<BackOfficeIdentityUser> GetManager(IOwinContext owin)
        {
            var mgr = owin.Get<TManager>() as BackOfficeUserManager2<BackOfficeIdentityUser>;
            if (mgr == null) throw new InvalidOperationException("Could not cast the registered back office user of type " + typeof(TManager) + " to " + typeof(BackOfficeUserManager2<BackOfficeIdentityUser>));
            return mgr;
        }
    }
}
