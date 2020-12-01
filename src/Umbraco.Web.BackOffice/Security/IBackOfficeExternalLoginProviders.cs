using Microsoft.AspNetCore.Authentication.OAuth;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Umbraco.Web.BackOffice.Security
{

    /// <summary>
    /// Service to return <see cref="BackOfficeExternalLoginProvider"/> instances
    /// </summary>
    public interface IBackOfficeExternalLoginProviders
    {
        BackOfficeExternalLoginProvider Get(string authenticationType);

        IEnumerable<BackOfficeExternalLoginProvider> GetBackOfficeProviders();

        /// <summary>
        /// Returns the authentication type for the last registered external login (oauth) provider that specifies an auto-login redirect option
        /// </summary>
        /// <param name="manager"></param>
        /// <returns></returns>
        string GetAutoLoginProvider();

        /// <summary>
        /// Returns true if there is any external provider that has the Deny Local Login option configured
        /// </summary>
        /// <returns></returns>
        bool HasDenyLocalLogin();
    }

}
