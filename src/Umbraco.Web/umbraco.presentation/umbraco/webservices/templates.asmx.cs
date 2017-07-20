using System;
using System.Web.Services;
using System.Web.Script.Services;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.WebServices;

namespace umbraco.webservices
{
    /// <summary>
    /// Summary description for templates.
    /// </summary>
    [WebService(Namespace="http://umbraco.org/webservices/")]
    [ScriptService]
    public class templates : UmbracoAuthorizedWebService
    {

        [WebMethod]
        [ScriptMethod]
        public string GetCodeSnippet(object templateId)
        {
            //NOTE: The legacy code threw an exception so will continue to do that.
            AuthorizeRequest(Constants.Applications.Settings.ToString(), true);

            var snippetPath = SystemDirectories.Umbraco + "/scripting/templates/cshtml/";
            var filePath = IOHelper.MapPath(snippetPath + templateId);

            //Directory check.. only allow files in script dir and below to be edited
            if (filePath.StartsWith(IOHelper.MapPath(snippetPath)))
            {
                var templateFile =
                    System.IO.File.OpenText(filePath);
                var content = templateFile.ReadToEnd();
                templateFile.Close();
                return content;
            }
            else
            {
                throw new ArgumentException("Couldn't open snippet - Illegal path");

            }
        }

    }
}
