<%@ Page MasterPageFile="../../masterpages/umbracoPage.Master" Language="C#" AutoEventWireup="true" CodeBehind="editFormsSecurity.aspx.cs" Inherits="Umbraco.Forms.UI.Pages.editFormsSecurity" %>

<%@ Register Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>


<asp:Content ID="Content" ContentPlaceHolderID="body" runat="server">

    <umb:UmbracoPanel ID="Panel1" runat="server" hasMenu="true" Text="Forms security">
    
    <umb:Pane ID="paneMainSettings" runat="server">
        
        <umb:PropertyPanel ID="ppManageDataSources" runat="server" Text="Manage DataSources">
            <asp:CheckBox ID="cbManageDataSources" runat="server" />
        </umb:PropertyPanel>
        
        <umb:PropertyPanel ID="ppManagePreValueSources" runat="server" Text="Manage PreValueSources">
            <asp:CheckBox ID="cbManagePreValueSources" runat="server" />
        </umb:PropertyPanel>
        
        <umb:PropertyPanel ID="ppManageWorkflows" runat="server" Text="Manage Workflows">
            <asp:CheckBox ID="cbManageWorkflows" runat="server" />
        </umb:PropertyPanel>
        
        <umb:PropertyPanel ID="ppManageForms" runat="server" Text="Manage Forms">
            <asp:CheckBox ID="cbManageForms" runat="server" />
        </umb:PropertyPanel>
        
    </umb:Pane>
    
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <asp:PlaceHolder ID="phFormSettings" runat="server"></asp:PlaceHolder>
            </ContentTemplate>
        </asp:UpdatePanel>

    
    </umb:UmbracoPanel>
    
</asp:Content>