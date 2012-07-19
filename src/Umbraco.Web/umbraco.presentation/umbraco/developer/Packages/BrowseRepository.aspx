<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="../../masterpages/umbracoPage.Master" Title="Browse Repository" CodeBehind="BrowseRepository.aspx.cs" Inherits="umbraco.presentation.developer.packages.BrowseRepository" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ContentPlaceHolderID="body" runat="server">
<cc1:UmbracoPanel id="Panel1" Text="Browse package repository" runat="server" Width="612px" Height="600px" hasMenu="false">
    <cc1:Feedback ID="fb" runat="server" />
    <asp:Literal runat="server" ID="iframeGen" />
</cc1:UmbracoPanel>
</asp:Content>

<asp:Content ContentPlaceHolderID="footer" runat="server">
   <script type="text/javascript">
     jQuery(document).ready(function() {
       var frame = jQuery("#repoFrame");
       var win = jQuery(window);
       frame.height(win.height() - frame.offset().top - 40);
       frame.width(win.width() - 35);
     });
   </script>
</asp:Content>
