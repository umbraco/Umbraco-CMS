<%@ Page language="c#" Codebehind="test.aspx.cs" AutoEventWireup="True" Inherits="umbraco.cms.presentation.test" trace="false" validateRequest="false" %>
<html>
<body>
<form runat="server" id="form">
<asp:ScriptManager ID="scriptmanager" runat="server"></asp:ScriptManager>
<asp:placeholder id="testHolder" runat="server"></asp:placeholder>
<asp:Button ID="button" runat="server" onclick="button_Click" />
<asp:PlaceHolder ID="result" runat="server"></asp:PlaceHolder>
</form></body></html>