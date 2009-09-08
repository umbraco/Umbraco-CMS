<%@ Page language="c#" Codebehind="paste.aspx.cs" AutoEventWireup="True" Inherits="umbraco.presentation.tinymce.paste" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" > 

<html>
  <head>
		<title>umbraco :: <%=umbraco.ui.Text("defaultdialogs", "paste", this.getUser())%> &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</title>
    <meta name="GENERATOR" Content="Microsoft Visual Studio .NET 7.1">
    <meta name="CODE_LANGUAGE" Content="C#">
    <meta name=vs_defaultClientScript content="JavaScript">
    <meta name=vs_targetSchema content="http://schemas.microsoft.com/intellisense/ie5">
	<script language="javascript" type="text/javascript" src="../../../umbraco_client/tinymce/tiny_mce_popup.js"></script>
	<LINK href="../../css/umbracoGui.css" type="text/css" rel="stylesheet">
	<style>body {margin: 2px;}</style>
<SCRIPT LANGUAGE="JavaScript">
<!--
function returnPasteValue()
{
	for (var i=0;i<document.forms[0].length;i++) {
		if(document.forms[0][i].name = "action") {
			if (document.forms[0][i].checked) {
				if (document.forms[0][i].value == 'removeSpecial') {
        			var inst = tinyMCE.getInstanceById(tinyMCE.getWindowArg('editor_id'));
        			tinyMCE.execInstanceCommand(tinyMCE.getWindowArg('editor_id'), "mceCleanup", true);
        			break;
			    } else if (document.forms[0][i].value == 'removeAll') {
			        var _content = tinyMCE.getContent().replace(/<br \/>|<\/p>/g, '\n').replace(/<(.|\n)*?>/g,'').replace(/\n/g,'<br />');
			        tinyMCE.setContent(_content);
    			    break;        
			    }
            }
		}
	}
    window.close();
} 

function doSubmit() {returnPasteValue();}

	var functionsFrame = this;
	var tabFrame = this;
	var isDialog = true;
	var submitOnEnter = true;
-->
	</script>
<script type="text/javascript" src="../../js/umbracoCheckKeys.js"></script>
  </head>
  <body>
	
	<h3><%=umbraco.ui.Text("defaultdialogs", "paste", this.getUser())%></h3>
    <form method="post">
	<TABLE WIDTH="100%" CELLPADDING=4 CELLSPACING=0 class="propertyPane">
		<tr><td colspan="2" class="propertyHeader">
			<span style="color: #666">
			<%=umbraco.ui.Text("paste", "errorMessage", this.getUser())%><br/><br/></span>
			<input type="radio" name="action" id="removeSpecial" value="removeSpecial" checked> <label for="removeSpecial"><%=umbraco.ui.Text("paste", "removeSpecialFormattering", this.getUser())%></label><br/>
			<input type="radio" name="action" id="removeAll" value="removeAll"> <label for="removeAll"><%=umbraco.ui.Text("paste", "removeAll", this.getUser())%></label><br/>
			<input type="radio" name="action" id="doNothing" value="doNothing"> <label for="doNothing"><%=umbraco.ui.Text("paste", "doNothing", this.getUser())%></label><br/>
			<br/>
	<input type="button" class="guiInputButton" onClick="returnPasteValue()" value="<%=umbraco.ui.Text("general", "ok", this.getUser())%>">
			</td>
		</tr>
	</table><br />

     </form>
	
  </body>
</html>
