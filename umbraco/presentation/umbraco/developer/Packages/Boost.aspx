<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="../../masterpages/umbracoPage.Master" Title="Install boost" CodeBehind="Boost.aspx.cs" Inherits="umbraco.presentation.developer.packages.Boost" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>


<asp:Content ContentPlaceHolderID="head" runat="server">

<umb:JsInclude ID="JsInclude1" runat="server" FilePath="ui/jqueryui.js" PathNameAlias="UmbracoClient" />

<script type="text/javascript">
        function showProgress(button, elementId) {
            var img = document.getElementById(elementId);
            img.style.visibility = "visible";
            button.style.display = "none";
            
          }

          function openDemoModal(id, title) {
              UmbClientMgr.openModalWindow("http://packages.umbraco.org/viewPackageData.aspx?id=" + id, title, true, 750, 550)
          }

          function InstallPackages(button, elementId) {
            showProgress(button, elementId);
          }
</script>

<style type="text/css">
#list1a a.accordianOpener{cursor: pointer; font-size: 14px; font-weight: bold; display: block; padding: 5px; text-decoration: none;}
#list1a a.accordianOpener small{display: block;}
#list1a div.accordianContainer{border-bottom: 1px solid #D9D7D7; display : none;}
#list1a .nitroCB{display: block; padding: 2px; margin: 0px 0px 5px 20px;}

#list1a input{float: left; margin: 0px 10px 20px 0px;}
#list1a div.nitro{float: left; padding: 0px 10px 0px 0px; width: 90%;}
#list1a div.nitro h3{padding: 0px; margin: 0px; line-height: 14px; font-size: 14px;}
#list1a .installed h3, #list1a .installed h3 *{color: #666;}
#list1a .installed h3 span{font-size: 11px; padding-left: 25px;} 

#list1a{padding-bottom: 25px;}
</style>
</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">
  <cc1:UmbracoPanel id="Panel1" Text="Runway" runat="server" Width="612px" Height="600px" hasMenu="false">
    <cc1:Feedback ID="fb" runat="server" />
    
    <cc1:Pane id="boostInstalled" runat="server">
        
    <asp:Panel ID="nitroPanel"  runat="server"></asp:Panel>
    
    <span id="loadingBar" style="visibility: hidden;">
        <cc1:ProgressBar ID="progbar" runat="server" Title="Please wait..." />
    </span>
    </cc1:Pane>
    
    
    
    <cc1:Pane id="boostNotInstalled" Text="Install runway" runat="server">
    <h3><%= umbraco.ui.Text("installer", "runwayWhatIsRunway") %>?</h3>
    
    <%= umbraco.ui.Text("installer", "runwaySimpleSiteText") %>
    
    <p>
    <button  onclick="if (confirm('Are you sure you wish to install:\n\Runway\n\n')); window.location.href = 'installer.aspx?guid=ae41aad0-1c30-11dd-bd0b-0800200c9a66&repoGuid=65194810-1f85-11dd-bd0b-0800200c9a66'; return false;"><%= umbraco.ui.Text("install") %> Runway</button>
    </p>
    
    </cc1:Pane>
  </cc1:UmbracoPanel>

  <script type="text/javascript">
    jQuery().ready(function() {
        jQuery('#list1a').accordion({ header: '.accordianOpener', autoheight: true });
    });
   </script>

</asp:Content>