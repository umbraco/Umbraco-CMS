using System;
using System.Collections.Generic;
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
                             RawPasswordValue = password,
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

        public static IEnumerable<IMember> CreateSimpleMember(IMemberType memberType, int amount, Action<int, IMember> onCreating = null)
        {
            var list = new List<IMember>();

            for (int i = 0; i < amount; i++)
            {
                var name = "Member No-" + i;
                var member = new Member(name, "test" + i + "@test.com", "test" + i, "test" + i, memberType);
                member.SetValue("title", name + " member" + i);
                member.SetValue("bodyText", "This is a subpage" + i);
                member.SetValue("author", "John Doe" + i);

                if (onCreating != null)
                {
                    onCreating(i, member);
                }

                member.ResetDirtyProperties(false);
                
                list.Add(member);
            }

            return list;
        } 
    }
}