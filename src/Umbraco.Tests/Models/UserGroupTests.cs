using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Serialization;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class UserGroupTests
    {
        [SetUp]
        public void Setup()
        {
            Current.Reset();
            Current.UnlockConfigs();
            Current.Configs.Add(SettingsForTests.GetDefaultGlobalSettings);
            Current.Configs.Add(SettingsForTests.GetDefaultUmbracoSettings);
        }

        [Test]

        public void Can_Deep_Clone()
        {
            var item = Build();

            var clone = (UserGroup)item.DeepClone();

            Assert.AreNotSame(clone, item);
            Assert.AreEqual(clone, item);

            Assert.AreEqual(clone.AllowedSections.Count(), item.AllowedSections.Count());
            Assert.AreNotSame(clone.AllowedSections, item.AllowedSections);

            //Verify normal properties with reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
            {
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(item, null));
            }
        }

        [Test]
        public void Can_Serialize_Without_Error()
        {
            var ss = new SerializationService(new JsonNetSerializer());

            var item = Build();

            var result = ss.ToStream(item);
            var json = result.ResultStream.ToJsonString();
            Debug.Print(json);
        }

        private UserGroup Build()
        {
            var item = new UserGroup()
            {
                Id = 3,
                Key = Guid.NewGuid(),
                UpdateDate = DateTime.Now,
                CreateDate = DateTime.Now,
                Name = "Test",
                Alias = "alias",
                Icon = "icon",
                Permissions = new []{"a", "b", "c"},
                DeleteDate = null,
                StartContentId = null,
                StartMediaId = null,
            };
            item.AddAllowedSection("A");
            item.AddAllowedSection("B");

            return item;
        }
    }
}
