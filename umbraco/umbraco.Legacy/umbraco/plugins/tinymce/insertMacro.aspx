<%@ Page language="c#" validateRequest="false" Codebehind="insertMacro.aspx.cs" AutoEventWireup="True" Inherits="umbraco.presentation.tinymce.insertMacro" trace="false" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>umbraco :: <%=umbraco.ui.Text("general", "insert",this.getUser())%> <%=umbraco.ui.Text("general", "macro",this.getUser())%> &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</title>
		<meta name="GENERATOR" Content="Microsoft Visual Studio .NET 7.1">
		<meta name="CODE_LANGUAGE" Content="C#">
		<meta name="vs_defaultClientScript" content="JavaScript">
		<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
		<link type="text/css" rel="stylesheet" href="../../css/umbracoGui.css">
		<html xmlns="http://www.w3.org/1999/xhtml">
    	<script language="javascript" type="text/javascript" src="../../../umbraco_client/tinymce/tiny_mce_popup.js"></script>
		<script>
		function umbracoEditMacroDo(fieldTag, macroName, renderedContent) {
			var inst = tinyMCE.getInstanceById(tinyMCE.getWindowArg('editor_id'));
			var elm = inst.getFocusElement();

			// is it edit macro?
			if (elm.nodeName == "DIV" && tinyMCE.getAttrib(elm, 'class').indexOf('umbMacroHolder') >= 0) {
				tinyMCE.setOuterHTML(elm, renderedContent);
			}
			else {
				tinyMCEPopup.execCommand("mceInsertContent", false, renderedContent);
			}
			tinyMCE.selectedInstance.repaint();
			tinyMCEPopup.close();
		}

		function saveTreepickerValue(appAlias, macroAlias) {
			var treePicker = window.showModalDialog('../../dialogs/treePicker.aspx?app=' + appAlias + '&treeType=' + appAlias, 'treePicker', 'dialogWidth=350px;dialogHeight=300px;scrollbars=no;center=yes;border=thin;help=no;status=no')
			document.forms[0][macroAlias].value = treePicker;
			document.getElementById("label" + macroAlias).innerHTML = "</b><i>updated with id: " + treePicker + "</i><b><br/>";
		}
		
		var macroAliases = new Array();
		
		function registerAlias(alias) {
			macroAliases[macroAliases.length] = alias;
		}

		function pseudoHtmlEncode(text) {
			return text.replace(/\"/gi,"&amp;quot;").replace(/\</gi,"&amp;lt;").replace(/\>/gi,"&amp;gt;");
		}
		</script>
		<script language="javascript" type="text/javascript">
		<!--
			function init() {
				var inst = tinyMCE.selectedInstance;
				var elm = inst.getFocusElement();
			}

			function insertSomething() {
				tinyMCEPopup.close();
			}
		//-->
		</script>
	</HEAD>
	<body MS_POSITIONING="GridLayout" onload="tinyMCEPopup.executeOnLoad('init();');">
		<form id="Form1" runat="server">
			<div style="PADDING-RIGHT: 17px; PADDING-LEFT: 3px; FLOAT: right; PADDING-BOTTOM: 3px; PADDING-TOP: 3px"><a href="javascript:parent.window.close()" class="guiDialogTiny"><%=umbraco.ui.Text("general", "closewindow", this.getUser())%></a></div>
			
		<asp:Panel id="theForm" runat="server">
		<input type="hidden" name="macroMode" value="<%=Request["mode"]%>"/>
		<%if (Request["umb_macroID"] != null || Request["umb_macroAlias"] != null) {%>
			<h3><%=umbraco.ui.Text("general", "edit",this.getUser())%> <asp:Literal id="macroName" runat="server"></asp:Literal></h3>
			<input type="hidden" name="umb_macroID" value="<%=umbraco.helper.Request("umb_macroID")%>"/>
			<input type="hidden" name="umb_macroAlias" value="<%=umbraco.helper.Request("umb_macroAlias")%>"/>
			<TABLE class="propertyPane" cellSpacing="0" cellPadding="4" width="100%" border="0">
				<asp:PlaceHolder ID="macroProperties" Runat="server" />
			</TABLE>
			
			<asp:button id="renderMacro" runat="server" text="ok"></asp:button>
		<%} else {%>
			<h3><%=umbraco.ui.Text("general", "insert",this.getUser())%> <%=umbraco.ui.Text("general", "macro", this.getUser())%></h3>
			<TABLE class="propertyPane" cellSpacing="0" cellPadding="4" width="100%" border="0">
				<tr><td>
				<asp:ListBox Rows="1" ID="umb_macroAlias" Runat="server"></asp:ListBox> <input type="submit" value="<%=umbraco.ui.Text("general", "ok", this.getUser())%>"/>
				</td></tr>
			</TABLE>		
		<%}%>
		
		</asp:Panel>
		<div id="renderContent" style="display: none">
		
		<asp:PlaceHolder id="renderHolder" runat="server"></asp:PlaceHolder>
	
		</div>
		</form>
		
	</body>
</HTML>
