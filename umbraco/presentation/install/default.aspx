<%@ Page Language="c#" CodeBehind="default.aspx.cs" AutoEventWireup="True" Inherits="umbraco.presentation.install._default" EnableViewState="False" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<%@ Register Src="~/install/Title.ascx" TagPrefix="umb1" TagName="PageTitle" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    
    <umb1:PageTitle runat="server" />

    <cc1:UmbracoClientDependencyLoader runat="server" id="ClientLoader"  />
	
    <link rel="Stylesheet" href="style.css" type="text/css" />


	<umb:CssInclude ID="CssInclude2" runat="server" FilePath="modal/style.css" PathNameAlias="UmbracoClient" />
	
	<umb:JsInclude ID="JsInclude4" runat="server" FilePath="ui/jquery.js" PathNameAlias="UmbracoClient" Priority="0" />
	<umb:JsInclude ID="JsInclude3" runat="server" FilePath="Application/NamespaceManager.js" PathNameAlias="UmbracoClient" Priority="0" />
	<umb:JsInclude ID="JsInclude1" runat="server" FilePath="modal/modal.js" PathNameAlias="UmbracoClient" Priority="10" />
	<umb:JsInclude ID="JsInclude2" runat="server" FilePath="passwordStrength/passwordstrength.js" PathNameAlias="UmbracoClient" Priority="11" />
    <umb:JsInclude ID="JsInclude5" runat="server" FilePath="installer.js"  Priority="0" />

</head>
<body class="<%= currentStepClass %>">
	
    <script type="text/javascript">

        function nextStep(button, elementId) {
            showProgress(button, elementId);
            //setTimeout('document.location.href = \'default.aspx?installStep=' + document.getElementById("step").value + '\'', 100);
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


    <div id="main">
       
    
    <h1 id="header"><asp:Literal ID="lt_header" runat="server" /></h1>

    <div id="step">
        <asp:PlaceHolder ID="PlaceHolderStep" runat="server"></asp:PlaceHolder>
    </div>
    
    <div id="buttons">
       <div id="loadingBar">
          <cc1:ProgressBar ID="prgbar" runat="server" Title="Loading..." /><br />
       </div>
            
       <asp:Button ID="next" CssClass="next" OnClientClick="showProgress(this,'loadingBar'); return true;" OnCommand="onNextCommand" Text="Next &raquo;" runat="server" />
    </div>
    
    </div>
    <p style="text-align: center"><em>The new installer is <strong>work in progress</strong> and will <strong>change</strong> look / feel for final release of JUNO</em></p>
    <input type="hidden" runat="server" value="welcome" id="step">
    
    </form>
    
    <div id="umbModalBox">
      <div id="umbModalBoxHeader"></div><a href="#" id="umbracModalBoxClose" class="jqmClose">&times;</a>
      <div id="umbModalBoxContent"><iframe frameborder="0" id="umbModalBoxIframe" src=""></iframe></div>
    </div>
</body>
</html>
