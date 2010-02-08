<%@ Control Language="c#" AutoEventWireup="True" Codebehind="simple.ascx.cs" Inherits="umbraco.cms.presentation.create.controls.simple" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<input type="hidden" name="nodeType">
<div style="MARGIN-TOP: 20px"><%=umbraco.ui.Text("name")%>:<asp:RequiredFieldValidator id="RequiredFieldValidator1" ErrorMessage="*" ControlToValidate="rename" runat="server">*</asp:RequiredFieldValidator><br />

<asp:TextBox id="rename" CssClass="bigInput" Runat="server" width="350px"></asp:TextBox>
<!-- added to support missing postback on enter in IE -->
<asp:TextBox runat="server" style="visibility:hidden;display:none;" ID="Textbox1"/>

</div>

<div style="padding-top: 25px;">
	<asp:Button id="sbmt" Runat="server" style="Width:90px" onclick="sbmt_Click"></asp:Button>
	&nbsp; <em><%= umbraco.ui.Text("or") %></em> &nbsp;
  <a href="#" style="color: blue"  onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("cancel")%></a>
</div>
