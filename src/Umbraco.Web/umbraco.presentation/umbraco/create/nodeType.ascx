<%@ Control Language="c#" AutoEventWireup="True" Codebehind="nodeType.ascx.cs" Inherits="umbraco.cms.presentation.create.controls.nodeType" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<cc1:Pane runat="server">
    <cc1:PropertyPanel runat="server" text="Master Document Type">
        <asp:ListBox id="masterType" Runat="server" cssClass="bigInput input-large-type input-block-level" Rows="1" SelectionMode="Single"></asp:ListBox>
        <asp:Literal ID="masterTypePreDefined" runat="server" Visible="false"></asp:Literal>
    </cc1:PropertyPanel>
    
    <cc1:PropertyPanel runat="server" id="pp_name" text="Name">
        <asp:TextBox id="rename" Runat="server" CssClass="bigInput input-large-type input-block-level"></asp:TextBox>
        <asp:RequiredFieldValidator id="RequiredFieldValidator1" ErrorMessage="*" ControlToValidate="rename" runat="server">*</asp:RequiredFieldValidator>
    </cc1:PropertyPanel>

    <cc1:PropertyPanel runat="server">
        <asp:CheckBox ID="createTemplate" Runat="server" Checked="true" Text="Create matching template"></asp:CheckBox>
    </cc1:PropertyPanel>


    <asp:TextBox runat="server" style="visibility:hidden;display:none;" ID="Textbox1"/>


    <asp:CustomValidator ID="CustomValidation1" ErrorMessage="* A document type with this name/alias already exists" OnServerValidate="validationDoctypeName" ControlToValidate="rename" ForeColor="red" runat="server" />
    <asp:CustomValidator ID="CustomValidation2" ErrorMessage="<br/>* The name of the document type will result in an invalid alias" OnServerValidate="validationDoctypeAlias" ControlToValidate="rename" ForeColor="red" runat="server" />
</cc1:Pane>

<cc1:Pane runat="server" CssClass="btn-toolbar umb-btn-toolbar">
     <a href="#" class="btn btn-link" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("cancel")%></a>
     <asp:Button id="sbmt" Runat="server" CssClass="btn btn-primary" onclick="sbmt_Click"></asp:Button>
</cc1:Pane>