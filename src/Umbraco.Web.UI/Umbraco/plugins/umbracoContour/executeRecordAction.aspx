<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="../../masterpages/umbracoDialog.Master" CodeBehind="executeRecordAction.aspx.cs" Inherits="Umbraco.Forms.UI.Dialogs.executeRecordAction" %>
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

<umb:feedback runat="server" ID="fb"/>

<asp:PlaceHolder ID="placeholder" runat="server"> <div id="dialogcontainer">
    <div>
        
    <umb:Pane runat="server" ID="settingsHolder" Visible="false">
        <asp:PlaceHolder ID="PlaceHolder1" runat="server"></asp:PlaceHolder>
    </umb:Pane>
        
    <div class="dialogcontrols">
        <asp:Button ID="Button1" runat="server" Text="Execute" onclick="Button1_Click" />
        <em> or </em>
        <a href="javascript:parent.CloseRecordActionSettingsDialog(null);">Cancel</a>
    </div>
    </div>
</div>
</asp:PlaceHolder>
    
</asp:Content>