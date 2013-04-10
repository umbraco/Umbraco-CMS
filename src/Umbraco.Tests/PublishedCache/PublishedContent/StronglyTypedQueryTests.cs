using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.PublishedCache.XmlPublishedCache;

namespace Umbraco.Tests.PublishedContent
{
	[TestFixture]
	public class StronglyTypedQueryTests : PublishedContentTestBase
	{
	    public override void Initialize()
	    {
	        base.Initialize();
	    }

	    public override void TearDown()
        {
            base.TearDown();
        }

        protected override DatabaseBehavior DatabaseTestBehavior
        {
            get { return DatabaseBehavior.NoDatabasePerFixture; }
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

        
		[Test]
		public void Type_Test()
		{
			var doc = GetNode(1);
			var result = doc.NewsArticles(TraversalType.Descendants).ToArray();
			Assert.AreEqual("John doe", result[0].ArticleAuthor);
			Assert.AreEqual("John Smith", result[1].ArticleAuthor);
		}

        
		[Test]
		public void As_Test()
		{
			var doc = GetNode(1);
			var result = doc.AsHome();
			Assert.AreEqual("Test site", result.SiteName);

			Assert.Throws<InvalidOperationException>(() => doc.AsContentPage());
		}

	}

	//NOTE: Some of these class will be moved in to the core once all this is working the way we want

	#region Gen classes & supporting classes

	//TOOD: SD: This class could be the way that the UmbracoHelper deals with looking things up in the background, we might not
	// even expose it publicly but it could handle any caching (per request) that might be required when looking up any objects...
	// though we might not need it at all, not sure yet.
	// However, what we need to do is implement the GetDocumentsByType method of the IPublishedStore, see the TODO there.
	// It might be nicer to have a QueryContext on the UmbracoHelper (we can still keep the Content and TypedContent, etc...
	// methods, but these would just wrap the QueryContext attached to it. Other methods on the QueryContext will be 
	// ContentByType, TypedContentByType, etc... then we can also have extension methods like below for strongly typed
	// access like: GetAllHomes, GetAllNewsArticles, etc...

	//public class QueryDataContext
	//{
	//	private readonly IPublishedContentStore _contentStore;
	//	private readonly UmbracoContext _umbracoContext;

	//	internal QueryDataContext(IPublishedContentStore contentStore, UmbracoContext umbracoContext)
	//	{
	//		_contentStore = contentStore;
	//		_umbracoContext = umbracoContext;
	//	}

	//	public IPublishedContent GetDocumentById(int id)
	//	{		
	//		return _contentStore.GetDocumentById(_umbracoContext, id);
	//	}

	//	public IEnumerable<IPublishedContent> GetByDocumentType(string alias)
	//	{
			
	//	} 
	//}

	public enum TraversalType
	{
		Children,
		Ancestors,
		AncestorsOrSelf,
		Descendants,
		DescendantsOrSelf
	}

	public static class StronglyTypedQueryExtensions
	{
		private static IEnumerable<IPublishedContent> GetEnumerable(this IPublishedContent content, string docTypeAlias, TraversalType traversalType = TraversalType.Children)
		{
			switch (traversalType)
			{
				case TraversalType.Children:
					return content.Children.Where(x => x.DocumentTypeAlias == docTypeAlias);
				case TraversalType.Ancestors:
					return content.Ancestors().Where(x => x.DocumentTypeAlias == docTypeAlias);
				case TraversalType.AncestorsOrSelf:
					return content.AncestorsOrSelf().Where(x => x.DocumentTypeAlias == docTypeAlias);
				case TraversalType.Descendants:
					return content.Descendants().Where(x => x.DocumentTypeAlias == docTypeAlias);
				case TraversalType.DescendantsOrSelf:
					return content.DescendantsOrSelf().Where(x => x.DocumentTypeAlias == docTypeAlias);
				default:
					throw new ArgumentOutOfRangeException("traversalType");
			}
		}

		private static T AsDocumentType<T>(this IPublishedContent content, string alias, Func<IPublishedContent, T> creator)
		{
			if (content.DocumentTypeAlias == alias) return creator(content);
			throw new InvalidOperationException("The content type cannot be cast to " + typeof(T).FullName + " since it is type: " + content.DocumentTypeAlias);
		}

		public static HomeContentItem AsHome(this IPublishedContent content)
		{
			return content.AsDocumentType("Home", x => new HomeContentItem(x));
		}

		public static IEnumerable<HomeContentItem> Homes(this IPublishedContent content, TraversalType traversalType = TraversalType.Children)
		{			
			return content.GetEnumerable("Home", traversalType).Select(x => new HomeContentItem(x));
		}

		public static NewsArticleContentItem AsNewsArticle(this IPublishedContent content)
		{
			return content.AsDocumentType("NewsArticle", x => new NewsArticleContentItem(x));
		}

		public static IEnumerable<NewsArticleContentItem> NewsArticles(this IPublishedContent content, TraversalType traversalType = TraversalType.Children)
		{
			return content.GetEnumerable("NewsArticle", traversalType).Select(x => new NewsArticleContentItem(x));
		}

		public static NewsLandingPageContentItem AsNewsLandingPage(this IPublishedContent content)
		{
			return content.AsDocumentType("NewsLandingPage", x => new NewsLandingPageContentItem(x));
		}

		public static IEnumerable<NewsLandingPageContentItem> NewsLandingPages(this IPublishedContent content, TraversalType traversalType = TraversalType.Children)
		{
			return content.GetEnumerable("NewsLandingPage", traversalType).Select(x => new NewsLandingPageContentItem(x));
		}

		public static ContentPageContentItem AsContentPage(this IPublishedContent content)
		{
			return content.AsDocumentType("ContentPage", x => new ContentPageContentItem(x));
		}

		public static IEnumerable<ContentPageContentItem> ContentPages(this IPublishedContent content, TraversalType traversalType = TraversalType.Children)
		{
			return content.GetEnumerable("ContentPage", traversalType).Select(x => new ContentPageContentItem(x));
		}
	}

    public class PublishedContentWrapper : IPublishedContent, IOwnerCollectionAware<IPublishedContent>
	{
		protected IPublishedContent WrappedContent { get; private set; }

		public PublishedContentWrapper(IPublishedContent content)
		{
			WrappedContent = content;
		}

		public string Url
		{
			get { return WrappedContent.Url; }
		}

		public PublishedItemType ItemType
		{
			get { return WrappedContent.ItemType; }
		}

		public IPublishedContent Parent
		{
			get { return WrappedContent.Parent; }
		}

		public int Id
		{
			get { return WrappedContent.Id; }
		}
		public int TemplateId
		{
			get { return WrappedContent.TemplateId; }
		}
		public int SortOrder
		{
			get { return WrappedContent.SortOrder; }
		}
		public string Name
		{
			get { return WrappedContent.Name; }
		}
		public string UrlName
		{
			get { return WrappedContent.UrlName; }
		}
		public string DocumentTypeAlias
		{
			get { return WrappedContent.DocumentTypeAlias; }
		}
		public int DocumentTypeId
		{
			get { return WrappedContent.DocumentTypeId; }
		}
		public string WriterName
		{
			get { return WrappedContent.WriterName; }
		}
		public string CreatorName
		{
			get { return WrappedContent.CreatorName; }
		}
		public int WriterId
		{
			get { return WrappedContent.WriterId; }
		}
		public int CreatorId
		{
			get { return WrappedContent.CreatorId; }
		}
		public string Path
		{
			get { return WrappedContent.Path; }
		}
		public DateTime CreateDate
		{
			get { return WrappedContent.CreateDate; }
		}
		public DateTime UpdateDate
		{
			get { return WrappedContent.UpdateDate; }
		}
		public Guid Version
		{
			get { return WrappedContent.Version; }
		}
		public int Level
		{
			get { return WrappedContent.Level; }
		}
		public ICollection<IPublishedContentProperty> Properties
		{
			get { return WrappedContent.Properties; }
		}

		public object this[string propertyAlias]
		{
			get { return GetProperty(propertyAlias).Value; }
		}

		public IEnumerable<IPublishedContent> Children
		{
			get { return WrappedContent.Children; }
		}
		public IPublishedContentProperty GetProperty(string alias)
		{
			return WrappedContent.GetProperty(alias);
		}

        private IEnumerable<IPublishedContent> _ownersCollection;

        /// <summary>
        /// Need to get/set the owner collection when an item is returned from the result set of a query
        /// </summary>
        /// <remarks>
        /// Based on this issue here: http://issues.umbraco.org/issue/U4-1797
        /// </remarks>
        IEnumerable<IPublishedContent> IOwnerCollectionAware<IPublishedContent>.OwnersCollection
        {
            get
            {
                var publishedContentBase = WrappedContent as IOwnerCollectionAware<IPublishedContent>;
                if (publishedContentBase != null)
                {
                    return publishedContentBase.OwnersCollection;
                }

                //if the owners collection is null, we'll default to it's siblings
                if (_ownersCollection == null)
                {
                    //get the root docs if parent is null
                    _ownersCollection = this.Siblings();
                }
                return _ownersCollection;
            }
            set
            {
                var publishedContentBase = WrappedContent as IOwnerCollectionAware<IPublishedContent>;
                if (publishedContentBase != null)
                {
                    publishedContentBase.OwnersCollection = value;
                }
                else
                {
                    _ownersCollection = value;
                }
            }
        }
	}

	public partial class HomeContentItem : ContentPageContentItem
	{
		public HomeContentItem(IPublishedContent content)
			: base(content)
		{
		}

		public string SiteName
		{
			get { return WrappedContent.GetPropertyValue<string>("siteName"); }
		}
		public string SiteDescription
		{
			get { return WrappedContent.GetPropertyValue<string>("siteDescription"); }
		}
	}

	public partial class NewsLandingPageContentItem : ContentPageContentItem
	{
		public NewsLandingPageContentItem(IPublishedContent content)
			: base(content)
		{
		}

		public string PageTitle
		{
			get { return WrappedContent.GetPropertyValue<string>("pageTitle"); }
		}		
	}

	public partial class NewsArticleContentItem : PublishedContentWrapper
	{
		public NewsArticleContentItem(IPublishedContent content)
			: base(content)
		{
		}

		public string ArticleContent
		{
			get { return WrappedContent.GetPropertyValue<string>("articleContent"); }
		}
		public DateTime ArticleDate
		{
			get { return WrappedContent.GetPropertyValue<DateTime>("articleDate"); }
		}
		public string ArticleAuthor
		{
			get { return WrappedContent.GetPropertyValue<string>("articleAuthor"); }
		}
	}

	public partial class ContentPageContentItem : PublishedContentWrapper
	{
		public ContentPageContentItem(IPublishedContent content)
			: base(content)
		{
		}

		public string BodyContent
		{
			get { return WrappedContent.GetPropertyValue<string>("bodyContent"); }
		}
	}

	#endregion
}