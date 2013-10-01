using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Tests.TestHelpers.Entities
{
    public class MockedMember
    {
        public static Member CreateSimpleContent(IMemberType contentType, string name, string email, string password, string username, int parentId)
        {
            var member = new Member(name, email, username, password, parentId, contentType)
                         {
                             CreatorId = 0,
                             Email = email,
                             Password = password,
                             Username = username
                         };

            member.SetValue("title", name + " member");
            member.SetValue("bodyText", "This is a subpage");
            member.SetValue("author", "John Doe");

            member.ResetDirtyProperties(false);

            return member;
        } 
    }
}