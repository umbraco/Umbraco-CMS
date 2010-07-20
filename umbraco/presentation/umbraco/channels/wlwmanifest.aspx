<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="wlwmanifest.aspx.cs" Inherits="umbraco.presentation.channels.wlwmanifest" %><?xml version="1.0" encoding="utf-8" ?><manifest xmlns="http://schemas.microsoft.com/wlw/manifest/weblog">
 <weblog>
 <imageUrl>http://umbraco.org/images/liveWriterIcon.png</imageUrl>
 <watermarkImageUrl>http://umbraco.org/images/liveWriterWatermark.png</watermarkImageUrl>
 <homepageLinkText>View your site/weblog</homepageLinkText>
 <adminLinkText>Edit your site/weblog</adminLinkText>
 <adminUrl>{blog-homepage-url}<%= umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) %>/</adminUrl>
 <postEditingUrl>{blog-homepage-url}<%= umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco)%>/actions/editContent.aspx?id={post-id}</postEditingUrl>

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
 </options></manifest>