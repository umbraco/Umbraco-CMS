<%@ Page Language="c#" CodeBehind="treeInit.aspx.cs" AutoEventWireup="True" Inherits="umbraco.cms.presentation.TreeInit" %>
<%@ Register Src="~/umbraco/controls/TreeControl.ascx" TagName="TreeControl" TagPrefix="umbraco" %>
<%@ Register TagPrefix="umb" Namespace="umbraco.presentation.ClientDependency.Controls" Assembly="umbraco.presentation.ClientDependency" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title></title>
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
    <umb:ClientDependencyLoader runat="server" id="ClientLoader" EmbedType="Header" IsDebugMode="false" >
		<Paths>
			<umb:ClientDependencyPath Name="UmbracoClient" Path='<%#umbraco.GlobalSettings.ClientPath%>' />
			<umb:ClientDependencyPath Name="UmbracoRoot" Path='<%#umbraco.GlobalSettings.Path%>' />
		</Paths>		
	</umb:ClientDependencyLoader>
    
    <umb:CssInclude ID="CssInclude1" runat="server" FilePath="css/umbracoGui.css" PathNameAlias="UmbracoRoot" />
    
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true" LoadScriptsBeforeUI="true">
	</asp:ScriptManager>
    <div>
		<umbraco:TreeControl runat="server" ID="JTree"></umbraco:TreeControl>
    </div>
    </form>
</body>
</html>
