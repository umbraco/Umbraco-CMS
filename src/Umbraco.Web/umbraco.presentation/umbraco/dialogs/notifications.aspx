<%@ Page Language="c#" MasterPageFile="../masterpages/umbracoDialog.Master" Codebehind="notifications.aspx.cs" AutoEventWireup="True"
  Inherits="umbraco.dialogs.notifications" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ContentPlaceHolderID="body" runat="server">

<div class="umb-dialog-body form-horizontal">
    
    <cc1:Pane ID="pane_form" runat="server">


    </cc1:Pane>
</div>
<div runat="server" ID="pl_buttons" class="umb-dialog-footer btn-toolbar umb-btn-toolbar">
        <a href="#" class="btn btn-link" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("general", "cancel")%></a>  
       <asp:Button ID="Button1" runat="server" CssClass="btn btn-primary" OnClick="Button1_Click"></asp:Button>
</div>
</asp:Content>