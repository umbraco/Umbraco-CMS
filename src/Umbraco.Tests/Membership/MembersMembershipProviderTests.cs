using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;
using Umbraco.Web.Security.Providers;

namespace Umbraco.Tests.Membership
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class MembersMembershipProviderTests : TestWithDatabaseBase
    {
        private MembersMembershipProvider MembersMembershipProvider => new MembersMembershipProvider(MemberService, MemberTypeService);

        public IMemberService MemberService => Current.Factory.GetInstance<IMemberService>();
        public IMemberTypeService MemberTypeService => Current.Factory.GetInstance<IMemberTypeService>();

        [Test]
        public void ValidateUser_must_increase_failed_attempts()
        {
            // Arrange
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var member = MockedMember.CreateSimpleMember(memberType, "test", "test@test.com",  "test");
            ServiceContext.MemberService.Save(member);

            var wrongPassword = "wrongPassword";
            var numberOfFailedAttempts = 6;

            // Act
            var attemptsBefore = ServiceContext.MemberService.GetById(member.Id).FailedPasswordAttempts;
            for (int i = 0; i < numberOfFailedAttempts; i++)
            {
                MembersMembershipProvider.ValidateUser(member.Username, wrongPassword);
            }
            var attemptsAfter = ServiceContext.MemberService.GetById(member.Id).FailedPasswordAttempts;

            // Assert
            Assert.AreEqual(0, attemptsBefore);
            Assert.AreEqual(numberOfFailedAttempts, attemptsAfter);
        }
    }
}
