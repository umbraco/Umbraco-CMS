<%@ Page Language="c#" MasterPageFile="../masterpages/umbracoDialog.Master" Codebehind="cruds.aspx.cs" AutoEventWireup="True" Inherits="umbraco.dialogs.cruds" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
<style>
  .guiDialogTinyMark{font-size: 9px !Important;}
</style>
</asp:Content>
<asp:Content ContentPlaceHolderID="body" runat="server">

<div class="umb-dialog-body form-horizontal">
    <cc1:Pane ID="pane_form" runat="server">
        <cc1:Feedback ID="feedback1" runat="server" />

        <cc1:PropertyPanel runat="server">
            <asp:PlaceHolder ID="PlaceHolder1" runat="server"></asp:PlaceHolder>
        </cc1:PropertyPanel>
    </cc1:Pane>
</div>

<div runat="server" ID="panel_buttons" class="umb-dialog-footer btn-toolbar umb-btn-toolbar">
        <a href="#" class="btn btn-link" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("general", "cancel")%></a>  
       <asp:Button ID="Button1" runat="server" CssClass="btn btn-primary" OnClick="Button1_Click"></asp:Button>
</div>
 </asp:Content>