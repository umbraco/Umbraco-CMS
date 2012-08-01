<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="../../masterpages/umbracoPage.Master"Inherits="umbraco.presentation.developer.packages.directoryBrowser" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Import Namespace="System.IO" %>


<asp:Content ContentPlaceHolderID="head" runat="server">
<script language="C#" runat="server">
    //Import Namespace="System.Math"
    string lsTitle;
    string lsLink;
    string lsScriptName;
    string lsWebPath;
    string target = "";

    public void Page_Load()
    {
        Response.Cache.SetExpires(DateTime.Now.AddSeconds(5));
        Response.Cache.SetCacheability(HttpCacheability.Public);
        lsTitle = Request.QueryString.Get("title");
        target = Request.QueryString.Get("target");
        if (lsTitle == null || lsTitle == "") { lsTitle = "Web Browse"; }
    }

    private void RptErr(string psMessage)
    {
        Response.Write("<DIV align=\"left\" width=\"100%\"><B>Script Reported Error: </B>&nbsp;" + psMessage + "</DIV><BR>");
    }

    private string GetNavLink(string psHref, string psText)
    {
        return ("/<a class=\"tdheadA\" href=\"" + lsScriptName + "?path=" + psHref + "&title=" + lsTitle + "&link=" + lsLink + "\">" + psText + "</a>");
    }
</script>

    <style type="text/css">

a{color:#3C6B96;}

.tdDir a{padding: 3px; padding-left: 25px; background: url(../../images/foldericon.png) no-repeat 2px 2px;}
.tdFile a{padding: 3px; padding-left: 25px; background: url(../../images/file.png) no-repeat 2px 2px;}
small a{color: #999; padding-left: 3px !Important; background-image: none !Important; text-decoration: none;}
</style>

<script type="text/javascript">
  function postPath(path) {
    top.right.document.getElementById('<%=target%>').value = path;
    UmbClientMgr.closeModalWindow();
  }
</script>

</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">
<cc1:Pane runat="server" Width="100px" ID="pane">
    <%
        try
        {

            //Variables used in script
            string sSubDir; int i; int j;
            string sPrevLink = "";
            string sebChar = umbraco.IO.IOHelper.DirSepChar.ToString();
            decimal iLen; string sLen;

            //Write header, get link param
            lsLink = Request.QueryString.Get("link");
            if (lsLink != null && lsLink != "") { Response.Write("<A href=\"" + lsLink + "\">[&nbsp;Return&nbsp;]</A><BR>"); }

            //Work on path and ensure no back tracking
            sSubDir = Request.QueryString.Get("path");
            if (sSubDir == null || sSubDir == "") { sSubDir = "/"; }

            sSubDir = sSubDir.Replace(umbraco.IO.IOHelper.DirSepChar.ToString(), ""); 
            sSubDir = sSubDir.Replace("//", "/");
            sSubDir = sSubDir.Replace("..", "./"); 
            sSubDir = sSubDir.Replace('/', umbraco.IO.IOHelper.DirSepChar);

            //Clean path for processing and collect path varitations
            if (sSubDir.Substring(0, 1) != sebChar) { sSubDir = sebChar + sSubDir; }
            if (sSubDir.Substring(sSubDir.Length - 1, 1) != "\\") { sSubDir = sSubDir + sebChar; }

            //Get name of the browser script file
            lsScriptName = Request.ServerVariables.Get("SCRIPT_NAME");
            j = lsScriptName.LastIndexOf("/");
            if (j > 0) { lsScriptName = lsScriptName.Substring(j + 1, lsScriptName.Length - (j + 1)).ToLower(); }

            //Create navigation string and other path strings
            sPrevLink += GetNavLink("", "root");
            if (sSubDir != sebChar)
            {
                j = 0; i = 0;
                do
                {
                    i = sSubDir.IndexOf(sebChar, j + 1);
                    lsWebPath += sSubDir.Substring(j + 1, i - (j + 1)) + "/";
                    sPrevLink += GetNavLink(lsWebPath, sSubDir.Substring(j + 1, i - (j + 1)));
                    j = i;
                } while (i != sSubDir.Length - 1);
            }

            //Output header
            Response.Write("<table cellpadding=3 cellspacing=1><tbody>");

            //Output directorys
            DirectoryInfo oDirInfo = new DirectoryInfo(umbraco.IO.IOHelper.MapPath("~/" + sSubDir));
            DirectoryInfo[] oDirs = oDirInfo.GetDirectories();
            foreach (DirectoryInfo oDir in oDirs)
            {
                try
                {
                    Response.Write("<tr><td class=\"tdDir\"><a href=\"" + lsScriptName + "?path=" + lsWebPath + oDir.Name + "&title=" + lsTitle + "&link=" + lsLink + "&target=" + target + "\">" + oDir.Name + "</a>  <small><a href=\"javascript:postPath('/" + lsWebPath + oDir.Name + "')\"> (Include entire folder)</small></td></tr>");
                }
                catch (Exception ex)
                {
                    Response.Write("<tr><td class=\"tdDir\">" + oDir.Name + " (Access Denied)</td></tr>");
                }
            }

            //Ouput files
            FileInfo[] oFiles = oDirInfo.GetFiles();
            foreach (FileInfo oFile in oFiles)
            {
                if (oFile.Name.ToLower() != lsScriptName)
                {
                    iLen = oFile.Length;
                    if (iLen >= 1048960) { iLen = iLen / 1048960; sLen = "mb"; } else { iLen = iLen / 1024; sLen = "kb"; }
                    sLen = Decimal.Round(iLen, 2).ToString() + sLen;
                    Response.Write("<tr><td class=\"tdFile\"><a href=\"javascript:postPath('/" + lsWebPath + oFile.Name + "')\">" + oFile.Name + "</a></td></tr>");
                }
            }

            //Output footer
            Response.Write("</tbody></table></center>");

        }
        catch (Exception ex)
        {
            RptErr(ex.Message);
        }
    %>
    </cc1:Pane>
</asp:Content>