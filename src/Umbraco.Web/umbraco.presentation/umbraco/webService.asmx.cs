using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using System.Xml;
using UmbracoExamine;
using System.Collections.Generic;
using Examine;
using umbraco.presentation;
using System.Linq;

namespace umbraco
{

    //TODO: There's no app checking security in here which means that any authorized user can query for all content and all media 
    // with all information exposed even when they don't have access to content or media.

    /// <summary>
    /// Summary description for webService.
    /// </summary>
    /// 

    [WebService(Namespace = "http://umbraco.org/webservices/")]
    public class webService : System.Web.Services.WebService
    {
        public webService()
        {
            //CODEGEN: This call is required by the ASP.NET Web Services Designer
            InitializeComponent();
        }

        [WebMethod]
        public XmlNode GetNode(int NodeId, string ContextID)
        {
            XmlDocument xd = new XmlDocument();
            if (BasePages.BasePage.ValidateUserContextID(ContextID))
            {
                return new cms.businesslogic.CMSNode(NodeId).ToXml(xd, false);
            }
            else
                return null;
        }

        [WebMethod]
        public XmlNode GetNodeValidate(int NodeId, string Login, string Password)
        {
            XmlDocument xd = new XmlDocument();
            if (BusinessLogic.User.validateCredentials(Login, Password))
            {
                return new cms.businesslogic.CMSNode(NodeId).ToXml(xd, false);
            }
            else
                return null;
        }

        [WebMethod]
        public XmlNode GetDocument(int NodeId, string ContextID)
        {
            XmlDocument xd = new XmlDocument();
            if (BasePages.BasePage.ValidateUserContextID(ContextID))
            {
                return new cms.businesslogic.web.Document(NodeId).ToXml(xd, false);
            }
            else
                return null;
        }

        [WebMethod]
        public XmlNode GetMedia(int NodeId, string ContextID)
        {
            XmlDocument xd = new XmlDocument();
            if (BasePages.BasePage.ValidateUserContextID(ContextID))
            {
                return new cms.businesslogic.media.Media(NodeId).ToXml(xd, false);
            }
            else
                return null;
        }

        [WebMethod]
        public XmlNode GetMediaValidate(int NodeId, string Login, string Password)
        {
            XmlDocument xd = new XmlDocument();
            if (BusinessLogic.User.validateCredentials(Login, Password))
            {
                return new cms.businesslogic.media.Media(NodeId).ToXml(xd, false);
            }
            else
                return null;
        }


        [WebMethod]
        public XmlNode GetDocumentValidate(int NodeId, string Login, string Password)
        {
            XmlDocument xd = new XmlDocument();
            if (BusinessLogic.User.validateCredentials(Login, Password))
            {
                return new cms.businesslogic.web.Document(NodeId).ToXml(xd, false);
            }
            else
                return null;
        }

        [WebMethod]
        public XmlNode GetDocumentsBySearchValidate(string Query, int StartNodeId, string Login, string Password)
        {
            XmlDocument xd = new XmlDocument();
            if (BusinessLogic.User.validateCredentials(Login, Password))
            {
                return doQuery(Query, xd, StartNodeId);
            }
            else
            {
                XmlNode result = xd.CreateNode(XmlNodeType.Element, "error", "");
                result.AppendChild(xmlHelper.addTextNode(xd, "error", "Not a valid login"));
                return result;
            }
        }

        [WebMethod]
        public XmlNode GetDocumentsBySearch(string Query, int StartNodeId, string ContextID)
        {
            XmlDocument xd = new XmlDocument();
            if (BasePages.BasePage.ValidateUserContextID(ContextID))
            {
                return doQuery(Query.ToLower(), xd, StartNodeId);
            }
            else
            {
                XmlNode result = xd.CreateNode(XmlNodeType.Element, "error", "");
                result.AppendChild(xmlHelper.addTextNode(xd, "error", "Not a valid login"));
                return result;
            }
        }

        private XmlNode doQuery(string Query, XmlDocument xd, int StartNodeId)
        {
            XmlNode result = xd.CreateNode(XmlNodeType.Element, "documents", "");
            try
            {
                //if the query starts with "*" then query all fields
                var internalSearcher = UmbracoContext.Current.InternalSearchProvider;
                var criteria = internalSearcher.CreateSearchCriteria(IndexTypes.Content);
                IEnumerable<SearchResult> results;
                if (Query.StartsWith("*"))
                {
                    results = internalSearcher.Search("*", true);
                }
                else
                {
                    var operation = criteria.NodeName(Query.ToLower());
                    if (StartNodeId > 0)
                    {
                        operation.Or().Id(StartNodeId);
                    }

                    results = internalSearcher.Search(operation.Compile()).Take(20);
                }
                
                //var criteria = new SearchCriteria(Query
                //    , Query.StartsWith("*") ? new string[] { } : new string[] { "nodeName", "id" }
                //    , new string[] { }
                //    , false
                //    , StartNodeId > 0 ? (int?)StartNodeId : null
                //    , 20);

                foreach (var r in results)
                {
                    XmlElement x = xd.CreateElement("document");
                    x.SetAttribute("id", r.Id.ToString());
                    x.SetAttribute("nodeName", r.Fields["nodeName"]);
                    result.AppendChild(x);
                }
            }
            catch (Exception ee)
            {
                XmlElement x = xd.CreateElement("document");
                x.SetAttribute("id", "0");
                x.SetAttribute("nodeName", "Error in search: " + ee.ToString());
                result.AppendChild(x);
            }
            return result;
        }

        #region Component Designer generated code

        //Required by the Web Services Designer 
        private IContainer components = null;

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

    }
}
