// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services
{
    [TestFixture]
    [Category("Slow")]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, PublishedRepositoryEvents = true, WithApplication = true)]
    public class MemberServiceTests : UmbracoIntegrationTest
    {
        private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

        private IMemberService MemberService => GetRequiredService<IMemberService>();

        [Test]
        public void Can_Update_Member_Property_Values()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IMember member = MemberBuilder.CreateSimpleMember(memberType, "hello", "helloworld@test123.com", "hello", "hello");
            member.SetValue("title", "title of mine");
            member.SetValue("bodyText", "hello world");
            MemberService.Save(member);

            // re-get
            member = MemberService.GetById(member.Id);
            member.SetValue("title", "another title of mine");          // Change a value
            member.SetValue("bodyText", null);                          // Clear a value
            member.SetValue("author", "new author");                    // Add a value
            MemberService.Save(member);

            // re-get
            member = MemberService.GetById(member.Id);
            Assert.AreEqual("another title of mine", member.GetValue("title"));
            Assert.IsNull(member.GetValue("bodyText"));
            Assert.AreEqual("new author", member.GetValue("author"));
        }

        [Test]
        public void Can_Get_By_Username()
        {
            IMemberType memberType = MemberTypeService.Get("member");
            IMember member = new Member("xname", "xemail", "xusername", "xrawpassword", memberType, true);
            MemberService.Save(member);

            IMember member2 = MemberService.GetByUsername(member.Username);

            Assert.IsNotNull(member2);
            Assert.AreEqual(member.Email, member2.Email);
        }

        [Test]
        public void Can_Set_Last_Login_Date()
        {
            DateTime now = DateTime.Now;
            IMemberType memberType = MemberTypeService.Get("member");
            IMember member = new Member("xname", "xemail", "xusername", "xrawpassword", memberType, true)
            {
                LastLoginDate = now,
                UpdateDate = now,
            };
            MemberService.Save(member);

            DateTime newDate = now.AddDays(10);
            member.LastLoginDate = newDate;
            member.UpdateDate = newDate;
            MemberService.Save(member);

            // re-get
            member = MemberService.GetById(member.Id);

            Assert.That(member.LastLoginDate, Is.EqualTo(newDate).Within(1).Seconds);
            Assert.That(member.UpdateDate, Is.EqualTo(newDate).Within(1).Seconds);
        }

        // These might seem like some somewhat pointless tests, but there's some funky things going on in MemberRepository when saving.
        [Test]
        public void Can_Set_Failed_Password_Attempts()
        {
            IMemberType memberType = MemberTypeService.Get("member");
            IMember member = new Member("xname", "xemail", "xusername", "xrawpassword", memberType, true)
            {
                FailedPasswordAttempts = 1,
            };
            MemberService.Save(member);

            member.FailedPasswordAttempts = 2;
            MemberService.Save(member);

            member = MemberService.GetById(member.Id);

            Assert.AreEqual(2, member.FailedPasswordAttempts);
        }

        [Test]
        public void Can_Set_Is_Approved()
        {
            IMemberType memberType = MemberTypeService.Get("member");
            IMember member = new Member("xname", "xemail", "xusername", "xrawpassword", memberType, true);
            MemberService.Save(member);

            member.IsApproved = false;
            MemberService.Save(member);

            member = MemberService.GetById(member.Id);

            Assert.IsFalse(member.IsApproved);
        }

        [Test]
        public void Can_Set_Is_Locked_Out()
        {
            IMemberType memberType = MemberTypeService.Get("member");
            IMember member = new Member("xname", "xemail", "xusername", "xrawpassword", memberType, true)
            {
                IsLockedOut = false,
            };
            MemberService.Save(member);

            member.IsLockedOut = true;
            MemberService.Save(member);

            member = MemberService.GetById(member.Id);

            Assert.IsTrue(member.IsLockedOut);
        }

        [Test]
        public void Can_Set_Last_Lockout_Date()
        {
            DateTime now = DateTime.Now;
            IMemberType memberType = MemberTypeService.Get("member");
            IMember member = new Member("xname", "xemail", "xusername", "xrawpassword", memberType, true)
            {
                LastLockoutDate = now,
            };
            MemberService.Save(member);

            DateTime newDate = now.AddDays(10);
            member.LastLockoutDate = newDate;
            MemberService.Save(member);

            // re-get
            member = MemberService.GetById(member.Id);

            Assert.That(member.LastLockoutDate, Is.EqualTo(newDate).Within(1).Seconds);
        }

        [Test]
        public void Can_set_Last_Login_Date()
        {
            DateTime now = DateTime.Now;
            IMemberType memberType = MemberTypeService.Get("member");
            IMember member = new Member("xname", "xemail", "xusername", "xrawpassword", memberType, true)
            {
                LastLoginDate = now,
            };
            MemberService.Save(member);

            DateTime newDate = now.AddDays(10);
            member.LastLoginDate = newDate;
            MemberService.Save(member);

            // re-get
            member = MemberService.GetById(member.Id);

            Assert.That(member.LastLoginDate, Is.EqualTo(newDate).Within(1).Seconds);
        }

        [Test]
        public void Can_Set_Last_Password_Change_Date()
        {
            DateTime now = DateTime.Now;
            IMemberType memberType = MemberTypeService.Get("member");
            IMember member = new Member("xname", "xemail", "xusername", "xrawpassword", memberType, true)
            {
                LastPasswordChangeDate = now,
            };
            MemberService.Save(member);

            DateTime newDate = now.AddDays(10);
            member.LastPasswordChangeDate = newDate;
            MemberService.Save(member);

            // re-get
            member = MemberService.GetById(member.Id);

            Assert.That(member.LastPasswordChangeDate, Is.EqualTo(newDate).Within(1).Seconds);
        }

        [Test]
        public void Can_Create_Member_With_Properties()
        {
            IMemberType memberType = MemberTypeService.Get("member");
            IMember member = new Member("xname", "xemail", "xusername", "xrawpassword", memberType, true);
            MemberService.Save(member);

            member = MemberService.GetById(member.Id);
            Assert.AreEqual("xemail", member.Email);

            var contentTypeFactory = new PublishedContentTypeFactory(new NoopPublishedModelFactory(), new PropertyValueConverterCollection(() => Enumerable.Empty<IPropertyValueConverter>()), GetRequiredService<IDataTypeService>());
            var pmemberType = new PublishedContentType(memberType, contentTypeFactory);

            var publishedSnapshotAccessor = new TestPublishedSnapshotAccessor();
            var variationContextAccessor = new TestVariationContextAccessor();
            IPublishedContent pmember = PublishedMember.Create(member, pmemberType, false, publishedSnapshotAccessor, variationContextAccessor, GetRequiredService<IPublishedModelFactory>());

            // contains the umbracoMember... properties created when installing, on the member type
            // contains the other properties, that PublishedContentType adds (BuiltinMemberProperties)
            string[] aliases = new[]
            {
                Constants.Conventions.Member.Comments,
                nameof(IMember.Email),
                nameof(IMember.Username),
                nameof(IMember.Comments),
                nameof(IMember.IsApproved),
                nameof(IMember.IsLockedOut),
                nameof(IMember.LastLockoutDate),
                nameof(IMember.CreateDate),
                nameof(IMember.LastLoginDate),
                nameof(IMember.LastPasswordChangeDate)
            };

            var properties = pmember.Properties.ToList();

            Assert.IsTrue(properties.Select(x => x.Alias).ContainsAll(aliases));

            IPublishedProperty email = properties[aliases.IndexOf(nameof(IMember.Email))];
            Assert.AreEqual("xemail", email.GetSourceValue());
        }

        [Test]
        public void Can_Create_Member()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IMember member = MemberBuilder.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
            MemberService.Save(member);

            Assert.AreNotEqual(0, member.Id);
            IMember foundMember = MemberService.GetById(member.Id);
            Assert.IsNotNull(foundMember);
            Assert.AreEqual("test@test.com", foundMember.Email);
        }

        [Test]
        public void Can_Create_Member_With_Long_TLD_In_Email()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IMember member = MemberBuilder.CreateSimpleMember(memberType, "test", "test@test.marketing", "pass", "test");
            MemberService.Save(member);

            Assert.AreNotEqual(0, member.Id);
            IMember foundMember = MemberService.GetById(member.Id);
            Assert.IsNotNull(foundMember);
            Assert.AreEqual("test@test.marketing", foundMember.Email);
        }

        [Test]
        public void Can_Create_Role()
        {
            MemberService.AddRole("MyTestRole");

            IEnumerable<IMemberGroup> found = MemberService.GetAllRoles();

            Assert.AreEqual(1, found.Count());
            Assert.AreEqual("MyTestRole", found.Single().Name);
        }

        [Test]
        public void Can_Create_Duplicate_Role()
        {
            MemberService.AddRole("MyTestRole");
            MemberService.AddRole("MyTestRole");

            IEnumerable<IMemberGroup> found = MemberService.GetAllRoles();

            Assert.AreEqual(1, found.Count());
            Assert.AreEqual("MyTestRole", found.Single().Name);
        }

        [Test]
        public void Can_Get_All_Roles()
        {
            MemberService.AddRole("MyTestRole1");
            MemberService.AddRole("MyTestRole2");
            MemberService.AddRole("MyTestRole3");

            IEnumerable<IMemberGroup> found = MemberService.GetAllRoles();

            Assert.AreEqual(3, found.Count());
        }

        [Test]
        public void Can_Get_All_Roles_IDs()
        {
            MemberService.AddRole("MyTestRole1");
            MemberService.AddRole("MyTestRole2");
            MemberService.AddRole("MyTestRole3");

            IEnumerable<int> found = MemberService.GetAllRolesIds();

            Assert.AreEqual(3, found.Count());
        }

        [Test]
        public void Can_Replace_Roles()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IMember member = MemberBuilder.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
            MemberService.Save(member);

            string[] roleNames1 = new[] { "TR1", "TR2" };
            MemberService.AssignRoles(new[] { member.Id }, roleNames1);
            IEnumerable<string> memberRoles = MemberService.GetAllRoles(member.Id);
            CollectionAssert.AreEquivalent(roleNames1, memberRoles);

            string[] roleNames2 = new[] { "TR3", "TR4" };
            MemberService.ReplaceRoles(new[] { member.Id }, roleNames2);
            memberRoles = MemberService.GetAllRoles(member.Id);
            CollectionAssert.AreEquivalent(roleNames2, memberRoles);
        }

        [Test]
        public void Can_Get_All_Roles_By_Member_Id()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IMember member = MemberBuilder.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
            MemberService.Save(member);

            MemberService.AddRole("MyTestRole1");
            MemberService.AddRole("MyTestRole2");
            MemberService.AddRole("MyTestRole3");
            MemberService.AssignRoles(new[] { member.Id }, new[] { "MyTestRole1", "MyTestRole2" });

            IEnumerable<string> memberRoles = MemberService.GetAllRoles(member.Id);

            Assert.AreEqual(2, memberRoles.Count());
        }

        [Test]
        public void Can_Get_All_Roles_Ids_By_Member_Id()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IMember member = MemberBuilder.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
            MemberService.Save(member);

            MemberService.AddRole("MyTestRole1");
            MemberService.AddRole("MyTestRole2");
            MemberService.AddRole("MyTestRole3");
            MemberService.AssignRoles(new[] { member.Id }, new[] { "MyTestRole1", "MyTestRole2" });

            IEnumerable<int> memberRoles = MemberService.GetAllRolesIds(member.Id);

            Assert.AreEqual(2, memberRoles.Count());
        }

        [Test]
        public void Can_Get_All_Roles_By_Member_Username()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IMember member = MemberBuilder.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
            MemberService.Save(member);

            // need to test with '@' symbol in the lookup
            IMember member2 = MemberBuilder.CreateSimpleMember(memberType, "test2", "test2@test.com", "pass", "test2@test.com");
            MemberService.Save(member2);

            MemberService.AddRole("MyTestRole1");
            MemberService.AddRole("MyTestRole2");
            MemberService.AddRole("MyTestRole3");
            MemberService.AssignRoles(new[] { member.Id, member2.Id }, new[] { "MyTestRole1", "MyTestRole2" });

            IEnumerable<string> memberRoles = MemberService.GetAllRoles("test");
            Assert.AreEqual(2, memberRoles.Count());

            IEnumerable<string> memberRoles2 = MemberService.GetAllRoles("test2@test.com");
            Assert.AreEqual(2, memberRoles2.Count());
        }

        [Test]
        public void Can_Delete_Role()
        {
            MemberService.AddRole("MyTestRole1");

            MemberService.DeleteRole("MyTestRole1", false);

            IEnumerable<IMemberGroup> memberRoles = MemberService.GetAllRoles();

            Assert.AreEqual(0, memberRoles.Count());
        }

        [Test]
        public void Throws_When_Deleting_Assigned_Role()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IMember member = MemberBuilder.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
            MemberService.Save(member);

            MemberService.AddRole("MyTestRole1");
            MemberService.AssignRoles(new[] { member.Id }, new[] { "MyTestRole1", "MyTestRole2" });

            Assert.Throws<InvalidOperationException>(() => MemberService.DeleteRole("MyTestRole1", true));
        }

        [Test]
        public void Can_Get_Members_In_Role()
        {
            MemberService.AddRole("MyTestRole1");
            int roleId;
            using (IScope scope = ScopeProvider.CreateScope())
            {
                roleId = ScopeAccessor.AmbientScope.Database.ExecuteScalar<int>("SELECT id from umbracoNode where [text] = 'MyTestRole1'");
                scope.Complete();
            }

            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            Member member1 = MemberBuilder.CreateSimpleMember(memberType, "test1", "test1@test.com", "pass", "test1");
            MemberService.Save(member1);
            Member member2 = MemberBuilder.CreateSimpleMember(memberType, "test2", "test2@test.com", "pass", "test2");
            MemberService.Save(member2);

            using (IScope scope = ScopeProvider.CreateScope())
            {
                ScopeAccessor.AmbientScope.Database.Insert(new Member2MemberGroupDto { MemberGroup = roleId, Member = member1.Id });
                ScopeAccessor.AmbientScope.Database.Insert(new Member2MemberGroupDto { MemberGroup = roleId, Member = member2.Id });
                scope.Complete();
            }

            IEnumerable<IMember> membersInRole = MemberService.GetMembersInRole("MyTestRole1");
            Assert.AreEqual(2, membersInRole.Count());
        }

        [Test]
        public void Cannot_Save_Member_With_Empty_Name()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IMember member = MemberBuilder.CreateSimpleMember(memberType, string.Empty, "test@test.com", "pass", "test");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => MemberService.Save(member));
        }

        [TestCase("MyTestRole1", "test1", StringPropertyMatchType.StartsWith, 1)]
        [TestCase("MyTestRole1", "test", StringPropertyMatchType.StartsWith, 3)]
        [TestCase("MyTestRole1", "test1", StringPropertyMatchType.Exact, 1)]
        [TestCase("MyTestRole1", "test", StringPropertyMatchType.Exact, 0)]
        [TestCase("MyTestRole1", "st2", StringPropertyMatchType.EndsWith, 1)]
        [TestCase("MyTestRole1", "test%", StringPropertyMatchType.Wildcard, 3)]
        public void Find_Members_In_Role(string roleName1, string usernameToMatch, StringPropertyMatchType matchType, int resultCount)
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            Member member1 = MemberBuilder.CreateSimpleMember(memberType, "test1", "test1@test.com", "pass", "test1");
            MemberService.Save(member1);
            Member member2 = MemberBuilder.CreateSimpleMember(memberType, "test2", "test2@test.com", "pass", "test2");
            MemberService.Save(member2);
            Member member3 = MemberBuilder.CreateSimpleMember(memberType, "test3", "test3@test.com", "pass", "test3");
            MemberService.Save(member3);

            MemberService.AssignRoles(new[] { member1.Id, member2.Id, member3.Id }, new[] { roleName1 });

            IEnumerable<IMember> result = MemberService.FindMembersInRole(roleName1, usernameToMatch, matchType);
            Assert.AreEqual(resultCount, result.Count());
        }

        [Test]
        public void Associate_Members_To_Roles_With_Member_Id()
        {
            MemberService.AddRole("MyTestRole1");

            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            Member member1 = MemberBuilder.CreateSimpleMember(memberType, "test1", "test1@test.com", "pass", "test1");
            MemberService.Save(member1);
            Member member2 = MemberBuilder.CreateSimpleMember(memberType, "test2", "test2@test.com", "pass", "test2");
            MemberService.Save(member2);

            // temp make sure they exist
            Assert.IsNotNull(MemberService.GetById(member1.Id));
            Assert.IsNotNull(MemberService.GetById(member2.Id));

            MemberService.AssignRoles(new[] { member1.Id, member2.Id }, new[] { "MyTestRole1" });

            IEnumerable<IMember> membersInRole = MemberService.GetMembersInRole("MyTestRole1");

            Assert.AreEqual(2, membersInRole.Count());
        }

        [Test]
        public void Associate_Members_To_Roles_With_Member_Id_Casing()
        {
            MemberService.AddRole("MyTestRole1");

            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            Member member1 = MemberBuilder.CreateSimpleMember(memberType, "test1", "test1@test.com", "pass", "test1");
            MemberService.Save(member1);
            Member member2 = MemberBuilder.CreateSimpleMember(memberType, "test2", "test2@test.com", "pass", "test2");
            MemberService.Save(member2);

            // temp make sure they exist
            Assert.IsNotNull(MemberService.GetById(member1.Id));
            Assert.IsNotNull(MemberService.GetById(member2.Id));

            MemberService.AssignRoles(new[] { member1.Id, member2.Id }, new[] { "mytestrole1" });

            IEnumerable<IMember> membersInRole = MemberService.GetMembersInRole("MyTestRole1");

            Assert.AreEqual(2, membersInRole.Count());
        }

        [Test]
        public void Associate_Members_To_Roles_With_Member_Username()
        {
            MemberService.AddRole("MyTestRole1");

            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            Member member1 = MemberBuilder.CreateSimpleMember(memberType, "test1", "test1@test.com", "pass", "test1");
            MemberService.Save(member1);
            Member member2 = MemberBuilder.CreateSimpleMember(memberType, "test2", "test2@test.com", "pass", "test2");
            MemberService.Save(member2);

            MemberService.AssignRoles(new[] { member1.Username, member2.Username }, new[] { "MyTestRole1" });

            IEnumerable<IMember> membersInRole = MemberService.GetMembersInRole("MyTestRole1");

            Assert.AreEqual(2, membersInRole.Count());
        }

        [Test]
        public void Associate_Members_To_Roles_With_Member_Username_Containing_At_Symbols()
        {
            MemberService.AddRole("MyTestRole1");

            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            Member member1 = MemberBuilder.CreateSimpleMember(memberType, "test1", "test1@test.com", "pass", "test1@test.com");
            MemberService.Save(member1);
            Member member2 = MemberBuilder.CreateSimpleMember(memberType, "test2", "test2@test.com", "pass", "test2@test.com");
            MemberService.Save(member2);

            MemberService.AssignRoles(new[] { member1.Username, member2.Username }, new[] { "MyTestRole1" });

            IEnumerable<IMember> membersInRole = MemberService.GetMembersInRole("MyTestRole1");

            Assert.AreEqual(2, membersInRole.Count());
        }

        [Test]
        public void Associate_Members_To_Roles_With_New_Role()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            Member member1 = MemberBuilder.CreateSimpleMember(memberType, "test1", "test1@test.com", "pass", "test1");
            MemberService.Save(member1);
            Member member2 = MemberBuilder.CreateSimpleMember(memberType, "test2", "test2@test.com", "pass", "test2");
            MemberService.Save(member2);

            // implicitly create the role
            MemberService.AssignRoles(new[] { member1.Username, member2.Username }, new[] { "MyTestRole1" });

            IEnumerable<IMember> membersInRole = MemberService.GetMembersInRole("MyTestRole1");

            Assert.AreEqual(2, membersInRole.Count());
        }

        [Test]
        public void Remove_Members_From_Roles_With_Member_Id()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            Member member1 = MemberBuilder.CreateSimpleMember(memberType, "test1", "test1@test.com", "pass", "test1");
            MemberService.Save(member1);
            Member member2 = MemberBuilder.CreateSimpleMember(memberType, "test2", "test2@test.com", "pass", "test2");
            MemberService.Save(member2);

            MemberService.AssignRoles(new[] { member1.Id, member2.Id }, new[] { "MyTestRole1", "MyTestRole2" });

            MemberService.DissociateRoles(new[] { member1.Id }, new[] { "MyTestRole1" });
            MemberService.DissociateRoles(new[] { member1.Id, member2.Id }, new[] { "MyTestRole2" });

            IEnumerable<IMember> membersInRole = MemberService.GetMembersInRole("MyTestRole1");
            Assert.AreEqual(1, membersInRole.Count());
            membersInRole = MemberService.GetMembersInRole("MyTestRole2");
            Assert.AreEqual(0, membersInRole.Count());
        }

        [Test]
        public void Remove_Members_From_Roles_With_Member_Username()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            Member member1 = MemberBuilder.CreateSimpleMember(memberType, "test1", "test1@test.com", "pass", "test1");
            MemberService.Save(member1);
            Member member2 = MemberBuilder.CreateSimpleMember(memberType, "test2", "test2@test.com", "pass", "test2");
            MemberService.Save(member2);

            MemberService.AssignRoles(new[] { member1.Username, member2.Username }, new[] { "MyTestRole1", "MyTestRole2" });

            MemberService.DissociateRoles(new[] { member1.Username }, new[] { "MyTestRole1" });
            MemberService.DissociateRoles(new[] { member1.Username, member2.Username }, new[] { "MyTestRole2" });

            IEnumerable<IMember> membersInRole = MemberService.GetMembersInRole("MyTestRole1");
            Assert.AreEqual(1, membersInRole.Count());
            membersInRole = MemberService.GetMembersInRole("MyTestRole2");
            Assert.AreEqual(0, membersInRole.Count());
        }

        [Test]
        public void Can_Delete_member()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IMember member = MemberBuilder.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
            MemberService.Save(member);

            MemberService.Delete(member);
            IMember deleted = MemberService.GetById(member.Id);

            // Assert
            Assert.That(deleted, Is.Null);
        }

        [Test]
        public void Exists_By_Username()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IMember member = MemberBuilder.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
            MemberService.Save(member);
            IMember member2 = MemberBuilder.CreateSimpleMember(memberType, "test", "test2@test.com", "pass", "test2@test.com");
            MemberService.Save(member2);

            Assert.IsTrue(MemberService.Exists("test"));
            Assert.IsFalse(MemberService.Exists("notFound"));
            Assert.IsTrue(MemberService.Exists("test2@test.com"));
        }

        [Test]
        public void Exists_By_Id()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IMember member = MemberBuilder.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
            MemberService.Save(member);

            Assert.IsTrue(MemberService.Exists(member.Id));
            Assert.IsFalse(MemberService.Exists(9876));
        }

        [Test]
        public void Tracks_Dirty_Changes()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IMember member = MemberBuilder.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
            MemberService.Save(member);

            IMember resolved = MemberService.GetByEmail(member.Email);

            // NOTE: This will not trigger a property isDirty because this is not based on a 'Property', it is
            // just a c# property of the Member object
            resolved.Email = "changed@test.com";

            // NOTE: This will not trigger a property isDirty for the same reason above, but this is a new change, so leave this to make sure.
            resolved.FailedPasswordAttempts = 1234;

            // NOTE: this WILL trigger a property isDirty because setting this c# property actually sets a value of
            // the underlying 'Property'
            resolved.Comments = "This will make it dirty";

            var dirtyMember = (ICanBeDirty)resolved;
            var dirtyProperties = resolved.Properties.Where(x => x.IsDirty()).ToList();
            Assert.IsTrue(dirtyMember.IsDirty());
            Assert.AreEqual(1, dirtyProperties.Count);

            // Assert that email and failed password attempts is still set as dirty on the member it self
            Assert.IsTrue(dirtyMember.IsPropertyDirty(nameof(resolved.Email)));
            Assert.IsTrue(dirtyMember.IsPropertyDirty(nameof(resolved.FailedPasswordAttempts)));

            // Comment will also be marked as dirty on the member object because content base merges dirty properties.
            Assert.IsTrue(dirtyMember.IsPropertyDirty(Constants.Conventions.Member.Comments));
            Assert.AreEqual(3, dirtyMember.GetDirtyProperties().Count());
        }

        [Test]
        public void Get_By_Email()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IMember member = MemberBuilder.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
            MemberService.Save(member);

            Assert.IsNotNull(MemberService.GetByEmail(member.Email));
            Assert.IsNull(MemberService.GetByEmail("do@not.find"));
        }

        [Test]
        public void Get_Member_Name()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IMember member = MemberBuilder.CreateSimpleMember(memberType, "Test Real Name", "test@test.com", "pass", "testUsername");
            MemberService.Save(member);

            Assert.AreEqual("Test Real Name", member.Name);
        }

        [Test]
        public void Get_By_Username()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IMember member = MemberBuilder.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
            MemberService.Save(member);

            Assert.IsNotNull(MemberService.GetByUsername(member.Username));
            Assert.IsNull(MemberService.GetByUsername("notFound"));
        }

        [Test]
        public void Get_By_Object_Id()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IMember member = MemberBuilder.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
            MemberService.Save(member);

            Assert.IsNotNull(MemberService.GetById(member.Id));
            Assert.IsNull(MemberService.GetById(9876));
        }

        [Test]
        public void Get_All_Paged_Members()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10);
            MemberService.Save(members);

            IEnumerable<IMember> found = MemberService.GetAll(0, 2, out long totalRecs);

            Assert.AreEqual(2, found.Count());
            Assert.AreEqual(10, totalRecs);
            Assert.AreEqual("test0", found.First().Username);
            Assert.AreEqual("test1", found.Last().Username);
        }

        [Test]
        public void Get_All_Paged_Members_With_Filter()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10);
            MemberService.Save(members);

            IEnumerable<IMember> found = MemberService.GetAll(0, 2, out long totalRecs, "username", Direction.Ascending, true, null, "Member No-");

            Assert.AreEqual(2, found.Count());
            Assert.AreEqual(10, totalRecs);
            Assert.AreEqual("test0", found.First().Username);
            Assert.AreEqual("test1", found.Last().Username);

            found = MemberService.GetAll(0, 2, out totalRecs, "username", Direction.Ascending, true, null, "Member No-5");

            Assert.AreEqual(1, found.Count());
            Assert.AreEqual(1, totalRecs);
            Assert.AreEqual("test5", found.First().Username);
        }

        [Test]
        public void Find_By_Name_Starts_With()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10);
            MemberService.Save(members);

            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "Bob", "hello@test.com", "hello", "hello");
            MemberService.Save(customMember);

            IEnumerable<IMember> found = MemberService.FindMembersByDisplayName("B", 0, 100, out long totalRecs, StringPropertyMatchType.StartsWith);

            Assert.AreEqual(1, found.Count());
        }

        [Test]
        public void Find_By_Email_Starts_With()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10);
            MemberService.Save(members);

            // don't find this
            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            MemberService.Save(customMember);

            IEnumerable<IMember> found = MemberService.FindByEmail("tes", 0, 100, out long totalRecs, StringPropertyMatchType.StartsWith);

            Assert.AreEqual(10, found.Count());
        }

        [Test]
        public void Find_By_Email_Ends_With()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10);
            MemberService.Save(members);

            // include this
            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            MemberService.Save(customMember);

            IEnumerable<IMember> found = MemberService.FindByEmail("test.com", 0, 100, out long totalRecs, StringPropertyMatchType.EndsWith);

            Assert.AreEqual(11, found.Count());
        }

        [Test]
        public void Find_By_Email_Contains()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10);
            MemberService.Save(members);

            // include this
            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            MemberService.Save(customMember);

            IEnumerable<IMember> found = MemberService.FindByEmail("test", 0, 100, out long totalRecs, StringPropertyMatchType.Contains);

            Assert.AreEqual(11, found.Count());
        }

        [Test]
        public void Find_By_Email_Exact()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10);
            MemberService.Save(members);

            // include this
            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            MemberService.Save(customMember);

            IEnumerable<IMember> found = MemberService.FindByEmail("hello@test.com", 0, 100, out long totalRecs, StringPropertyMatchType.Exact);

            Assert.AreEqual(1, found.Count());
        }

        [Test]
        public void Find_By_Login_Starts_With()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10);
            MemberService.Save(members);

            // don't find this
            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            MemberService.Save(customMember);

            IEnumerable<IMember> found = MemberService.FindByUsername("tes", 0, 100, out long totalRecs, StringPropertyMatchType.StartsWith);

            Assert.AreEqual(10, found.Count());
        }

        [Test]
        public void Find_By_Login_Ends_With()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10);
            MemberService.Save(members);

            // include this
            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            MemberService.Save(customMember);

            IEnumerable<IMember> found = MemberService.FindByUsername("llo", 0, 100, out long totalRecs, StringPropertyMatchType.EndsWith);

            Assert.AreEqual(1, found.Count());
        }

        [Test]
        public void Find_By_Login_Contains()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10);
            MemberService.Save(members);

            // include this
            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hellotest");
            MemberService.Save(customMember);

            IEnumerable<IMember> found = MemberService.FindByUsername("test", 0, 100, out long totalRecs, StringPropertyMatchType.Contains);

            Assert.AreEqual(11, found.Count());
        }

        [Test]
        public void Find_By_Login_Exact()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10);
            MemberService.Save(members);

            // include this
            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            MemberService.Save(customMember);

            IEnumerable<IMember> found = MemberService.FindByUsername("hello", 0, 100, out long totalRecs, StringPropertyMatchType.Exact);

            Assert.AreEqual(1, found.Count());
        }

        [Test]
        public void Get_By_Property_String_Value_Exact()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10);
            MemberService.Save(members);
            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            MemberService.Save(customMember);

            IEnumerable<IMember> found = MemberService.GetMembersByPropertyValue(
                "title", "hello member", StringPropertyMatchType.Exact);

            Assert.AreEqual(1, found.Count());
        }

        [Test]
        public void Get_By_Property_String_Value_Contains()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10);
            MemberService.Save(members);
            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            MemberService.Save(customMember);

            IEnumerable<IMember> found = MemberService.GetMembersByPropertyValue(
                "title", " member", StringPropertyMatchType.Contains);

            Assert.AreEqual(11, found.Count());
        }

        [Test]
        public void Get_By_Property_String_Value_Starts_With()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10);
            MemberService.Save(members);
            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            MemberService.Save(customMember);

            IEnumerable<IMember> found = MemberService.GetMembersByPropertyValue(
                "title", "Member No", StringPropertyMatchType.StartsWith);

            Assert.AreEqual(10, found.Count());
        }

        [Test]
        public void Get_By_Property_String_Value_Ends_With()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10);
            MemberService.Save(members);
            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.SetValue("title", "title of mine");
            MemberService.Save(customMember);

            IEnumerable<IMember> found = MemberService.GetMembersByPropertyValue(
                "title", "mine", StringPropertyMatchType.EndsWith);

            Assert.AreEqual(1, found.Count());
        }

        [Test]
        public void Get_By_Property_Int_Value_Exact()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            memberType.AddPropertyType(
                new PropertyType(
                    ShortStringHelper,
                    Constants.PropertyEditors.Aliases.Integer,
                    ValueStorageType.Integer,
                    "number")
                    {
                        Name = "Number",
                        DataTypeId = -51,  // NOTE: This is what really determines the db type - the above definition doesn't really do anything
                    }, "content", "Content");
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10, (i, member) => member.SetValue("number", i));
            MemberService.Save(members);

            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.SetValue("number", 2);
            MemberService.Save(customMember);

            IEnumerable<IMember> found = MemberService.GetMembersByPropertyValue(
                "number", 2, ValuePropertyMatchType.Exact);

            Assert.AreEqual(2, found.Count());
        }

        [Test]
        public void Get_By_Property_Int_Value_Greater_Than()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            memberType.AddPropertyType(
                new PropertyType(
                    ShortStringHelper,
                    Constants.PropertyEditors.Aliases.Integer,
                    ValueStorageType.Integer,
                    "number")
                    {
                        Name = "Number",
                        DataTypeId = -51,  // NOTE: This is what really determines the db type - the above definition doesn't really do anything
                    }, "content", "Content");
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10, (i, member) => member.SetValue("number", i));
            MemberService.Save(members);

            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.SetValue("number", 10);
            MemberService.Save(customMember);

            IEnumerable<IMember> found = MemberService.GetMembersByPropertyValue(
                "number", 3, ValuePropertyMatchType.GreaterThan);

            Assert.AreEqual(7, found.Count());
        }

        [Test]
        public void Get_By_Property_Int_Value_Greater_Than_Equal_To()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            memberType.AddPropertyType(
                new PropertyType(
                    ShortStringHelper,
                    Constants.PropertyEditors.Aliases.Integer,
                    ValueStorageType.Integer,
                    "number")
                {
                    Name = "Number",
                    DataTypeId = -51,  // NOTE: This is what really determines the db type - the above definition doesn't really do anything
                }, "content", "Content");
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10, (i, member) => member.SetValue("number", i));
            MemberService.Save(members);

            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.SetValue("number", 10);
            MemberService.Save(customMember);

            IEnumerable<IMember> found = MemberService.GetMembersByPropertyValue(
                "number", 3, ValuePropertyMatchType.GreaterThanOrEqualTo);

            Assert.AreEqual(8, found.Count());
        }

        [Test]
        public void Get_By_Property_Int_Value_Less_Than()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            memberType.AddPropertyType(
                new PropertyType(
                    ShortStringHelper,
                    Constants.PropertyEditors.Aliases.DateTime,
                    ValueStorageType.Date,
                    "number")
                    {
                        Name = "Number",
                        DataTypeId = -51,  // NOTE: This is what really determines the db type - the above definition doesn't really do anything
                    }, "content", "Content");
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10, (i, member) => member.SetValue("number", i));
            MemberService.Save(members);

            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.SetValue("number", 1);
            MemberService.Save(customMember);

            IEnumerable<IMember> found = MemberService.GetMembersByPropertyValue(
                "number", 5, ValuePropertyMatchType.LessThan);

            Assert.AreEqual(6, found.Count());
        }

        [Test]
        public void Get_By_Property_Int_Value_Less_Than_Or_Equal()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            memberType.AddPropertyType(
                new PropertyType(
                    ShortStringHelper,
                    Constants.PropertyEditors.Aliases.Integer,
                    ValueStorageType.Integer,
                    "number")
                    {
                        Name = "Number",
                        DataTypeId = -51,  // NOTE: This is what really determines the db type - the above definition doesn't really do anything
                    }, "content", "Content");
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10, (i, member) => member.SetValue("number", i));
            MemberService.Save(members);

            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.SetValue("number", 1);
            MemberService.Save(customMember);

            IEnumerable<IMember> found = MemberService.GetMembersByPropertyValue(
                "number", 5, ValuePropertyMatchType.LessThanOrEqualTo);

            Assert.AreEqual(7, found.Count());
        }

        [Test]
        public void Get_By_Property_Date_Value_Exact()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            memberType.AddPropertyType(
                new PropertyType(
                    ShortStringHelper,
                    Constants.PropertyEditors.Aliases.Integer,
                    ValueStorageType.Integer,
                    "date")
                    {
                        Name = "Date",
                        DataTypeId = -36,  // NOTE: This is what really determines the db type - the above definition doesn't really do anything
                    }, "content", "Content");
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10, (i, member) => member.SetValue("date", new DateTime(2013, 12, 20, 1, i, 0)));
            MemberService.Save(members);

            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.SetValue("date", new DateTime(2013, 12, 20, 1, 2, 0));
            MemberService.Save(customMember);

            IEnumerable<IMember> found = MemberService.GetMembersByPropertyValue(
                "date", new DateTime(2013, 12, 20, 1, 2, 0), ValuePropertyMatchType.Exact);

            Assert.AreEqual(2, found.Count());
        }

        [Test]
        public void Get_By_Property_Date_Value_Greater_Than()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            memberType.AddPropertyType(
                new PropertyType(
                    ShortStringHelper,
                    Constants.PropertyEditors.Aliases.Integer,
                    ValueStorageType.Integer,
                    "date")
                    {
                        Name = "Date",
                        DataTypeId = -36,  // NOTE: This is what really determines the db type - the above definition doesn't really do anything
                    }, "content", "Content");
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10, (i, member) => member.SetValue("date", new DateTime(2013, 12, 20, 1, i, 0)));
            MemberService.Save(members);

            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.SetValue("date", new DateTime(2013, 12, 20, 1, 10, 0));
            MemberService.Save(customMember);

            IEnumerable<IMember> found = MemberService.GetMembersByPropertyValue(
                "date", new DateTime(2013, 12, 20, 1, 3, 0), ValuePropertyMatchType.GreaterThan);

            Assert.AreEqual(7, found.Count());
        }

        [Test]
        public void Get_By_Property_Date_Value_Greater_Than_Equal_To()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            memberType.AddPropertyType(
                new PropertyType(
                    ShortStringHelper,
                    Constants.PropertyEditors.Aliases.Integer,
                    ValueStorageType.Integer,
                    "date")
                    {
                        Name = "Date",
                        DataTypeId = -36,  // NOTE: This is what really determines the db type - the above definition doesn't really do anything
                    }, "content", "Content");
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10, (i, member) => member.SetValue("date", new DateTime(2013, 12, 20, 1, i, 0)));
            MemberService.Save(members);

            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.SetValue("date", new DateTime(2013, 12, 20, 1, 10, 0));
            MemberService.Save(customMember);

            IEnumerable<IMember> found = MemberService.GetMembersByPropertyValue(
                "date", new DateTime(2013, 12, 20, 1, 3, 0), ValuePropertyMatchType.GreaterThanOrEqualTo);

            Assert.AreEqual(8, found.Count());
        }

        [Test]
        public void Get_By_Property_Date_Value_Less_Than()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            memberType.AddPropertyType(
                new PropertyType(
                    ShortStringHelper,
                    Constants.PropertyEditors.Aliases.Integer,
                    ValueStorageType.Integer,
                    "date")
                    {
                        Name = "Date",
                        DataTypeId = -36,  // NOTE: This is what really determines the db type - the above definition doesn't really do anything
                    }, "content", "Content");
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10, (i, member) => member.SetValue("date", new DateTime(2013, 12, 20, 1, i, 0)));
            MemberService.Save(members);

            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.SetValue("date", new DateTime(2013, 12, 20, 1, 1, 0));
            MemberService.Save(customMember);

            IEnumerable<IMember> found = MemberService.GetMembersByPropertyValue(
                "date", new DateTime(2013, 12, 20, 1, 5, 0), ValuePropertyMatchType.LessThan);

            Assert.AreEqual(6, found.Count());
        }

        [Test]
        public void Get_By_Property_Date_Value_Less_Than_Or_Equal()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            memberType.AddPropertyType(
                new PropertyType(
                    ShortStringHelper,
                    Constants.PropertyEditors.Aliases.Integer,
                    ValueStorageType.Integer,
                    "date")
                    {
                        Name = "Date",
                        DataTypeId = -36,  // NOTE: This is what really determines the db type - the above definition doesn't really do anything
                    }, "content", "Content");
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10, (i, member) => member.SetValue("date", new DateTime(2013, 12, 20, 1, i, 0)));
            MemberService.Save(members);

            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.SetValue("date", new DateTime(2013, 12, 20, 1, 1, 0));
            MemberService.Save(customMember);

            IEnumerable<IMember> found = MemberService.GetMembersByPropertyValue(
                "date", new DateTime(2013, 12, 20, 1, 5, 0), ValuePropertyMatchType.LessThanOrEqualTo);

            Assert.AreEqual(7, found.Count());
        }

        [Test]
        public void Count_All_Members()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10);
            MemberService.Save(members);
            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            MemberService.Save(customMember);

            int found = MemberService.GetCount(MemberCountType.All);

            Assert.AreEqual(11, found);
        }

        [Test]
        public void Count_All_Locked_Members()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10, (i, member) => member.IsLockedOut = i % 2 == 0);
            MemberService.Save(members);

            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.IsLockedOut = true;
            MemberService.Save(customMember);

            int found = MemberService.GetCount(MemberCountType.LockedOut);

            Assert.AreEqual(6, found);
        }

        [Test]
        public void Count_All_Approved_Members()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            IEnumerable<IMember> members = MemberBuilder.CreateMultipleSimpleMembers(memberType, 10, (i, member) => member.IsApproved = i % 2 == 0);
            MemberService.Save(members);

            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.IsApproved = false;
            MemberService.Save(customMember);

            int found = MemberService.GetCount(MemberCountType.Approved);

            Assert.AreEqual(5, found);
        }

        [Test]
        public void Setting_Property_On_Built_In_Member_Property_When_Property_Doesnt_Exist_On_Type_Is_Ok()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            memberType.RemovePropertyType(Constants.Conventions.Member.Comments);
            MemberTypeService.Save(memberType);
            Assert.IsFalse(memberType.PropertyTypes.Any(x => x.Alias == Constants.Conventions.Member.Comments));

            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");

            // this should not throw an exception
            customMember.Comments = "hello world";
            MemberService.Save(customMember);

            IMember found = MemberService.GetById(customMember.Id);

            Assert.IsTrue(found.Comments.IsNullOrWhiteSpace());
        }

        [Test]
        public void New_Member_Approved_By_Default()
        {
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);

            Member customMember = MemberBuilder.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            MemberService.Save(customMember);

            IMember found = MemberService.GetById(customMember.Id);

            Assert.IsTrue(found.IsApproved);
        }

        [Test]
        public void Can_CreateWithIdentity()
        {
            // Arrange
            IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
            MemberTypeService.Save(memberType);
            string username = Path.GetRandomFileName();

            // Act
            IMember member = MemberService.CreateMemberWithIdentity(username, $"{username}@domain.email", Path.GetFileNameWithoutExtension(username), memberType);
            IMember found = MemberService.GetById(member.Id);

            // Assert
            Assert.IsNotNull(member, "Verifying a member instance has been created");
            Assert.IsNotNull(found, "Verifying the created member instance has been retrieved");
            Assert.IsTrue(found?.Name == member?.Name, "Verifying the retrieved member instance has the expected name");
        }
    }
}
