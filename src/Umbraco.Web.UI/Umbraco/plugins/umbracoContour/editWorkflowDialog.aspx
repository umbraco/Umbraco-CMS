<%@ Page Language="C#"  MasterPageFile="../../masterpages/umbracoPage.Master" AutoEventWireup="true" CodeBehind="editWorkflowDialog.aspx.cs" Inherits="Umbraco.Forms.UI.Dialogs.editWorkflowDialog" %>
<%@ Register Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
<link rel="stylesheet" href="css/dialogs.css" type="text/css" media="screen" />
<script type="text/javascript" src="/umbraco_client/ui/jquery.js" /></script>

<style>

    #dialogcontainer
    {
        width: 570px;
    	height:380px;
    	overflow:auto;
    }
</style>
</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">
 <div id="dialogcontainer">
    
    
    <div>
    
    <asp:ValidationSummary ID="valSum" runat="server" CssClass="error" style="margin-top: 10px;" DisplayMode="BulletList" />
        <asp:PlaceHolder ID="phVals" runat="server"></asp:PlaceHolder>
    <umb:Pane ID="paneMainSettings" runat="server">
    
    <umb:PropertyPanel ID="PropertyPanel1" Text="Name" runat="server">
        <asp:TextBox ID="txtName" runat="server" CssClass="propertyFormInput"></asp:TextBox>
    </umb:PropertyPanel>
    
    <umb:PropertyPanel ID="PropertyPanel2" Text="Active" runat="server">
        <asp:CheckBox ID="cbActive" Checked="true" runat="server" />
    </umb:PropertyPanel>
    
    <umb:PropertyPanel ID="PropertyPanel3" Text="Type" runat="server">
        <asp:DropDownList ID="ddType" runat="server" AutoPostBack="true" CssClass="propertyFormInput"></asp:DropDownList>
    </umb:PropertyPanel>
    
    </umb:Pane>
    
    <umb:Pane runat="server" ID="settingsHolder" Visible="false">
        <asp:PlaceHolder ID="PlaceHolder1" runat="server"></asp:PlaceHolder>
    </umb:Pane>
        
    <div class="dialogcontrols">
        <asp:Button ID="Button1" runat="server" Text="Update" onclick="Button1_Click" />
        <em> or </em>
        <a href="javascript:parent.CloseUpdateWorkFlowDialog(null);">Cancel</a>
    </div>
    
    </div>
 
    
    </div>
</asp:Content>
