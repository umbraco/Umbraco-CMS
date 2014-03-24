using System;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.CodeFirst.TestModels;
using Umbraco.Tests.PublishedContent;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;

namespace Umbraco.Tests.CodeFirst
{
    [DatabaseTestBehavior(DatabaseBehavior.NoDatabasePerFixture)]
    [TestFixture]
    public class StronglyTypedMapperTest : PublishedContentTestBase
    {
        [Test]
        public void Type_Test()
        {
            var doc = GetNode(1);
            var currentContent = ContentTypeMapper.Map<Home>(doc);

            Assert.That(currentContent.SiteName, Is.EqualTo("Test site"));
            Assert.That(currentContent.SiteDescription, Is.EqualTo("this is a test site"));
        }

        [Test]
        public void Children_Of_Type_Test()
        {
            var doc = GetNode(1);
            var currentContent = ContentTypeMapper.Map<Home>(doc);
            var result = currentContent.ChildrenOfType<NewsLandingPage>().ToArray();
            Assert.AreEqual("page2/alias, 2ndpagealias", result[0].PageTitle);
        }

        [Test]
        public void Descendants_Of_Type_Test()
        {
            var doc = GetNode(1);
            var currentContent = ContentTypeMapper.Map<Home>(doc);
            var result = currentContent.Descendants<NewsArticle>().ToArray();

            Assert.AreEqual("John doe", result[0].ArticleAuthor);
            Assert.AreEqual("John Smith", result[1].ArticleAuthor);
        }

        [Test]
        public void Can_Split_Pascal_Casing()
        {
            string name = "SiteName";
            string result = Regex.Replace(name, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
            Assert.That(result, Is.EqualTo("Site Name"));

            string name2 = "MySiteDefinitionCase";
            string result2 = Regex.Replace(name2, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
            Assert.That(result2, Is.EqualTo("My Site Definition Case"));
        }

        #region Test setup
        public override void Initialize()
        {   
            // required so we can access property.Value
            //PropertyValueConvertersResolver.Current = new PropertyValueConvertersResolver();

            base.Initialize();            

            // need to specify a custom callback for unit tests
            // AutoPublishedContentTypes generates properties automatically
            // when they are requested, but we must declare those that we
            // explicitely want to be here...

            var propertyTypes = new[]
                {
                    // AutoPublishedContentType will auto-generate other properties
                    new PublishedPropertyType("siteDescription", 0, "?"), 
                    new PublishedPropertyType("siteName", 0, "?"), 
                    new PublishedPropertyType("articleContent", 0, "?"), 
                    new PublishedPropertyType("articleAuthor", 0, "?"), 
                    new PublishedPropertyType("articleDate", 0, "?"), 
                    new PublishedPropertyType("pageTitle", 0, "?"), 
                };
            var type = new AutoPublishedContentType(0, "anything", propertyTypes);
            PublishedContentType.GetPublishedContentTypeCallback = (alias) => type;
            Console.WriteLine("INIT STRONG {0}",
                PublishedContentType.Get(PublishedItemType.Content, "anything")
                    .PropertyTypes.Count());
        }

        public override void TearDown()
        {
            TestHelper.CleanUmbracoSettingsConfig();

            base.TearDown();
        }

        protected override string GetXmlContent(int templateId)
        {
            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<!DOCTYPE root[ 
<!ELEMENT Home ANY>
<!ATTLIST Home id ID #REQUIRED>
<!ELEMENT NewsArticle ANY>
<!ATTLIST NewsArticle id ID #REQUIRED>
<!ELEMENT NewsLandingPage ANY>
<!ATTLIST NewsLandingPage id ID #REQUIRED>
<!ELEMENT ContentPage ANY>
<!ATTLIST ContentPage id ID #REQUIRED>
]>
<root id=""-1"">
	<Home id=""1"" parentID=""-1"" level=""1"" writerID=""0"" creatorID=""0"" nodeType=""10"" template=""" + templateId + @""" sortOrder=""1"" createDate=""2012-06-12T14:13:17"" updateDate=""2012-07-20T18:50:43"" nodeName=""Home"" urlName=""home"" writerName=""admin"" creatorName=""admin"" path=""-1,1"" isDoc="""">
		<siteName><![CDATA[Test site]]></siteName>
		<siteDescription><![CDATA[this is a test site]]></siteDescription>
		<bodyContent><![CDATA[This is some body content on the home page]]></bodyContent>
		<NewsLandingPage id=""2"" parentID=""1"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""11"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-20T18:06:45"" updateDate=""2012-07-20T19:07:31"" nodeName=""news"" urlName=""news"" writerName=""admin"" creatorName=""admin"" path=""-1,1,2"" isDoc="""">
			<bodyContent><![CDATA[This is some body content on the news landing page]]></bodyContent>
			<pageTitle><![CDATA[page2/alias, 2ndpagealias]]></pageTitle>			
			<NewsArticle id=""3"" parentID=""2"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""12"" template=""" + templateId + @""" sortOrder=""2"" createDate=""2012-07-20T18:07:54"" updateDate=""2012-07-20T19:10:27"" nodeName=""Something happened"" urlName=""something-happened"" writerName=""admin"" creatorName=""admin"" path=""-1,1,2,3"" isDoc="""">
				<articleContent><![CDATA[Some cool stuff happened today]]></articleContent>
				<articleDate><![CDATA[2012-01-02 12:33:44]]></articleDate>
				<articleAuthor><![CDATA[John doe]]></articleAuthor>
			</NewsArticle>
			<NewsArticle id=""4"" parentID=""2"" level=""3"" writerID=""0"" creatorID=""0"" nodeType=""12"" template=""" + templateId + @""" sortOrder=""3"" createDate=""2012-07-20T18:08:08"" updateDate=""2012-07-20T19:10:52"" nodeName=""Then another thing"" urlName=""then-another-thing"" writerName=""admin"" creatorName=""admin"" path=""-1,1,2,4"" isDoc="""">
				<articleContent><![CDATA[Today, other cool things occurred]]></articleContent>
				<articleDate><![CDATA[2012-01-03 15:33:44]]></articleDate>
				<articleAuthor><![CDATA[John Smith]]></articleAuthor>
			</NewsArticle>			
		</NewsLandingPage>		
		<ContentPage id=""5"" parentID=""1"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""13"" template=""" + templateId + @""" sortOrder=""4"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-18T14:23:35"" nodeName=""First Content Page"" urlName=""content-page-1"" writerName=""admin"" creatorName=""admin"" path=""-1,1,5"" isDoc="""">
			<bodyContent><![CDATA[This is some body content on the first content page]]></bodyContent>
		</ContentPage>
		<ContentPage id=""6"" parentID=""1"" level=""2"" writerID=""0"" creatorID=""0"" nodeType=""13"" template=""" + templateId + @""" sortOrder=""4"" createDate=""2012-07-16T15:26:59"" updateDate=""2012-07-16T14:23:35"" nodeName=""Second Content Page"" urlName=""content-page-2"" writerName=""admin"" creatorName=""admin"" path=""-1,1,6"" isDoc="""">
			<bodyContent><![CDATA[This is some body content on the second content page]]></bodyContent>
		</ContentPage>
	</Home>
</root>";
        }

        internal IPublishedContent GetNode(int id)
        {
            var ctx = UmbracoContext.Current;
            var doc = ctx.ContentCache.GetById(id);
            Assert.IsNotNull(doc);
            return doc;
        }
        #endregion
    }
}