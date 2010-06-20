<%@ Page Language="c#" CodeBehind="default.aspx.cs" AutoEventWireup="True" Inherits="umbraco.presentation.install._default" EnableViewState="False" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<%@ Register Src="~/install/Title.ascx" TagPrefix="umb1" TagName="PageTitle" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    
    <umb1:PageTitle runat="server" />

    <cc1:UmbracoClientDependencyLoader runat="server" id="ClientLoader"  />
	
	<umb:CssInclude ID="CssInclude1" runat="server" FilePath="style.css" />
	<umb:CssInclude ID="CssInclude2" runat="server" FilePath="modal/style.css" PathNameAlias="UmbracoClient" />
	
	<umb:JsInclude ID="JsInclude4" runat="server" FilePath="ui/jquery.js" PathNameAlias="UmbracoClient" Priority="0" />
	<umb:JsInclude ID="JsInclude3" runat="server" FilePath="Application/NamespaceManager.js" PathNameAlias="UmbracoClient" Priority="0" />
	<umb:JsInclude ID="JsInclude1" runat="server" FilePath="modal/modal.js" PathNameAlias="UmbracoClient" Priority="10" />
	<umb:JsInclude ID="JsInclude2" runat="server" FilePath="passwordStrength/passwordstrength.js" PathNameAlias="UmbracoClient" Priority="11" />

    <style type="text/css">
      #generatedCategories{display: none;}
      #list1a{height: 300px; overflow: auto;}
      #list1a .installed h3, #list1a .installed h3 *{color: #666;}
      #list1a .installed h3 span{font-size: 11px; padding-left: 25px;} 
      
      .loadNitrosButton{position: absolute; bottom: 5px;}
      
      .actions a{color: #333; text-decoration: none; display:block; padding: 0px 25px 0px 10px; margin-top: 20px;}
      .actions a h3{color: #000; margin-bottom: 2px; text-decoration: underline}
      .actions a:hover{text-decoration: underline;}
      .passtestresult{margin-left: 15px; font-weight: bold; padding: 4px 10px 4px 10px !Important;}
      
    </style>      
    
</head>
<body>
	
    <script type="text/javascript">

        function nextStep(button, elementId) {
            showProgress(button, elementId);
            setTimeout('document.location.href = \'default.aspx?installStep=' + document.getElementById("step").value + '\'', 100);
        }

        function showProgress(button, elementId) {
            var img = document.getElementById(elementId);

            img.style.visibility = "visible";
            button.style.display = "none";
        }

        function InstallPackages(button, elementId) {
            var next = document.getElementById('<%= next.ClientID %>');
            next.style.display = "none";

            showProgress(button, elementId);
        }

        function toggleModules() {
            document.getElementById("generatedCategories").style.display = "block";
        }

        function openDemoModal(id, name) {
            UmbClientMgr.openModalWindow("http://packages.umbraco.org/viewPackageData.aspx?id=" + id, name, true, 750, 550)
            return false;
        }
    </script>

	

    <form id="Form1" method="post" runat="server">
    <asp:ScriptManager runat="server" ID="umbracoScriptManager">
    </asp:ScriptManager>
    <cc1:UmbracoPanel Style="text-align: left;" ID="Panel1" AutoResize="false" runat="server" Height="550px" Width="680px" Text="Umbraco Configuration Wizard">
        <div id="contentScroll">
            <asp:PlaceHolder ID="PlaceHolderStep" runat="server"></asp:PlaceHolder>
        </div>
        <div id="buttons">
            <div id="loadingBar">
               <cc1:ProgressBar ID="prgbar" runat="server" Title="Loading..." /><br />
            </div>
            
            <asp:Button ID="next" OnClientClick="nextStep(this,'loadingBar'); return false;" Text="Next &raquo;" runat="server" />
            
        </div>
    </cc1:UmbracoPanel>
    <input type="hidden" runat="server" value="welcome" id="step">
    </form>
    
    <div id="umbModalBox">
      <div id="umbModalBoxHeader"></div><a href="#" id="umbracModalBoxClose" class="jqmClose">&times;</a>
      <div id="umbModalBoxContent"><iframe frameborder="0" id="umbModalBoxIframe" src=""></iframe></div>
    </div>
</body>
</html>
