// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
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
    public class UserGroupTests
    {
        private UserGroupBuilder _builder;

        [SetUp]
        public void SetUp() => _builder = new UserGroupBuilder();

        [Test]
        public void Can_Deep_Clone()
        {
            IUserGroup item = Build();

            var clone = (IUserGroup)item.DeepClone();

            var x = clone.Equals(item);
            Assert.AreNotSame(clone, item);
            Assert.AreEqual(clone, item);

            Assert.AreEqual(clone.AllowedSections.Count(), item.AllowedSections.Count());
            Assert.AreNotSame(clone.AllowedSections, item.AllowedSections);

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
            IUserGroup item = Build();

            var json = JsonConvert.SerializeObject(item);
            Debug.Print(json);
        }

        private IUserGroup Build() =>
            _builder
                .WithId(3)
                .WithAllowedSections(new List<string>(){"A", "B"})
                .Build();


    }


}
