<%@ Page Language="C#" AutoEventWireup="True" MasterPageFile="../../masterpages/umbracoPage.Master" Title="Install starter kit" CodeBehind="StarterKits.aspx.cs" Inherits="Umbraco.Web.UI.Umbraco.Developer.Packages.StarterKits" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

<umb:JsInclude ID="JsInclude1" runat="server" FilePath="ui/jqueryui.js" PathNameAlias="UmbracoClient" />

<script type="text/javascript">

    var percentComplete = 0;

    jQuery(document).ready(function() {
        //bind to button click events
        jQuery("a.selectStarterKit").click(function() {
            jQuery(".progress-status").siblings(".install-dialog").hide();
            jQuery(".progress-status").show();
        });
    });

    function updateProgressBar(percent) {
        percentComplete = percent;
    }
    function updateStatusMessage(message, error) {
        if (message != null && message != undefined) {
            jQuery(".progress-status").text(message + " (" + percentComplete + "%)");
        }        
    }

</script>
<style type="text/css">
    
    .progress-status {
	    display: none;
    }

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
    
    <cc1:Pane id="StarterKitNotInstalled" Text="Install starter kit" runat="server">
        <h3>Available starter kits</h3>
        <p>You can choose from the following starter kits, each having specific functionality.</p>        
        <div class="progress-status">Please wait...</div>
        <div id="connectionError"></div>
        <div id="serverError"></div>       
        <div class="install-dialog">
            <asp:PlaceHolder ID="ph_starterkits" runat="server"></asp:PlaceHolder>
        </div>
    </cc1:Pane>

    <cc1:Pane id="installationCompleted" Text="Installation completed" runat="server" Visible="false">
        <p>Installation completed succesfully</p>
    </cc1:Pane>
    
    <cc1:Pane id="InstallationDirectoryNotAvailable" Text="Unable to install" runat="server" Visible="false">
        <p>We can not install starterkits when the install directory or package repository is not present.</p>
    </cc1:Pane>
      

  </cc1:UmbracoPanel>


</asp:Content>