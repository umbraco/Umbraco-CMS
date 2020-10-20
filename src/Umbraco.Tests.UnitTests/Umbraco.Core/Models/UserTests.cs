using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core.Models.Membership;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Common.Builders.Extensions;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Models
{
    [TestFixture]
    public class UserTests
    {
        private UserBuilder _builder;

        [SetUp]
        public void SetUp()
        {
            _builder = new UserBuilder();
        }

        [Test]
        public void Can_Deep_Clone()
        {
            var item = BuildUser();

            var clone = (User)item.DeepClone();

            Assert.AreNotSame(clone, item);
            Assert.AreEqual(clone, item);

            Assert.AreEqual(clone.AllowedSections.Count(), item.AllowedSections.Count());

            //Verify normal properties with reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(item, null));
        }

        [Test]
        public void Can_Serialize_Without_Error()
        {
            var item = BuildUser();

            var json = JsonConvert.SerializeObject(item);
            Debug.Print(json);
        }

        private User BuildUser()
        {
            return _builder
                .WithId(3)
                .WithLogin("username", "test pass")
                .WithName("Test")
                .WithEmail("test@test.com")
                .WithFailedPasswordAttempts(3)
                .WithIsApproved(true)
                .WithIsLockedOut(true)
                .WithComments("comments")
                .WithSessionTimeout(5)
                .WithStartContentIds(new[] { 3 })
                .WithStartMediaIds(new[] { 8 })
                .Build();
        }
    }
}
