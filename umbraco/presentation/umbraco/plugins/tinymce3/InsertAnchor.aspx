<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="InsertAnchor.aspx.cs" Inherits="umbraco.presentation.umbraco.plugins.tinymce3.InsertAnchor" %>
<%@ Register TagPrefix="umb" Namespace="umbraco.presentation.ClientDependency.Controls" Assembly="umbraco.presentation.ClientDependency" %>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title><%= umbraco.ui.Text("insertAnchor") %></title>
	<base target="_self" />
</head>
<body style="display: none">

	<umb:ClientDependencyLoader runat="server" id="ClientLoader" EmbedType="Header" IsDebugMode="false" >
		<Paths>
			<umb:ClientDependencyPath Name="UmbracoClient" Path='<%#umbraco.GlobalSettings.ClientPath%>' />
			<umb:ClientDependencyPath Name="UmbracoRoot" Path='<%#umbraco.GlobalSettings.Path%>' />
		</Paths>		
	</umb:ClientDependencyLoader>
	
	<umb:JsInclude ID="JsInclude4" runat="server" FilePath="tinymce3/tiny_mce_popup.js" PathNameAlias="UmbracoClient" Priority="100" />
	<umb:JsInclude ID="JsInclude1" runat="server" FilePath="tinymce3/themes/umbraco/js/anchor.js" PathNameAlias="UmbracoClient" Priority="101" />

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
