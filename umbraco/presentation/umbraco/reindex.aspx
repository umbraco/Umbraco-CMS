<%@ Page language="c#" Codebehind="republish.aspx.cs" AutoEventWireup="True" Inherits="umbraco.cms.presentation.reindex" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>umbraco - <%=umbraco.ui.Text("sitereindexed")%></title>
		<meta name="GENERATOR" Content="Microsoft Visual Studio .NET 7.1">
		<meta name="CODE_LANGUAGE" Content="C#">
		<meta name="vs_defaultClientScript" content="JavaScript">
		<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
		<LINK href="css/umbracoGui.css" type="text/css" rel="stylesheet">
	</HEAD>
	<body MS_POSITIONING="GridLayout" style="padding: 10px;">
		<form id="Form1" method="post" runat="server">
		<asp:Panel ID="indexed" runat="server">
		<h3>All content is re-indexed.</h3>
		</asp:Panel>
		<asp:Panel Visible="false" ID="inProgress" runat="server">
		<h3>Re-index in progress</h3>
		<p>
		<asp:Literal ID="reindexProgress" runat="server"></asp:Literal> of <asp:Literal ID="reindexTotal" runat="server"></asp:Literal> documents indexed. Currently indexing '<asp:Literal ID="reindexCurrent" runat="server"></asp:Literal>'<br />
		</p>
		<p>
		<a href="reindex.aspx">Refresh</a> <asp:PlaceHolder runat="server" ID="invoke" Visible="false">| <a href="?startIndexing=true">Start indexing again</a></asp:PlaceHolder><br />
		</p>
		</asp:Panel>
		<br/>
		<a href="#" onClick="javascript:window.close();" style="margin-left: 30px" class="guiDialogNormal"><%=umbraco.ui.Text("closewindow")%></a>
		</form>
	</body>
</HTML>
