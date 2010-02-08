<%@ Control Language="c#" AutoEventWireup="True" Codebehind="xslt.ascx.cs" Inherits="umbraco.presentation.create.xslt" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
Filename (without .xslt): <asp:RequiredFieldValidator id="RequiredFieldValidator1" ErrorMessage="*" ControlToValidate="rename" runat="server">*</asp:RequiredFieldValidator><br />
<input type="hidden" name="nodeType" value="-1">
<asp:TextBox id="rename" Runat="server" CssClass="bigInput" Width="350"></asp:TextBox>


<div style="MARGIN-TOP: 10px">Choose a template:<br />
<asp:ListBox id="xsltTemplate" Runat="server" Width="350" CssClass="bigInput" Rows="1" SelectionMode="Single">
	<asp:ListItem Value="clean.xslt">Clean</asp:ListItem>
</asp:ListBox>
</div>

<div style="MARGIN-TOP: 10px">
<asp:CheckBox ID="createMacro" Runat="server" Checked="true" Text="Create Macro"></asp:CheckBox>
</div>

<!-- added to support missing postback on enter in IE -->
<asp:TextBox runat="server" style="visibility:hidden;display:none;" ID="Textbox1"/>


<div style="MARGIN-TOP: 15px;">
<asp:Button id="sbmt" Runat="server" style="MARGIN-TOP: 14px" Width="90" onclick="sbmt_Click"></asp:Button>
&nbsp; <em><%= umbraco.ui.Text("or") %></em> &nbsp;
<a href="#" style="color: blue"  onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("cancel")%></a>
</div>
