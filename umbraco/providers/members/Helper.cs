using System;
using System.Collections.Generic;
using System.Text;
using umbraco.cms.businesslogic.member;

namespace umbraco.providers.members
{
    public class Helper
    {
        public static bool GuidPseudoTryParse(string guidToTest)
        {
            Guid memberUniqueId;
            try
            {
                memberUniqueId = new Guid(guidToTest);
            }
            catch (FormatException)
            {
                memberUniqueId = Guid.Empty;
                return false;
            }

            return true;
        }

        public static Member GetMemberByUsernameOrGuid(string userNameOrGuid)
        {
            Member m = null;

            // test if username is a GUID (then it comes from member core login)
            if (GuidPseudoTryParse(userNameOrGuid))
            {
                m = new Member(new Guid(userNameOrGuid));
            }
            else
            {
                m = Member.GetMemberFromLoginName(userNameOrGuid);
            }

            return m;
        }
    }
}
