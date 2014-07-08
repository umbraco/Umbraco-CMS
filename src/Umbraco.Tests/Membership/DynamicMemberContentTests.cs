using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using Lucene.Net.Util;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Tests.PublishedContent;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.Membership
{
    [DatabaseTestBehavior(DatabaseBehavior.NoDatabasePerFixture)]
    [TestFixture]
    public class DynamicMemberContentTests : PublishedContentTestBase
    {

        public override void Initialize()
        {
            // required so we can access property.Value
            //PropertyValueConvertersResolver.Current = new PropertyValueConvertersResolver();

            base.Initialize();

            // need to specify a custom callback for unit tests
            // AutoPublishedContentTypes generates properties automatically
            // when they are requested, but we must declare those that we
            // explicitely want to be here...

            var propertyTypes = new[]
                {
                    // AutoPublishedContentType will auto-generate other properties
                    new PublishedPropertyType("title", 0, "?"), 
                    new PublishedPropertyType("bodyText", 0, "?"), 
                    new PublishedPropertyType("author", 0, "?")
                };
            var type = new AutoPublishedContentType(0, "anything", propertyTypes);
            PublishedContentType.GetPublishedContentTypeCallback = (alias) => type;

        }

        [Test]
        public void Can_Get_Built_In_Properties()
        {
            var date = DateTime.Now;

            var member = new Member("test name", "test@email.com", "test username", "test password",
                GetMemberType());
            member.Comments = "test comment";
            member.IsApproved = true;
            member.IsLockedOut = false;
            member.CreateDate = date;
            member.LastLoginDate = date.AddMinutes(1);
            member.LastLockoutDate = date.AddMinutes(2);
            //NOTE: Last activity date is always the same as last login date since we don't have a place to store that data
            //member.LastLoginDate = date.AddMinutes(3);
            member.LastPasswordChangeDate = date.AddMinutes(4);
            member.PasswordQuestion = "test question";

            var mpc = new MemberPublishedContent(member);                

            var d = mpc.AsDynamic();

            Assert.AreEqual("test comment", d.Comments);
            Assert.AreEqual(date, d.CreationDate);
            Assert.AreEqual("test@email.com", d.Email);
            Assert.AreEqual(true, d.IsApproved);
            Assert.AreEqual(false, d.IsLockedOut);
            Assert.AreEqual(date.AddMinutes(1), d.LastActivityDate);
            Assert.AreEqual(date.AddMinutes(2), d.LastLockoutDate);
            Assert.AreEqual(date.AddMinutes(1), d.LastLoginDate);
            Assert.AreEqual(date.AddMinutes(4), d.LastPasswordChangedDate);
            Assert.AreEqual("test name", d.Name);
            Assert.AreEqual("test question", d.PasswordQuestion);
            Assert.AreEqual("test username", d.UserName);

        }

        [Test]
        public void Can_Get_Built_In_Properties_Camel_Case()
        {
            var date = DateTime.Now;

            var member = new Member("test name", "test@email.com", "test username", "test password",
               GetMemberType());
            member.Comments = "test comment";
            member.IsApproved = true;
            member.IsLockedOut = false;
            member.CreateDate = date;
            member.LastLoginDate = date.AddMinutes(1);
            member.LastLockoutDate = date.AddMinutes(2);
            //NOTE: Last activity date is always the same as last login date since we don't have a place to store that data
            //member.LastLoginDate = date.AddMinutes(3);
            member.LastPasswordChangeDate = date.AddMinutes(4);
            member.PasswordQuestion = "test question";

            var mpc = new MemberPublishedContent(member);

            var d = mpc.AsDynamic();

            Assert.AreEqual("test comment", d.comments);
            Assert.AreEqual(date, d.creationDate);
            Assert.AreEqual("test@email.com", d.email);
            Assert.AreEqual(true, d.isApproved);
            Assert.AreEqual(false, d.isLockedOut);
            Assert.AreEqual(date.AddMinutes(1), d.lastActivityDate);
            Assert.AreEqual(date.AddMinutes(2), d.lastLockoutDate);
            Assert.AreEqual(date.AddMinutes(1), d.lastLoginDate);
            Assert.AreEqual(date.AddMinutes(4), d.lastPasswordChangedDate);
            Assert.AreEqual("test name", d.name);
            Assert.AreEqual("test question", d.passwordQuestion);
            Assert.AreEqual("test username", d.userName);

        }

        [Test]
        public void Can_Get_Custom_Properties()
        {
            var date = DateTime.Now;

            var memberType = MockedContentTypes.CreateSimpleMemberType("Member", "Member");
            var member = MockedMember.CreateSimpleMember(memberType, "test name", "test@email.com", "test password", "test username");
            member.Comments = "test comment";
            member.IsApproved = true;
            member.IsLockedOut = false;
            member.CreateDate = date;
            member.LastLoginDate = date.AddMinutes(1);
            member.LastLockoutDate = date.AddMinutes(2);
            //NOTE: Last activity date is always the same as last login date since we don't have a place to store that data
            //member.LastLoginDate = date.AddMinutes(3);
            member.LastPasswordChangeDate = date.AddMinutes(4);
            member.PasswordQuestion = "test question";

            member.Properties["title"].Value = "Test Value 1";
            member.Properties["bodyText"].Value = "Test Value 2";
            member.Properties["author"].Value = "Test Value 3";
            var mpc = new MemberPublishedContent(member);

            var d = mpc.AsDynamic();

            Assert.AreEqual("Test Value 1", d.title);
            Assert.AreEqual("Test Value 1", d.Title);
            Assert.AreEqual("Test Value 2", d.bodyText);
            Assert.AreEqual("Test Value 2", d.BodyText);
            Assert.AreEqual("Test Value 3", d.author);
            Assert.AreEqual("Test Value 3", d.Author);


        }

        private IMemberType GetMemberType()
        {
            var entity = new MemberType(-1)
            {
                Alias = "Member"
            };

            entity.AddPropertyGroup(Umbraco.Core.Constants.Conventions.Member.StandardPropertiesGroupName);
            var standardPropertyTypes = Umbraco.Core.Constants.Conventions.Member.GetStandardPropertyTypeStubs();
            foreach (var standardPropertyType in standardPropertyTypes)
            {
                entity.AddPropertyType(standardPropertyType.Value, Umbraco.Core.Constants.Conventions.Member.StandardPropertiesGroupName);
            }
            return entity;
        }

    }
}
