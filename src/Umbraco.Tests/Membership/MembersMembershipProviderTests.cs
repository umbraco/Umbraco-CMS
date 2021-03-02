using System.Collections.Specialized;
using System.Web.Security;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Umbraco.Tests.Integration;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;
using Umbraco.Web;
using Umbraco.Web.Cache;
using Umbraco.Web.Security.Providers;

namespace Umbraco.Tests.Membership
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class MembersMembershipProviderTests : TestWithDatabaseBase
    {
        private MembersMembershipProvider MembersMembershipProvider { get; set; }
        private IDistributedCacheBinder DistributedCacheBinder { get; set; }

        public IMemberService MemberService => Current.Factory.GetInstance<IMemberService>();
        public IMemberTypeService MemberTypeService => Current.Factory.GetInstance<IMemberTypeService>();
        public ILogger Logger => Current.Factory.GetInstance<ILogger>();

        public override void SetUp()
        {
            base.SetUp();

            MembersMembershipProvider = new MembersMembershipProvider(MemberService, MemberTypeService);

            MembersMembershipProvider.Initialize("test", new NameValueCollection { { "passwordFormat", MembershipPasswordFormat.Clear.ToString() } });

            DistributedCacheBinder = new DistributedCacheBinder(new DistributedCache(), Mock.Of<IUmbracoContextFactory>(), Logger);
            DistributedCacheBinder.BindEvents(true);
        }

        [TearDown]
        public void Teardown()
        {
            DistributedCacheBinder?.UnbindEvents();
            DistributedCacheBinder = null;
        }

        protected override void Compose()
        {
            base.Compose();

            // the cache refresher component needs to trigger to refresh caches
            // but then, it requires a lot of plumbing ;(
            // FIXME: and we cannot inject a DistributedCache yet
            // so doing all this mess
            Composition.RegisterUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
            Composition.RegisterUnique(f => Mock.Of<IServerRegistrar>());
            Composition.WithCollectionBuilder<CacheRefresherCollectionBuilder>()
                .Add(() => Composition.TypeLoader.GetCacheRefreshers());
        }

        protected override AppCaches GetAppCaches()
        {
            // this is what's created core web runtime
            return new AppCaches(
                new DeepCloneAppCache(new ObjectCacheAppCache()),
                NoAppCache.Instance,
                new IsolatedCaches(type => new DeepCloneAppCache(new ObjectCacheAppCache())));
        }

        /// <summary>
        /// MembersMembershipProvider.ValidateUser is expected to increase the number of failed attempts and also read that same number.
        /// </summary>
        /// <remarks>
        /// This test requires the caching to be enabled, as it already is correct in the database.
        /// Shows the error described here: https://github.com/umbraco/Umbraco-CMS/issues/9861
        /// </remarks>
        [Test]
        public void ValidateUser__must_lock_out_users_after_max_attempts_of_wrong_password()
        {
            // Arrange
            IMemberType memberType = MockedContentTypes.CreateSimpleMemberType();
            ServiceContext.MemberTypeService.Save(memberType);
            var member = MockedMember.CreateSimpleMember(memberType, "test", "test@test.com",  "password","test");
            ServiceContext.MemberService.Save(member);

            var wrongPassword = "wrongPassword";
            var numberOfFailedAttempts = MembersMembershipProvider.MaxInvalidPasswordAttempts+2;

            // Act
            var memberBefore = ServiceContext.MemberService.GetById(member.Id);
            for (int i = 0; i < numberOfFailedAttempts; i++)
            {
                MembersMembershipProvider.ValidateUser(member.Username, wrongPassword);
            }
            var memberAfter = ServiceContext.MemberService.GetById(member.Id);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.AreEqual(0 , memberBefore.FailedPasswordAttempts, "Expected 0 failed password attempts before");
                Assert.IsFalse(memberBefore.IsLockedOut, "Expected the member NOT to be locked out before");

                Assert.AreEqual(MembersMembershipProvider.MaxInvalidPasswordAttempts, memberAfter.FailedPasswordAttempts, "Expected exactly the max possible failed password attempts after");
                Assert.IsTrue(memberAfter.IsLockedOut, "Expected the member to be locked out after");
            });

        }
    }
}
