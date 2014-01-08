using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Services
{
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

            Assert.IsTrue(ServiceContext.MemberService.Exists("test"));
            Assert.IsFalse(ServiceContext.MemberService.Exists("notFound"));
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

            Assert.IsNotNull(ServiceContext.MemberService.GetById((object)member.Id));
            Assert.IsNull(ServiceContext.MemberService.GetById((object)9876));
        }

        [Test]
        public void Get_All_Paged_Members()
        {
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var members = MockedMember.CreateSimpleMember(memberType, 10);
            ServiceContext.MemberService.Save(members);

            int totalRecs;
            var found = ServiceContext.MemberService.GetAllMembers(0, 2, out totalRecs);

            Assert.AreEqual(2, found.Count());
            Assert.AreEqual(10, totalRecs);
            Assert.AreEqual("test0", found.First().Username);
            Assert.AreEqual("test1", found.Last().Username);
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
            var found = ServiceContext.MemberService.FindMembersByEmail("tes", 0, 100, out totalRecs, StringPropertyMatchType.StartsWith);

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
            var found = ServiceContext.MemberService.FindMembersByEmail("test.com", 0, 100, out totalRecs, StringPropertyMatchType.EndsWith);

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
            var found = ServiceContext.MemberService.FindMembersByEmail("test", 0, 100, out totalRecs, StringPropertyMatchType.Contains);

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
            var found = ServiceContext.MemberService.FindMembersByEmail("hello@test.com", 0, 100, out totalRecs, StringPropertyMatchType.Exact);

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
            var found = ServiceContext.MemberService.FindMembersByUsername("tes", 0, 100, out totalRecs, StringPropertyMatchType.StartsWith);

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
            var found = ServiceContext.MemberService.FindMembersByUsername("llo", 0, 100, out totalRecs, StringPropertyMatchType.EndsWith);

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
            var found = ServiceContext.MemberService.FindMembersByUsername("test", 0, 100, out totalRecs, StringPropertyMatchType.Contains);

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
            var found = ServiceContext.MemberService.FindMembersByUsername("hello", 0, 100, out totalRecs, StringPropertyMatchType.Exact);

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
            memberType.AddPropertyType(new PropertyType(Constants.PropertyEditors.IntegerAlias, DataTypeDatabaseType.Integer)
                {
                    Alias = "number",
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
            memberType.AddPropertyType(new PropertyType(Constants.PropertyEditors.IntegerAlias, DataTypeDatabaseType.Integer)
            {
                Alias = "number",
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
            memberType.AddPropertyType(new PropertyType(Constants.PropertyEditors.IntegerAlias, DataTypeDatabaseType.Integer)
            {
                Alias = "number",
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
            memberType.AddPropertyType(new PropertyType(Constants.PropertyEditors.DateAlias, DataTypeDatabaseType.Date)
            {
                Alias = "number",
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
            memberType.AddPropertyType(new PropertyType(Constants.PropertyEditors.IntegerAlias, DataTypeDatabaseType.Integer)
            {
                Alias = "number",
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
            memberType.AddPropertyType(new PropertyType(Constants.PropertyEditors.IntegerAlias, DataTypeDatabaseType.Integer)
            {
                Alias = "date",
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
            memberType.AddPropertyType(new PropertyType(Constants.PropertyEditors.IntegerAlias, DataTypeDatabaseType.Integer)
            {
                Alias = "date",
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
            memberType.AddPropertyType(new PropertyType(Constants.PropertyEditors.IntegerAlias, DataTypeDatabaseType.Integer)
            {
                Alias = "date",
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
            memberType.AddPropertyType(new PropertyType(Constants.PropertyEditors.IntegerAlias, DataTypeDatabaseType.Integer)
            {
                Alias = "date",
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
            memberType.AddPropertyType(new PropertyType(Constants.PropertyEditors.IntegerAlias, DataTypeDatabaseType.Integer)
            {
                Alias = "date",
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

            var found = ServiceContext.MemberService.GetMemberCount(MemberCountType.All);

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

            var found = ServiceContext.MemberService.GetMemberCount(MemberCountType.Online);

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

            var found = ServiceContext.MemberService.GetMemberCount(MemberCountType.LockedOut);

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

            var found = ServiceContext.MemberService.GetMemberCount(MemberCountType.Approved);

            Assert.AreEqual(5, found);
        }

    }
}