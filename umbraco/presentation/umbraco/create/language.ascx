<%@ Control Language="c#" AutoEventWireup="True" Codebehind="language.ascx.cs" Inherits="umbraco.cms.presentation.create.controls.language" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<input type="hidden" name="nodeType">

<div style="MARGIN-TOP: 25px"><%=umbraco.ui.Text("choose")%> <%=umbraco.ui.Text("language")%>:<br />
<asp:DropDownList ID="Cultures" runat="server" Width="350px" CssClass="bigInput"></asp:DropDownList>
</div>

<div style="padding-top: 25px;">
<asp:Button id="sbmt" Runat="server" style="MARGIN-TOP: 14px" Width="90" onclick="sbmt_Click"></asp:Button>
&nbsp; <em><%= umbraco.ui.Text("or") %></em> &nbsp;<a href="#" style="color: blue"  onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("cancel")%></a>
</div>
