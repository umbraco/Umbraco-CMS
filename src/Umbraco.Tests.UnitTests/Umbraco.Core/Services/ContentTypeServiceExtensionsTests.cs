// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services
{
    [TestFixture]
    public class ContentTypeServiceExtensionsTests
    {
        private IShortStringHelper ShortStringHelper => new DefaultShortStringHelper(new DefaultShortStringHelperConfig());

        [Test]
        public void GetAvailableCompositeContentTypes_No_Overlap_By_Content_Type_And_Property_Type_Alias()
        {
            void AddPropType(string alias, IContentType ct)
            {
                var contentCollection = new PropertyTypeCollection(true)
                {
                    new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext) { Alias = alias, Name = "Title", Description = string.Empty, Mandatory = false, SortOrder = 1, DataTypeId = -88 }
                };
                var pg = new PropertyGroup(contentCollection)
                {
                    Alias = "test",
                    Name = "test",
                    SortOrder = 1
                };
                ct.PropertyGroups.Add(pg);
            }

            ContentType ct1 = ContentTypeBuilder.CreateBasicContentType("ct1", "CT1", null);
            ContentType ct2 = ContentTypeBuilder.CreateBasicContentType("ct2", "CT2", null);
            AddPropType("title", ct2);
            ContentType ct3 = ContentTypeBuilder.CreateBasicContentType("ct3", "CT3", null);
            AddPropType("title", ct3);
            ContentType ct4 = ContentTypeBuilder.CreateBasicContentType("ct4", "CT4", null);
            ContentType ct5 = ContentTypeBuilder.CreateBasicContentType("ct5", "CT5", null);
            AddPropType("blah", ct5);
            ct1.Id = 1;
            ct2.Id = 2;
            ct3.Id = 3;
            ct4.Id = 4;
            ct5.Id = 4;

            var service = new Mock<IContentTypeService>();

            IContentTypeComposition[] availableTypes = service.Object.GetAvailableCompositeContentTypes(
                ct1,
                new[] { ct1, ct2, ct3, ct4, ct5 },
                new[] { ct2.Alias },
                new[] { "blah" })
                .Results.Where(x => x.Allowed).Select(x => x.Composition).ToArray();

            Assert.AreEqual(1, availableTypes.Count());
            Assert.AreEqual(ct4.Id, availableTypes.ElementAt(0).Id);
        }

        [Test]
        public void GetAvailableCompositeContentTypes_No_Overlap_By_Property_Type_Alias()
        {
            void AddPropType(IContentType ct)
            {
                var contentCollection = new PropertyTypeCollection(true)
                {
                    new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext) { Alias = "title", Name = "Title", Description = string.Empty, Mandatory = false, SortOrder = 1, DataTypeId = -88 }
                };
                var pg = new PropertyGroup(contentCollection)
                {
                    Alias = "test",
                    Name = "test",
                    SortOrder = 1
                };
                ct.PropertyGroups.Add(pg);
            }

            ContentType ct1 = ContentTypeBuilder.CreateBasicContentType("ct1", "CT1", null);
            ContentType ct2 = ContentTypeBuilder.CreateBasicContentType("ct2", "CT2", null);
            AddPropType(ct2);
            ContentType ct3 = ContentTypeBuilder.CreateBasicContentType("ct3", "CT3", null);
            AddPropType(ct3);
            ContentType ct4 = ContentTypeBuilder.CreateBasicContentType("ct4", "CT4", null);
            ct1.Id = 1;
            ct2.Id = 2;
            ct3.Id = 3;
            ct4.Id = 4;

            var service = new Mock<IContentTypeService>();

            IContentTypeComposition[] availableTypes = service.Object.GetAvailableCompositeContentTypes(
                ct1,
                new[] { ct1, ct2, ct3, ct4 },
                new string[] { },
                new[] { "title" })
                .Results.Where(x => x.Allowed).Select(x => x.Composition).ToArray();

            Assert.AreEqual(1, availableTypes.Count());
            Assert.AreEqual(ct4.Id, availableTypes.ElementAt(0).Id);
        }

        [Test]
        public void GetAvailableCompositeContentTypes_No_Overlap_By_Content_Type()
        {
            void AddPropType(IContentType ct)
            {
                var contentCollection = new PropertyTypeCollection(true)
                {
                    new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext) { Alias = "title", Name = "Title", Description = string.Empty, Mandatory = false, SortOrder = 1, DataTypeId = -88 }
                };
                var pg = new PropertyGroup(contentCollection)
                {
                    Alias = "test",
                    Name = "test",
                    SortOrder = 1
                };
                ct.PropertyGroups.Add(pg);
            }

            ContentType ct1 = ContentTypeBuilder.CreateBasicContentType("ct1", "CT1", null);
            ContentType ct2 = ContentTypeBuilder.CreateBasicContentType("ct2", "CT2", null);
            AddPropType(ct2);
            ContentType ct3 = ContentTypeBuilder.CreateBasicContentType("ct3", "CT3", null);
            AddPropType(ct3);
            ContentType ct4 = ContentTypeBuilder.CreateBasicContentType("ct4", "CT4", null);
            ct1.Id = 1;
            ct2.Id = 2;
            ct3.Id = 3;
            ct4.Id = 4;

            var service = new Mock<IContentTypeService>();

            IContentTypeComposition[] availableTypes = service.Object.GetAvailableCompositeContentTypes(
                ct1,
                new[] { ct1, ct2, ct3, ct4 },
                new[] { ct2.Alias })
                .Results.Where(x => x.Allowed).Select(x => x.Composition).ToArray();

            Assert.AreEqual(1, availableTypes.Count());
            Assert.AreEqual(ct4.Id, availableTypes.ElementAt(0).Id);
        }

        [Test]
        public void GetAvailableCompositeContentTypes_Not_Itself()
        {
            ContentType ct1 = ContentTypeBuilder.CreateBasicContentType("ct1", "CT1", null);
            ContentType ct2 = ContentTypeBuilder.CreateBasicContentType("ct2", "CT2", null);
            ContentType ct3 = ContentTypeBuilder.CreateBasicContentType("ct3", "CT3", null);
            ct1.Id = 1;
            ct2.Id = 2;
            ct3.Id = 3;

            var service = new Mock<IContentTypeService>();

            IContentTypeComposition[] availableTypes = service.Object.GetAvailableCompositeContentTypes(
                ct1,
                new[] { ct1, ct2, ct3 })
                .Results.Where(x => x.Allowed).Select(x => x.Composition).ToArray();

            Assert.AreEqual(2, availableTypes.Count());
            Assert.AreEqual(ct2.Id, availableTypes.ElementAt(0).Id);
            Assert.AreEqual(ct3.Id, availableTypes.ElementAt(1).Id);
        }

        // This shows that a nested comp is not allowed
        [Test]
        public void GetAvailableCompositeContentTypes_No_Results_If_Already_A_Composition_By_Parent()
        {
            ContentType ct1 = ContentTypeBuilder.CreateBasicContentType("ct1", "CT1");
            ct1.Id = 1;
            ContentType ct2 = ContentTypeBuilder.CreateBasicContentType("ct2", "CT2", ct1);
            ct2.Id = 2;
            ContentType ct3 = ContentTypeBuilder.CreateBasicContentType("ct3", "CT3");
            ct3.Id = 3;

            var service = new Mock<IContentTypeService>();

            System.Collections.Generic.IEnumerable<ContentTypeAvailableCompositionsResult> availableTypes = service.Object.GetAvailableCompositeContentTypes(
                ct1,
                new[] { ct1, ct2, ct3 }).Results;

            Assert.AreEqual(0, availableTypes.Count());
        }

        // This shows that a nested comp is not allowed
        [Test]
        public void GetAvailableCompositeContentTypes_No_Results_If_Already_A_Composition()
        {
            ContentType ct1 = ContentTypeBuilder.CreateBasicContentType("ct1", "CT1");
            ContentType ct2 = ContentTypeBuilder.CreateBasicContentType("ct2", "CT2");
            ContentType ct3 = ContentTypeBuilder.CreateBasicContentType("ct3", "CT3");
            ct1.Id = 1;
            ct2.Id = 2;
            ct3.Id = 3;

            ct2.AddContentType(ct1);

            var service = new Mock<IContentTypeService>();

            System.Collections.Generic.IEnumerable<ContentTypeAvailableCompositionsResult> availableTypes = service.Object.GetAvailableCompositeContentTypes(
                ct1,
                new[] { ct1, ct2, ct3 }).Results;

            Assert.AreEqual(0, availableTypes.Count());
        }

        [Test]
        public void GetAvailableCompositeContentTypes_Do_Not_Include_Other_Composed_Types()
        {
            ContentType ct1 = ContentTypeBuilder.CreateBasicContentType("ct1", "CT1");
            ContentType ct2 = ContentTypeBuilder.CreateBasicContentType("ct2", "CT2");
            ContentType ct3 = ContentTypeBuilder.CreateBasicContentType("ct3", "CT3");
            ct1.Id = 1;
            ct2.Id = 2;
            ct3.Id = 3;

            ct2.AddContentType(ct3);

            var service = new Mock<IContentTypeService>();

            IContentTypeComposition[] availableTypes = service.Object.GetAvailableCompositeContentTypes(
                ct1,
                new[] { ct1, ct2, ct3 })
                .Results.Where(x => x.Allowed).Select(x => x.Composition).ToArray();

            Assert.AreEqual(1, availableTypes.Count());
            Assert.AreEqual(ct3.Id, availableTypes.Single().Id);
        }

        [Test]
        public void GetAvailableCompositeContentTypes_Include_Direct_Composed_Types()
        {
            ContentType ct1 = ContentTypeBuilder.CreateBasicContentType("ct1", "CT1");
            ContentType ct2 = ContentTypeBuilder.CreateBasicContentType("ct2", "CT2");
            ContentType ct3 = ContentTypeBuilder.CreateBasicContentType("ct3", "CT3");
            ct1.Id = 1;
            ct2.Id = 2;
            ct3.Id = 3;

            ct1.AddContentType(ct3);

            var service = new Mock<IContentTypeService>();

            IContentTypeComposition[] availableTypes = service.Object.GetAvailableCompositeContentTypes(
                ct1,
                new[] { ct1, ct2, ct3 })
                .Results.Where(x => x.Allowed).Select(x => x.Composition).ToArray();

            Assert.AreEqual(2, availableTypes.Count());
            Assert.AreEqual(ct2.Id, availableTypes.ElementAt(0).Id);
            Assert.AreEqual(ct3.Id, availableTypes.ElementAt(1).Id);
        }

        [Test]
        public void GetAvailableCompositeContentTypes_Include_Indirect_Composed_Types()
        {
            ContentType ct1 = ContentTypeBuilder.CreateBasicContentType("ct1", "CT1");
            ContentType ct2 = ContentTypeBuilder.CreateBasicContentType("ct2", "CT2");
            ContentType ct3 = ContentTypeBuilder.CreateBasicContentType("ct3", "CT3");
            ContentType ct4 = ContentTypeBuilder.CreateBasicContentType("ct4", "CT4");
            ct1.Id = 1;
            ct2.Id = 2;
            ct3.Id = 3;
            ct4.Id = 4;

            ct1.AddContentType(ct3);    // ct3 is direct to ct1
            ct3.AddContentType(ct4);    // ct4 is indirect to ct1

            var service = new Mock<IContentTypeService>();

            IContentTypeComposition[] availableTypes = service.Object.GetAvailableCompositeContentTypes(
                ct1,
                new[] { ct1, ct2, ct3 })
                .Results.Where(x => x.Allowed).Select(x => x.Composition).ToArray();

            Assert.AreEqual(3, availableTypes.Count());
            Assert.AreEqual(ct2.Id, availableTypes.ElementAt(0).Id);
            Assert.AreEqual(ct3.Id, availableTypes.ElementAt(1).Id);
            Assert.AreEqual(ct4.Id, availableTypes.ElementAt(2).Id);
        }
    }
}
