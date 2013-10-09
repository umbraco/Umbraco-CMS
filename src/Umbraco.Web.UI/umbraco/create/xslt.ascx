<%@ Control Language="c#" AutoEventWireup="True" Codebehind="xslt.ascx.cs" Inherits="Umbraco.Web.UI.Umbraco.Create.Xslt" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<cc1:Pane runat="server">
    <cc1:PropertyPanel runat="server" text="Filename (without .xslt)">
        <asp:TextBox id="rename" Runat="server" CssClass="bigInput input-large-type input-block-level"></asp:TextBox>
        <asp:RequiredFieldValidator id="RequiredFieldValidator1" ErrorMessage="*" ControlToValidate="rename" runat="server">*</asp:RequiredFieldValidator>
    </cc1:PropertyPanel>

    <cc1:PropertyPanel runat="server" Text="Choose a snippet:">
        <asp:ListBox id="xsltTemplate" Runat="server" CssClass="bigInput input-large-type input-block-level" Rows="1" SelectionMode="Single">
	        <asp:ListItem Value="clean.xslt">Clean</asp:ListItem>
        </asp:ListBox>
    </cc1:PropertyPanel>
    
    <cc1:PropertyPanel runat="server" Text="">
        <asp:CheckBox ID="createMacro" Runat="server" Checked="true" Text="Create Macro"></asp:CheckBox>
    </cc1:PropertyPanel>
</cc1:Pane>

<!-- added to support missing postback on enter in IE -->
<asp:TextBox runat="server" style="visibility:hidden;display:none;" ID="Textbox1"/>
<input type="hidden" name="nodeType" value="-1">


<cc1:Pane runat="server" CssClass="btn-toolbar umb-btn-toolbar">
     <a href="#" class="btn btn-link" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("cancel")%></a>
     <asp:Button id="sbmt" Runat="server" CssClass="btn btn-primary" onclick="sbmt_Click"></asp:Button>
</cc1:Pane>

