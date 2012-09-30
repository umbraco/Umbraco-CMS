<%@ Page Language="c#" CodeBehind="TreeInit.aspx.cs" AutoEventWireup="True" Inherits="Umbraco.Web.UI.Umbraco.TreeInit" %>
<%@ Register Src="controls/Tree/TreeControl.ascx" TagName="TreeControl" TagPrefix="umbraco" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title></title>
	
    <cc1:UmbracoClientDependencyLoader runat="server" id="ClientLoader" />
    
    <umb:CssInclude ID="CssInclude1" runat="server" FilePath="css/umbracoGui.css" PathNameAlias="UmbracoRoot" />
    
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
	</asp:ScriptManager>
    <div>
		<umbraco:TreeControl runat="server" ID="JTree" 
		    App='<%#TreeParams.App %>' TreeType='<%#TreeParams.TreeType %>'
            IsDialog="<%#TreeParams.IsDialog %>" ShowContextMenu="<%#TreeParams.ShowContextMenu %>" 
            DialogMode="<%#TreeParams.DialogMode %>" FunctionToCall="<%#TreeParams.FunctionToCall %>" 
            NodeKey="<%#TreeParams.NodeKey %>" StartNodeID="<%#TreeParams.StartNodeID %>"  />
    </div>
    </form>
</body>
</html>
