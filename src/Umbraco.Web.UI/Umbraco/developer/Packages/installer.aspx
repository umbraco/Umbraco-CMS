<%@ Page Language="c#" MasterPageFile="../../masterpages/umbracoPage.Master"
AutoEventWireup="True" Inherits="umbraco.presentation.developer.packages.Installer" Trace="false" ValidateRequest="false" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ContentPlaceHolderID="body" runat="server">
    <cc1:UmbracoPanel ID="Panel1" Text="Install package" runat="server" Width="496px" Height="584px">
        <cc1:Pane ID="pane_installing" runat="server" Visible="false" Text=""></cc1:Pane>
        <cc1:Pane ID="pane_optional" runat="server" Visible="false" />
    </cc1:UmbracoPanel>
</asp:Content>
