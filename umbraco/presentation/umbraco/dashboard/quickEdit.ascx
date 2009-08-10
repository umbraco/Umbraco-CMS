<%@ Control Language="c#" AutoEventWireup="True" Codebehind="quickEdit.ascx.cs" Inherits="umbraco.presentation.dashboard.quickEdit" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<umb:JsInclude ID="JsInclude1" runat="server" FilePath="js/autocomplete/jquery.autocomplete.js" PathNameAlias="UmbracoRoot" />
<umb:JsInclude ID="JsInclude2" runat="server" FilePath="js/quickedit.js" PathNameAlias="UmbracoRoot" />

<div class="umbracoSearchHolder">
	<input type="text" id="umbSearchField" accesskey="s" name="umbSearch" value="<%=umbraco.ui.Text("general", "typeToSearch")%>" />
</div>