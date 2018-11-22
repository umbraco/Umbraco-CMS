<%@ Control Language="c#" AutoEventWireup="True" Codebehind="QuickSearch.ascx.cs" Inherits="Umbraco.Web.UI.Umbraco.Search.QuickSearch" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<umb:JsInclude ID="JsInclude1" runat="server" FilePath="Search/quickSearch.js" PathNameAlias="UmbracoRoot" />
<umb:JsInclude ID="JsInclude3" runat="server" FilePath="Application/JQuery/jquery.autocomplete.js" PathNameAlias="UmbracoClient" />

<script type="text/javascript">
    jQuery(document).ready(function () {
        jQuery("#umbSearchField").UmbQuickSearch('<%= Umbraco.Core.IO.IOHelper.ResolveUrl( Umbraco.Core.IO.SystemDirectories.Umbraco ) + "/Search/QuickSearchHandler.ashx" %>');

        UmbClientMgr.historyManager().addEventHandler("navigating", function (e, app) {
            jQuery("#umbSearchField").flushCache();
        });
    });
</script>

<div class="umbracoSearchHolder">
	<input type="text" id="umbSearchField" accesskey="s" name="umbSearch" value="<%=umbraco.ui.Text("general", "typeToSearch")%>" />
</div>