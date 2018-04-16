<%@ Register Namespace="umbraco" TagPrefix="umb" Assembly="umbraco" %>
<%@ Page Language="c#" Codebehind="EditUser.aspx.cs" MasterPageFile="../masterpages/umbracoPage.Master" AutoEventWireup="True" Inherits="Umbraco.Web.UI.Umbraco.Users.EditUser" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<%@ Import Namespace="umbraco.cms.presentation.Trees" %>
<asp:Content ContentPlaceHolderID="body" runat="server">
      <cc1:TabView ID="UserTabs" Width="400px" Visible="true" runat="server" />

    <script type="text/javascript">
        jQuery(document).ready(function () {
            UmbClientMgr.appActions().bindSaveShortCut();
        });
    </script>
</asp:Content>
