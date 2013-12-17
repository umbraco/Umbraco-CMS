using System;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Tests.TestHelpers.Entities
{
    public class MockedMember
    {
        public static Member CreateSimpleMember(IMemberType contentType, string name, string email, string password, string username, Guid? key = null)
        {
            var member = new Member(name, email, username, password, contentType)
                         {
                             CreatorId = 0,
                             Email = email,
                             Password = password,
                             Username = username
                         };

            if (key.HasValue)
            {
                member.Key = key.Value;
            }

            member.SetValue("title", name + " member");
            member.SetValue("bodyText", "This is a subpage");
            member.SetValue("author", "John Doe");

            member.ResetDirtyProperties(false);

            return member;
        } 
    }
}