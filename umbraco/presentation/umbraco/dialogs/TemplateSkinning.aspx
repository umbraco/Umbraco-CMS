<%@ Page Language="C#" MasterPageFile="../masterpages/umbracoDialog.Master" AutoEventWireup="true" CodeBehind="TemplateSkinning.aspx.cs" Inherits="umbraco.presentation.umbraco.dialogs.TemplateSkinning" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<asp:Content ID="Content1" ContentPlaceHolderID="body" runat="server">
    
    <cc1:Pane ID="p_apply" runat="server" Visible="false">
        
        <cc1:PropertyPanel ID="PropertyPanel1" runat="server" Text="Select a skin">
            <asp:DropDownList ID="dd_skins" runat="server" /> <asp:LinkButton OnClick="openRepo" runat="server">Download more skins</asp:LinkButton>
        </cc1:PropertyPanel>
        
        <cc1:PropertyPanel ID="PropertyPanel2"  runat="server" Text=" "> 
                
        <br />
        
        <asp:Button ID="Button1" runat="server"  OnClick="apply" /> 
        
        <asp:PlaceHolder ID="ph_rollback" runat="server" Visible="false">
         <em>or</em> <asp:LinkButton ID="lb_rollback" OnClick="rollback" runat="server">Rollback current skin</asp:LinkButton>
        </asp:PlaceHolder>
        
        </cc1:PropertyPanel>  
    </cc1:Pane>

    <cc1:Pane ID="p_download" runat="server" Visible="false">
        ...connect to repo and list all skins available, mark those already installed...
    </cc1:Pane>
    
</asp:Content>
