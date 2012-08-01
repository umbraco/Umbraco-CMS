using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Xml;
using umbraco.IO;

namespace umbraco.presentation.install.steps
{
    public partial class renaming : System.Web.UI.UserControl
    {
        private string _oldAccessFilePath = IO.IOHelper.MapPath(IO.SystemDirectories.Data + "/access.xml");
        private string _newAccessFilePath = IO.IOHelper.MapPath(IO.SystemDirectories.Data + "/access.config");
        private bool _changesNeeded = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            // check xslt extensions
            identifyResult.Text += checkExtensionPaths("xsltExtensions.config", "XSLT Extension");

            // check rest extensions
            identifyResult.Text += checkExtensionPaths("restExtensions.config", "REST Extension");

            // check access.xml file
            identifyResult.Text += checkAccessFile();

            if (_changesNeeded)
            {
                changesNeeded.Visible = true;
            }
            else
            {
                noChangedNeeded.Visible = true;
                changesNeeded.Visible = false;
            }
        }

        private string checkAccessFile()
        {
            if (!newAccessFileExist() && oldAccessFileExist())
            {
                _changesNeeded = true;
                return "<li>Access.xml found. Needs to be renamed to access.config</li>";
            }
            else
            {
                return "<li>Public Access file is all good. No changes needed</li>";
            }
        }

        private bool oldAccessFileExist()
        {
            return File.Exists(_oldAccessFilePath);
        }

        private bool newAccessFileExist()
        {
            return File.Exists(_newAccessFilePath);
        }

        private string checkExtensionPaths(string filename, string extensionName)
        {
            string tempResult = "";
            foreach (XmlNode ext in GetExtensions(filename, "ext"))
            {
                if (ext.Attributes.GetNamedItem("assembly") != null &&
                    ext.Attributes.GetNamedItem("assembly").Value.StartsWith("/bin/"))
                {
                    tempResult += String.Format("<li>{0} with Alias '{1}' has assembly reference that contains /bin/. That part needs to be removed</li>",
                        extensionName,
                        ext.Attributes.GetNamedItem("alias").Value);
                }
            }

            if (String.IsNullOrEmpty(tempResult))
            {
                tempResult = String.Format("<li>{0}s are all good. No changes needed</li>", extensionName);
            }
            else
            {
                _changesNeeded = true;
            }

            return tempResult;
        }

        private void updateExtensionPaths(string filename)
        {
            filename = IOHelper.MapPath(SystemDirectories.Config + "/" + filename);
            XmlDocument xsltExt = new XmlDocument();
            xsltExt.Load(filename);

            foreach (XmlNode ext in xsltExt.SelectNodes("//ext"))
            {
                if (ext.Attributes.GetNamedItem("assembly") != null &&
                    ext.Attributes.GetNamedItem("assembly").Value.StartsWith("/bin/"))
                {
                    ext.Attributes.GetNamedItem("assembly").Value =
                        ext.Attributes.GetNamedItem("assembly").Value.Substring(5);
                }
            }

            xsltExt.Save(filename);

        }


        protected void updateChanges_Click(object sender, EventArgs e)
        {
            bool succes = true;
            string progressText = "";

            // rename access file
            if (oldAccessFileExist())
            {
                try
                {
                    File.Move(_oldAccessFilePath, IO.IOHelper.MapPath(IO.SystemFiles.AccessXml));
                    progressText += String.Format("<li>Public Access file renamed</li>");
                }
                catch (Exception ee)
                {
                    progressText += String.Format("<li>Error renaming access file: {0}</li>", ee.ToString());
                    succes = false;
                }
            }

            // update rest exts
            try
            {
                updateExtensionPaths("restExtensions.config");
                progressText += "<li>restExtensions.config ensured.</li>";
            }
            catch (Exception ee)
            {
                progressText += String.Format("<li>Error updating restExtensions.config: {0}</li>", ee.ToString());
                succes = false;
            }

            // update xslt exts
            try
            {
                updateExtensionPaths("xsltExtensions.config");
                progressText += "<li>xsltExtensions.config ensured.</li>";
            }
            catch (Exception ee)
            {
                progressText += String.Format("<li>Error updating xsltExtensions.config: {0}</li>", ee.ToString());
                succes = false;
            }

            string resultClass = succes ? "success" : "error";
            resultText.Text = String.Format("<div class=\"{0}\"><p>{1}</p></div>",
                resultClass,
                progressText);
            result.Visible = true;
            init.Visible = false;
        }

        private XmlNodeList GetExtensions(string filename, string elementName)
        {

            // Load the XSLT extensions configuration
            XmlDocument xsltExt = new XmlDocument();
            xsltExt.Load(IOHelper.MapPath(SystemDirectories.Config + "/" + filename));

            return xsltExt.SelectNodes("//" + elementName);
        }
    }
}