<%@ Page MasterPageFile="../../masterpages/umbracoPage.Master" Title="View Cache item" Language="c#" Codebehind="viewCacheItem.aspx.cs" AutoEventWireup="True"
  Inherits="umbraco.cms.presentation.developer.viewCacheItem" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ContentPlaceHolderID="body" runat="server">
    <cc1:UmbracoPanel ID="Panel1" runat="server" Width="612px" Height="375px" hasMenu="false">
      <div class="guiDialogNormal" style="margin: 10px">
        <b>Cache Alias:</b>
        <asp:Label ID="LabelCacheAlias" runat="server">Label</asp:Label>
        <br />
        <br />
        <b>Cache Value:</b>
        <asp:Label ID="LabelCacheValue" runat="server">Label</asp:Label>
      </div>
    </cc1:UmbracoPanel>
</asp:Content>