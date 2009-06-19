<%@ Register TagPrefix="cc2" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Page language="c#" Codebehind="EditMediaType.aspx.cs" MasterPageFile="../masterpages/umbracoPage.Master" AutoEventWireup="True" Inherits="umbraco.cms.presentation.settings.EditMediaType" %>
<%@ Register TagPrefix="uc1" TagName="ContentTypeControlNew" Src="../controls/ContentTypeControlNew.ascx" %>

<asp:Content ContentPlaceHolderID="body" runat="server">
			<uc1:ContentTypeControlNew id="ContentTypeControlNew1" runat="server"></uc1:ContentTypeControlNew>
</asp:Content>