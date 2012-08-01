<%@ Control Language="c#" AutoEventWireup="True" Codebehind="nodeType.ascx.cs" Inherits="umbraco.cms.presentation.create.controls.nodeType" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<p style="margin: 4px 0;">Master Document Type:<br />
<asp:ListBox id="masterType" Runat="server" cssClass="bigInput" Rows="1" SelectionMode="Single"></asp:ListBox>
<asp:Literal ID="masterTypePreDefined" runat="server" Visible="false"></asp:Literal>
</p>

<div style="MARGIN-TOP: 0px"><%=umbraco.ui.Text("name")%>:
<asp:RequiredFieldValidator id="RequiredFieldValidator1" ErrorMessage="*" ControlToValidate="rename" runat="server">*</asp:RequiredFieldValidator>

<asp:CustomValidator ID="CustomValidation1" ErrorMessage="* A document type with this name/alias already exists" OnServerValidate="validationDoctypeName" ControlToValidate="rename" runat="server" /><br />

<asp:TextBox id="rename" Runat="server" Width="350" CssClass="bigInput"></asp:TextBox>
</div>
<div style="margin-top: 0px; margin-left: -3px; margin-bottom: 0;">
<asp:CheckBox ID="createTemplate" Runat="server" Checked="true" Text="Create matching template"></asp:CheckBox>
</div>

<asp:TextBox runat="server" style="visibility:hidden;display:none;" ID="Textbox1"/>

<div style="margin-top: 10px;">
<asp:Button id="sbmt" Runat="server" style="MARGIN-TOP: 14px" Width="90" onclick="sbmt_Click"></asp:Button>
<em> or </em>
<a href="#" style="color: Blue; margin-left: 6px;" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("cancel")%></a>
</div>
