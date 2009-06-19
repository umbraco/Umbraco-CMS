<%@ Control Language="c#" AutoEventWireup="True" Codebehind="quickEdit.ascx.cs" Inherits="umbraco.presentation.dashboard.quickEdit" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>

<div class="umbracoSearchHolder">
<input type="text" id="umbSearchField" accesskey="s" name="umbSearch" value="<%=umbraco.ui.Text("general", "typeToSearch")%>" />
<script type="text/javascript" src="js/quickedit.js"></script>
</div>
