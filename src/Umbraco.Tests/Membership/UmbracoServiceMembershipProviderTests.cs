using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Web.Security;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Web.Security.Providers;

namespace Umbraco.Tests.Membership
{
    [TestFixture, RequiresSTA]
    public class UmbracoServiceMembershipProviderTests
    {
        [Test]
        public void Sets_Default_Member_Type_From_Service_On_Init()
        {
            var mServiceMock = new Mock<IMembershipMemberService>();
            var provider = new MembersMembershipProvider(mServiceMock.Object);
            mServiceMock.Setup(service => service.GetDefaultMemberType()).Returns("Blah");
            provider.Initialize("test", new NameValueCollection());
            
            Assert.AreEqual("Blah", provider.DefaultMemberTypeAlias);
        }

        [Test]
        public void Sets_Default_Member_Type_From_Config_On_Init()
        {
            var mServiceMock = new Mock<IMembershipMemberService>();
            var provider = new MembersMembershipProvider(mServiceMock.Object);
            mServiceMock.Setup(service => service.GetDefaultMemberType()).Returns("Blah");
            provider.Initialize("test", new NameValueCollection { { "defaultMemberTypeAlias", "Hello" } });

            Assert.AreEqual("Hello", provider.DefaultMemberTypeAlias);
        }

        [Test]
        public void Create_User_Already_Exists()
        {
            var mServiceMock = new Mock<IMembershipMemberService>();
            mServiceMock.Setup(service => service.Exists("test")).Returns(true);
            mServiceMock.Setup(service => service.GetDefaultMemberType()).Returns("Member");

            var provider = new MembersMembershipProvider(mServiceMock.Object);
            provider.Initialize("test", new NameValueCollection());

            MembershipCreateStatus status;
            var user = provider.CreateUser("test", "test", "testtest$1", "test@test.com", "test", "test", true, "test", out status);

            Assert.IsNull(user);
        }

        [Test]
        public void Create_User_Requires_Unique_Email()
        {
            var mServiceMock = new Mock<IMembershipMemberService>();
            mServiceMock.Setup(service => service.GetByEmail("test@test.com")).Returns(() => new Member("test", MockedContentTypes.CreateSimpleMemberType()));
            mServiceMock.Setup(service => service.GetDefaultMemberType()).Returns("Member");

            var provider = new MembersMembershipProvider(mServiceMock.Object);
            provider.Initialize("test", new NameValueCollection { { "requiresUniqueEmail", "true" } });

            MembershipCreateStatus status;
            var user = provider.CreateUser("test", "test", "testtest$1", "test@test.com", "test", "test", true, "test", out status);

            Assert.IsNull(user);
        }

        [Test]
        public void Answer_Is_Encrypted()
        {
            IMember createdMember = null;
            var memberType = MockedContentTypes.CreateSimpleMemberType();
            foreach (var p in Constants.Conventions.Member.GetStandardPropertyTypeStubs())
            {
                memberType.AddPropertyType(p.Value);
            }
            var mServiceMock = new Mock<IMembershipMemberService>();
            mServiceMock.Setup(service => service.Exists("test")).Returns(false);
            mServiceMock.Setup(service => service.GetByEmail("test@test.com")).Returns(() => null);
            mServiceMock.Setup(service => service.GetDefaultMemberType()).Returns("Member");
            mServiceMock.Setup(
                service => service.CreateWithIdentity(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                        .Callback((string u, string e, string p, string m) =>
                            {
                                createdMember = new Member("test", e, u, p, memberType);
                            })
                        .Returns(() => createdMember);
            var provider = new MembersMembershipProvider(mServiceMock.Object);            
            provider.Initialize("test", new NameValueCollection());
            

            MembershipCreateStatus status;
            provider.CreateUser("test", "test", "testtest$1", "test@test.com", "test", "test", true, "test", out status);

            Assert.AreNotEqual("test", createdMember.RawPasswordAnswerValue);
            Assert.AreEqual(provider.EncryptString("test"), createdMember.RawPasswordAnswerValue);
        }

        [Test]
        public void Password_Encrypted()
        {
            IMember createdMember = null;
            var memberType = MockedContentTypes.CreateSimpleMemberType();
            foreach (var p in Constants.Conventions.Member.GetStandardPropertyTypeStubs())
            {
                memberType.AddPropertyType(p.Value);
            }
            var mServiceMock = new Mock<IMembershipMemberService>();
            mServiceMock.Setup(service => service.Exists("test")).Returns(false);
            mServiceMock.Setup(service => service.GetByEmail("test@test.com")).Returns(() => null);
            mServiceMock.Setup(service => service.GetDefaultMemberType()).Returns("Member");
            mServiceMock.Setup(
                service => service.CreateWithIdentity(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                        .Callback((string u, string e, string p, string m) =>
                            {
                                createdMember = new Member("test", e, u, p, memberType);
                            })
                        .Returns(() => createdMember);

            var provider = new MembersMembershipProvider(mServiceMock.Object);
            provider.Initialize("test", new NameValueCollection { { "passwordFormat", "Encrypted" } });
            

            MembershipCreateStatus status;
            provider.CreateUser("test", "test", "testtest$1", "test@test.com", "test", "test", true, "test", out status);

            Assert.AreNotEqual("test", createdMember.RawPasswordValue);
            var decrypted = provider.DecryptPassword(createdMember.RawPasswordValue);
            Assert.AreEqual("testtest$1", decrypted);
        }

        [Test]
        public void Password_Hashed_With_Salt()
        {
            IMember createdMember = null;
            var memberType = MockedContentTypes.CreateSimpleMemberType();
            foreach (var p in Constants.Conventions.Member.GetStandardPropertyTypeStubs())
            {
                memberType.AddPropertyType(p.Value);
            }
            var mServiceMock = new Mock<IMembershipMemberService>();
            mServiceMock.Setup(service => service.Exists("test")).Returns(false);
            mServiceMock.Setup(service => service.GetByEmail("test@test.com")).Returns(() => null);
            mServiceMock.Setup(service => service.GetDefaultMemberType()).Returns("Member");
            mServiceMock.Setup(
                service => service.CreateWithIdentity(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                        .Callback((string u, string e, string p, string m) =>
                        {
                            createdMember = new Member("test", e, u, p, memberType);
                        })
                        .Returns(() => createdMember);

            var provider = new MembersMembershipProvider(mServiceMock.Object);
            provider.Initialize("test", new NameValueCollection { { "passwordFormat", "Hashed" }, { "hashAlgorithmType", "HMACSHA256" } });
            

            MembershipCreateStatus status;
            provider.CreateUser("test", "test", "testtest$1", "test@test.com", "test", "test", true, "test", out status);

            Assert.AreNotEqual("test", createdMember.RawPasswordValue);
            
            string salt;
            var storedPassword = provider.StoredPassword(createdMember.RawPasswordValue, out salt);
            var hashedPassword = provider.EncryptOrHashPassword("testtest$1", salt);
            Assert.AreEqual(hashedPassword, storedPassword);
        }
        
    }
}