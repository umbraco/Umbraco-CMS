<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="../masterpages/umbracoPage.Master" CodeBehind="search.aspx.cs" Inherits="umbraco.presentation.members.search" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register Src="~/umbraco/members/MemberSearch.ascx" TagName="MemberSearch" TagPrefix="umb" %>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
<cc1:UmbracoPanel id="Panel2" runat="server" Height="224px" Width="412px" hasMenu="false">
                <cc1:Pane runat="server">
                   <cc1:PropertyPanel runat="server">
                    <umb:MemberSearch runat="server" />
                    </cc1:PropertyPanel>
                    </cc1:Pane>
    </cc1:UmbracoPanel>
</asp:Content>
