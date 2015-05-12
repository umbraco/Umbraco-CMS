using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Cache;
using Umbraco.Web.Templates;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using umbraco.cms.helpers;
using umbraco.scripting;
using umbraco.DataLayer;
using Umbraco.Core.IO;
using Language = umbraco.cms.businesslogic.language.Language;
using Media = umbraco.cms.businesslogic.media.Media;
using Member = umbraco.cms.businesslogic.member.Member;
using PropertyType = umbraco.cms.businesslogic.propertytype.PropertyType;
using Relation = umbraco.cms.businesslogic.relation.Relation;
using UmbracoContext = umbraco.presentation.UmbracoContext;

namespace umbraco
{
    /// <summary>
    /// Function library for umbraco. Includes various helper-methods and methods to
    /// save and load data from umbraco. 
    /// 
    /// Especially usefull in XSLT where any of these methods can be accesed using the umbraco.library name-space. Example:
    /// &lt;xsl:value-of select="umbraco.library:NiceUrl(@id)"/&gt;
    /// </summary>
    public class library
    {
		/// <summary>
		/// Returns a new UmbracoHelper so that we can start moving the logic from some of these methods to it
		/// </summary>
		/// <returns></returns>
		private static UmbracoHelper GetUmbracoHelper()
		{
			return new UmbracoHelper(Umbraco.Web.UmbracoContext.Current);
		}

        #region Declarations

        /// <summary>
        /// Used by umbraco's publishing enginge, to determine if publishing is currently active
        /// </summary>
        public static bool IsPublishing = false;
        /// <summary>
        /// Used by umbraco's publishing enginge, to how many nodes is publish in the current publishing cycle
        /// </summary>
        public static int NodesPublished = 0;
        /// <summary>
        /// Used by umbraco's publishing enginge, to determine the start time of the current publishing cycle.
        /// </summary>
        public static DateTime PublishStart;
        private page _page;
        private static readonly object libraryCacheLock = new object();

        #endregion

        #region Properties

        protected static ISqlHelper SqlHelper
        {
            get { return umbraco.BusinessLogic.Application.SqlHelper; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Empty constructor
        /// </summary>
        public library()
        {
        }

        public library(int id)
        {
            _page = new page(((System.Xml.IHasXmlNode)GetXmlNodeById(id.ToString()).Current).GetNode());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="library"/> class.
        /// </summary>
        /// <param name="Page">The page.</param>
        public library(page page)
        {
            _page = page;
        }

        #endregion

        #region Python Helper functions

        /// <summary>
        /// Executes the given python script and returns the standardoutput.
        /// The Globals known from python macros are not accessible in this context.
        /// Neither macro or page nor the globals known from python macros are 
        /// accessible in this context. Only stuff we initialized in site.py
        /// can be used.
        /// </summary>
        /// <param name="file">The filename of the python script including the extension .py</param>
        /// <returns>Returns the StandardOutput</returns>
        public static string PythonExecuteFile(string file)
        {
            try
            {
                string path = IOHelper.MapPath(SystemDirectories.MacroScripts + "/" + file);
                object res = python.executeFile(path);
                return res.ToString();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        /// <summary>
        /// Executes the given python expression and returns the standardoutput.
        /// The Globals known from python macros are not accessible in this context.
        /// Neighter macro or page nor the globals known from python macros are 
        /// accessible in this context. Only stuff we initialized in site.py
        /// can be used.
        /// </summary>
        /// <param name="expression">Python expression to execute</param>
        /// <returns>Returns the StandardOutput</returns>
        public static string PythonExecute(string expression)
        {
            try
            {
                object res = python.execute(expression);
                return res.ToString();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        #endregion

        #region Publish Helper Methods

        
        /// <summary>
        /// Unpublish a node, by removing it from the runtime xml index. Note, prior to this the Document should be 
        /// marked unpublished by setting the publish property on the document object to false
        /// </summary>
        /// <param name="DocumentId">The Id of the Document to be unpublished</param>
        [Obsolete("This method is no longer used, a document's cache will be removed automatically when the document is deleted or unpublished")]
        public static void UnPublishSingleNode(int DocumentId)
        {
            DistributedCache.Instance.RemovePageCache(DocumentId);
        }

        /// <summary>
        /// Publishes a Document by adding it to the runtime xml index. Note, prior to this the Document should be 
        /// marked published by calling Publish(User u) on the document object.
        /// </summary>
        /// <param name="documentId">The Id of the Document to be published</param>
        [Obsolete("This method is no longer used, a document's cache will be updated automatically when the document is published")]
        public static void UpdateDocumentCache(int documentId)
        {
            DistributedCache.Instance.RefreshPageCache(documentId);
        }

        /// <summary>
        /// Publishes the single node, this method is obsolete
        /// </summary>
        /// <param name="DocumentId">The document id.</param>
        [Obsolete("Please use: umbraco.library.UpdateDocumentCache")]
        public static void PublishSingleNode(int DocumentId)
        {
            UpdateDocumentCache(DocumentId);
        }
        
        /// <summary>
        /// Refreshes the xml cache for all nodes
        /// </summary>
        public static void RefreshContent()
        {
            DistributedCache.Instance.RefreshAllPageCache();
        }

        /// <summary>
        /// Re-publishes all nodes under a given node
        /// </summary>
        /// <param name="nodeID">The ID of the node and childnodes that should be republished</param>
        [Obsolete("Please use: umbraco.library.RefreshContent")]
        public static string RePublishNodes(int nodeID)
        {
            DistributedCache.Instance.RefreshAllPageCache();

            return string.Empty;
        }

        /// <summary>
        /// Re-publishes all nodes under a given node
        /// </summary>
        /// <param name="nodeID">The ID of the node and childnodes that should be republished</param>
        [Obsolete("Please use: umbraco.library.RefreshContent")]
        public static void RePublishNodesDotNet(int nodeID)
        {
            DistributedCache.Instance.RefreshAllPageCache();
        }

        /// <summary>
        /// Refreshes the runtime xml index. 
        /// Note: This *doesn't* mark any non-published document objects as published
        /// </summary>
        /// <param name="nodeID">Always use -1</param>
        /// <param name="SaveToDisk">Not used</param>
        [Obsolete("Please use: content.Instance.RefreshContentFromDatabaseAsync")]
        public static void RePublishNodesDotNet(int nodeID, bool SaveToDisk)
        {
            DistributedCache.Instance.RefreshAllPageCache();
        }

        #endregion

        #region Xslt Helper functions

        /// <summary>
        /// This will convert a json structure to xml for use in xslt
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static XPathNodeIterator JsonToXml(string json)
        {
            try
            {
                if (json.StartsWith("["))
                {
                    //we'll assume it's an array, in which case we need to add a root
                    json = "{\"arrayitem\":" + json + "}";
                }
                var xml = JsonConvert.DeserializeXmlNode(json, "json", false);
                return xml.CreateNavigator().Select("/json");
            }
            catch (Exception ex)
            {
                var xd = new XmlDocument();
                xd.LoadXml(string.Format("<error>Could not convert JSON to XML. Error: {0}</error>", ex));
                return xd.CreateNavigator().Select("/error");
            }
        }

        /// <summary>
        /// Add a session variable to the current user
        /// </summary>
        /// <param name="key">The Key of the variable</param>
        /// <param name="value">The Value</param>
        public static void setSession(string key, string value)
        {
            if (HttpContext.Current.Session != null)
                HttpContext.Current.Session[key] = value;
        }

        /// <summary>
        /// Add a cookie variable to the current user
        /// </summary>
        /// <param name="key">The Key of the variable</param>
        /// <param name="value">The Value of the variable</param>
        public static void setCookie(string key, string value)
        {
            StateHelper.SetCookieValue(key, value);
        }

        /// <summary>
        /// Returns a string with a friendly url from a node.
        /// IE.: Instead of having /482 (id) as an url, you can have
        /// /screenshots/developer/macros (spoken url)
        /// </summary>
        /// <param name="nodeID">Identifier for the node that should be returned</param>
        /// <returns>String with a friendly url from a node</returns>
        public static string NiceUrl(int nodeID)
        {
        	return GetUmbracoHelper().NiceUrl(nodeID);            
        }

        /// <summary>
        /// This method will always add the root node to the path. You should always use NiceUrl, as that is the
        /// only one who checks for toplevel node settings in the web.config
        /// </summary>
        /// <param name="nodeID">Identifier for the node that should be returned</param>
        /// <returns>String with a friendly url from a node</returns>
        [Obsolete]
        public static string NiceUrlFullPath(int nodeID)
        {
            throw new NotImplementedException("It was broken anyway...");
        }

        /// <summary>
        /// This method will always add the domain to the path if the hostnames are set up correctly. 
        /// </summary>
        /// <param name="nodeID">Identifier for the node that should be returned</param>
        /// <returns>String with a friendly url with full domain from a node</returns>
        public static string NiceUrlWithDomain(int nodeID)
        {
        	return GetUmbracoHelper().NiceUrlWithDomain(nodeID);
        }

        /// <summary>
        /// This method will always add the domain to the path. 
        /// </summary>
        /// <param name="nodeID">Identifier for the node that should be returned</param>
        /// <param name="ignoreUmbracoHostNames">Ignores the umbraco hostnames and returns the url prefixed with the requested host (including scheme and port number)</param>
        /// <returns>String with a friendly url with full domain from a node</returns>
        internal static string NiceUrlWithDomain(int nodeID, bool ignoreUmbracoHostNames)
        {
            if (ignoreUmbracoHostNames)
                return HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + NiceUrl(nodeID);

            return NiceUrlWithDomain(nodeID);
        }

        public static string ResolveVirtualPath(string path)
        {
			return Umbraco.Core.IO.IOHelper.ResolveUrl(path);
        }


        /// <summary>
        /// Returns a string with the data from the given element of a node. Both elements (data-fields)
        /// and properties can be used - ie:
        /// getItem(1, nodeName) will return a string with the name of the node with id=1 even though
        /// nodeName is a property and not an element (data-field).
        /// </summary>
        /// <param name="nodeID">Identifier for the node that should be returned</param>
        /// <param name="alias">The element that should be returned</param>
        /// <returns>Returns a string with the data from the given element of a node</returns>
        public static string GetItem(int nodeID, String alias)
        {
	        var doc = Umbraco.Web.UmbracoContext.Current.ContentCache.GetById(nodeID);

			if (doc == null)
				return string.Empty;

	        switch (alias)
	        {
				case "id":
			        return doc.Id.ToString();
				case "version":
			        return doc.Version.ToString();
				case "parentID":
			        return doc.Parent.Id.ToString();
				case "level":
			        return doc.Level.ToString();
				case "writerID":
			        return doc.WriterId.ToString();
				case "template":
			        return doc.TemplateId.ToString();
				case "sortOrder":
			        return doc.SortOrder.ToString();
				case "createDate":
					return doc.CreateDate.ToString("yyyy-MM-dd'T'HH:mm:ss");
				case "updateDate":
					return doc.UpdateDate.ToString("yyyy-MM-dd'T'HH:mm:ss");
				case "nodeName":
			        return doc.Name;
				case "writerName":
			        return doc.WriterName;
				case "path":
			        return doc.Path;
				case "creatorName":
			        return doc.CreatorName;
	        }

            // in 4.9.0 the method returned the raw XML from the cache, unparsed
            // starting with 5c20f4f (4.10?) the method returns prop.Value.ToString()
            //   where prop.Value is parsed for internal links + resolve urls - but not for macros
            //   comments say "fixing U4-917 and U4-821" which are not related
            // if we return DataValue.ToString() we're back to the original situation
            // if we return Value.ToString() we'll have macros parsed and that's nice
            //
            // so, use Value.ToString() here.
	        var prop = doc.GetProperty(alias);
	        return prop == null ? string.Empty : prop.Value.ToString();
        }

        /// <summary>
        /// Checks with the Assigned domains settings and retuns an array the the Domains matching the node
        /// </summary>
        /// <param name="NodeId">Identifier for the node that should be returned</param>
        /// <returns>A Domain array with all the Domains that matches the nodeId</returns>
        public static Domain[] GetCurrentDomains(int NodeId)
        {
            string[] pathIds = GetItem(NodeId, "path").Split(',');
            for (int i = pathIds.Length - 1; i > 0; i--)
            {
                Domain[] retVal = Domain.GetDomainsById(int.Parse(pathIds[i]));
                if (retVal.Length > 0)
                {
                    return retVal;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns a string with the data from the given element of the current node. Both elements (data-fields)
        /// and properties can be used - ie:
        /// getItem(nodeName) will return a string with the name of the current node/page even though
        /// nodeName is a property and not an element (data-field).
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static string GetItem(String alias)
        {
            try
            {
                int currentID = int.Parse(HttpContext.Current.Items["pageID"].ToString());
                return GetItem(currentID, alias);
            }
            catch (Exception ItemException)
            {
                HttpContext.Current.Trace.Warn("library.GetItem", "Error reading '" + alias + "'", ItemException);
                return string.Empty;
            }
        }

        /// <summary>
        /// Returns that name of a generic property
        /// </summary>
        /// <param name="ContentTypeAlias">The Alias of the content type (ie. Document Type, Member Type or Media Type)</param>
        /// <param name="PropertyTypeAlias">The Alias of the Generic property (ie. bodyText or umbracoNaviHide)</param>
        /// <returns>A string with the name. If nothing matches the alias, an empty string is returned</returns>
        public static string GetPropertyTypeName(string ContentTypeAlias, string PropertyTypeAlias)
        {
            try
            {
                umbraco.cms.businesslogic.ContentType ct = umbraco.cms.businesslogic.ContentType.GetByAlias(ContentTypeAlias);
                PropertyType pt = ct.getPropertyType(PropertyTypeAlias);
                return pt.Name;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Returns the Member Name from an umbraco member object
        /// </summary>
        /// <param name="MemberId">The identifier of the Member</param>
        /// <returns>The Member name matching the MemberId, an empty string is member isn't found</returns>
        public static string GetMemberName(int MemberId)
        {
            if (MemberId != 0)
            {
                try
                {
                    Member m = new Member(MemberId);
                    return m.Text;
                }
                catch
                {
                    return string.Empty;
                }
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// Get a media object as an xml object
        /// </summary>
        /// <param name="MediaId">The identifier of the media object to be returned</param>
        /// <param name="Deep">If true, children of the media object is returned</param>
        /// <returns>An umbraco xml node of the media (same format as a document node)</returns>
        public static XPathNodeIterator GetMedia(int MediaId, bool Deep)
        {
            try
            {
                if (UmbracoConfig.For.UmbracoSettings().Content.UmbracoLibraryCacheDuration > 0)
                {
                    var xml = ApplicationContext.Current.ApplicationCache.GetCacheItem(
                        string.Format(
                            "{0}_{1}_{2}", CacheKeys.MediaCacheKey, MediaId, Deep),
                        TimeSpan.FromSeconds(UmbracoConfig.For.UmbracoSettings().Content.UmbracoLibraryCacheDuration),
                        () => GetMediaDo(MediaId, Deep));

                    if (xml != null)
                    {                   
                        //returning the root element of the Media item fixes the problem
                        return xml.CreateNavigator().Select("/");
                    }
                        
                }
                else
                {
                    var xml = GetMediaDo(MediaId, Deep);
                    
                    //returning the root element of the Media item fixes the problem
                    return xml.CreateNavigator().Select("/");
                }
            }
            catch(Exception ex)
            {
                LogHelper.Error<library>("An error occurred looking up media", ex);
            }

            LogHelper.Debug<library>("No media result for id {0}", () => MediaId);

            var errorXml = new XElement("error", string.Format("No media is maching '{0}'", MediaId));
            return errorXml.CreateNavigator().Select("/");
        }

        private static XElement GetMediaDo(int mediaId, bool deep)
        {
            var media = ApplicationContext.Current.Services.MediaService.GetById(mediaId);
            if (media == null) return null;
            var serializer = new EntityXmlSerializer();
            var serialized = serializer.Serialize(
                ApplicationContext.Current.Services.MediaService, 
                ApplicationContext.Current.Services.DataTypeService,
                ApplicationContext.Current.Services.UserService,                
                media, 
                deep);
            return serialized;
        }

        /// <summary>
        /// Get a member as an xml object
        /// </summary>
        /// <param name="MemberId">The identifier of the member object to be returned</param>
        /// <returns>An umbraco xml node of the member (same format as a document node), but with two additional attributes on the "node" element:
        /// "email" and "loginName".
        /// </returns>
        public static XPathNodeIterator GetMember(int MemberId)
        {
            try
            {
                if (UmbracoConfig.For.UmbracoSettings().Content.UmbracoLibraryCacheDuration > 0)
                {
                    var xml = ApplicationContext.Current.ApplicationCache.GetCacheItem(
                        string.Format(
                            "{0}_{1}", CacheKeys.MemberLibraryCacheKey, MemberId),
                        TimeSpan.FromSeconds(UmbracoConfig.For.UmbracoSettings().Content.UmbracoLibraryCacheDuration),
                        () => GetMemberDo(MemberId));

                    if (xml != null)
                    {
                        return xml.CreateNavigator().Select("/");
                    }
                }
                else
                {
                    return GetMemberDo(MemberId).CreateNavigator().Select("/");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<library>("An error occurred looking up member", ex);
            }

            LogHelper.Debug<library>("No member result for id {0}", () => MemberId);

            var xd = new XmlDocument();
            xd.LoadXml(string.Format("<error>No member is maching '{0}'</error>", MemberId));
            return xd.CreateNavigator().Select("/");
        }

        private static XElement GetMemberDo(int MemberId)
        {
            var member = ApplicationContext.Current.Services.MemberService.GetById(MemberId);
            if (member == null) return null;
            var serializer = new EntityXmlSerializer();
            var serialized = serializer.Serialize(
                ApplicationContext.Current.Services.DataTypeService, member);
            return serialized;
        }

        /// <summary>
        /// Get the current member as an xml node
        /// </summary>
        /// <returns>Look in documentation for umbraco.library.GetMember(MemberId) for more information</returns>
        public static XPathNodeIterator GetCurrentMember()
        {
            Member m = Member.GetCurrentMember();
            if (m != null)
            {
                XmlDocument mXml = new XmlDocument();
                mXml.LoadXml(m.ToXml(mXml, false).OuterXml);
                XPathNavigator xp = mXml.CreateNavigator();
                return xp.Select("/node");
            }

            XmlDocument xd = new XmlDocument();
            xd.LoadXml(
                "<error>No current member exists (best practice is to validate with 'isloggedon()' prior to this call)</error>");
            return xd.CreateNavigator().Select("/");
        }

        /// <summary>
        /// Whether or not the current user is logged in (as a member)
        /// </summary>
        /// <returns>True is the current user is logged in</returns>
        public static bool IsLoggedOn()
        {
        	return GetUmbracoHelper().MemberIsLoggedOn();
        }

        public static XPathNodeIterator AllowedGroups(int documentId, string path)
        {
            XmlDocument xd = new XmlDocument();
            xd.LoadXml("<roles/>");
            foreach (string role in Access.GetAccessingMembershipRoles(documentId, path))
                xd.DocumentElement.AppendChild(xmlHelper.addTextNode(xd, "role", role));
            return xd.CreateNavigator().Select(".");
        }

        /// <summary>
        /// Check if a document object is protected by the "Protect Pages" functionality in umbraco
        /// </summary>
        /// <param name="DocumentId">The identifier of the document object to check</param>
        /// <param name="Path">The full path of the document object to check</param>
        /// <returns>True if the document object is protected</returns>
        public static bool IsProtected(int DocumentId, string Path)
        {
        	return GetUmbracoHelper().IsProtected(DocumentId, Path);
        }

        /// <summary>
        /// Check if the current user has access to a document
        /// </summary>
        /// <param name="NodeId">The identifier of the document object to check</param>
        /// <param name="Path">The full path of the document object to check</param>
        /// <returns>True if the current user has access or if the current document isn't protected</returns>
        public static bool HasAccess(int NodeId, string Path)
        {
        	return GetUmbracoHelper().MemberHasAccess(NodeId, Path);
        }


        /// <summary>
        /// Returns an MD5 hash of the string specified
        /// </summary>
        /// <param name="text">The text to create a hash from</param>
        /// <returns>Md5 has of the string</returns>
        public static string md5(string text)
        {
			return text.ToMd5();
        }

        /// <summary>
        /// Compare two dates
        /// </summary>
        /// <param name="firstDate">The first date to compare</param>
        /// <param name="secondDate">The second date to compare</param>
        /// <returns>True if the first date is greater than the second date</returns>
        public static bool DateGreaterThan(string firstDate, string secondDate)
        {
            if (DateTime.Parse(firstDate) > DateTime.Parse(secondDate))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Compare two dates
        /// </summary>
        /// <param name="firstDate">The first date to compare</param>
        /// <param name="secondDate">The second date to compare</param>
        /// <returns>True if the first date is greater than or equal the second date</returns>
        public static bool DateGreaterThanOrEqual(string firstDate, string secondDate)
        {
            if (DateTime.Parse(firstDate) >= DateTime.Parse(secondDate))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Check if a date is greater than today
        /// </summary>
        /// <param name="firstDate">The date to check</param>
        /// <returns>True if the date is greater that today (ie. at least the day of tomorrow)</returns>
        public static bool DateGreaterThanToday(string firstDate)
        {
            DateTime first = DateTime.Parse(firstDate);
            first = new DateTime(first.Year, first.Month, first.Day);
            DateTime today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            TimeSpan TS = new TimeSpan(first.Ticks - today.Ticks);
            if (TS.Days > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Check if a date is greater than or equal today
        /// </summary>
        /// <param name="firstDate">The date to check</param>
        /// <returns>True if the date is greater that or equal today (ie. at least today or the day of tomorrow)</returns>
        public static bool DateGreaterThanOrEqualToday(string firstDate)
        {
            DateTime first = DateTime.Parse(firstDate);
            first = new DateTime(first.Year, first.Month, first.Day);
            DateTime today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            TimeSpan TS = new TimeSpan(first.Ticks - today.Ticks);
            if (TS.Days >= 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Get the current date
        /// </summary>
        /// <returns>Current date i xml format (ToString("s"))</returns>
        public static string CurrentDate()
        {
            return DateTime.Now.ToString("s");
        }

        /// <summary>
        /// Add a value to a date
        /// </summary>
        /// <param name="Date">The Date to user</param>
        /// <param name="AddType">The type to add: "y": year, "m": month, "d": day, "h": hour, "min": minutes, "s": seconds</param>
        /// <param name="add">An integer value to add</param>
        /// <returns>A date in xml format (ToString("s"))</returns>
        public static string DateAdd(string Date, string AddType, int add)
        {
            return DateAddWithDateTimeObject(DateTime.Parse(Date), AddType, add);
        }

        /// <summary>
        /// Get the day of week from a date matching the current culture settings
        /// </summary>
        /// <param name="Date">The date to use</param>
        /// <returns>A string with the DayOfWeek matching the current contexts culture settings</returns>
        public static string GetWeekDay(string Date)
        {
            return DateTime.Parse(Date).ToString("dddd");
        }

        /// <summary>
        /// Add a value to a date. Similar to the other overload, but uses a datetime object instead of a string
        /// </summary>
        /// <param name="Date">The Date to user</param>
        /// <param name="AddType">The type to add: "y": year, "m": month, "d": day, "h": hour, "min": minutes, "s": seconds</param>
        /// <param name="add">An integer value to add</param>
        /// <returns>A date in xml format (ToString("s"))</returns>
        public static string DateAddWithDateTimeObject(DateTime Date, string AddType, int add)
        {
            switch (AddType.ToLower())
            {
                case "y":
                    Date = Date.AddYears(add);
                    break;
                case "m":
                    Date = Date.AddMonths(add);
                    break;
                case "d":
                    Date = Date.AddDays(add);
                    break;
                case "h":
                    Date = Date.AddHours(add);
                    break;
                case "min":
                    Date = Date.AddMinutes(add);
                    break;
                case "s":
                    Date = Date.AddSeconds(add);
                    break;
            }

            return Date.ToString("s");
        }

        /// <summary>
        /// Return the difference between 2 dates, in either minutes, seconds or years.
        /// </summary>
        /// <param name="firstDate">The first date.</param>
        /// <param name="secondDate">The second date.</param>
        /// <param name="diffType">format to return, can only be: s,m or y:  s = seconds, m = minutes, y = years.</param>
        /// <returns>A timespan as a integer</returns>
        public static int DateDiff(string firstDate, string secondDate, string diffType)
        {
            TimeSpan TS = DateTime.Parse(firstDate).Subtract(DateTime.Parse(secondDate));

            switch (diffType.ToLower())
            {
                case "m":
                    return Convert.ToInt32(TS.TotalMinutes);
                case "s":
                    return Convert.ToInt32(TS.TotalSeconds);
                case "y":
                    return Convert.ToInt32(TS.TotalDays / 365);
            }
            // return default
            return 0;
        }

        /// <summary>
        /// Formats a string to the specified formate.
        /// </summary>
        /// <param name="Date">The date.</param>
        /// <param name="Format">The format, compatible with regular .net date formats</param>
        /// <returns>A date in the new format as a string</returns>
        public static string FormatDateTime(string Date, string Format)
        {
            DateTime result;
            if (DateTime.TryParse(Date, out result))
                return result.ToString(Format);
            return string.Empty;
        }

        /// <summary>
        /// Converts a string to Long Date and returns it as a string
        /// </summary>
        /// <param name="Date">The date.</param>
        /// <param name="WithTime">if set to <c>true</c> the date will include time.</param>
        /// <param name="TimeSplitter">The splitter between date and time.</param>
        /// <returns>A Long Date as a string.</returns>
        public static string LongDate(string Date, bool WithTime, string TimeSplitter)
        {
            DateTime result;
            if (DateTime.TryParse(Date, out result))
            {
                if (WithTime)
                    return result.ToLongDateString() + TimeSplitter + result.ToLongTimeString();
                return result.ToLongDateString();
            }
            return string.Empty;
        }

        /// <summary>
        /// Checks whether the Culture with the specified name exixts in the standard .net cultureInfo.
        /// </summary>
        /// <param name="cultureName">Name of the culture.</param>
        /// <returns></returns>
        public static bool CultureExists(string cultureName)
        {
            CultureInfo[] ci = CultureInfo.GetCultures(CultureTypes.AllCultures);
            CultureInfo c = Array.Find(ci, delegate(CultureInfo culture) { return culture.Name == cultureName; });
            return c != null;
        }

        /// <summary>
        /// Converts a string to datetime in the longdate with day name format.
        /// </summary>
        /// <param name="Date">The date.</param>
        /// <param name="DaySplitter">String between day name and date</param>
        /// <param name="WithTime">if set to <c>true</c> the datetiem will include time.</param>
        /// <param name="TimeSplitter">String between date and time.</param>
        /// <param name="GlobalAlias">Culture name.</param>
        /// <returns>A datetime in the longdate formate with day name, as a string</returns>
        public static string LongDateWithDayName(string Date, string DaySplitter, bool WithTime, string TimeSplitter,
                                                 string GlobalAlias)
        {
            if (!CultureExists(GlobalAlias))
                return string.Empty;

            DateTime result;
            CultureInfo.GetCultureInfo(GlobalAlias);
            DateTimeFormatInfo dtInfo = CultureInfo.GetCultureInfo(GlobalAlias).DateTimeFormat;
            if (DateTime.TryParse(Date, dtInfo, DateTimeStyles.None, out result))
            {
                if (WithTime)
                    return
                        result.ToString(dtInfo.LongDatePattern) + TimeSplitter + result.ToString(dtInfo.LongTimePattern);
                return result.ToString(dtInfo.LongDatePattern);
            }
            return string.Empty;
        }

        /// <summary>
        /// Converts a string to a Long Date and returns it as a string
        /// </summary>
        /// <param name="Date">The date.</param>
        /// <returns>A Long Date as a string.</returns>
        public static string LongDate(string Date)
        {
            DateTime result;
            if (DateTime.TryParse(Date, out result))
                return result.ToLongDateString();
            return string.Empty;
        }

        /// <summary>
        /// Converts a string to a Short Date and returns it as a string
        /// </summary>
        /// <param name="Date">The date.</param>
        /// <returns>A Short Date as a string.</returns>
        public static string ShortDate(string Date)
        {
            DateTime result;
            if (DateTime.TryParse(Date, out result))
                return result.ToShortDateString();
            return string.Empty;
        }

        /// <summary>
        /// Converts a string to a Short Date, with a specific culture, and returns it as a string
        /// </summary>
        /// <param name="Date">The date.</param>
        /// <param name="GlobalAlias">Culture name</param>
        /// <returns>A short date with a specific culture, as a string</returns>
        public static string ShortDateWithGlobal(string Date, string GlobalAlias)
        {
            if (!CultureExists(GlobalAlias))
                return string.Empty;

            DateTime result;
            if (DateTime.TryParse(Date, out result))
            {
                DateTimeFormatInfo dtInfo = CultureInfo.GetCultureInfo(GlobalAlias).DateTimeFormat;
                return result.ToString(dtInfo.ShortDatePattern);
            }
            return string.Empty;
        }

        /// <summary>
        /// Converts a string to a Short Date with time, with a specific culture, and returns it as a string
        /// </summary>
        /// <param name="Date">The date.</param>
        /// <param name="GlobalAlias">Culture name</param>
        /// <returns>A short date withi time, with a specific culture, as a string</returns>
        public static string ShortDateWithTimeAndGlobal(string Date, string GlobalAlias)
        {
            if (!CultureExists(GlobalAlias))
                return string.Empty;

            DateTime result;
            if (DateTime.TryParse(Date, out result))
            {
                DateTimeFormatInfo dtInfo = CultureInfo.GetCultureInfo(GlobalAlias).DateTimeFormat;
                return result.ToString(dtInfo.ShortDatePattern) + " " +
                       result.ToString(dtInfo.ShortTimePattern);
            }
            return string.Empty;
        }

        /// <summary>
        /// Converts a datetime string to the ShortTime format.
        /// </summary>
        /// <param name="Date">The date.</param>
        /// <returns></returns>
        public static string ShortTime(string Date)
        {
            DateTime result;
            if (DateTime.TryParse(Date, out result))
                return result.ToShortTimeString();
            return string.Empty;
        }

        /// <summary>
        /// Converts a datetime string to the ShortDate format.
        /// </summary>
        /// <param name="Date">The date.</param>
        /// <param name="WithTime">if set to <c>true</c> the date will include time.</param>
        /// <param name="TimeSplitter">String dividing date and time</param>
        /// <returns></returns>
        public static string ShortDate(string Date, bool WithTime, string TimeSplitter)
        {
            DateTime result;
            if (DateTime.TryParse(Date, out result))
            {
                if (WithTime)
                    return result.ToShortDateString() + TimeSplitter + result.ToLongTimeString();
                return result.ToShortDateString();
            }
            return string.Empty;
        }

        /// <summary>
        /// Replaces text line breaks with html line breaks
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The text with text line breaks replaced with html linebreaks (<br/>)</returns>
        public static string ReplaceLineBreaks(string text)
        {
        	return GetUmbracoHelper().ReplaceLineBreaksForHtml(text);
        }

        /// <summary>
        /// Renders the content of a macro. Uses the normal template umbraco macro markup as input.
        /// This only works properly with xslt macros. 
        /// Python and .ascx based macros will not render properly, as viewstate is not included.
        /// </summary>
        /// <param name="Text">The macro markup to be rendered.</param>
        /// <param name="PageId">The page id.</param>
        /// <returns>The rendered macro as a string</returns>
        public static string RenderMacroContent(string Text, int PageId)
        {
            try
            {
                page p = new page(((IHasXmlNode)GetXmlNodeById(PageId.ToString()).Current).GetNode());
                template t = new template(p.Template);
                Control c = t.parseStringBuilder(new StringBuilder(Text), p);

                StringWriter sw = new StringWriter();
                HtmlTextWriter hw = new HtmlTextWriter(sw);
                c.RenderControl(hw);

                return sw.ToString();
            }
            catch (Exception ee)
            {
                return string.Format("<!-- Error generating macroContent: '{0}' -->", ee);
            }
        }

        /// <summary>
        /// Renders a template.
        /// </summary>
        /// <param name="PageId">The page id.</param>
        /// <param name="TemplateId">The template id.</param>
        /// <returns>The rendered template as a string</returns>
        public static string RenderTemplate(int PageId, int TemplateId)
        {
            if (UmbracoConfig.For.UmbracoSettings().Templates.UseAspNetMasterPages)
            {
                using (var sw = new StringWriter())
                {
                    try
                    {
                        var altTemplate = TemplateId == -1 ? null : (int?)TemplateId;
                        var templateRenderer = new TemplateRenderer(Umbraco.Web.UmbracoContext.Current, PageId, altTemplate);
                        templateRenderer.Render(sw);
                    }
                    catch (Exception ee)
                    {
                        sw.Write("<!-- Error rendering template with id {0}: '{1}' -->", PageId, ee);
                    }

                    return sw.ToString();
                }
            }
            else
            {

                var p = new page(((IHasXmlNode)GetXmlNodeById(PageId.ToString()).Current).GetNode());
                p.RenderPage(TemplateId);
                var c = p.PageContentControl;
                
				using (var sw = new StringWriter())
                using(var hw = new HtmlTextWriter(sw))
                {
					c.RenderControl(hw);
					return sw.ToString();    
                }
                
            }
        }

        /// <summary>
        /// Renders the default template for a specific page.
        /// </summary>
        /// <param name="PageId">The page id.</param>
        /// <returns>The rendered template as a string.</returns>
        public static string RenderTemplate(int PageId)
        {
	        return RenderTemplate(PageId, -1);
        }

        /// <summary>
        /// Registers the client script block.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="script">The script.</param>
        /// <param name="addScriptTags">if set to <c>true</c> [add script tags].</param>
        public static void RegisterClientScriptBlock(string key, string script, bool addScriptTags)
        {
            Page p = HttpContext.Current.CurrentHandler as Page;

            if (p != null)
                p.ClientScript.RegisterClientScriptBlock(p.GetType(), key, script, addScriptTags);
        }

        /// <summary>
        /// Registers the client script include.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="url">The URL.</param>
        public static void RegisterStyleSheetFile(string key, string url)
        {
            Page p = HttpContext.Current.CurrentHandler as Page;

            if (p != null)
            {
                System.Web.UI.HtmlControls.HtmlGenericControl include = new System.Web.UI.HtmlControls.HtmlGenericControl("link");
                include.ID = key;
                include.Attributes.Add("rel", "stylesheet");
                include.Attributes.Add("type", "text/css");
                include.Attributes.Add("href", url);

                if (p.Header != null)
                {
                    if (p.Header.FindControl(key) == null)
                    {
                        p.Header.Controls.Add(include);
                    }
                }
                else
                {
                    //This is a fallback in case there is no header
                    p.ClientScript.RegisterClientScriptBlock(p.GetType(), key, "<link rel='stylesheet' href='" + url + "' />");
                }
            }
        }

        /// <summary>
        /// Registers the client script include.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="url">The URL.</param>
        public static void RegisterJavaScriptFile(string key, string url)
        {
            Page p = HttpContext.Current.CurrentHandler as Page;

            if (p != null)
            {

                if (ClientDependency.Core.Controls.ClientDependencyLoader.Instance == null)
                {
                    System.Web.UI.HtmlControls.HtmlGenericControl include = new System.Web.UI.HtmlControls.HtmlGenericControl("script");
                    include.ID = key;
                    include.Attributes.Add("type", "text/javascript");
                    include.Attributes.Add("src", url);

                    if (p.Header != null)
                    {
                        if (p.Header.FindControl(key) == null)
                        {
                            p.Header.Controls.Add(include);
                        }
                    }
                    else
                    {
                        //This is a fallback in case there is no header
                        p.ClientScript.RegisterClientScriptInclude(p.GetType(), key, url);
                    }
                }
                else
                {
                    ClientDependency.Core.Controls.ClientDependencyLoader.Instance.RegisterDependency(url, ClientDependency.Core.ClientDependencyType.Javascript);
                }
            }
        }

        /// <summary>
        /// Adds a reference to the jQuery javascript file from the client/ui folder using "jQuery" as a key
        /// Recommended to use instead of RegisterJavaScriptFile for all nitros/packages that uses jQuery
        /// </summary>
        public static void AddJquery()
        {
            RegisterJavaScriptFile("jQuery", String.Format("{0}/ui/jquery.js", IOHelper.ResolveUrl(SystemDirectories.UmbracoClient)));
        }


        /// <summary>
        /// Strips all html from a string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>Returns the string without any html tags.</returns>
        public static string StripHtml(string text)
        {
            string pattern = @"<(.|\n)*?>";
            return Regex.Replace(text, pattern, string.Empty);
        }

        /// <summary>
        /// Truncates a string if it's too long
        /// </summary>
        /// <param name="Text">The text to eventually truncate</param>
        /// <param name="MaxLength">The maximum number of characters (length)</param>
        /// <param name="AddString">String to append if text is truncated (ie "...")</param>
        /// <returns>A truncated string if text if longer than MaxLength appended with the addString parameters. If text is shorter
        /// then MaxLength then the full - non-truncated - string is returned</returns>
        public static string TruncateString(string Text, int MaxLength, string AddString)
        {
            if (Text.Length > MaxLength)
                return Text.Substring(0, MaxLength - AddString.Length) + AddString;
            else
                return Text;
        }

        /// <summary>
        /// Split a string into xml elements
        /// </summary>
        /// <param name="StringToSplit">The full text to spil</param>
        /// <param name="Separator">The separator</param>
        /// <returns>An XPathNodeIterator containing the substrings in the format of <values><value></value></values></returns>
        public static XPathNodeIterator Split(string StringToSplit, string Separator)
        {
            string[] values = StringToSplit.Split(Convert.ToChar(Separator));
            XmlDocument xd = new XmlDocument();
            xd.LoadXml("<values/>");
            foreach (string id in values)
            {
                XmlNode node = xmlHelper.addTextNode(xd, "value", id);
                xd.DocumentElement.AppendChild(node);
            }
            XPathNavigator xp = xd.CreateNavigator();
            return xp.Select("/values");
        }

        /// <summary>
        /// Removes the starting and ending paragraph tags in a string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>Returns the string without starting and endning paragraph tags</returns>
        public static string RemoveFirstParagraphTag(string text)
        {
            if (String.IsNullOrEmpty(text))
                return "";

            if (text.Length > 5)
            {
                if (text.ToUpper().Substring(0, 3) == "<P>")
                    text = text.Substring(3, text.Length - 3);
                if (text.ToUpper().Substring(text.Length - 4, 4) == "</P>")
                    text = text.Substring(0, text.Length - 4);
            }
            return text;
        }

        /// <summary>
        /// Replaces a specified value with a new one.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns></returns>
        public static string Replace(string text, string oldValue, string newValue)
        {
            return text.Replace(oldValue, newValue);
        }

        /// <summary>
        /// Returns the Last index of the specified value
        /// </summary>
        /// <param name="Text">The text.</param>
        /// <param name="Value">The value.</param>
        /// <returns>Return the last index of a value as a integer </returns>
        public static int LastIndexOf(string Text, string Value)
        {
            return Text.LastIndexOf(Value);
        }

        /// <summary>
        /// Gets the prevalues from a umbraco DataType with the specified data type id.
        /// </summary>
        /// <param name="DataTypeId">The data type id.</param>
        /// <returns>Returns the prevalues as a XPathNodeIterator in the format:
        ///     <preValues>
        ///         <prevalue id="[id]">[value]</prevalue>
        ///     </preValues> 
        ///</returns>
        public static XPathNodeIterator GetPreValues(int DataTypeId)
        {
            XmlDocument xd = new XmlDocument();
            xd.LoadXml("<preValues/>");

            using (IRecordsReader dr = SqlHelper.ExecuteReader("Select id, [value] from cmsDataTypeprevalues where DataTypeNodeId = @dataTypeId order by sortorder",
                SqlHelper.CreateParameter("@dataTypeId", DataTypeId)))
            {
                while (dr.Read())
                {
                    XmlNode n = xmlHelper.addTextNode(xd, "preValue", dr.GetString("value"));
                    n.Attributes.Append(xmlHelper.addAttribute(xd, "id", dr.GetInt("id").ToString()));
                    xd.DocumentElement.AppendChild(n);
                }
            }
            XPathNavigator xp = xd.CreateNavigator();
            return xp.Select("/preValues");
        }

        /// <summary>
        /// Gets the umbraco data type prevalue with the specified Id as string.
        /// </summary>
        /// <param name="Id">The id.</param>
        /// <returns>Returns the prevalue as a string</returns>
        public static string GetPreValueAsString(int Id)
        {
            try
            {
                return SqlHelper.ExecuteScalar<string>("select [value] from cmsDataTypePreValues where id = @id",
                                                       SqlHelper.CreateParameter("@id", Id));
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the dictionary item with the specified key and it's child dictionary items.
        /// The language version is based on the culture of the current Url.
        /// </summary>
        /// <param name="Key">The key.</param>
        /// <returns>A XpathNodeIterator in the format:
        /// <DictionaryItems>
        ///     <DictionaryItem key="[dictionaryItemKey]">[dictionaryItemValue]</DictionaryItem>
        /// </DictionaryItems>
        /// </returns>
        public static XPathNodeIterator GetDictionaryItems(string Key)
        {
            XmlDocument xd = new XmlDocument();
            xd.LoadXml("<DictionaryItems/>");

            try
            {
                //int languageId = GetCurrentLanguageId();
                int languageId = Language.GetByCultureCode(System.Threading.Thread.CurrentThread.CurrentUICulture.Name).id;

                Dictionary.DictionaryItem di = new Dictionary.DictionaryItem(Key);

                foreach (Dictionary.DictionaryItem item in di.Children)
                {
                    XmlNode xe;
                    try
                    {
                        if (languageId != 0)
                            xe = xmlHelper.addTextNode(xd, "DictionaryItem", item.Value(languageId));
                        else
                            xe = xmlHelper.addTextNode(xd, "DictionaryItem", item.Value());
                    }
                    catch
                    {
                        xe = xmlHelper.addTextNode(xd, "DictionaryItem", string.Empty);
                    }
                    xe.Attributes.Append(xmlHelper.addAttribute(xd, "key", item.key));
                    xd.DocumentElement.AppendChild(xe);
                }
            }
            catch (Exception ee)
            {
                xd.DocumentElement.AppendChild(
                    xmlHelper.addTextNode(xd, "Error", ee.ToString()));
            }

            XPathNavigator xp = xd.CreateNavigator();
            return xp.Select("/");
        }

        /// <summary>
        /// Gets the dictionary item with the specified key.
        /// </summary>
        /// <param name="Key">The key.</param>
        /// <returns>A dictionary items value as a string.</returns>
        public static string GetDictionaryItem(string Key)
        {
	        return GetUmbracoHelper().GetDictionaryValue(Key);			
        }

        /// <summary>
        /// Gets the current page.
        /// </summary>
        /// <returns>An XpathNodeIterator containing the current page as Xml.</returns>
        public static XPathNodeIterator GetXmlNodeCurrent()
        {
            try
            {
                var nav = Umbraco.Web.UmbracoContext.Current.ContentCache.GetXPathNavigator();
                nav.MoveToId(HttpContext.Current.Items["pageID"].ToString());
                return nav.Select(".");
            }
            catch (Exception ex)
            {
                LogHelper.Error<library>("Could not retrieve current xml node", ex);
            }

            XmlDocument xd = new XmlDocument();
            xd.LoadXml("<error>No current node exists</error>");
            return xd.CreateNavigator().Select("/");
        }

        /// <summary>
        /// Gets the page with the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>Returns the node with the specified id as xml in the form of a XPathNodeIterator</returns>
        public static XPathNodeIterator GetXmlNodeById(string id)
        {
            // 4.7.1 UmbracoContext is null if we're running in publishing thread which we need to support
            XmlDocument xmlDoc = GetThreadsafeXmlDocument();

            if (xmlDoc.GetElementById(id) != null)
            {
                XPathNavigator xp = xmlDoc.CreateNavigator();
                xp.MoveToId(id);
                return xp.Select(".");
            }
            else
            {
                XmlDocument xd = new XmlDocument();
                xd.LoadXml(string.Format("<error>No published item exist with id {0}</error>", id));
                return xd.CreateNavigator().Select(".");
            }
        }

        //TODO: WTF, why is this here? This won't matter if there's an UmbracoContext or not, it will call the same underlying method!
        // only difference is that the UmbracoContext way will check if its in preview mode.
        private static XmlDocument GetThreadsafeXmlDocument()
        {
            return UmbracoContext.Current != null
                       ? UmbracoContext.Current.GetXml()
                       : content.Instance.XmlContent;
        }

        /// <summary>
        /// Queries the umbraco Xml cache with the specified Xpath query
        /// </summary>
        /// <param name="xpathQuery">The XPath query</param>
        /// <returns>Returns nodes matching the xpath query as a XpathNodeIterator</returns>
        public static XPathNodeIterator GetXmlNodeByXPath(string xpathQuery)
        {
            XPathNavigator xp = GetThreadsafeXmlDocument().CreateNavigator();

            return xp.Select(xpathQuery);
        }

        /// <summary>
        /// Gets the entire umbraco xml cache.
        /// </summary>
        /// <returns>Returns the entire umbraco Xml cache as a XPathNodeIterator</returns>
        public static XPathNodeIterator GetXmlAll()
        {
            XPathNavigator xp = GetThreadsafeXmlDocument().CreateNavigator();
            return xp.Select("/root");
        }

        /// <summary>
        /// Fetches a xml file from the specified path on the server.
        /// The path can be relative ("/path/to/file.xml") or absolute ("c:\folder\file.xml")
        /// </summary>
        /// <param name="Path">The path.</param>
        /// <param name="Relative">if set to <c>true</c> the path is relative.</param>
        /// <returns>The xml file as a XpathNodeIterator</returns>
        public static XPathNodeIterator GetXmlDocument(string Path, bool Relative)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                if (Relative)
                    xmlDoc.Load(IOHelper.MapPath(Path));
                else
                    xmlDoc.Load(Path);
            }
            catch (Exception err)
            {
                xmlDoc.LoadXml(string.Format("<error path=\"{0}\" relative=\"{1}\">{2}</error>",
                                             HttpContext.Current.Server.HtmlEncode(Path), Relative, err));
            }
            XPathNavigator xp = xmlDoc.CreateNavigator();
            return xp.Select("/");
        }

        /// <summary>
        /// Fetches a xml file from the specified url.
        /// the Url can be a local url or even from a remote server.
        /// </summary>
        /// <param name="Url">The URL.</param>
        /// <returns>The xml file as a XpathNodeIterator</returns>
        public static XPathNodeIterator GetXmlDocumentByUrl(string Url)
        {
            XmlDocument xmlDoc = new XmlDocument();
            WebRequest request = WebRequest.Create(Url);
            try
            {
                WebResponse response = request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                XmlTextReader reader = new XmlTextReader(responseStream);

                xmlDoc.Load(reader);

                response.Close();
                responseStream.Close();
            }
            catch (Exception err)
            {
                xmlDoc.LoadXml(string.Format("<error url=\"{0}\">{1}</error>",
                                             HttpContext.Current.Server.HtmlEncode(Url), err));
            }
            XPathNavigator xp = xmlDoc.CreateNavigator();
            return xp.Select("/");
        }

        /// <summary>
        /// Gets the XML document by URL Cached.
        /// </summary>
        /// <param name="Url">The URL.</param>
        /// <param name="CacheInSeconds">The cache in seconds (so 900 would be 15 minutes). This is independent of the global cache refreshing, as it doesn't gets flushed on publishing (like the macros do)</param>
        /// <returns></returns>
        public static XPathNodeIterator GetXmlDocumentByUrl(string Url, int CacheInSeconds)
        {

            object urlCache =
                            HttpContext.Current.Cache.Get("GetXmlDoc_" + Url);
            if (urlCache != null)
                return (XPathNodeIterator)urlCache;
            else
            {
                XPathNodeIterator result =
                    GetXmlDocumentByUrl(Url);

                HttpContext.Current.Cache.Insert("GetXmlDoc_" + Url,
                    result, null, DateTime.Now.Add(new TimeSpan(0, 0, CacheInSeconds)), TimeSpan.Zero, System.Web.Caching.CacheItemPriority.Low, null);
                return result;
            }

        }

        /// <summary>
        /// Returns the Xpath query for a node with the specified id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>The Xpath query for the node with the specified id as a string</returns>
        public static string QueryForNode(string id)
        {
            string XPathQuery = string.Empty;
            if (UmbracoContext.Current.GetXml().GetElementById(id) != null)
            {
                string[] path =
                    UmbracoContext.Current.GetXml().GetElementById(id).Attributes["path"].Value.Split((",").ToCharArray());
                for (int i = 1; i < path.Length; i++)
                {
                    if (i > 1)
                        XPathQuery += "/node [@id = " + path[i] + "]";
                    else
                        XPathQuery += " [@id = " + path[i] + "]";
                }
            }

            return XPathQuery;
        }

        /// <summary>
        /// Helper function to get a value from a comma separated string. Usefull to get
        /// a node identifier from a Page's path string
        /// </summary>
        /// <param name="path">The comma separated string</param>
        /// <param name="level">The index to be returned</param>
        /// <returns>A string with the value of the index</returns>
        public static string GetNodeFromLevel(string path, int level)
        {
            try
            {
                string[] newPath = path.Split(',');
                if (newPath.Length >= level)
                    return newPath[level].ToString();
                else
                    return string.Empty;
            }
            catch
            {
                return "<!-- error in GetNodeFromLevel -->";
            }
        }

        /// <summary>
        /// Sends an e-mail using the System.Net.Mail.MailMessage object
        /// </summary>
        /// <param name="FromMail">The sender of the e-mail</param>
        /// <param name="ToMail">The recipient of the e-mail</param>
        /// <param name="Subject">E-mail subject</param>
        /// <param name="Body">The complete content of the e-mail</param>
        /// <param name="IsHtml">Set to true when using Html formatted mails</param>
        public static void SendMail(string FromMail, string ToMail, string Subject, string Body, bool IsHtml)
        {
            try
            {
                // create the mail message 
                MailMessage mail = new MailMessage(FromMail.Trim(), ToMail.Trim());

                // populate the message
                mail.Subject = Subject;
                if (IsHtml)
                    mail.IsBodyHtml = true;
                else
                    mail.IsBodyHtml = false;

                mail.Body = Body;

                // send it
                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Send(mail);
            }
            catch (Exception ee)
            {
                LogHelper.Error<library>("umbraco.library.SendMail: Error sending mail.", ee);
            }
        }

        /// <summary> 
        /// These random methods are from Eli Robillards blog - kudos for the work :-)
        /// http://weblogs.asp.net/erobillard/archive/2004/05/06/127374.aspx
        /// 
        /// Get a Random object which is cached between requests. 
        /// The advantage over creating the object locally is that the .Next 
        /// call will always return a new value. If creating several times locally 
        /// with a generated seed (like millisecond ticks), the same number can be 
        /// returned. 
        /// </summary> 
        /// <returns>A Random object which is cached between calls.</returns> 
        public static Random GetRandom(int seed)
        {
            Random r = (Random)HttpContext.Current.Cache.Get("RandomNumber");
            if (r == null)
            {
                if (seed == 0)
                    r = new Random();
                else
                    r = new Random(seed);
                HttpContext.Current.Cache.Insert("RandomNumber", r);
            }
            return r;
        }

        /// <summary> 
        /// GetRandom with no parameters. 
        /// </summary> 
        /// <returns>A Random object which is cached between calls.</returns> 
        public static Random GetRandom()
        {
            return GetRandom(0);
        }

        /// <summary>
        /// Get any value from the current Request collection. Please note that there also specialized methods for
        /// Querystring, Form, Servervariables and Cookie collections
        /// </summary>
        /// <param name="key">Name of the Request element to be returned</param>
        /// <returns>A string with the value of the Requested element</returns>
        public static string Request(string key)
        {
            if (HttpContext.Current.Request[key] != null)
                return HttpContext.Current.Request[key];
            else
                return string.Empty;
        }

        /// <summary>
        /// Changes the mime type of the current page.
        /// </summary>
        /// <param name="MimeType">The mime type (like text/xml)</param>
        public static void ChangeContentType(string MimeType)
        {
            if (!String.IsNullOrEmpty(MimeType))
            {
                HttpContext.Current.Response.ContentType = MimeType;
            }
        }

        /// <summary>
        /// Get any value from the current Items collection.
        /// </summary>
        /// <param name="key">Name of the Items element to be returned</param>
        /// <returns>A string with the value of the Items element</returns>
        public static string ContextKey(string key)
        {
            if (HttpContext.Current.Items[key] != null)
                return HttpContext.Current.Items[key].ToString();
            else
                return string.Empty;
        }

        /// <summary>
        /// Get any value from the current Http Items collection
        /// </summary>
        /// <param name="key">Name of the Item element to be returned</param>
        /// <returns>A string with the value of the Requested element</returns>
        public static string GetHttpItem(string key)
        {
            if (HttpContext.Current.Items[key] != null)
                return HttpContext.Current.Items[key].ToString();
            else
                return string.Empty;
        }

        /// <summary>
        /// Get any value from the current Form collection
        /// </summary>
        /// <param name="key">Name of the Form element to be returned</param>
        /// <returns>A string with the value of the form element</returns>
        public static string RequestForm(string key)
        {
            if (HttpContext.Current.Request.Form[key] != null)
                return HttpContext.Current.Request.Form[key];
            else
                return string.Empty;
        }

        /// <summary>
        /// Get any value from the current Querystring collection
        /// </summary>
        /// <param name="key">Name of the querystring element to be returned</param>
        /// <returns>A string with the value of the querystring element</returns>
        public static string RequestQueryString(string key)
        {
            if (HttpContext.Current.Request.QueryString[key] != null)
                return HttpContext.Current.Request.QueryString[key];
            else
                return string.Empty;
        }

        /// <summary>
        /// Get any value from the users cookie collection
        /// </summary>
        /// <param name="key">Name of the cookie to return</param>
        /// <returns>A string with the value of the cookie</returns>
        public static string RequestCookies(string key)
        {
            // zb-00004 #29956 : refactor cookies handling
            var value = StateHelper.GetCookieValue(key);
            return value ?? "";
        }

        /// <summary>
        /// Get any element from the server variables collection
        /// </summary>
        /// <param name="key">The key for the element to be returned</param>
        /// <returns>A string with the value of the requested element</returns>
        public static string RequestServerVariables(string key)
        {
            if (HttpContext.Current.Request.ServerVariables[key] != null)
                return HttpContext.Current.Request.ServerVariables[key];
            else
                return string.Empty;
        }

        /// <summary>
        /// Get any element from current user session
        /// </summary>
        /// <param name="key">The key for the element to be returned</param>
        /// <returns>A string with the value of the requested element</returns>
        public static string Session(string key)
        {
            if (HttpContext.Current.Session != null && HttpContext.Current.Session[key] != null)
                return HttpContext.Current.Session[key].ToString();
            else
                return string.Empty;
        }

        /// <summary>
        /// Returns the current ASP.NET session identifier
        /// </summary>
        /// <returns>The current ASP.NET session identifier</returns>
        public static string SessionId()
        {
            if (HttpContext.Current.Session != null)
                return HttpContext.Current.Session.SessionID;
            else
                return string.Empty;
        }

        /// <summary>
        /// URL-encodes a string 
        /// </summary>
        /// <param name="Text">The string to be encoded</param>
        /// <returns>A URL-encoded string</returns>
        public static string UrlEncode(string Text)
        {
            return HttpUtility.UrlEncode(Text);
        }

        /// <summary>
        /// HTML-encodes a string 
        /// </summary>
        /// <param name="Text">The string to be encoded</param>
        /// <returns>A HTML-encoded string</returns>
        public static string HtmlEncode(string Text)
        {
            return HttpUtility.HtmlEncode(Text);
        }

        public static Relation[] GetRelatedNodes(int NodeId)
        {
            return new CMSNode(NodeId).Relations;
        }

        [Obsolete("Use DistributedCache.Instance.RemoveMediaCache instead")]
        public static void ClearLibraryCacheForMedia(int mediaId)
        {
            DistributedCache.Instance.RemoveMediaCache(mediaId);      
        }

        [Obsolete("Use DistributedCache.Instance.RemoveMediaCache instead")]
        public static void ClearLibraryCacheForMediaDo(int mediaId)
        {
            DistributedCache.Instance.RemoveMediaCache(mediaId);
        }

        [Obsolete("Use DistributedCache.Instance.RefreshMemberCache instead")]
        public static void ClearLibraryCacheForMember(int mediaId)
        {
            DistributedCache.Instance.RefreshMemberCache(mediaId);
        }

        [Obsolete("Use DistributedCache.Instance.RefreshMemberCache instead")]
        public static void ClearLibraryCacheForMemberDo(int memberId)
        {
            DistributedCache.Instance.RefreshMemberCache(memberId);
        }

        /// <summary>
        /// Gets the related nodes, of the node with the specified Id, as XML.
        /// </summary>
        /// <param name="NodeId">The node id.</param>
        /// <returns>The related nodes as a XpathNodeIterator in the format:
        ///     <code>
        ///         <relations>
        ///             <relation typeId="[typeId]" typeName="[typeName]" createDate="[createDate]" parentId="[parentId]" childId="[childId]"><node>[standard umbraco node Xml]</node></relation>
        ///         </relations>
        ///     </code>
        /// </returns>
        public static XPathNodeIterator GetRelatedNodesAsXml(int NodeId)
        {
            XmlDocument xd = new XmlDocument();
            xd.LoadXml("<relations/>");
            var rels = new CMSNode(NodeId).Relations;
            foreach (Relation r in rels)
            {
                XmlElement n = xd.CreateElement("relation");
                n.AppendChild(xmlHelper.addCDataNode(xd, "comment", r.Comment));
                n.Attributes.Append(xmlHelper.addAttribute(xd, "typeId", r.RelType.Id.ToString()));
                n.Attributes.Append(xmlHelper.addAttribute(xd, "typeName", r.RelType.Name));
                n.Attributes.Append(xmlHelper.addAttribute(xd, "createDate", r.CreateDate.ToString()));
                n.Attributes.Append(xmlHelper.addAttribute(xd, "parentId", r.Parent.Id.ToString()));
                n.Attributes.Append(xmlHelper.addAttribute(xd, "childId", r.Child.Id.ToString()));

                // Append the node that isn't the one we're getting the related nodes from
                if (NodeId == r.Child.Id)
                    n.AppendChild(r.Parent.ToXml(xd, false));
                else
                    n.AppendChild(r.Child.ToXml(xd, false));
                xd.DocumentElement.AppendChild(n);
            }
            XPathNavigator xp = xd.CreateNavigator();
            return xp.Select(".");
        }

        /// <summary>
        /// Returns the identifier of the current page
        /// </summary>
        /// <returns>The identifier of the current page</returns>
        public int PageId()
        {
            if (_page != null)
                return _page.PageID;
            else
                return -1;
        }

        /// <summary>
        /// Returns the title of the current page
        /// </summary>
        /// <returns>The title of the current page</returns>
        public string PageName()
        {
            if (_page != null)
                return _page.PageName;
            else
                return string.Empty;
        }

        /// <summary>
        /// Returns any element from the currentpage including generic properties
        /// </summary>
        /// <param name="key">The name of the page element to return</param>
        /// <returns>A string with the element value</returns>
        public string PageElement(string key)
        {
            if (_page != null)
            {
                if (_page.Elements[key] != null)
                    return _page.Elements[key].ToString();
                else
                    return string.Empty;
            }
            else
                return string.Empty;
        }



        /// <summary>
        /// Cleans the spified string with tidy
        /// </summary>
        /// <param name="StringToTidy">The string to tidy.</param>
        /// <param name="LiveEditing">if set to <c>true</c> [Live Editing].</param>
        /// <returns></returns>
        public static string Tidy(string StringToTidy, bool LiveEditing)
        {
            return cms.helpers.xhtml.TidyHtml(StringToTidy);
        }

        

        #endregion

        #region Template Control Mapping Functions

        /// <summary>
        /// Creates an Umbraco item for the specified field of the specified node.
        /// This brings the <c>umbraco:Item</c> element functionality to XSLT documents,
        /// which enables Live Editing of XSLT generated content.
        /// </summary>
        /// <param name="nodeId">The ID of the node to create.</param>
        /// <param name="fieldName">Name of the field to create.</param>
        /// <returns>An Umbraco item.</returns>
        public string Item(int nodeId, string fieldName)
        {
            return Item(nodeId, fieldName, null);
        }

        /// <summary>
        /// Creates an Umbraco item for the specified field of the specified node.
        /// This brings the <c>umbraco:Item</c> element functionality to XSLT documents,
        /// which enables Live Editing of XSLT generated content.
        /// </summary>
        /// <param name="nodeId">The ID of the node to create.</param>
        /// <param name="fieldName">Name of the field to create.</param>
        /// <param name="displayValue">
        ///     Value that is displayed to the user, which can be different from the field value.
        ///     Ignored if <c>null</c>.
        ///     Inside an XSLT document, an XPath expression might be useful to generate this value,
        ///     analogous to the functionality of the <c>Xslt</c> property of an <c>umbraco:Item</c> element.
        /// </param>
        /// <returns>An Umbraco item.</returns>
        public string Item(int nodeId, string fieldName, string displayValue)
        {
            // require a field name
            if (String.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException("fieldName");

            // encode the display value, if present, as an inline XSLT expression
            // escaping is disabled, since the user can choose to set
            // disable-output-escaping="yes" on the value-of element calling this function.
            string xslt = displayValue == null
                          ? String.Empty
                          : string.Format("xslt=\"'{0}'\" xsltdisableescaping=\"true\"",
                                          HttpUtility.HtmlEncode(displayValue).Replace("'", "&amp;apos;"));

            // return a placeholder, the actual item will be created later on
            // in the CreateControlsFromText method of macro
            return string.Format("[[[[umbraco:Item nodeId=\"{0}\" field=\"{1}\" {2}]]]]", nodeId, fieldName, xslt);
        }

        #endregion
    }
}
