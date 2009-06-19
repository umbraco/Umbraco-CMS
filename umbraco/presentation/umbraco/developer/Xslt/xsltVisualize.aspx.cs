using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.presentation.umbraco.templateControls;
using System.Text;
using System.Xml;
using System.IO;

namespace umbraco.presentation.umbraco.developer.Xslt
{
    public partial class xsltVisualize : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            HttpCookie cookie = Request.Cookies.Get("XSLTVisualizerPage");

            // Check if cookie exists in the current request.
            if (cookie != null)
            {
                contentPicker.Text = cookie.Value;
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
                System.IO.File.OpenText(GlobalSettings.FullpathToRoot + GlobalSettings.Path.TrimStart('/') + System.IO.Path.DirectorySeparatorChar +
                    "xslt" + System.IO.Path.DirectorySeparatorChar + "templates" + System.IO.Path.DirectorySeparatorChar +
                    "Clean.xslt");
                xslt = xsltFile.ReadToEnd();
                xsltFile.Close();

                // parse xslt
                xslt = xslt.Replace("<!-- start writing XSLT -->", xsltSelection.Value);

                // prepare support for XSLT extensions
                xslt = macro.AddXsltExtensionsToHeader(xslt);

            }

            Dictionary<string, object> parameters = new Dictionary<string, object>(1);
            parameters.Add("currentPage", library.GetXmlNodeById(contentPicker.Text));


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
            HttpCookie cookie = Request.Cookies.Get("XSLTVisualizerPage");

            // Check if cookie exists in the current request.
            if (cookie == null)
            {
                cookie = new HttpCookie("XSLTVisualizerPage");
            }
            cookie.Value = contentPicker.Text;
            cookie.Expires = DateTime.Now.AddMinutes(20d);
            Response.Cookies.Add(cookie);

        }

    }
}
