<%@ Page MasterPageFile="../../masterpages/umbracoPage.Master" Language="C#" AutoEventWireup="true" CodeBehind="editWorkflow.aspx.cs" Inherits="Umbraco.Forms.UI.Pages.editWorkflow" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>

<asp:Content ID="Content" ContentPlaceHolderID="body" runat="server">

    <umb:UmbracoPanel ID="Panel1" runat="server" hasMenu="true" Text="Edit Workflow">
    
        <umb:Pane ID="Pane1" runat="server">
            <umb:PropertyPanel ID="ppName" runat="server" Text="Name">
                <asp:TextBox ID="txtName" runat="server" CssClass="guiInputText guiInputStandardSize"></asp:TextBox>
            </umb:PropertyPanel>
            <umb:PropertyPanel ID="ppType" runat="server" Text="Type">
                <asp:DropDownList ID="ddType" runat="server" AutoPostBack="true" CssClass="guiInputText guiInputStandardSize"></asp:DropDownList>
                
                
               
                
                
            </umb:PropertyPanel>
        </umb:Pane>
        
        <umb:Pane ID="paneSettings" runat="server" Visible="false">
            
        </umb:Pane>
    </umb:UmbracoPanel>
    
</asp:Content>