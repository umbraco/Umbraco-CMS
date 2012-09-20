using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace umbraco.presentation.umbraco
{
    public class Help
    {
        public string DefaultURL { get; set; }
        public List<HelpConfigPage> HelpConfigPages { get; set; }

        public Help(XmlNode helpConfigNode)
        {
            DefaultURL = GetXmlAttributeAsString(helpConfigNode.Attributes["defaultUrl"]);

            HelpConfigPages = new List<HelpConfigPage>();

            foreach (XmlNode linkNode in helpConfigNode.SelectNodes("link"))
            {

                HelpConfigPages.Add(new HelpConfigPage
                {
                    Application = GetXmlAttributeAsString(linkNode.Attributes["application"]),
                    ApplicationUrl = GetXmlAttributeAsString(linkNode.Attributes["applicationUrl"]),
                    Language = GetXmlAttributeAsString(linkNode.Attributes["language"]),
                    UserType = GetXmlAttributeAsString(linkNode.Attributes["userType"]),
                    HelpUrl = GetXmlAttributeAsString(linkNode.Attributes["helpUrl"])
                });
            }
        }

        public string ResolveHelpUrl(HelpPage requestedHelpPage)
        {
            HelpConfigPage bestMatchingConfigPage = null;

            int currentBestMatchCount = 0;

            foreach (HelpConfigPage helpConfigPage in HelpConfigPages)
            {
                int attributeMatchCount = 0;
                
                if ((helpConfigPage.Application != "" && String.Compare(helpConfigPage.Application, requestedHelpPage.Application, true) != 0) ||
                    (helpConfigPage.ApplicationUrl != "" && String.Compare(helpConfigPage.ApplicationUrl, requestedHelpPage.ApplicationUrl, true) != 0) ||
                    (helpConfigPage.Language != "" && String.Compare(helpConfigPage.Language, requestedHelpPage.Language, true) != 0) ||
                    (helpConfigPage.UserType != "" && String.Compare(helpConfigPage.UserType, requestedHelpPage.UserType, true) != 0))
                {
                    continue;
                }

                if (String.Compare(helpConfigPage.Application, requestedHelpPage.Application, true) == 0) attributeMatchCount++;
                if (String.Compare(helpConfigPage.ApplicationUrl, requestedHelpPage.ApplicationUrl, true) == 0) attributeMatchCount++;
                if (String.Compare(helpConfigPage.Language, requestedHelpPage.Language, true) == 0) attributeMatchCount++;
                if (String.Compare(helpConfigPage.UserType, requestedHelpPage.UserType, true) == 0) attributeMatchCount++;

                if (attributeMatchCount > currentBestMatchCount)
                {
                    currentBestMatchCount = attributeMatchCount;
                    bestMatchingConfigPage = helpConfigPage;
                }
            }
            return bestMatchingConfigPage == null ? GenerateDefaultUrl(requestedHelpPage) : GenerateConfiguredUrl(bestMatchingConfigPage);
        }

        public string GenerateConfiguredUrl(HelpConfigPage helpConfigPage)
        {
            return String.Format(helpConfigPage.HelpUrl,
                helpConfigPage.Application,
                helpConfigPage.ApplicationUrl,
                helpConfigPage.Language,
                helpConfigPage.UserType).ToLower();
        }

        public string GenerateDefaultUrl(HelpPage helpPage)
        {
            return string.Format(DefaultURL,
                helpPage.Application,
                helpPage.ApplicationUrl);
        }

        private string GetXmlAttributeAsString(XmlAttribute attribute) {

            if (attribute == null) return "";

            return attribute.Value.Trim();

        }
    }

    public class HelpPage
    {
        public string Application { get; set; }
        public string ApplicationUrl { get; set; }
        public string Language { get; set; }
        public string UserType { get; set; }
    }

    public class HelpConfigPage : HelpPage
    {
        public string HelpUrl { get; set; }
    }

    public partial class helpRedirect : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Help help = new Help(UmbracoSettings.HelpPages);

            HelpPage requestedHelpPage = new HelpPage {
                 Application = Request.QueryString["Application"],
                 ApplicationUrl = Request.QueryString["ApplicationUrl"], 
                 Language = Request.QueryString["Language"], 
                 UserType = Request.QueryString["UserType"]
            };

            Response.Redirect(help.ResolveHelpUrl(requestedHelpPage));
        }
    }
}