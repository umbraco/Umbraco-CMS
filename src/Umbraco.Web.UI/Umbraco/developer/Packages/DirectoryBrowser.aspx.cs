using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.UI.Pages;

namespace Umbraco.Web.UI.Umbraco.Developer.Packages
{
    public partial class DirectoryBrowser : UmbracoEnsuredPage
    {
        public DirectoryBrowser()
        {
            CurrentApp = Constants.Applications.Packages;
        }

        string _lsScriptName;
        string _lsWebPath;
        protected string Target = "";
        private readonly Regex _xssElementIdClean = new Regex(@"^([a-zA-Z0-9-_:\.]+)");

        private readonly StringBuilder _sb = new StringBuilder();

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Response.Cache.SetExpires(DateTime.Now.AddSeconds(5));
            Response.Cache.SetCacheability(HttpCacheability.Public);

            //we need to clean this string:
            //http://issues.umbraco.org/issue/U4-2027
            var target = Request.QueryString.Get("target");
            if (target.IsNullOrWhiteSpace())
                throw new InvalidOperationException("The target query string must be set to a valid html element id");
            var matched = _xssElementIdClean.Matches(target);
            if (matched.Count == 0)
                throw new InvalidOperationException("The target query string must be set to a valid html element id");

            Target = matched[0].Value;

            try
            {

                //Variables used in script
                var sebChar = IOHelper.DirSepChar.ToString();

                //Work on path and ensure no back tracking
                string sSubDir = Request.QueryString.Get("path");
                if (string.IsNullOrEmpty(sSubDir)) { sSubDir = "/"; }

                sSubDir = sSubDir.Replace(IOHelper.DirSepChar.ToString(), "");
                sSubDir = sSubDir.Replace("//", "/");
                sSubDir = sSubDir.Replace("..", "./");
                sSubDir = sSubDir.Replace('/', IOHelper.DirSepChar);

                //Clean path for processing and collect path varitations
                if (sSubDir.Substring(0, 1) != sebChar) { sSubDir = sebChar + sSubDir; }
                if (sSubDir.Substring(sSubDir.Length - 1, 1) != "\\") { sSubDir = sSubDir + sebChar; }

                //Get name of the browser script file
                _lsScriptName = Request.ServerVariables.Get("SCRIPT_NAME");
                var j = _lsScriptName.LastIndexOf("/");
                if (j > 0) { _lsScriptName = _lsScriptName.Substring(j + 1, _lsScriptName.Length - (j + 1)).ToLower(); }

                //Create navigation string and other path strings
                GetNavLink("", "root");
                if (sSubDir != sebChar)
                {
                    j = 0; int i = 0;
                    do
                    {
                        i = sSubDir.IndexOf(sebChar, j + 1);
                        _lsWebPath += sSubDir.Substring(j + 1, i - (j + 1)) + "/";
                        GetNavLink(_lsWebPath, sSubDir.Substring(j + 1, i - (j + 1)));
                        j = i;
                    } while (i != sSubDir.Length - 1);
                }

                //Output header
                _sb.Append("<table cellpadding=3 cellspacing=1><tbody>");

                //Output directorys
                var oDirInfo = new DirectoryInfo(IOHelper.MapPath("~/" + sSubDir));
                var oDirs = oDirInfo.GetDirectories();
                foreach (var oDir in oDirs)
                {
                    try
                    {
                        _sb.Append("<tr><td class=\"tdDir\"><a href=\"" + _lsScriptName + "?path=" + _lsWebPath + oDir.Name + "&target=" + Target + "\">" + oDir.Name + "</a>  <small><a href=\"javascript:postPath('/" + _lsWebPath + oDir.Name + "')\"> (Include entire folder)</small></td></tr>");
                    }
                    catch (Exception)
                    {
                        _sb.Append("<tr><td class=\"tdDir\">" + oDir.Name + " (Access Denied)</td></tr>");
                    }
                }

                //Ouput files
                var oFiles = oDirInfo.GetFiles();
                foreach (var oFile in oFiles.Where(oFile => oFile.Name.ToLower() != _lsScriptName))
                {
                    decimal iLen = oFile.Length;
                    string sLen;
                    if (iLen >= 1048960) { iLen = iLen / 1048960; sLen = "mb"; } else { iLen = iLen / 1024; sLen = "kb"; }
                    sLen = Decimal.Round(iLen, 2).ToString() + sLen;
                    _sb.Append("<tr><td class=\"tdFile\"><a href=\"javascript:postPath('/" + _lsWebPath + oFile.Name + "')\">" + oFile.Name + "</a></td></tr>");
                }

                //Output footer
                _sb.Append("</tbody></table></center>");

            }
            catch (Exception ex)
            {
                RptErr(ex.Message);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            Output.Controls.Add(new LiteralControl(_sb.ToString()));
        }

        private void RptErr(string psMessage)
        {
            _sb.Append("<DIV align=\"left\" width=\"100%\"><B>Script Reported Error: </B>&nbsp;" + psMessage + "</DIV><BR>");
        }

        private string GetNavLink(string psHref, string psText)
        {
            return ("/<a class=\"tdheadA\" href=\"" + _lsScriptName + "?path=" + psHref + "\">" + psText + "</a>");
        }

    }
}
