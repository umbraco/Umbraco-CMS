using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Services
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture, RequiresSTA]
    public class MemberServiceTests : BaseServiceTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void Can_Create_Role()
        {
            ServiceContext.MemberService.AddRole("MyTestRole");

            var found = ServiceContext.MemberService.GetAllRoles();

            Assert.AreEqual(1, found.Count());
            Assert.AreEqual("MyTestRole", found.Single());
        }

        [Test]
        public void Can_Create_Duplicate_Role()
        {
            ServiceContext.MemberService.AddRole("MyTestRole");
            ServiceContext.MemberService.AddRole("MyTestRole");

            var found = ServiceContext.MemberService.GetAllRoles();

            Assert.AreEqual(1, found.Count());
            Assert.AreEqual("MyTestRole", found.Single());
        }

        [Test]
        public void Can_Get_All_Roles()
        {
            ServiceContext.MemberService.AddRole("MyTestRole1");
            ServiceContext.MemberService.AddRole("MyTestRole2");
            ServiceContext.MemberService.AddRole("MyTestRole3");

            var found = ServiceContext.MemberService.GetAllRoles();

            Assert.AreEqual(3, found.Count());
        }

        [Test]
        public void Can_Get_All_Roles_By_Member_Id()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            IMember member = MockedMember.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
            ServiceContext.MemberService.Save(member);

            ServiceContext.MemberService.AddRole("MyTestRole1");
            ServiceContext.MemberService.AddRole("MyTestRole2");
            ServiceContext.MemberService.AddRole("MyTestRole3");
            ServiceContext.MemberService.AssignRoles(new[] { member.Id }, new[] { "MyTestRole1", "MyTestRole2" });

            var memberRoles = ServiceContext.MemberService.GetAllRoles(member.Id);

            Assert.AreEqual(2, memberRoles.Count());

        }

        [Test]
        public void Can_Get_All_Roles_By_Member_Username()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            IMember member = MockedMember.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
            ServiceContext.MemberService.Save(member);
            //need to test with '@' symbol in the lookup
            IMember member2 = MockedMember.CreateSimpleMember(memberType, "test2", "test2@test.com", "pass", "test2@test.com");
            ServiceContext.MemberService.Save(member2);

            ServiceContext.MemberService.AddRole("MyTestRole1");
            ServiceContext.MemberService.AddRole("MyTestRole2");
            ServiceContext.MemberService.AddRole("MyTestRole3");
            ServiceContext.MemberService.AssignRoles(new[] { member.Id, member2.Id }, new[] { "MyTestRole1", "MyTestRole2" });

            var memberRoles = ServiceContext.MemberService.GetAllRoles("test");
            Assert.AreEqual(2, memberRoles.Count());

            var memberRoles2 = ServiceContext.MemberService.GetAllRoles("test2@test.com");
            Assert.AreEqual(2, memberRoles2.Count());
        }

        [Test]
        public void Can_Delete_Role()
        {
            ServiceContext.MemberService.AddRole("MyTestRole1");

            ServiceContext.MemberService.DeleteRole("MyTestRole1", false);

            var memberRoles = ServiceContext.MemberService.GetAllRoles();

            Assert.AreEqual(0, memberRoles.Count());
        }

        [Test]
        public void Throws_When_Deleting_Assigned_Role()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            IMember member = MockedMember.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
            ServiceContext.MemberService.Save(member);

            ServiceContext.MemberService.AddRole("MyTestRole1");            
            ServiceContext.MemberService.AssignRoles(new[] { member.Id }, new[] { "MyTestRole1", "MyTestRole2" });

            Assert.Throws<InvalidOperationException>(() => ServiceContext.MemberService.DeleteRole("MyTestRole1", true));
        }

        [Test]
        public void Can_Get_Members_In_Role()
        {
            ServiceContext.MemberService.AddRole("MyTestRole1");
            var roleId = DatabaseContext.Database.ExecuteScalar<int>("SELECT id from umbracoNode where [text] = 'MyTestRole1'");

            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var member1 = MockedMember.CreateSimpleMember(memberType, "test1", "test1@test.com", "pass", "test1");
            ServiceContext.MemberService.Save(member1);
            var member2 = MockedMember.CreateSimpleMember(memberType, "test2", "test2@test.com", "pass", "test2");
            ServiceContext.MemberService.Save(member2);

            DatabaseContext.Database.Insert(new Member2MemberGroupDto {MemberGroup = roleId, Member = member1.Id});
            DatabaseContext.Database.Insert(new Member2MemberGroupDto { MemberGroup = roleId, Member = member2.Id });

            var membersInRole = ServiceContext.MemberService.GetMembersInRole("MyTestRole1");
            Assert.AreEqual(2, membersInRole.Count());
        }

        [TestCase("MyTestRole1", "test1", StringPropertyMatchType.StartsWith, 1)]
        [TestCase("MyTestRole1", "test", StringPropertyMatchType.StartsWith, 3)]
        [TestCase("MyTestRole1", "test1", StringPropertyMatchType.Exact, 1)]
        [TestCase("MyTestRole1", "test", StringPropertyMatchType.Exact, 0)]
        [TestCase("MyTestRole1", "st2", StringPropertyMatchType.EndsWith, 1)]
        [TestCase("MyTestRole1", "test%", StringPropertyMatchType.Wildcard, 3)]
        public void Find_Members_In_Role(string roleName1, string usernameToMatch, StringPropertyMatchType matchType, int resultCount)
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var member1 = MockedMember.CreateSimpleMember(memberType, "test1", "test1@test.com", "pass", "test1");
            ServiceContext.MemberService.Save(member1);
            var member2 = MockedMember.CreateSimpleMember(memberType, "test2", "test2@test.com", "pass", "test2");
            ServiceContext.MemberService.Save(member2);
            var member3 = MockedMember.CreateSimpleMember(memberType, "test3", "test3@test.com", "pass", "test3");
            ServiceContext.MemberService.Save(member3);

            ServiceContext.MemberService.AssignRoles(new[] { member1.Id, member2.Id, member3.Id }, new[] { roleName1 });

            var result = ServiceContext.MemberService.FindMembersInRole(roleName1, usernameToMatch, matchType);
            Assert.AreEqual(resultCount, result.Count());
        }

        [Test]
        public void Associate_Members_To_Roles_With_Member_Id()
        {
            ServiceContext.MemberService.AddRole("MyTestRole1");

            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var member1 = MockedMember.CreateSimpleMember(memberType, "test1", "test1@test.com", "pass", "test1");
            ServiceContext.MemberService.Save(member1);
            var member2 = MockedMember.CreateSimpleMember(memberType, "test2", "test2@test.com", "pass", "test2");
            ServiceContext.MemberService.Save(member2);

            ServiceContext.MemberService.AssignRoles(new[] { member1.Id, member2.Id }, new[] { "MyTestRole1" });

            var membersInRole = ServiceContext.MemberService.GetMembersInRole("MyTestRole1");

            Assert.AreEqual(2, membersInRole.Count());
        }

        [Test]
        public void Associate_Members_To_Roles_With_Member_Username()
        {
            ServiceContext.MemberService.AddRole("MyTestRole1");

            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var member1 = MockedMember.CreateSimpleMember(memberType, "test1", "test1@test.com", "pass", "test1");
            ServiceContext.MemberService.Save(member1);
            var member2 = MockedMember.CreateSimpleMember(memberType, "test2", "test2@test.com", "pass", "test2");
            ServiceContext.MemberService.Save(member2);

            ServiceContext.MemberService.AssignRoles(new[] { member1.Username, member2.Username }, new[] { "MyTestRole1" });

            var membersInRole = ServiceContext.MemberService.GetMembersInRole("MyTestRole1");

            Assert.AreEqual(2, membersInRole.Count());
        }

        [Test]
        public void Associate_Members_To_Roles_With_Member_Username_Containing_At_Symbols()
        {
            ServiceContext.MemberService.AddRole("MyTestRole1");

            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var member1 = MockedMember.CreateSimpleMember(memberType, "test1", "test1@test.com", "pass", "test1@test.com");
            ServiceContext.MemberService.Save(member1);
            var member2 = MockedMember.CreateSimpleMember(memberType, "test2", "test2@test.com", "pass", "test2@test.com");
            ServiceContext.MemberService.Save(member2);

            ServiceContext.MemberService.AssignRoles(new[] { member1.Username, member2.Username }, new[] { "MyTestRole1" });

            var membersInRole = ServiceContext.MemberService.GetMembersInRole("MyTestRole1");

            Assert.AreEqual(2, membersInRole.Count());
        }

        [Test]
        public void Associate_Members_To_Roles_With_New_Role()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var member1 = MockedMember.CreateSimpleMember(memberType, "test1", "test1@test.com", "pass", "test1");
            ServiceContext.MemberService.Save(member1);
            var member2 = MockedMember.CreateSimpleMember(memberType, "test2", "test2@test.com", "pass", "test2");
            ServiceContext.MemberService.Save(member2);

            //implicitly create the role
            ServiceContext.MemberService.AssignRoles(new[] { member1.Username, member2.Username }, new[] { "MyTestRole1" });

            var membersInRole = ServiceContext.MemberService.GetMembersInRole("MyTestRole1");

            Assert.AreEqual(2, membersInRole.Count());
        }

        [Test]
        public void Remove_Members_From_Roles_With_Member_Id()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var member1 = MockedMember.CreateSimpleMember(memberType, "test1", "test1@test.com", "pass", "test1");
            ServiceContext.MemberService.Save(member1);
            var member2 = MockedMember.CreateSimpleMember(memberType, "test2", "test2@test.com", "pass", "test2");
            ServiceContext.MemberService.Save(member2);

            ServiceContext.MemberService.AssignRoles(new[] { member1.Id, member2.Id }, new[] { "MyTestRole1", "MyTestRole2" });

            ServiceContext.MemberService.DissociateRoles(new[] {member1.Id }, new[] {"MyTestRole1"});
            ServiceContext.MemberService.DissociateRoles(new[] { member1.Id, member2.Id }, new[] { "MyTestRole2" });

            var membersInRole = ServiceContext.MemberService.GetMembersInRole("MyTestRole1");
            Assert.AreEqual(1, membersInRole.Count());
            membersInRole = ServiceContext.MemberService.GetMembersInRole("MyTestRole2");
            Assert.AreEqual(0, membersInRole.Count());
        }

        [Test]
        public void Remove_Members_From_Roles_With_Member_Username()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var member1 = MockedMember.CreateSimpleMember(memberType, "test1", "test1@test.com", "pass", "test1");
            ServiceContext.MemberService.Save(member1);
            var member2 = MockedMember.CreateSimpleMember(memberType, "test2", "test2@test.com", "pass", "test2");
            ServiceContext.MemberService.Save(member2);

            ServiceContext.MemberService.AssignRoles(new[] { member1.Username, member2.Username }, new[] { "MyTestRole1", "MyTestRole2" });

            ServiceContext.MemberService.DissociateRoles(new[] { member1.Username }, new[] { "MyTestRole1" });
            ServiceContext.MemberService.DissociateRoles(new[] { member1.Username, member2.Username }, new[] { "MyTestRole2" });

            var membersInRole = ServiceContext.MemberService.GetMembersInRole("MyTestRole1");
            Assert.AreEqual(1, membersInRole.Count());
            membersInRole = ServiceContext.MemberService.GetMembersInRole("MyTestRole2");
            Assert.AreEqual(0, membersInRole.Count());
        }

        [Test]
        public void Can_Delete_member()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            IMember member = MockedMember.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
            ServiceContext.MemberService.Save(member);

            ServiceContext.MemberService.Delete(member);
            var deleted = ServiceContext.MemberService.GetById(member.Id);

            // Assert
            Assert.That(deleted, Is.Null);
        }

        [Test]
        public void ContentXml_Created_When_Saved()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            IMember member = MockedMember.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
            ServiceContext.MemberService.Save(member);

            var xml = DatabaseContext.Database.FirstOrDefault<ContentXmlDto>("WHERE nodeId = @Id", new { Id = member.Id });
            Assert.IsNotNull(xml);
        }

        [Test]
        public void Exists_By_Username()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            IMember member = MockedMember.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
            ServiceContext.MemberService.Save(member);
            IMember member2 = MockedMember.CreateSimpleMember(memberType, "test", "test2@test.com", "pass", "test2@test.com");
            ServiceContext.MemberService.Save(member2);

            Assert.IsTrue(ServiceContext.MemberService.Exists("test"));
            Assert.IsFalse(ServiceContext.MemberService.Exists("notFound"));
            Assert.IsTrue(ServiceContext.MemberService.Exists("test2@test.com"));
        }

        [Test]
        public void Exists_By_Id()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            IMember member = MockedMember.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
            ServiceContext.MemberService.Save(member);

            Assert.IsTrue(ServiceContext.MemberService.Exists(member.Id));
            Assert.IsFalse(ServiceContext.MemberService.Exists(9876));
        }

        [Test]
        public void Tracks_Dirty_Changes()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            IMember member = MockedMember.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
            ServiceContext.MemberService.Save(member);

            var resolved = ServiceContext.MemberService.GetByEmail(member.Email);

            //NOTE: This will not trigger a property isDirty because this is not based on a 'Property', it is
            // just a c# property of the Member object
            resolved.Email = "changed@test.com";
            //NOTE: this WILL trigger a property isDirty because setting this c# property actually sets a value of
            // the underlying 'Property'
            resolved.FailedPasswordAttempts = 1234;

            var dirtyMember = (ICanBeDirty)resolved;
            var dirtyProperties = resolved.Properties.Where(x => x.IsDirty()).ToList();
            Assert.IsTrue(dirtyMember.IsDirty());
            Assert.AreEqual(1, dirtyProperties.Count());
        }

        [Test]
        public void Get_By_Email()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            IMember member = MockedMember.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
            ServiceContext.MemberService.Save(member);

            Assert.IsNotNull(ServiceContext.MemberService.GetByEmail(member.Email));
            Assert.IsNull(ServiceContext.MemberService.GetByEmail("do@not.find"));
        }

        [Test]
        public void Get_Member_Name()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            IMember member = MockedMember.CreateSimpleMember(memberType, "Test Real Name", "test@test.com", "pass", "testUsername");
            ServiceContext.MemberService.Save(member);


            Assert.AreEqual("Test Real Name", member.Name);
        }

        [Test]
        public void Get_Member_Name_In_Created_Event()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);

            TypedEventHandler<IMemberService, NewEventArgs<IMember>> callback = (sender, args) =>
            {
                Assert.AreEqual("Test Real Name", args.Entity.Name);
            };

            MemberService.Created += callback;
            var member = ServiceContext.MemberService.CreateMember("testUsername", "test@test.com", "Test Real Name", memberType);
            MemberService.Created -= callback;
        }

        [Test]
        public void Get_By_Username()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            IMember member = MockedMember.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
            ServiceContext.MemberService.Save(member);

            Assert.IsNotNull(ServiceContext.MemberService.GetByUsername(member.Username));
            Assert.IsNull(ServiceContext.MemberService.GetByUsername("notFound"));
        }

        [Test]
        public void Get_By_Object_Id()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            IMember member = MockedMember.CreateSimpleMember(memberType, "test", "test@test.com", "pass", "test");
            ServiceContext.MemberService.Save(member);

            Assert.IsNotNull(ServiceContext.MemberService.GetById(member.Id));
            Assert.IsNull(ServiceContext.MemberService.GetById(9876));
        }

        [Test]
        public void Get_All_Paged_Members()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10);
            ServiceContext.MemberService.Save(members);

            int totalRecs;
            var found = ServiceContext.MemberService.GetAll(0, 2, out totalRecs);

            Assert.AreEqual(2, found.Count());
            Assert.AreEqual(10, totalRecs);
            Assert.AreEqual("test0", found.First().Username);
            Assert.AreEqual("test1", found.Last().Username);
        }

        [Test]
        public void Find_By_Name_Starts_With()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10);
            ServiceContext.MemberService.Save(members);
            
            var customMember = MockedMember.CreateSimpleMember(memberType, "Bob", "hello@test.com", "hello", "hello");
            ServiceContext.MemberService.Save(customMember);

            int totalRecs;
            var found = ServiceContext.MemberService.FindMembersByDisplayName("B", 0, 100, out totalRecs, StringPropertyMatchType.StartsWith);

            Assert.AreEqual(1, found.Count());
        }

        [Test]
        public void Find_By_Email_Starts_With()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10);
            ServiceContext.MemberService.Save(members);
            //don't find this
            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello","hello");
            ServiceContext.MemberService.Save(customMember);

            int totalRecs;
            var found = ServiceContext.MemberService.FindByEmail("tes", 0, 100, out totalRecs, StringPropertyMatchType.StartsWith);

            Assert.AreEqual(10, found.Count());
        }

        [Test]
        public void Find_By_Email_Ends_With()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10);
            ServiceContext.MemberService.Save(members);
            //include this
            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            ServiceContext.MemberService.Save(customMember);

            int totalRecs;
            var found = ServiceContext.MemberService.FindByEmail("test.com", 0, 100, out totalRecs, StringPropertyMatchType.EndsWith);

            Assert.AreEqual(11, found.Count());
        }

        [Test]
        public void Find_By_Email_Contains()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10);
            ServiceContext.MemberService.Save(members);
            //include this
            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            ServiceContext.MemberService.Save(customMember);

            int totalRecs;
            var found = ServiceContext.MemberService.FindByEmail("test", 0, 100, out totalRecs, StringPropertyMatchType.Contains);

            Assert.AreEqual(11, found.Count());
        }

        [Test]
        public void Find_By_Email_Exact()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10);
            ServiceContext.MemberService.Save(members);
            //include this
            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            ServiceContext.MemberService.Save(customMember);

            int totalRecs;
            var found = ServiceContext.MemberService.FindByEmail("hello@test.com", 0, 100, out totalRecs, StringPropertyMatchType.Exact);

            Assert.AreEqual(1, found.Count());
        }

        [Test]
        public void Find_By_Login_Starts_With()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10);
            ServiceContext.MemberService.Save(members);
            //don't find this
            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            ServiceContext.MemberService.Save(customMember);

            int totalRecs;
            var found = ServiceContext.MemberService.FindByUsername("tes", 0, 100, out totalRecs, StringPropertyMatchType.StartsWith);

            Assert.AreEqual(10, found.Count());
        }

        [Test]
        public void Find_By_Login_Ends_With()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10);
            ServiceContext.MemberService.Save(members);
            //include this
            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            ServiceContext.MemberService.Save(customMember);

            int totalRecs;
            var found = ServiceContext.MemberService.FindByUsername("llo", 0, 100, out totalRecs, StringPropertyMatchType.EndsWith);

            Assert.AreEqual(1, found.Count());
        }

        [Test]
        public void Find_By_Login_Contains()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10);
            ServiceContext.MemberService.Save(members);
            //include this
            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hellotest");
            ServiceContext.MemberService.Save(customMember);

            int totalRecs;
            var found = ServiceContext.MemberService.FindByUsername("test", 0, 100, out totalRecs, StringPropertyMatchType.Contains);

            Assert.AreEqual(11, found.Count());
        }

        [Test]
        public void Find_By_Login_Exact()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10);
            ServiceContext.MemberService.Save(members);
            //include this
            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            ServiceContext.MemberService.Save(customMember);

            int totalRecs;
            var found = ServiceContext.MemberService.FindByUsername("hello", 0, 100, out totalRecs, StringPropertyMatchType.Exact);

            Assert.AreEqual(1, found.Count());
        }

        [Test]
        public void Get_By_Property_String_Value_Exact()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10);
            ServiceContext.MemberService.Save(members);            
            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            ServiceContext.MemberService.Save(customMember);

            var found = ServiceContext.MemberService.GetMembersByPropertyValue(
                "title", "hello member", StringPropertyMatchType.Exact);

            Assert.AreEqual(1, found.Count());
        }

        [Test]
        public void Get_By_Property_String_Value_Contains()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10);
            ServiceContext.MemberService.Save(members);            
            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            ServiceContext.MemberService.Save(customMember);

            var found = ServiceContext.MemberService.GetMembersByPropertyValue(
                "title", " member", StringPropertyMatchType.Contains);

            Assert.AreEqual(11, found.Count());
        }

        [Test]
        public void Get_By_Property_String_Value_Starts_With()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10);
            ServiceContext.MemberService.Save(members);
            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            ServiceContext.MemberService.Save(customMember);

            var found = ServiceContext.MemberService.GetMembersByPropertyValue(
                "title", "Member No", StringPropertyMatchType.StartsWith);

            Assert.AreEqual(10, found.Count());
        }

        [Test]
        public void Get_By_Property_String_Value_Ends_With()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10);
            ServiceContext.MemberService.Save(members);
            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.SetValue("title", "title of mine");
            ServiceContext.MemberService.Save(customMember);

            var found = ServiceContext.MemberService.GetMembersByPropertyValue(
                "title", "mine", StringPropertyMatchType.EndsWith);

            Assert.AreEqual(1, found.Count());
        }

        [Test]
        public void Get_By_Property_Int_Value_Exact()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            memberType.AddPropertyType(new PropertyType(Constants.PropertyEditors.IntegerAlias, DataTypeDatabaseType.Integer, "number")
                {
                    Name = "Number",
                    //NOTE: This is what really determines the db type - the above definition doesn't really do anything
                    DataTypeDefinitionId = -51
                }, "Content");
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10, (i, member) => member.SetValue("number", i));
            ServiceContext.MemberService.Save(members);

            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.SetValue("number", 2);
            ServiceContext.MemberService.Save(customMember);

            var found = ServiceContext.MemberService.GetMembersByPropertyValue(
                "number", 2, ValuePropertyMatchType.Exact);

            Assert.AreEqual(2, found.Count());
        }

        [Test]
        public void Get_By_Property_Int_Value_Greater_Than()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            memberType.AddPropertyType(new PropertyType(Constants.PropertyEditors.IntegerAlias, DataTypeDatabaseType.Integer, "number")
            {
                Name = "Number",
                //NOTE: This is what really determines the db type - the above definition doesn't really do anything
                DataTypeDefinitionId = -51
            }, "Content");
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10, (i, member) => member.SetValue("number", i));
            ServiceContext.MemberService.Save(members);

            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.SetValue("number", 10);
            ServiceContext.MemberService.Save(customMember);

            var found = ServiceContext.MemberService.GetMembersByPropertyValue(
                "number", 3, ValuePropertyMatchType.GreaterThan);

            Assert.AreEqual(7, found.Count());
        }

        [Test]
        public void Get_By_Property_Int_Value_Greater_Than_Equal_To()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            memberType.AddPropertyType(new PropertyType(Constants.PropertyEditors.IntegerAlias, DataTypeDatabaseType.Integer, "number")
            {
                Name = "Number",
                //NOTE: This is what really determines the db type - the above definition doesn't really do anything
                DataTypeDefinitionId = -51
            }, "Content");
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10, (i, member) => member.SetValue("number", i));
            ServiceContext.MemberService.Save(members);

            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.SetValue("number", 10);
            ServiceContext.MemberService.Save(customMember);

            var found = ServiceContext.MemberService.GetMembersByPropertyValue(
                "number", 3, ValuePropertyMatchType.GreaterThanOrEqualTo);

            Assert.AreEqual(8, found.Count());
        }

        [Test]
        public void Get_By_Property_Int_Value_Less_Than()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            memberType.AddPropertyType(new PropertyType(Constants.PropertyEditors.DateAlias, DataTypeDatabaseType.Date, "number")
            {
                Name = "Number",
                //NOTE: This is what really determines the db type - the above definition doesn't really do anything
                DataTypeDefinitionId = -51
            }, "Content");
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10, (i, member) => member.SetValue("number", i));
            ServiceContext.MemberService.Save(members);

            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.SetValue("number", 1);
            ServiceContext.MemberService.Save(customMember);

            var found = ServiceContext.MemberService.GetMembersByPropertyValue(
                "number", 5, ValuePropertyMatchType.LessThan);

            Assert.AreEqual(6, found.Count());
        }

        [Test]
        public void Get_By_Property_Int_Value_Less_Than_Or_Equal()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            memberType.AddPropertyType(new PropertyType(Constants.PropertyEditors.IntegerAlias, DataTypeDatabaseType.Integer, "number")
            {
                Name = "Number",
                //NOTE: This is what really determines the db type - the above definition doesn't really do anything
                DataTypeDefinitionId = -51
            }, "Content");
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10, (i, member) => member.SetValue("number", i));
            ServiceContext.MemberService.Save(members);

            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.SetValue("number", 1);
            ServiceContext.MemberService.Save(customMember);

            var found = ServiceContext.MemberService.GetMembersByPropertyValue(
                "number", 5, ValuePropertyMatchType.LessThanOrEqualTo);

            Assert.AreEqual(7, found.Count());
        }

        [Test]
        public void Get_By_Property_Date_Value_Exact()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            memberType.AddPropertyType(new PropertyType(Constants.PropertyEditors.IntegerAlias, DataTypeDatabaseType.Integer, "date")
            {
                Name = "Date",
                //NOTE: This is what really determines the db type - the above definition doesn't really do anything
                DataTypeDefinitionId = -36
            }, "Content");
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10, (i, member) => member.SetValue("date", new DateTime(2013, 12, 20, 1, i, 0)));
            ServiceContext.MemberService.Save(members);

            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.SetValue("date", new DateTime(2013, 12, 20, 1, 2, 0));
            ServiceContext.MemberService.Save(customMember);

            var found = ServiceContext.MemberService.GetMembersByPropertyValue(
                "date", new DateTime(2013, 12, 20, 1, 2, 0), ValuePropertyMatchType.Exact);

            Assert.AreEqual(2, found.Count());
        }

        [Test]
        public void Get_By_Property_Date_Value_Greater_Than()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            memberType.AddPropertyType(new PropertyType(Constants.PropertyEditors.IntegerAlias, DataTypeDatabaseType.Integer, "date")
            {
                Name = "Date",
                //NOTE: This is what really determines the db type - the above definition doesn't really do anything
                DataTypeDefinitionId = -36
            }, "Content");
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10, (i, member) => member.SetValue("date", new DateTime(2013, 12, 20, 1, i, 0)));
            ServiceContext.MemberService.Save(members);

            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.SetValue("date", new DateTime(2013, 12, 20, 1, 10, 0));
            ServiceContext.MemberService.Save(customMember);

            var found = ServiceContext.MemberService.GetMembersByPropertyValue(
                "date", new DateTime(2013, 12, 20, 1, 3, 0), ValuePropertyMatchType.GreaterThan);

            Assert.AreEqual(7, found.Count());
        }

        [Test]
        public void Get_By_Property_Date_Value_Greater_Than_Equal_To()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            memberType.AddPropertyType(new PropertyType(Constants.PropertyEditors.IntegerAlias, DataTypeDatabaseType.Integer, "date")
            {
                Name = "Date",
                //NOTE: This is what really determines the db type - the above definition doesn't really do anything
                DataTypeDefinitionId = -36
            }, "Content");
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10, (i, member) => member.SetValue("date", new DateTime(2013, 12, 20, 1, i, 0)));
            ServiceContext.MemberService.Save(members);

            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.SetValue("date", new DateTime(2013, 12, 20, 1, 10, 0));
            ServiceContext.MemberService.Save(customMember);

            var found = ServiceContext.MemberService.GetMembersByPropertyValue(
                "date", new DateTime(2013, 12, 20, 1, 3, 0), ValuePropertyMatchType.GreaterThanOrEqualTo);

            Assert.AreEqual(8, found.Count());
        }

        [Test]
        public void Get_By_Property_Date_Value_Less_Than()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            memberType.AddPropertyType(new PropertyType(Constants.PropertyEditors.IntegerAlias, DataTypeDatabaseType.Integer, "date")
            {
                Name = "Date",
                //NOTE: This is what really determines the db type - the above definition doesn't really do anything
                DataTypeDefinitionId = -36
            }, "Content");
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10, (i, member) => member.SetValue("date", new DateTime(2013, 12, 20, 1, i, 0)));
            ServiceContext.MemberService.Save(members);

            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.SetValue("date", new DateTime(2013, 12, 20, 1, 1, 0));
            ServiceContext.MemberService.Save(customMember);

            var found = ServiceContext.MemberService.GetMembersByPropertyValue(
                "date", new DateTime(2013, 12, 20, 1, 5, 0), ValuePropertyMatchType.LessThan);

            Assert.AreEqual(6, found.Count());            
        }

        [Test]
        public void Get_By_Property_Date_Value_Less_Than_Or_Equal()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            memberType.AddPropertyType(new PropertyType(Constants.PropertyEditors.IntegerAlias, DataTypeDatabaseType.Integer, "date")
            {
                Name = "Date",
                //NOTE: This is what really determines the db type - the above definition doesn't really do anything
                DataTypeDefinitionId = -36
            }, "Content");
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10, (i, member) => member.SetValue("date", new DateTime(2013, 12, 20, 1, i, 0)));
            ServiceContext.MemberService.Save(members);

            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.SetValue("date", new DateTime(2013, 12, 20, 1, 1, 0));
            ServiceContext.MemberService.Save(customMember);

            var found = ServiceContext.MemberService.GetMembersByPropertyValue(
                "date", new DateTime(2013, 12, 20, 1, 5, 0), ValuePropertyMatchType.LessThanOrEqualTo);

            Assert.AreEqual(7, found.Count());
        }

        [Test]
        public void Count_All_Members()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();            
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10);
            ServiceContext.MemberService.Save(members);
            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            ServiceContext.MemberService.Save(customMember);

            var found = ServiceContext.MemberService.GetCount(MemberCountType.All);

            Assert.AreEqual(11, found);
        }

        [Test]
        public void Count_All_Online_Members()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();            
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10, (i, member) => member.LastLoginDate = DateTime.Now.AddMinutes(i * -2));
            ServiceContext.MemberService.Save(members);

            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.SetValue(Constants.Conventions.Member.LastLoginDate, DateTime.Now);
            ServiceContext.MemberService.Save(customMember);

            var found = ServiceContext.MemberService.GetCount(MemberCountType.Online);

            Assert.AreEqual(9, found);
        }

        [Test]
        public void Count_All_Locked_Members()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10, (i, member) => member.IsLockedOut = i % 2 == 0);
            ServiceContext.MemberService.Save(members);

            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.SetValue(Constants.Conventions.Member.IsLockedOut, true);
            ServiceContext.MemberService.Save(customMember);

            var found = ServiceContext.MemberService.GetCount(MemberCountType.LockedOut);

            Assert.AreEqual(6, found);
        }

        [Test]
        public void Count_All_Approved_Members()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10, (i, member) => member.IsApproved = i % 2 == 0);
            ServiceContext.MemberService.Save(members);

            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            customMember.SetValue(Constants.Conventions.Member.IsApproved, false);
            ServiceContext.MemberService.Save(customMember);

            var found = ServiceContext.MemberService.GetCount(MemberCountType.Approved);

            Assert.AreEqual(5, found);
        }

        [Test]
        public void Setting_Property_On_Built_In_Member_Property_When_Property_Doesnt_Exist_On_Type_Is_Ok()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();            
            ServiceContext.MemberTypeService.Save(memberType);
            memberType.RemovePropertyType(Constants.Conventions.Member.Comments);
            ServiceContext.MemberTypeService.Save(memberType);
            Assert.IsFalse(memberType.PropertyTypes.Any(x => x.Alias == Constants.Conventions.Member.Comments));

            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            //this should not throw an exception
            customMember.Comments = "hello world";
            ServiceContext.MemberService.Save(customMember);

            var found = ServiceContext.MemberService.GetById(customMember.Id);

            Assert.IsTrue(found.Comments.IsNullOrWhiteSpace());
        }

        /// <summary>
        /// Because we are forcing some of the built-ins to be Labels which have an underlying db type as nvarchar but we need
        /// to ensure that the dates/int get saved to the correct column anyways. 
        /// </summary>
        [Test]
        public void Setting_DateTime_Property_On_Built_In_Member_Property_Saves_To_Correct_Column()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var member = MockedMember.CreateSimpleMember(memberType, "test", "test@test.com", "test", "test");
            var date = DateTime.Now;
            member.LastLoginDate = DateTime.Now;            
            ServiceContext.MemberService.Save(member);

            var result = ServiceContext.MemberService.GetById(member.Id);
            Assert.AreEqual(
                date.TruncateTo(DateTimeExtensions.DateTruncate.Second), 
                result.LastLoginDate.TruncateTo(DateTimeExtensions.DateTruncate.Second));

            //now ensure the col is correct
            var sql = new Sql().Select("cmsPropertyData.*")
                .From<PropertyDataDto>()
                .InnerJoin<PropertyTypeDto>()
                .On<PropertyDataDto, PropertyTypeDto>(dto => dto.PropertyTypeId, dto => dto.Id)
                .Where<PropertyDataDto>(dto => dto.NodeId == member.Id)
                .Where<PropertyTypeDto>(dto => dto.Alias == Constants.Conventions.Member.LastLoginDate);
            
            var colResult = DatabaseContext.Database.Fetch<PropertyDataDto>(sql);

            Assert.AreEqual(1, colResult.Count);
            Assert.IsTrue(colResult.First().Date.HasValue);
            Assert.IsFalse(colResult.First().Integer.HasValue);
            Assert.IsNull(colResult.First().Text);
            Assert.IsNull(colResult.First().VarChar);
        }

        [Test]
        public void New_Member_Approved_By_Default()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);

            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            ServiceContext.MemberService.Save(customMember);

            var found = ServiceContext.MemberService.GetById(customMember.Id);

            Assert.IsTrue(found.IsApproved);
        }

        [Test]
        public void Ensure_Content_Xml_Created()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);

            var customMember = MockedMember.CreateSimpleMember(memberType, "hello", "hello@test.com", "hello", "hello");
            ServiceContext.MemberService.Save(customMember);

            var provider = new PetaPocoUnitOfWorkProvider(Logger);

            using (var uow = provider.GetUnitOfWork())
            {
                Assert.IsTrue(uow.Database.Exists<ContentXmlDto>(customMember.Id));
            }

        }

    }
}