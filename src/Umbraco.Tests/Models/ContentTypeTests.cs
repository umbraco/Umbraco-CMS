using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Serialization;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class ContentTypeTests : UmbracoTestBase
    {
        [Test]
        [Ignore("Ignoring this test until we actually enforce this, see comments in ContentTypeBase.PropertyTypesChanged")]
        public void Cannot_Add_Duplicate_Property_Aliases()
        {
            var contentType = MockedContentTypes.CreateBasicContentType();

            contentType.PropertyGroups.Add(new PropertyGroup(new PropertyTypeCollection(false, new[]
            {
                new PropertyType("testPropertyEditor", ValueStorageType.Nvarchar){ Alias = "myPropertyType" }
            })));

            Assert.Throws<InvalidOperationException>(() =>
                contentType.PropertyTypeCollection.Add(
                    new PropertyType("testPropertyEditor", ValueStorageType.Nvarchar) { Alias = "myPropertyType" }));

        }

        [Test]
        [Ignore("Ignoring this test until we actually enforce this, see comments in ContentTypeBase.PropertyTypesChanged")]
        public void Cannot_Update_Duplicate_Property_Aliases()
        {
            var contentType = MockedContentTypes.CreateBasicContentType();

            contentType.PropertyGroups.Add(new PropertyGroup(new PropertyTypeCollection(false, new[]
            {
                new PropertyType("testPropertyEditor", ValueStorageType.Nvarchar){ Alias = "myPropertyType" }
            })));

            contentType.PropertyTypeCollection.Add(new PropertyType("testPropertyEditor", ValueStorageType.Nvarchar) { Alias = "myPropertyType2" });

            var toUpdate = contentType.PropertyTypeCollection["myPropertyType2"];

            Assert.Throws<InvalidOperationException>(() => toUpdate.Alias = "myPropertyType");

        }

        [Test]
        public void Can_Deep_Clone_Content_Type_Sort()
        {
            var contentType = new ContentTypeSort(new Lazy<int>(() => 3), 4, "test");
            var clone = (ContentTypeSort)contentType.DeepClone();
            Assert.AreNotSame(clone, contentType);
            Assert.AreEqual(clone, contentType);
            Assert.AreEqual(clone.Id.Value, contentType.Id.Value);
            Assert.AreEqual(clone.SortOrder, contentType.SortOrder);
            Assert.AreEqual(clone.Alias, contentType.Alias);

        }

        [Test]
        public void Can_Deep_Clone_Content_Type_With_Reset_Identities()
        {
            var contentType = MockedContentTypes.CreateTextPageContentType();
            contentType.Id = 99;

            var i = 200;
            foreach (var propertyType in contentType.PropertyTypes)
            {
                propertyType.Id = ++i;
            }
            foreach (var group in contentType.PropertyGroups)
            {
                group.Id = ++i;
            }
            //add a property type without a property group
            contentType.PropertyTypeCollection.Add(
                new PropertyType("test", ValueStorageType.Ntext, "title2") { Name = "Title2", Description = "", Mandatory = false, SortOrder = 1, DataTypeId = -88 });

            contentType.AllowedTemplates = new[] { new Template("Name", "name") { Id = 200 }, new Template("Name2", "name2") { Id = 201 } };
            contentType.AllowedContentTypes = new[] { new ContentTypeSort(new Lazy<int>(() => 888), 8, "sub"), new ContentTypeSort(new Lazy<int>(() => 889), 9, "sub2") };
            contentType.Id = 10;
            contentType.CreateDate = DateTime.Now;
            contentType.CreatorId = 22;
            contentType.SetDefaultTemplate(new Template((string)"Test Template", (string)"testTemplate")
            {
                Id = 88
            });
            contentType.Description = "test";
            contentType.Icon = "icon";
            contentType.IsContainer = true;
            contentType.Thumbnail = "thumb";
            contentType.Key = Guid.NewGuid();
            contentType.Level = 3;
            contentType.Path = "-1,4,10";
            contentType.SortOrder = 5;
            contentType.Trashed = false;
            contentType.UpdateDate = DateTime.Now;

            //ensure that nothing is marked as dirty
            contentType.ResetDirtyProperties(false);

            var clone = (ContentType)contentType.DeepCloneWithResetIdentities("newAlias");

            Assert.AreEqual("newAlias", clone.Alias);
            Assert.AreNotEqual("newAlias", contentType.Alias);
            Assert.IsFalse(clone.HasIdentity);

            foreach (var propertyGroup in clone.PropertyGroups)
            {
                Assert.IsFalse(propertyGroup.HasIdentity);
                foreach (var propertyType in propertyGroup.PropertyTypes)
                {
                    Assert.IsFalse(propertyType.HasIdentity);
                }
            }

            foreach (var propertyType in clone.PropertyTypes.Where(x => x.HasIdentity))
            {
                Assert.IsFalse(propertyType.HasIdentity);
            }
        }

        private static IProfilingLogger GetTestProfilingLogger()
        {
            var logger = new DebugDiagnosticsLogger();
            var profiler = new TestProfiler();
            return new ProfilingLogger(logger, profiler);
        }

        [Ignore("fixme - ignored test")]
        [Test]
        public void Can_Deep_Clone_Content_Type_Perf_Test()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            contentType.Id = 99;

            var i = 200;
            foreach (var propertyType in contentType.PropertyTypes)
            {
                propertyType.Id = ++i;
            }
            foreach (var group in contentType.PropertyGroups)
            {
                group.Id = ++i;
            }
            contentType.AllowedTemplates = new[] { new Template((string)"Name", (string)"name") { Id = 200 }, new Template((string)"Name2", (string)"name2") { Id = 201 } };
            contentType.AllowedContentTypes = new[] { new ContentTypeSort(new Lazy<int>(() => 888), 8, "sub"), new ContentTypeSort(new Lazy<int>(() => 889), 9, "sub2") };
            contentType.Id = 10;
            contentType.CreateDate = DateTime.Now;
            contentType.CreatorId = 22;
            contentType.SetDefaultTemplate(new Template((string)"Test Template", (string)"testTemplate")
            {
                Id = 88
            });
            contentType.Description = "test";
            contentType.Icon = "icon";
            contentType.IsContainer = true;
            contentType.Thumbnail = "thumb";
            contentType.Key = Guid.NewGuid();
            contentType.Level = 3;
            contentType.Path = "-1,4,10";
            contentType.SortOrder = 5;
            contentType.Trashed = false;
            contentType.UpdateDate = DateTime.Now;

            var proflog = GetTestProfilingLogger();

            using (proflog.DebugDuration<ContentTypeTests>("STARTING PERF TEST"))
            {
                for (var j = 0; j < 1000; j++)
                {
                    using (proflog.DebugDuration<ContentTypeTests>("Cloning content type"))
                    {
                        var clone = (ContentType)contentType.DeepClone();
                    }
                }
            }
        }

        [Test]
        public void Can_Deep_Clone_Content_Type()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            contentType.Id = 99;

            var i = 200;
            foreach (var propertyType in contentType.PropertyTypes)
            {
                propertyType.Id = ++i;
            }
            foreach (var group in contentType.PropertyGroups)
            {
                group.Id = ++i;
            }
            contentType.AllowedTemplates = new[] { new Template((string)"Name", (string)"name") { Id = 200 }, new Template((string)"Name2", (string)"name2") { Id = 201 } };
            contentType.AllowedContentTypes = new[] { new ContentTypeSort(new Lazy<int>(() => 888), 8, "sub"), new ContentTypeSort(new Lazy<int>(() => 889), 9, "sub2") };
            contentType.Id = 10;
            contentType.CreateDate = DateTime.Now;
            contentType.CreatorId = 22;
            contentType.SetDefaultTemplate(new Template((string)"Test Template", (string)"testTemplate")
            {
                Id = 88
            });
            contentType.Description = "test";
            contentType.Icon = "icon";
            contentType.IsContainer = true;
            contentType.Thumbnail = "thumb";
            contentType.Key = Guid.NewGuid();
            contentType.Level = 3;
            contentType.Path = "-1,4,10";
            contentType.SortOrder = 5;
            contentType.Trashed = false;
            contentType.UpdateDate = DateTime.Now;

            // Act
            var clone = (ContentType)contentType.DeepClone();

            // Assert
            Assert.AreNotSame(clone, contentType);
            Assert.AreEqual(clone, contentType);
            Assert.AreEqual(clone.Id, contentType.Id);
            Assert.AreEqual(clone.AllowedTemplates.Count(), contentType.AllowedTemplates.Count());
            for (var index = 0; index < contentType.AllowedTemplates.Count(); index++)
            {
                Assert.AreNotSame(clone.AllowedTemplates.ElementAt(index), contentType.AllowedTemplates.ElementAt(index));
                Assert.AreEqual(clone.AllowedTemplates.ElementAt(index), contentType.AllowedTemplates.ElementAt(index));
            }
            Assert.AreNotSame(clone.PropertyGroups, contentType.PropertyGroups);
            Assert.AreEqual(clone.PropertyGroups.Count, contentType.PropertyGroups.Count);
            for (var index = 0; index < contentType.PropertyGroups.Count; index++)
            {
                Assert.AreNotSame(clone.PropertyGroups[index], contentType.PropertyGroups[index]);
                Assert.AreEqual(clone.PropertyGroups[index], contentType.PropertyGroups[index]);
            }
            Assert.AreNotSame(clone.PropertyTypes, contentType.PropertyTypes);
            Assert.AreEqual(clone.PropertyTypes.Count(), contentType.PropertyTypes.Count());
            Assert.AreEqual(0, clone.NoGroupPropertyTypes.Count());
            for (var index = 0; index < contentType.PropertyTypes.Count(); index++)
            {
                Assert.AreNotSame(clone.PropertyTypes.ElementAt(index), contentType.PropertyTypes.ElementAt(index));
                Assert.AreEqual(clone.PropertyTypes.ElementAt(index), contentType.PropertyTypes.ElementAt(index));
            }

            Assert.AreEqual(clone.CreateDate, contentType.CreateDate);
            Assert.AreEqual(clone.CreatorId, contentType.CreatorId);
            Assert.AreEqual(clone.Key, contentType.Key);
            Assert.AreEqual(clone.Level, contentType.Level);
            Assert.AreEqual(clone.Path, contentType.Path);
            Assert.AreEqual(clone.SortOrder, contentType.SortOrder);
            Assert.AreNotSame(clone.DefaultTemplate, contentType.DefaultTemplate);
            Assert.AreEqual(clone.DefaultTemplate, contentType.DefaultTemplate);
            Assert.AreEqual(clone.DefaultTemplateId, contentType.DefaultTemplateId);
            Assert.AreEqual(clone.Trashed, contentType.Trashed);
            Assert.AreEqual(clone.UpdateDate, contentType.UpdateDate);
            Assert.AreEqual(clone.Thumbnail, contentType.Thumbnail);
            Assert.AreEqual(clone.Icon, contentType.Icon);
            Assert.AreEqual(clone.IsContainer, contentType.IsContainer);

            //This double verifies by reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
            {
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(contentType, null));
            }

            //need to ensure the event handlers are wired

            var asDirty = (ICanBeDirty)clone;

            Assert.IsFalse(asDirty.IsPropertyDirty("PropertyTypes"));
            clone.AddPropertyType(new PropertyType("test", ValueStorageType.Nvarchar, "blah"));
            Assert.IsTrue(asDirty.IsPropertyDirty("PropertyTypes"));
            Assert.IsFalse(asDirty.IsPropertyDirty("PropertyGroups"));
            clone.AddPropertyGroup("hello");
            Assert.IsTrue(asDirty.IsPropertyDirty("PropertyGroups"));
        }

        [Test]
        public void Can_Serialize_Content_Type_Without_Error()
        {
            var ss = new SerializationService(new JsonNetSerializer());

            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            contentType.Id = 99;

            var i = 200;
            foreach (var propertyType in contentType.PropertyTypes)
            {
                propertyType.Id = ++i;
            }
            contentType.AllowedTemplates = new[] { new Template((string)"Name", (string)"name") { Id = 200 }, new Template((string)"Name2", (string)"name2") { Id = 201 } };
            contentType.AllowedContentTypes = new[] { new ContentTypeSort(new Lazy<int>(() => 888), 8, "sub"), new ContentTypeSort(new Lazy<int>(() => 889), 9, "sub2") };
            contentType.Id = 10;
            contentType.CreateDate = DateTime.Now;
            contentType.CreatorId = 22;
            contentType.SetDefaultTemplate(new Template((string)"Test Template", (string)"testTemplate")
            {
                Id = 88
            });
            contentType.Description = "test";
            contentType.Icon = "icon";
            contentType.IsContainer = true;
            contentType.Thumbnail = "thumb";
            contentType.Key = Guid.NewGuid();
            contentType.Level = 3;
            contentType.Path = "-1,4,10";
            contentType.SortOrder = 5;
            contentType.Trashed = false;
            contentType.UpdateDate = DateTime.Now;

            var result = ss.ToStream(contentType);
            var json = result.ResultStream.ToJsonString();
            Debug.Print(json);
        }

        [Test]
        public void Can_Deep_Clone_Media_Type()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateImageMediaType();
            contentType.Id = 99;

            var i = 200;
            foreach (var propertyType in contentType.PropertyTypes)
            {
                propertyType.Id = ++i;
            }
            contentType.Id = 10;
            contentType.CreateDate = DateTime.Now;
            contentType.CreatorId = 22;
            contentType.Description = "test";
            contentType.Icon = "icon";
            contentType.IsContainer = true;
            contentType.Thumbnail = "thumb";
            contentType.Key = Guid.NewGuid();
            contentType.Level = 3;
            contentType.Path = "-1,4,10";
            contentType.SortOrder = 5;
            contentType.Trashed = false;
            contentType.UpdateDate = DateTime.Now;

            // Act
            var clone = (MediaType)contentType.DeepClone();

            // Assert
            Assert.AreNotSame(clone, contentType);
            Assert.AreEqual(clone, contentType);
            Assert.AreEqual(clone.Id, contentType.Id);
            Assert.AreEqual(clone.PropertyGroups.Count, contentType.PropertyGroups.Count);
            for (var index = 0; index < contentType.PropertyGroups.Count; index++)
            {
                Assert.AreNotSame(clone.PropertyGroups[index], contentType.PropertyGroups[index]);
                Assert.AreEqual(clone.PropertyGroups[index], contentType.PropertyGroups[index]);
            }
            Assert.AreEqual(clone.PropertyTypes.Count(), contentType.PropertyTypes.Count());
            for (var index = 0; index < contentType.PropertyTypes.Count(); index++)
            {
                Assert.AreNotSame(clone.PropertyTypes.ElementAt(index), contentType.PropertyTypes.ElementAt(index));
                Assert.AreEqual(clone.PropertyTypes.ElementAt(index), contentType.PropertyTypes.ElementAt(index));
            }
            Assert.AreEqual(clone.CreateDate, contentType.CreateDate);
            Assert.AreEqual(clone.CreatorId, contentType.CreatorId);
            Assert.AreEqual(clone.Key, contentType.Key);
            Assert.AreEqual(clone.Level, contentType.Level);
            Assert.AreEqual(clone.Path, contentType.Path);
            Assert.AreEqual(clone.SortOrder, contentType.SortOrder);
            Assert.AreEqual(clone.Trashed, contentType.Trashed);
            Assert.AreEqual(clone.UpdateDate, contentType.UpdateDate);
            Assert.AreEqual(clone.Thumbnail, contentType.Thumbnail);
            Assert.AreEqual(clone.Icon, contentType.Icon);
            Assert.AreEqual(clone.IsContainer, contentType.IsContainer);

            //This double verifies by reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
            {
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(contentType, null));
            }
        }

        [Test]
        public void Can_Serialize_Media_Type_Without_Error()
        {
            var ss = new SerializationService(new JsonNetSerializer());

            // Arrange
            var contentType = MockedContentTypes.CreateImageMediaType();
            contentType.Id = 99;

            var i = 200;
            foreach (var propertyType in contentType.PropertyTypes)
            {
                propertyType.Id = ++i;
            }
            contentType.Id = 10;
            contentType.CreateDate = DateTime.Now;
            contentType.CreatorId = 22;
            contentType.Description = "test";
            contentType.Icon = "icon";
            contentType.IsContainer = true;
            contentType.Thumbnail = "thumb";
            contentType.Key = Guid.NewGuid();
            contentType.Level = 3;
            contentType.Path = "-1,4,10";
            contentType.SortOrder = 5;
            contentType.Trashed = false;
            contentType.UpdateDate = DateTime.Now;

            var result = ss.ToStream(contentType);
            var json = result.ResultStream.ToJsonString();
            Debug.Print(json);
        }

        [Test]
        public void Can_Deep_Clone_Member_Type()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateSimpleMemberType();
            contentType.Id = 99;

            var i = 200;
            foreach (var propertyType in contentType.PropertyTypes)
            {
                propertyType.Id = ++i;
            }
            contentType.Id = 10;
            contentType.CreateDate = DateTime.Now;
            contentType.CreatorId = 22;
            contentType.Description = "test";
            contentType.Icon = "icon";
            contentType.IsContainer = true;
            contentType.Thumbnail = "thumb";
            contentType.Key = Guid.NewGuid();
            contentType.Level = 3;
            contentType.Path = "-1,4,10";
            contentType.SortOrder = 5;
            contentType.Trashed = false;
            contentType.UpdateDate = DateTime.Now;
            contentType.SetMemberCanEditProperty("title", true);
            contentType.SetMemberCanViewProperty("bodyText", true);

            // Act
            var clone = (MemberType)contentType.DeepClone();

            // Assert
            Assert.AreNotSame(clone, contentType);
            Assert.AreEqual(clone, contentType);
            Assert.AreEqual(clone.Id, contentType.Id);
            Assert.AreEqual(clone.PropertyGroups.Count, contentType.PropertyGroups.Count);
            for (var index = 0; index < contentType.PropertyGroups.Count; index++)
            {
                Assert.AreNotSame(clone.PropertyGroups[index], contentType.PropertyGroups[index]);
                Assert.AreEqual(clone.PropertyGroups[index], contentType.PropertyGroups[index]);
            }
            Assert.AreEqual(clone.PropertyTypes.Count(), contentType.PropertyTypes.Count());
            for (var index = 0; index < contentType.PropertyTypes.Count(); index++)
            {
                Assert.AreNotSame(clone.PropertyTypes.ElementAt(index), contentType.PropertyTypes.ElementAt(index));
                Assert.AreEqual(clone.PropertyTypes.ElementAt(index), contentType.PropertyTypes.ElementAt(index));
            }
            Assert.AreEqual(clone.CreateDate, contentType.CreateDate);
            Assert.AreEqual(clone.CreatorId, contentType.CreatorId);
            Assert.AreEqual(clone.Key, contentType.Key);
            Assert.AreEqual(clone.Level, contentType.Level);
            Assert.AreEqual(clone.Path, contentType.Path);
            Assert.AreEqual(clone.SortOrder, contentType.SortOrder);
            Assert.AreEqual(clone.Trashed, contentType.Trashed);
            Assert.AreEqual(clone.UpdateDate, contentType.UpdateDate);
            Assert.AreEqual(clone.Thumbnail, contentType.Thumbnail);
            Assert.AreEqual(clone.Icon, contentType.Icon);
            Assert.AreEqual(clone.IsContainer, contentType.IsContainer);
            Assert.AreEqual(clone.MemberTypePropertyTypes, contentType.MemberTypePropertyTypes);

            //This double verifies by reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
            {
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(contentType, null));
            }
        }

        [Test]
        public void Can_Serialize_Member_Type_Without_Error()
        {
            var ss = new SerializationService(new JsonNetSerializer());

            // Arrange
            var contentType = MockedContentTypes.CreateSimpleMemberType();
            contentType.Id = 99;

            var i = 200;
            foreach (var propertyType in contentType.PropertyTypes)
            {
                propertyType.Id = ++i;
            }
            contentType.Id = 10;
            contentType.CreateDate = DateTime.Now;
            contentType.CreatorId = 22;
            contentType.Description = "test";
            contentType.Icon = "icon";
            contentType.IsContainer = true;
            contentType.Thumbnail = "thumb";
            contentType.Key = Guid.NewGuid();
            contentType.Level = 3;
            contentType.Path = "-1,4,10";
            contentType.SortOrder = 5;
            contentType.Trashed = false;
            contentType.UpdateDate = DateTime.Now;
            contentType.SetMemberCanEditProperty("title", true);
            contentType.SetMemberCanViewProperty("bodyText", true);

            var result = ss.ToStream(contentType);
            var json = result.ResultStream.ToJsonString();
            Debug.Print(json);
        }
    }
}
