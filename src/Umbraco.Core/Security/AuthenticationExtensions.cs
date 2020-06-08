using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Umbraco.Core.BackOffice;

namespace Umbraco.Core.Security
{
    public static class AuthenticationExtensions
    {
        /// <summary>
        /// Ensures that the thread culture is set based on the back office user's culture
        /// </summary>
        /// <param name="identity"></param>
        public static void EnsureCulture(this IIdentity identity)
        {
            if (identity is UmbracoBackOfficeIdentity umbIdentity && umbIdentity.IsAuthenticated)
            {
                Thread.CurrentThread.CurrentUICulture =
                    Thread.CurrentThread.CurrentCulture = UserCultures.GetOrAdd(umbIdentity.Culture, s => new CultureInfo(s));
            }
        }


        /// <summary>
        /// Used so that we aren't creating a new CultureInfo object for every single request
        /// </summary>
        private static readonly ConcurrentDictionary<string, CultureInfo> UserCultures = new ConcurrentDictionary<string, CultureInfo>();
    }
}
