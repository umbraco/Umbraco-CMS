<%@ Control Language="c#" AutoEventWireup="True" Codebehind="media.ascx.cs" Inherits="umbraco.cms.presentation.create.controls.media" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>

<%=umbraco.ui.Text("name")%>: <asp:RequiredFieldValidator id="RequiredFieldValidator1" ErrorMessage="*" ControlToValidate="rename" runat="server">*</asp:RequiredFieldValidator><br />
<asp:TextBox id="rename" Runat="server" Width="350px" CssClass="bigInput"></asp:TextBox>

<div style="MARGIN-TOP: 10px">
<%=umbraco.ui.Text("choose")%> <%=umbraco.ui.Text("media")%> <%=umbraco.ui.Text("type")%>:<br />
<asp:ListBox id="nodeType" Runat="server" Width="350px" CssClass="bigInput" Rows="1" SelectionMode="Single"></asp:ListBox><br />
</div>

<!-- added to support missing postback on enter in IE -->
<asp:TextBox runat="server" style="visibility:hidden;display:none;" ID="Textbox1"/>
<div style="MARGIN-TOP: 15px;">
<asp:Button id="sbmt" Runat="server" style="MARGIN-TOP: 14px" Width="90" onclick="sbmt_Click"></asp:Button>
<em> or </em>
<a href="#" style="color: Blue; margin-left: 6px;" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("cancel")%></a>
</div>
