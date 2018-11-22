using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Text;
using System.Xml;
using System.IO;
using Umbraco.Core;
using Umbraco.Core.IO;

namespace umbraco.presentation.umbraco.developer.Xslt
{
    [WebformsPageTreeAuthorize(Constants.Trees.Xslt)]
    public partial class xsltVisualize : BasePages.UmbracoEnsuredPage
    {
        
		// zb-00004 #29956 : refactor cookies names & handling
		static global::umbraco.BusinessLogic.StateHelper.Cookies.Cookie cookie
			= new global::umbraco.BusinessLogic.StateHelper.Cookies.Cookie("UMB_XSLTVISPG", TimeSpan.FromMinutes(20)); // was "XSLTVisualizerPage"

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Check if cookie exists in the current request.
				// zb-00004 #29956 : refactor cookies names & handling
				if (cookie.HasValue)
                    contentPicker.Value = cookie.GetValue();
            }            

        }

        protected void visualizeDo_Click(object sender, EventArgs e)
        {

            // get xslt file
            string xslt = "";
            if (xsltSelection.Value.Contains("<xsl:stylesheet"))
            {
                xslt = xsltSelection.Value;
            }
            else
            {
                System.IO.StreamReader xsltFile =
                System.IO.File.OpenText(
                    IOHelper.MapPath(SystemDirectories.Umbraco + "/xslt/templates/clean.xslt")
                );

                xslt = xsltFile.ReadToEnd();
                xsltFile.Close();

                // parse xslt
                xslt = xslt.Replace("<!-- start writing XSLT -->", xsltSelection.Value);

                // prepare support for XSLT extensions
                xslt = macro.AddXsltExtensionsToHeader(xslt);

            }

            Dictionary<string, object> parameters = new Dictionary<string, object>(1);
            parameters.Add("currentPage", library.GetXmlNodeById(contentPicker.Value));


            // apply the XSLT transformation
            string xsltResult = "";
            XmlTextReader xslReader = null;
            try
            {
                xslReader = new XmlTextReader(new StringReader(xslt));
                System.Xml.Xsl.XslCompiledTransform xsl = macro.CreateXsltTransform(xslReader, false);
                xsltResult = macro.GetXsltTransformResult(new XmlDocument(), xsl, parameters);
            }
            catch (Exception ee)
            {
                xsltResult = string.Format(
                    "<div class=\"error\"><h3>Error parsing the XSLT:</h3><p>{0}</p></div>",
                    ee.ToString());
            }
            finally
            {
                xslReader.Close();
            }
            visualizeContainer.Visible = true;

            // update output
            visualizeArea.Text = !String.IsNullOrEmpty(xsltResult) ? "<div id=\"result\">" + xsltResult + "</Div>" : "<div class=\"notice\"><p>The XSLT didn't generate any output</p></div>";


            // add cookie with current page
			// zb-00004 #29956 : refactor cookies names & handling
			cookie.SetValue(contentPicker.Value);
        }

    }
}
