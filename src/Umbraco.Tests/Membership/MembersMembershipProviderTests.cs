using System.Collections.Specialized;
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
    public class MembersMembershipProviderTests
    {
       
        //[Test]
        //public void Set_Default_Member_Type_On_Init()

        //[Test]
        //public void Create_User_Already_Exists()
        //{

        //}

        //[Test]
        //public void Create_User_Requires_Unique_Email()
        //{

        //}

        [Test]
        public void Answer_Is_Encrypted()
        {
            IMember createdMember = null;
            var memberType = MockedContentTypes.CreateSimpleMemberType();
            foreach (var p in Constants.Conventions.Member.GetStandardPropertyTypeStubs())
            {
                memberType.AddPropertyType(p.Value);
            }
            var mServiceMock = new Mock<IMemberService>();
            mServiceMock.Setup(service => service.Exists("test")).Returns(false);
            mServiceMock.Setup(service => service.GetByEmail("test@test.com")).Returns(() => null);
            mServiceMock.Setup(
                service => service.CreateMember(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                        .Callback((string u, string e, string p, string m) =>
                            {
                                createdMember = new Member("test", e, u, p, memberType);
                            })
                        .Returns(() => createdMember);
            var provider = new MembersMembershipProvider(mServiceMock.Object);            
            provider.Initialize("test", new NameValueCollection());
            

            MembershipCreateStatus status;
            provider.CreateUser("test", "test", "test", "test@test.com", "test", "test", true, "test", out status);

            Assert.AreNotEqual("test", createdMember.PasswordAnswer);
            Assert.AreEqual(provider.EncryptString("test"), createdMember.PasswordAnswer);
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
            var mServiceMock = new Mock<IMemberService>();
            mServiceMock.Setup(service => service.Exists("test")).Returns(false);
            mServiceMock.Setup(service => service.GetByEmail("test@test.com")).Returns(() => null);
            mServiceMock.Setup(
                service => service.CreateMember(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                        .Callback((string u, string e, string p, string m) =>
                            {
                                createdMember = new Member("test", e, u, p, memberType);
                            })
                        .Returns(() => createdMember);

            var provider = new MembersMembershipProvider(mServiceMock.Object);
            provider.Initialize("test", new NameValueCollection { { "passwordFormat", "Encrypted" } });
            

            MembershipCreateStatus status;
            provider.CreateUser("test", "test", "test", "test@test.com", "test", "test", true, "test", out status);

            Assert.AreNotEqual("test", createdMember.Password);
            var decrypted = provider.DecryptPassword(createdMember.Password);
            Assert.AreEqual("test", decrypted);
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
            var mServiceMock = new Mock<IMemberService>();
            mServiceMock.Setup(service => service.Exists("test")).Returns(false);
            mServiceMock.Setup(service => service.GetByEmail("test@test.com")).Returns(() => null);
            mServiceMock.Setup(
                service => service.CreateMember(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                        .Callback((string u, string e, string p, string m) =>
                        {
                            createdMember = new Member("test", e, u, p, memberType);
                        })
                        .Returns(() => createdMember);

            var provider = new MembersMembershipProvider(mServiceMock.Object);
            provider.Initialize("test", new NameValueCollection { { "passwordFormat", "Hashed" }, { "hashAlgorithmType", "HMACSHA256" } });
            

            MembershipCreateStatus status;
            provider.CreateUser("test", "test", "test", "test@test.com", "test", "test", true, "test", out status);

            Assert.AreNotEqual("test", createdMember.Password);
            
            string salt;
            var storedPassword = provider.StoredPassword(createdMember.Password, out salt);
            var hashedPassword = provider.EncryptOrHashPassword("test", salt);
            Assert.AreEqual(hashedPassword, storedPassword);
        }
        
        //[Test]
        //public void Password_Encrypted_Validated_With_Salt()

        //[Test]
        //public void Password_Encrypted_Validated_With_Salt()

    }
}