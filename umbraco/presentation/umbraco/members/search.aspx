<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="../masterpages/umbracoPage.Master" CodeBehind="search.aspx.cs" Inherits="umbraco.presentation.members.search" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register Src="~/umbraco/members/MemberSearch.ascx" TagName="MemberSearch" TagPrefix="umb" %>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
 <cc1:UmbracoPanel ID="panel1" runat="server">
    <umb:MemberSearch runat="server" />
    </cc1:UmbracoPanel>
</asp:Content>
