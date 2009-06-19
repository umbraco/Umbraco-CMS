<%@ Control Language="c#" AutoEventWireup="True" Codebehind="quickEdit.ascx.cs" Inherits="umbraco.presentation.dashboard.quickEdit" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<%@ Register TagPrefix="umb" Namespace="umbraco.presentation.ClientDependency" Assembly="umbraco.presentation.ClientDependency" %>

<umb:ClientDependencyInclude runat="server" ID="ClientDependencyInclude3" DependencyType="Javascript" FilePath="js/autocomplete/jquery.autocomplete.js" PathNameAlias="UmbracoRoot" />

<div class="umbracoSearchHolder">
<input type="text" id="umbSearchField" accesskey="s" name="umbSearch" value="<%=umbraco.ui.Text("general", "typeToSearch")%>" />
<script type="text/javascript" src="js/quickedit.js"></script>
</div>
