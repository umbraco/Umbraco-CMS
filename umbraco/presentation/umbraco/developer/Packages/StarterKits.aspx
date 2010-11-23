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
    .declineStarterKits 
    {
        display:none;
    }
    
    #starterKits, #starterKitDesigns {list-style: none; margin: 0; padding: 0px;}
    #starterKits li, #starterKitDesigns li{float: left; margin: 15px; display: block; width: 130px; padding: 5px; border: 1px solid #efefef; text-align: center;}
    #starterKits li a, #starterKitDesigns li a{text-decoration: none; color: #999}
    #starterKits li span, #starterKitDesigns li span{display: block; text-align: center; padding-top: 10px;}
    #starterKits li div, #starterKitDesigns li div{display: none !Important;}
    #starterKits li img, #starterKitDesigns li img{border: none;}

    
    #starterKitDesc, #starterKitDesignDesc, #installingSkin, #installingStarterKit{clear: both; font-size: 1.5em; font-weight: bold; color: #999; padding: 10px;}
       
</style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
  <cc1:UmbracoPanel id="Panel1" Text="Starter kit" runat="server" Width="612px" Height="600px" hasMenu="false">
    <cc1:Feedback ID="fb" runat="server" />
    
    <cc1:Pane id="StarterKitInstalled" Text="Install skin" runat="server">
        <h3>Starter kit already installed</h3>
        <p>Lorem ipsum</p>
        <asp:PlaceHolder ID="ph_skins" runat="server"></asp:PlaceHolder>
    </cc1:Pane>
    
    
    
    <cc1:Pane id="StarterKitNotInstalled" Text="Install starter kit" runat="server">
        <h3>Starter kit info</h3>
        <p>Lorem ipsum</p>
         <asp:PlaceHolder ID="ph_starterkits" runat="server"></asp:PlaceHolder>
    </cc1:Pane>


  </cc1:UmbracoPanel>


</asp:Content>