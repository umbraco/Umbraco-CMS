using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Tests.Testing;
using Umbraco.Web;

namespace Umbraco.Tests.PublishedContent
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class PublishedContentExtensionTests : PublishedContentTestBase
    {
        private UmbracoContext ctx;
        private string xmlContent = "";
        private bool createContentTypes = true;
        private Dictionary<string, PublishedContentType> _contentTypes;

        protected override string GetXmlContent(int templateId)
        {
            return xmlContent;
        }

        [Test]
        public void IsDocumentType_NonRecursive_ActualType_ReturnsTrue()
        {
            InitializeInheritedContentTypes();

            var publishedContent = ctx.ContentCache.GetById(1100);
            Assert.That(publishedContent.IsDocumentType("inherited", false));
        }

        [Test]
        public void IsDocumentType_NonRecursive_BaseType_ReturnsFalse()
        {
            InitializeInheritedContentTypes();

            var publishedContent = ctx.ContentCache.GetById(1100);
            Assert.That(publishedContent.IsDocumentType("base", false), Is.False);
        }

        [Test]
        public void IsDocumentType_Recursive_ActualType_ReturnsTrue()
        {
            InitializeInheritedContentTypes();

            var publishedContent = ctx.ContentCache.GetById(1100);
            Assert.That(publishedContent.IsDocumentType("inherited", true));
        }

        [Test]
        public void IsDocumentType_Recursive_BaseType_ReturnsTrue()
        {
            InitializeInheritedContentTypes();
            ContentTypesCache.GetPublishedContentTypeByAlias = null; // fixme this is not pretty

            var publishedContent = ctx.ContentCache.GetById(1100);
            Assert.That(publishedContent.IsDocumentType("base", true));
        }

        [Test]
        public void IsDocumentType_Recursive_InvalidBaseType_ReturnsFalse()
        {
            InitializeInheritedContentTypes();

            var publishedContent = ctx.ContentCache.GetById(1100);
            Assert.That(publishedContent.IsDocumentType("invalidbase", true), Is.False);
        }

        private void InitializeInheritedContentTypes()
        {
            ctx = GetUmbracoContext("/", 1, null, true);
            if (createContentTypes)
            {
                var contentTypeService = Current.Services.ContentTypeService;
                var baseType = new ContentType(-1) { Alias = "base", Name = "Base" };
                const string contentTypeAlias = "inherited";
                var inheritedType = new ContentType(baseType, contentTypeAlias) { Alias = contentTypeAlias, Name = "Inherited" };
                contentTypeService.Save(baseType);
                contentTypeService.Save(inheritedType);
                _contentTypes = new Dictionary<string, PublishedContentType>
                {
                    { baseType.Alias, new PublishedContentType(baseType, null) },
                    { inheritedType.Alias, new PublishedContentType(inheritedType, null) }
                };
                ContentTypesCache.GetPublishedContentTypeByAlias = alias => _contentTypes[alias];
                createContentTypes = false;
            }

            ContentTypesCache.GetPublishedContentTypeByAlias = alias => _contentTypes[alias];

            xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE root[
<!ELEMENT inherited ANY>
<!ATTLIST inherited id ID #REQUIRED>
]>
<root id=""-1"">
    <inherited id=""1100"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""1"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Home"" urlName=""home"" writerName=""admin"" creatorName=""admin"" path=""-1,1046"" isDoc=""""/>
</root>";
        }
    }
}
