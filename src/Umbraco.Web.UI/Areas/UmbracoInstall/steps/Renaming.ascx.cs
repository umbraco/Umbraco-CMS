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

            string resultClass = succes ? "success" : "error";
            resultText.Text = String.Format("<div class=\"{0}\"><p>{1}</p></div>",
                resultClass,
                progressText);
            result.Visible = true;
            init.Visible = false;
        }

    }
}