<?xml version="1.0" encoding="UTF-8" ?>
<%@ Page Language="C#" AutoEventWireup="true" Inherits="System.Web.UI.Page" %>
<%@ Import Namespace="Umbraco.Core.IO" %>
<%@ Import Namespace="umbraco" %>
<script runat="server">
protected override void OnInit(EventArgs e)
{
    Response.ContentType = "text/xml";
}
protected override void OnLoad(EventArgs e)
{
    var useXhtml = false;
    if (bool.TryParse(GlobalSettings.EditXhtmlMode, out useXhtml) && !useXhtml)
    {
        xhtml.Text = "no";
    }
    else
    {
        xhtml.Text = "yes";
    }
}
</script>
<manifest xmlns="http://schemas.microsoft.com/wlw/manifest/weblog">
 <weblog>
 <imageUrl>http://umbraco.org/images/liveWriterIcon.png</imageUrl>
 <watermarkImageUrl>http://umbraco.org/images/liveWriterWatermark.png</watermarkImageUrl>
 <homepageLinkText>View your site/weblog</homepageLinkText>
 <adminLinkText>Edit your site/weblog</adminLinkText>
 <adminUrl>{blog-homepage-url}<%= IOHelper.ResolveUrl(SystemDirectories.Umbraco) %>/</adminUrl>
 <postEditingUrl>{blog-homepage-url}<%= IOHelper.ResolveUrl(SystemDirectories.Umbraco)%>/actions/editContent.aspx?id={post-id}</postEditingUrl>

 </weblog>
 <views>
 <default>WebLayout</default>
 </views>
 <options>
 <supportsScripts>Yes</supportsScripts>
 <supportsEmbeds>Yes</supportsEmbeds>
 <supportsHtmlTitles>Yes</supportsHtmlTitles>
 <supportsEmptyTitles>No</supportsEmptyTitles>
 <maxRecentPosts>100</maxRecentPosts>
 <supportsNewCategories>Yes</supportsNewCategories>
 <supportsExcerpt>Yes</supportsExcerpt>
 <supportsPages>No</supportsPages>
 <supportsPageParent>No</supportsPageParent>
 <supportsPageOrder>No</supportsPageOrder>
 <supportsAutoUpdate>Yes</supportsAutoUpdate>
 <supportsMultipleCategories>Yes</supportsMultipleCategories>
 <requiresXHTML><asp:Literal runat="server" id="xhtml"></asp:Literal></requiresXHTML>
 </options>
</manifest>
