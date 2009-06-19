<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="InsertAnchor.aspx.cs" Inherits="umbraco.presentation.umbraco.plugins.tinymce3.InsertAnchor" %>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title><%= umbraco.ui.Text("insertAnchor") %></title>
	<script type="text/javascript" src="/umbraco_client/tinymce3/tiny_mce_popup.js"></script>
	<script type="text/javascript" src="/umbraco_client/tinymce3/themes/umbraco/js/anchor.js"></script>
	<base target="_self" />
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
