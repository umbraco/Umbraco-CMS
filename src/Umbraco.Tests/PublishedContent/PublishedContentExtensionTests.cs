using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;

namespace Umbraco.Tests.PublishedContent
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerFixture)]
	[TestFixture]
    public class PublishedContentExtensionTests : PublishedContentTestBase
	{
		private UmbracoContext ctx;
		private string xmlContent = "";
		private bool createContentTypes = true;
        
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
				var contentTypeService = ctx.Application.Services.ContentTypeService;
				var baseType = new ContentType(-1) {Alias = "base", Name = "Base"};
				var inheritedType = new ContentType(baseType) {Alias = "inherited", Name = "Inherited"};
				contentTypeService.Save(baseType);
				contentTypeService.Save(inheritedType);
				createContentTypes = false;
			}
			#region setup xml content
			xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE root[ 
<!ELEMENT inherited ANY>
<!ATTLIST inherited id ID #REQUIRED>
]>
<root id=""-1"">
	<inherited id=""1100"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""1044"" template=""1"" sortOrder=""1"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Home"" urlName=""home"" writerName=""admin"" creatorName=""admin"" path=""-1,1046"" isDoc=""""/>
</root>";
			#endregion
		}
	}
}
