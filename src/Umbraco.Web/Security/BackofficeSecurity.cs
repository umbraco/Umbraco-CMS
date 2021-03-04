using System;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Web.Security
{
    // NOTE: Moved to netcore
    public class BackOfficeSecurity : IBackOfficeSecurity
    {
        public IUser CurrentUser => throw new NotImplementedException();

        public Attempt<int> GetUserId()
        {
            throw new NotImplementedException();
        }

        public bool IsAuthenticated()
        {
            throw new NotImplementedException();
        }

        public bool UserHasSectionAccess(string section, IUser user)
        {
            throw new NotImplementedException();
        }

    }
}
