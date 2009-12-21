<%@ Page Language="C#" MasterPageFile="../masterpages/umbracoDialog.Master" AutoEventWireup="true"
    CodeBehind="ExportCode.aspx.cs" Inherits="umbraco.presentation.umbraco.dialogs.ExportCode" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<asp:Content ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .label
        {
            width: 150px;
            float: left;
            display: block;
        }
    </style>
</asp:Content>
<asp:Content ContentPlaceHolderID="body" runat="server">
    <p id="pageName" style="text-align: center;">
        <%=
           umbraco.ui.GetText("exportDocumentTypeAsCode-Full") 
        %>
    </p>
    <cc1:Pane ID="pane_language" runat="server">
        <em class="label">Language:</em>
        <asp:DropDownList ID="ddlLanguage" runat="server">
            <asp:ListItem Value="CSharp" Text="C#" Selected="True" />
            <asp:ListItem Value="VB" Text="Visual Basic .NET" />
        </asp:DropDownList>
    </cc1:Pane>
    <cc1:Pane ID="pane_contextName" runat="server">
        <div>
            <em class="label">DataContext Name:</em>
            <asp:TextBox ID="txtDataContextName" runat="server" Style="width: 180px;" Text="Umbraco" />
        </div>
        <div>
            <em class="label">Namespace:</em>
            <asp:TextBox ID="txtNamespace" runat="server" Style="width: 180px;" Text="Umbraco" />
        </div>
    </cc1:Pane>
    <cc1:Pane ID="pane_abstractions" runat="server">
        <div>
            <em class="label">Generate Interfaces:</em>
            <asp:CheckBox ID="chkAsInterfaces" runat="server" />
        </div>
        <div>
            <em class="label">Generate with Interface Inheritance:</em>
            <asp:CheckBox ID="chkIncludeIterfaceInheritance" runat="server" />
        </div>
    </cc1:Pane>
    <div style="margin-top: 10px;">
        <asp:Button ID="btnGenerate" runat="server" Text="Submit" OnClick="btnGenerate_Click" style="MARGIN-TOP: 14px" />
        <em>or </em><a href="#" style="color: Blue; margin-left: 6px;" onclick="UmbClientMgr.mainWindow().closeModal()">
            <%=umbraco.ui.Text("cancel")%></a>
    </div>
</asp:Content>
