using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;
using umbraco;
using System.Xml.Linq;
using System.Xml;
using umbraco.cms.businesslogic.web;
using System.Collections;
using System.Xml.XPath;
using umbraco.DataLayer;
using umbraco.BusinessLogic;
using UmbracoExamine.Config;
using Examine.LuceneEngine;
using System.Data.SqlClient;
using System.Diagnostics;

namespace UmbracoExamine.DataServices
{
    public class UmbracoContentService : IContentService
    {

        private readonly ApplicationContext _applicationContext;

		public UmbracoContentService()
			: this(ApplicationContext.Current)
		{

		}

        public UmbracoContentService(ApplicationContext applicationContext)
		{
            _applicationContext = applicationContext;
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
            var xmlContent = XDocument.Parse("<content></content>");
            foreach (var c in _applicationContext.Services.ContentService.GetRootContent())
            {
                xmlContent.Root.Add(c.ToDeepXml(_applicationContext.Services.PackagingService));				
            }
            var result = ((IEnumerable)xmlContent.XPathEvaluate(xpath)).Cast<XElement>();
            return result.ToXDocument();
        }

        /// <summary>
        /// Check if the node is protected
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsProtected(int nodeId, string path)
        {
            return _applicationContext.Services.PublicAccessService.IsProtected(path.EnsureEndsWith("," + nodeId));
        }

	    /// <summary>
        /// Returns a list of all of the user defined property names in Umbraco
        /// </summary>
        /// <returns></returns>
		
		public IEnumerable<string> GetAllUserPropertyNames()
	    {
            try
            {
                var result = _applicationContext.DatabaseContext.Database.Fetch<string>("select distinct alias from cmsPropertyType order by alias");
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.Error<UmbracoContentService>("EXCEPTION OCCURRED reading GetAllUserPropertyNames", ex);
                return Enumerable.Empty<string>();
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
