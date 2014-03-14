using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using Umbraco.Core.Security;

namespace Umbraco.Core.Security
{
    internal static class MembershipProviderExtensions
    {
        /// <summary>
        /// Returns true if the provider specified is a built-in Umbraco users provider
        /// </summary>
        /// <param name="membershipProvider"></param>
        /// <returns></returns>
        public static bool IsUmbracoUsersProvider(this MembershipProvider membershipProvider)
        {
            return (membershipProvider is IUsersMembershipProvider);
        }

        /// <summary>
        /// Returns true if the provider specified is a built-in Umbraco membership provider
        /// </summary>
        /// <param name="membershipProvider"></param>
        /// <returns></returns>
        public static bool IsUmbracoMembershipProvider(this MembershipProvider membershipProvider)
        {
            return (membershipProvider is UmbracoMembershipProviderBase);
        }

        //TODO: Add role provider checks too

        public static UmbracoMembershipProviderBase AsUmbracoMembershipProvider(this MembershipProvider membershipProvider)
        {
            return (UmbracoMembershipProviderBase)membershipProvider;
        }

    }
}
