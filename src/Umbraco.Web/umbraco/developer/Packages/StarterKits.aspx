<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="../../masterpages/umbracoPage.Master" Title="Install starter kit" CodeBehind="StarterKits.aspx.cs" Inherits="umbraco.presentation.umbraco.developer.Packages.StarterKits" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

<umb:JsInclude ID="JsInclude1" runat="server" FilePath="ui/jqueryui.js" PathNameAlias="UmbracoClient" />

<script type="text/javascript">
    function showProgress(button, elementId) {
        var img = document.getElementById(elementId);
        img.style.visibility = "visible";
        button.style.display = "none";

    }

  
    function InstallPackages(button, elementId) {
        showProgress(button, elementId);
    }
</script>
<style type="text/css">
    .add-thanks
    {
        position:absolute;
        left:-2500;
        display:none !important;
    }
    
    .zoom-list li {float: left; margin: 15px; display: block; width: 180px;}
    
    .btn-prev, .btn-next, .paging, .btn-preview, .faik-mask , .faik-mask-ie6
    {
        display:none;
    }
       
    .image {float: left; margin: 15px; display: block; width: 140px;}
    
    .image .gal-drop{padding-top:10px;}
    
    ul{list-style-type: none;}
</style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
  <cc1:UmbracoPanel id="Panel1" Text="Starter kit" runat="server" Width="612px" Height="600px" hasMenu="false">
    <cc1:Feedback ID="fb" runat="server" />
    
    <cc1:Pane id="StarterKitInstalled" Text="Install skin" runat="server">
        <h3>Available skins</h3>
        <p>You can choose from the following skins.</p>
        <asp:PlaceHolder ID="ph_skins" runat="server"></asp:PlaceHolder>
    </cc1:Pane>
    
    
    
    <cc1:Pane id="StarterKitNotInstalled" Text="Install starter kit" runat="server">
        <h3>Available starter kits</h3>
        <p>You can choose from the following starter kits, each having specific functionality.</p>
         <asp:PlaceHolder ID="ph_starterkits" runat="server"></asp:PlaceHolder>
    </cc1:Pane>

    <cc1:Pane id="installationCompleted" Text="Installation completed" runat="server" Visible="false">
        <p>Installation completed succesfully</p>
    </cc1:Pane>
  </cc1:UmbracoPanel>


</asp:Content>