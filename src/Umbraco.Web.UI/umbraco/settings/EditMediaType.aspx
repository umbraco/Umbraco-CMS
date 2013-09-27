<%@ Register TagPrefix="cc2" Namespace="umbraco.uicontrols" Assembly="controls" %>

<%@ Page Language="c#" CodeBehind="EditMediaType.aspx.cs" MasterPageFile="../masterpages/umbracoPage.Master"
    Async="true" AsyncTimeOut="300"
    AutoEventWireup="True" Inherits="umbraco.cms.presentation.settings.EditMediaType" %>

<%@ Register TagPrefix="uc1" TagName="ContentTypeControlNew" Src="../controls/ContentTypeControlNew.ascx" %>
<asp:Content ContentPlaceHolderID="body" runat="server">
    <uc1:ContentTypeControlNew ID="ContentTypeControlNew1" runat="server"></uc1:ContentTypeControlNew>
    <script type="text/javascript">
        jQuery(document).ready(function () {
            UmbClientMgr.appActions().bindSaveShortCut();
        });
    </script>
</asp:Content>
