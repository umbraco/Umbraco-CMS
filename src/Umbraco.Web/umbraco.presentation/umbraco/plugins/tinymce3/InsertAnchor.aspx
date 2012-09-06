<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="InsertAnchor.aspx.cs" Inherits="umbraco.presentation.umbraco.plugins.tinymce3.InsertAnchor" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title><%= umbraco.ui.Text("insertAnchor") %></title>
	
    <base target="_self" />

    <cc1:UmbracoClientDependencyLoader runat="server" id="ClientLoader" />
	
	<umb:JsInclude ID="JsInclude4" runat="server" FilePath="tinymce3/tiny_mce_popup.js" PathNameAlias="UmbracoClient" Priority="100" />
	<umb:JsInclude ID="JsInclude1" runat="server" FilePath="tinymce3/themes/umbraco/js/anchor.js" PathNameAlias="UmbracoClient" Priority="101" />
    
    
</head>
<body style="display: none">

	

<form onsubmit="AnchorDialog.update();return false;" action="#">
	<table border="0" cellpadding="4" cellspacing="0">
		<tr>
			<td nowrap="nowrap"><%= umbraco.ui.Text("name") %>:</td>
			<td><input name="anchorName" type="text" class="mceFocus" id="anchorName" value="" style="width: 200px" /></td>
		</tr>
	</table>

	<div class="mceActionPanel">
		<div style="float: left">
			<input type="submit" id="insert" name="insert" value="<%= umbraco.ui.Text("update") %>" />
		</div>

		<div style="float: right">
			<input type="button" id="cancel" name="cancel" value="<%= umbraco.ui.Text("cancel") %>" onclick="tinyMCEPopup.close();" />
		</div>
	</div>
</form>
</body>
</html>
