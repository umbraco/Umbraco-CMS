<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PartialView.ascx.cs" Inherits="Umbraco.Web.UI.Umbraco.Create.PartialView" %>
<%@ Import Namespace="umbraco" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<cc1:Pane runat="server">
    <cc1:PropertyPanel runat="server" text="Filename (without .cshtml)">
        <asp:TextBox id="FileName" Runat="server" CssClass="bigInput input-large-type input-block-level"></asp:TextBox>
        <asp:RequiredFieldValidator id="RequiredFieldValidator1" ErrorMessage="*" ControlToValidate="FileName" runat="server">*</asp:RequiredFieldValidator>
    </cc1:PropertyPanel>

    <cc1:PropertyPanel runat="server" Text="Template">
        <asp:ListBox id="PartialViewTemplate" Runat="server" Width="350" CssClass="bigInput input-large-type input-block-level" Rows="1" SelectionMode="Single">
	        <asp:ListItem Value="clean.xslt">Clean</asp:ListItem>
        </asp:ListBox>
    </cc1:PropertyPanel>
    
    <cc1:PropertyPanel runat="server">
        <asp:CheckBox ID="CreateMacroCheckBox" Runat="server" Checked="true" Text="Create Macro"></asp:CheckBox>
    </cc1:PropertyPanel>
</cc1:Pane>
<!-- added to support missing postback on enter in IE -->
<asp:TextBox runat="server" style="visibility:hidden;display:none;" ID="Textbox1"/>
<input type="hidden" name="nodeType" value="-1">


<cc1:Pane runat="server" CssClass="btn-toolbar umb-btn-toolbar">
     <a href="#" class="btn btn-link" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("cancel")%></a>
     <asp:Button id="sbmt" Runat="server" CssClass="btn btn-primary" Text="Save" onclick="SubmitButton_Click"></asp:Button>
</cc1:Pane>