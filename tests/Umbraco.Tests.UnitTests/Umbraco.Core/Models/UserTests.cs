// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models
{
    [TestFixture]
    public class UserTests
    {
        private UserBuilder _builder;

        [SetUp]
        public void SetUp() => _builder = new UserBuilder();

        [Test]
        public void Can_Deep_Clone()
        {
            User item = BuildUser();

            var clone = (User)item.DeepClone();

            Assert.AreNotSame(clone, item);
            Assert.AreEqual(clone, item);

            Assert.AreEqual(clone.AllowedSections.Count(), item.AllowedSections.Count());

            // Verify normal properties with reflection
            PropertyInfo[] allProps = clone.GetType().GetProperties();
            foreach (PropertyInfo propertyInfo in allProps)
            {
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(item, null));
            }
        }

        [Test]
        public void Can_Serialize_Without_Error()
        {
            User item = BuildUser();

            var json = JsonConvert.SerializeObject(item);
            Debug.Print(json);
        }

        private User BuildUser() =>
            _builder
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
