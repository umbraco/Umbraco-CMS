<%@ Page Language="c#" CodeBehind="treeInit.aspx.cs" AutoEventWireup="True" Inherits="umbraco.cms.presentation.TreeInit" %>

<%@ Register Src="~/umbraco/controls/TreeControl.ascx" TagName="TreeControl" TagPrefix="umbraco" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title></title>
    <link rel="stylesheet" type="text/css" href="/umbraco_client/Tree/Themes/tree_component.css" />
	<link rel="stylesheet" type="text/css" href="/umbraco/css/umbracoGui.css" />
	<style type="text/css">
        body
        {
            background: #fff;
            margin: 0px;
            padding: 0px;
        }
    </style>
    <!--[if IE 6]>
    <style type="text/css">
          .sprTree{
            background-image: url(images/umbraco/sprites_ie6.gif) !Important;
          } 
    </style>
    <![endif]-->
</head>
<body>
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true" LoadScriptsBeforeUI="true">
		<Scripts>			
			<asp:ScriptReference Path="~/umbraco_client/Application/NamespaceManager.js" />
			<asp:ScriptReference Path="~/umbraco_client/ui/jquery.js" />			
			<asp:ScriptReference Path="~/umbraco_client/Application/JQuery/jquery.metadata.min.js" />	
			<asp:ScriptReference Path="~/umbraco_client/Application/UmbracoApplicationActions.js" />
			<asp:ScriptReference Path="~/umbraco_client/Application/UmbracoUtils.js" />
			<asp:ScriptReference Path="~/umbraco_client/Application/UmbracoClientManager.js" />
		</Scripts>
	</asp:ScriptManager>
    <div>
		<umbraco:TreeControl runat="server" ID="JTree"></umbraco:TreeControl>
    </div>
    </form>
</body>
</html>
