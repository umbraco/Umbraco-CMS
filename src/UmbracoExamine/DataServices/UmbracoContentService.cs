using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using System.Xml.Linq;
using System.Collections;
using System.Xml.XPath;
using Examine.LuceneEngine;

namespace UmbracoExamine.DataServices
{
    public class UmbracoContentService : IContentService
    {
        protected ApplicationContext ApplicationContext { get; private set; }

		public UmbracoContentService()
			: this(ApplicationContext.Current)
		{ }

        public UmbracoContentService(ApplicationContext applicationContext)
		{
            ApplicationContext = applicationContext;
		}

        /// <summary>
        /// removes html markup from a string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		public string StripHtml(string value)
        {
			return value.StripHtml();
        }

        /// <summary>
        /// Gets published content by xpath
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
		public XDocument GetPublishedContentByXPath(string xpath)
        {
			//TODO: Remove the need for this, the best way would be to remove all requirements of examine based on Xml but that
			// would take some time. Another way in the in-term would be to add a static delegate to this class which can be set
			// on the WebBootManager to set how to get the XmlNodeByXPath but that is still ugly :(
            return LegacyLibrary.GetXmlNodeByXPath(xpath).ToXDocument();
        }

        /// <summary>
        /// This is quite an intensive operation...
        /// get all root content, then get the XML structure for all children,
        /// then run xpath against the navigator that's created
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        [Obsolete("This should no longer be used, latest content will be indexed by using the IContentService directly")]
		public XDocument GetLatestContentByXPath(string xpath)
        {
            using (var scope = ApplicationContext.Current.ScopeProvider.CreateScope())
            {
                var xmlContent = XDocument.Parse("<content></content>");
                var rootContent = ApplicationContext.Services.ContentService.GetRootContent();
                foreach (var c in rootContent)
                {
                    // not sure this uses the database, but better be save
                    xmlContent.Root.Add(c.ToDeepXml(ApplicationContext.Services.PackagingService));
                }
                var result = ((IEnumerable)xmlContent.XPathEvaluate(xpath)).Cast<XElement>();
                scope.Complete();
                return result.ToXDocument();
            }
        }

        /// <summary>
        /// Check if the node is protected
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsProtected(int nodeId, string path)
        {
            using (var scope = ApplicationContext.Current.ScopeProvider.CreateScope())
            {
                var ret = ApplicationContext.Services.PublicAccessService.IsProtected(path.EnsureEndsWith("," + nodeId));
                scope.Complete();
                return ret;
            }
        }

	    /// <summary>
        /// Returns a list of all of the user defined property names in Umbraco
        /// </summary>
        /// <returns></returns>
		
		public virtual IEnumerable<string> GetAllUserPropertyNames()
	    {
            using (var scope = ApplicationContext.Current.ScopeProvider.CreateScope())
            {
                try
	            {
                    //only return the property type aliases for media and content

	                var result = ApplicationContext.DatabaseContext.Database.Fetch<string>(
                        @"select distinct cmsPropertyType.alias from cmsPropertyType 
                        inner join cmsContentType on cmsContentType.nodeId = cmsPropertyType.contentTypeId
                        inner join umbracoNode on umbracoNode.id = cmsContentType.nodeId
                        where umbracoNode.nodeObjectType = @contentNodeObjectType OR umbracoNode.nodeObjectType = @mediaNodeObjectType
                        order by alias", new
                        {
                            contentNodeObjectType = Constants.ObjectTypes.DocumentType,
                            mediaNodeObjectType = Constants.ObjectTypes.MediaType
                        });

                    scope.Complete();
	                return result;
	            }
	            catch (Exception ex)
	            {
	                LogHelper.Error<UmbracoContentService>("EXCEPTION OCCURRED reading GetAllUserPropertyNames", ex);
	                return Enumerable.Empty<string>();
	            }
	        }
	    }

        /// <summary>
        /// Returns a list of all system field names in Umbraco
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllSystemPropertyNames()
        {
            return UmbracoContentIndexer.IndexFieldPolicies.Select(x => x.Name);
        }

    }
}
