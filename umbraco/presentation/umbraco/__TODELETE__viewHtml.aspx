<%@ Page language="c#" Codebehind="__TODELETE__viewHtml.aspx.cs" AutoEventWireup="True" Inherits="umbraco.cms.presentation.viewHtml" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<html>
	<head>
		<title>umbraco : <%=umbraco.ui.Text("general", "edit", base.getUser())%> HTML</title>
	</head>
	<body style="height: 100%">
		<form name="codeForm">
			<textarea name="code" style="font-family: courier; font-size: 11px; width: 100%;" rows="30"></textarea>
			<br />
			<input type="button" value="Tidy HTML" onclick="if (confirm('Tidy occurs when saving your document. Invoking Tidy now might break umbraco macros in richtext editor. Are you sure?')) tidyHtml();"> <input type="button" Value=" <%=umbraco.ui.Text("general", "update", base.getUser())%> " onClick="parent.opener.currentRichTextObject.innerHTML = document.codeForm.code.value; window.close();">
		</form>
		<script type="text/javascript" src="js/xmlextras.js"></script>
		<script type="text/javascript" src="js/xmlRequest.js"></script>
		<script language="javascript">
		var temp = parent.opener.currentRichTextObject.innerHTML;
		
		function updateHtml() {
			html = umbracoXmlRequestResultTxt();
			if (html != '[error]') {
			html.replace(/</g, "&lt;");
			html.replace(/>/g, "&gt;");
			document.codeForm.code.value = html;
			} else 
				alert('Error Tidy\'ing html - original and un-tidyed html below');
		}

document.codeForm.code.value = temp;
		</script>
	</body>
</html>
