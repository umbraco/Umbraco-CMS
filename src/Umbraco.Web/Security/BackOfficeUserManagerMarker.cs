using System;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// This class is only here due to the fact that IOwinContext Get / Set only work in generics, if they worked
    /// with regular 'object' then we wouldn't have to use this work around but because of that we have to use this
    /// class to resolve the 'real' type of the registered user manager
    /// </summary>
    /// <typeparam name="TManager"></typeparam>
    /// <typeparam name="TUser"></typeparam>
    internal class BackOfficeUserManagerMarker<TManager, TUser> : IBackOfficeUserManagerMarker
        where TManager : BackOfficeUserManager<TUser>
        where TUser : BackOfficeIdentityUser
    {
        public BackOfficeUserManager<BackOfficeIdentityUser> GetManager(IOwinContext owin)
        {
            var mgr = owin.Get<TManager>() as BackOfficeUserManager<BackOfficeIdentityUser>;
            if (mgr == null) throw new InvalidOperationException("Could not cast the registered back office user of type " + typeof(TManager) + " to " + typeof(BackOfficeUserManager<BackOfficeIdentityUser>));
            return mgr;
        }
    }
}
