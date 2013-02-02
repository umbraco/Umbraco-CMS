using System;
using System.IO;
using System.Xml;
using Umbraco.Core.IO;

namespace Umbraco.Web.UI.Install.Steps
{
    public partial class Renaming : StepUserControl
    {
        private readonly string _oldAccessFilePath = IOHelper.MapPath(SystemDirectories.Data + "/access.xml");
        private readonly string _newAccessFilePath = IOHelper.MapPath(SystemDirectories.Data + "/access.config");
        private bool _changesNeeded = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            // check xslt extensions
            identifyResult.Text += CheckExtensionPaths("xsltExtensions.config", "XSLT Extension");

            // check rest extensions
            identifyResult.Text += CheckExtensionPaths("restExtensions.config", "REST Extension");

            // check access.xml file
            identifyResult.Text += CheckAccessFile();

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

        private string CheckAccessFile()
        {
            if (!NewAccessFileExist() && OldAccessFileExist())
            {
                _changesNeeded = true;
                return "<li>Access.xml found. Needs to be renamed to access.config</li>";
            }
            return "<li>Public Access file is all good. No changes needed</li>";
        }

        private bool OldAccessFileExist()
        {
            return File.Exists(_oldAccessFilePath);
        }

        private bool NewAccessFileExist()
        {
            return File.Exists(_newAccessFilePath);
        }

        private string CheckExtensionPaths(string filename, string extensionName)
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

        private static void UpdateExtensionPaths(string filename)
        {
            filename = IOHelper.MapPath(SystemDirectories.Config + "/" + filename);
            var xsltExt = new XmlDocument();
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


        protected void UpdateChangesClick(object sender, EventArgs e)
        {
            bool succes = true;
            string progressText = "";

            // rename access file
            if (OldAccessFileExist())
            {
                try
                {
                    File.Move(_oldAccessFilePath, IOHelper.MapPath(SystemFiles.AccessXml));
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
                UpdateExtensionPaths("restExtensions.config");
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
                UpdateExtensionPaths("xsltExtensions.config");
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
            var xsltExt = new XmlDocument();
            xsltExt.Load(IOHelper.MapPath(SystemDirectories.Config + "/" + filename));

            return xsltExt.SelectNodes("//" + elementName);
        }
    }
}