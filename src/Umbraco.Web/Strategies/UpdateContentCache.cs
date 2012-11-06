using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Web.Publishing;
using Umbraco.Web.Services;
using umbraco.interfaces;
using umbraco.presentation.nodeFactory;
using Node = umbraco.NodeFactory.Node;

namespace Umbraco.Web.Strategies
{
    internal class UpdateContentCache : IApplicationStartupHandler
    {
        // Sync access to internal cache
        private static readonly object XmlContentInternalSyncLock = new object();
        private const string XmlContextContentItemKey = "UmbracoXmlContextContent";
        private readonly HttpContextBase _httpContext;
        private readonly ServiceContext _serviceContext;

        public UpdateContentCache()
        {
            _httpContext = new HttpContextWrapper(HttpContext.Current);
            _serviceContext = new ServiceContext(_httpContext);

            PublishingStrategy.Published += PublishingStrategy_Published;
        }

        public UpdateContentCache(HttpContextBase httpContext)
        {
            _httpContext = httpContext;
            _serviceContext = new ServiceContext(_httpContext);

            PublishingStrategy.Published += PublishingStrategy_Published;
        }

        void PublishingStrategy_Published(object sender, PublishingEventArgs e)
        {
            if((sender is IContent) == false) return;

            //var e = new DocumentCacheEventArgs();
            //FireBeforeUpdateDocumentCache(d, e);

            var content = sender as IContent;

            // lock the xml cache so no other thread can write to it at the same time
            // note that some threads could read from it while we hold the lock, though
            lock (XmlContentInternalSyncLock)
            {
                // modify a clone of the cache because even though we're into the write-lock
                // we may have threads reading at the same time. why is this an option?
                XmlDocument wip = UmbracoSettings.CloneXmlCacheOnPublish
                    ? CloneXmlDoc(XmlContent)
                    : XmlContent;

                ClearContextCache();

                XmlContent = UpdateXmlAndSitemap(content, wip, false);//Update sitemap is usually set to true
            }

            // clear cached field values
            if (_httpContext != null)
            {
                Cache httpCache = _httpContext.Cache;
                string cachedFieldKeyStart = String.Format("contentItem{0}_", content.Id);
                var foundKeys = new List<string>();
                foreach (DictionaryEntry cacheItem in httpCache)
                {
                    string key = cacheItem.Key.ToString();
                    if (key.StartsWith(cachedFieldKeyStart))
                        foundKeys.Add(key);
                }
                foreach (string foundKey in foundKeys)
                {
                    httpCache.Remove(foundKey);
                }
            }

            //Action.RunActionHandlers(d, ActionPublish.Instance);
            //FireAfterUpdateDocumentCache(d, e);
        }

        private XmlDocument UpdateXmlAndSitemap(IContent content, XmlDocument xmlContentCopy, bool updateSitemapProvider)
        {
            // check if document *is* published, it could be unpublished by an event
            if (content.Published)
            {
                int parentId = content.Level == 1 ? -1 : content.ParentId;
                var contentXmlNode = content.ToXml(false).GetXmlNode();
                var xmlNode = xmlContentCopy.ImportNode(contentXmlNode, true);
                xmlContentCopy = AppendContentXml(content.Id, content.Level, parentId, xmlNode, xmlContentCopy);

                // update sitemapprovider
                if (updateSitemapProvider && SiteMap.Provider is UmbracoSiteMapProvider)
                {
                    try
                    {
                        var prov = (UmbracoSiteMapProvider)SiteMap.Provider;
                        var n = new Node(content.Id, true);
                        if (!String.IsNullOrEmpty(n.Url) && n.Url != "/#")
                        {
                            prov.UpdateNode(n);
                        }
                        else
                        {
                            //Log.Add(LogTypes.Error, content.Id, "Can't update Sitemap Provider due to empty Url in node");
                        }
                    }
                    catch (Exception ee)
                    {
                        //Log.Add(LogTypes.Error, content.Id, string.Format("Error adding node to Sitemap Provider in PublishNodeDo(): {0}", ee));
                    }
                }
            }

            return xmlContentCopy;
        }

        private XmlDocument AppendContentXml(int id, int level, int parentId, XmlNode docNode, XmlDocument xmlContentCopy)
        {
            // Find the document in the xml cache
            XmlNode x = xmlContentCopy.GetElementById(id.ToString());

            // if the document is not there already then it's a new document
            // we must make sure that its document type exists in the schema
            var xmlContentCopy2 = xmlContentCopy;
            if (x == null && UmbracoSettings.UseLegacyXmlSchema == false)
            {
                //TODO Look into the validate schema method - seems a bit odd
                //Move to Contract ?
                xmlContentCopy = ValidateSchema(docNode.Name, xmlContentCopy);
                if (xmlContentCopy != xmlContentCopy2)
                    docNode = xmlContentCopy.ImportNode(docNode, true);
            }

            // Find the parent (used for sortering and maybe creation of new node)
            XmlNode parentNode;
            if (level == 1)
                parentNode = xmlContentCopy.DocumentElement;
            else
                parentNode = xmlContentCopy.GetElementById(parentId.ToString());

            if (parentNode != null)
            {
                if (x == null)
                {
                    x = docNode;
                    parentNode.AppendChild(x);
                }
                else
                {
                    //TODO
                    //TransferValuesFromDocumentXmlToPublishedXml(docNode, x); 
                }

                // TODO: Update with new schema!
                string xpath = UmbracoSettings.UseLegacyXmlSchema ? "./node" : "./* [@id]";
                XmlNodeList childNodes = parentNode.SelectNodes(xpath);

                // Maybe sort the nodes if the added node has a lower sortorder than the last
                if (childNodes.Count > 0)
                {
                    int siblingSortOrder =
                        int.Parse(childNodes[childNodes.Count - 1].Attributes.GetNamedItem("sortOrder").Value);
                    int currentSortOrder = int.Parse(x.Attributes.GetNamedItem("sortOrder").Value);
                    if (childNodes.Count > 1 && siblingSortOrder > currentSortOrder)
                    {
                        //SortNodes(ref parentNode);
                    }
                }
            }

            return xmlContentCopy;
        }

        private XmlDocument CloneXmlDoc(XmlDocument xmlDoc)
        {
            var xmlCopy = (XmlDocument)xmlDoc.CloneNode(true);
            return xmlCopy;
        }

        /// <summary>
        /// Clear HTTPContext cache if any
        /// </summary>
        private void ClearContextCache()
        {
            // If running in a context very important to reset context cache or else new nodes are missing
            if (_httpContext != null && _httpContext.Items.Contains(XmlContextContentItemKey))
                _httpContext.Items.Remove(XmlContextContentItemKey);
        }

        private XmlDocument ValidateSchema(string docTypeAlias, XmlDocument xmlDoc)
        {
            // check if doctype is defined in schema else add it
            // can't edit the doctype of an xml document, must create a new document

            var doctype = xmlDoc.DocumentType;
            var subset = doctype.InternalSubset;
            if (!subset.Contains(string.Format("<!ATTLIST {0} id ID #REQUIRED>", docTypeAlias)))
            {
                subset = string.Format("<!ELEMENT {0} ANY>\r\n<!ATTLIST {0} id ID #REQUIRED>\r\n{1}", docTypeAlias, subset);
                var xmlDoc2 = new XmlDocument();
                doctype = xmlDoc2.CreateDocumentType("root", null, null, subset);
                xmlDoc2.AppendChild(doctype);
                var root = xmlDoc2.ImportNode(xmlDoc.DocumentElement, true);
                xmlDoc2.AppendChild(root);

                // apply
                xmlDoc = xmlDoc2;
            }

            return xmlDoc;
        }

        private XmlDocument XmlContent
        {
            get
            {
                var content = _httpContext.Items[XmlContextContentItemKey] as XmlDocument;
                if (content == null)
                {
                    //content = global::umbraco.content.Instance.XmlContent;
                    var dtd = _serviceContext.ContentTypeService.GetDtd();

                    content = new XmlDocument();
                    content.LoadXml(
                        String.Format("<?xml version=\"1.0\" encoding=\"utf-8\" ?>{0}{1}{0}<root id=\"-1\"/>",
                                      Environment.NewLine,
                                      dtd));

                    _httpContext.Items[XmlContextContentItemKey] = content;
                }
                return content;
            }
            set { _httpContext.Items[XmlContextContentItemKey] = value; }
        }
    }
}