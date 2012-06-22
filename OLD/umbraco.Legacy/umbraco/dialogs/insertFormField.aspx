<%@ Page language="c#" Codebehind="insertFormField.aspx.cs" AutoEventWireup="True" Inherits="umbraco.dialogs.insertFormField" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>umbraco :: <%=umbraco.ui.Text("buttons", "formFieldInsert", this.getUser())%> &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;</title>
		<meta name="GENERATOR" Content="Microsoft Visual Studio .NET 7.1">
		<meta name="CODE_LANGUAGE" Content="C#">
		<meta name="vs_defaultClientScript" content="JavaScript">
		<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
		<LINK href="../css/umbracoGui.css" type="text/css" rel="stylesheet">
		<style>body {margin: 2px;}</style>
<SCRIPT LANGUAGE="JavaScript">
<!--

function insertFormField() {
	var formTypes = document.forms[0].formType
	for (var i=0; i<formTypes.length; i++) {
		if (formTypes[i].checked)
			var formType = formTypes[i].value;
	}
	
	var hiddenFieldAdd = "style=\"width: 0px; height: 0px\"";
	var formNavn = document.forms[0].formNavn.value;
	var formSize = document.forms[0].formSize.value;
	var formHeight = document.forms[0].formHeight.value;
	var formValue = document.forms[0].formValue.value;
	var formRedirect = document.forms[0].formRedirect.value;
	
	if (formType == 'textarea') 
		var formElement = '<textarea name="umbForm' + formNavn + '" rows="' + formHeight + '" cols="' + formSize + '"></textarea>';
	else {
		if (formType == 'submit') 
			var formElement = '<input type="hidden" ' + hiddenFieldAdd + ' name="umbracoAction" value="mailForm"><input type="hidden" ' + hiddenFieldAdd + ' name="umbracoRedirect" value="' + formRedirect + '"><input type="hidden" ' + hiddenFieldAdd + ' name="sendTo" value="' + document.forms[0].formSendTo.value + '"><input type="' + formType + '" name="sbmt" size="' + formSize + '" value="' + formValue + '">';
		else
			var formElement = '<input type="' + formType + '" name="umbForm' + formNavn + '" size="' + formSize + '" value="' + formValue + '">';
	}
	
	window.returnValue = formElement;
	window.close();
}


function formatForm(formType) {
	formEnable('formNavn');
	formDisable('formRedirect');

	if (formType == 'radio' || formType == 'checkbox') {
		formDisable('formSize');
		formDisable('formHeight');
		formDisable('formSendTo');
	}
	if (formType == 'text') {
		formEnable('formSize');
		formDisable('formHeight');
		formDisable('formSendTo');
	}
	if (formType == 'textarea') {
		formEnable('formSize');
		formEnable('formHeight');
		formDisable('formSendTo');
	}
	if (formType == 'submit') {
		formDisable('formNavn');
		formDisable('formSize');
		formDisable('formHeight');
		formEnable('formSendTo');
		formEnable('formRedirect');
	}
	
}

function formDisable(element) {
	document.getElementById(element+ "Label").className = 'guiDialogDisabled';
	document.forms[0][element].className = 'guiInputDisabled';
	document.forms[0][element].disabled = true;
}

function formEnable(element) {
	document.getElementById(element+ "Label").className = 'guiDialogNormal';
	document.forms[0][element].className = 'guiInputText';
	document.forms[0][element].disabled = false;
}
function doSubmit() {insertFormField();}

	var functionsFrame = this;
	var tabFrame = parent.parent.right;
	var isDialog = true;
	var submitOnEnter = true;
-->
	</script>
	<script type="text/javascript" src="js/umbracoCheckKeys.js"></script>
		<script language="javascript">
			function dialogHandler(id) {
				document.forms[0].linkText.value = id;
			}

		</script>
	</HEAD>
	<body MS_POSITIONING="GridLayout">
			
		<h3><%=umbraco.ui.Text("buttons", "formFieldInsert", this.getUser())%></h3>
		<form id="Form1" method="post" runat="server">
			<TABLE class="propertyPane" id="macroProperties" cellSpacing="0" cellPadding="4" width="98%"
				border="0" runat="server">
				<TBODY>
					<TR>
						<TD class="propertyHeader" width="30%"><%=umbraco.ui.Text("type", this.getUser())%></TD>
						<TD class="propertyContent">
<input type="radio" name="formType" value="submit" onClick="formatForm(this.value)"> <img src="../images/editor/formButton.gif"> 
<input type="radio" name="formType" value="checkbox" onClick="formatForm(this.value)"> <img src="../images/editor/formCheckbox.gif"> 
<input type="radio" name="formType" value="radio" onClick="formatForm(this.value)"> <img src="../images/editor/formRadio.gif"> 
<input type="radio" name="formType" value="select" onClick="formatForm(this.value)"> <img src="../images/editor/formSelect.gif"> 
<input type="radio" name="formType" value="text" onClick="formatForm(this.value)"> <img src="../images/editor/formText.gif"> 
<input type="radio" name="formType" value="textarea" onClick="formatForm(this.value)"> <img src="../images/editor/formTextarea.gif"><br />
</td>
					</TR>
					<TR>
						<TD class="propertyHeader" width="30%"><%=umbraco.ui.Text("name", this.getUser())%>
						</TD>
						<TD class="propertyContent" id="formNavnLabel">
		                <input type="text" name="formNavn" value="<%=umbraco.helper.Request("formNavn")%>" size="40" class="guiInputText"><br />
						</TD>
					</TR>
					<TR>
						<TD class="propertyHeader" id="formSizeLabel" width="30%"><%=umbraco.ui.Text("width", this.getUser())%></TD>
						<TD class="propertyContent">
		                <input type="text" name="formSize" value="<%=umbraco.helper.Request("formSize")%>" size="6" class="guiInputText" disabled><br />
						</TD>
					</TR>
					<TR>
						<TD class="propertyHeader" id="formHeightLabel" width="30%"><%=umbraco.ui.Text("height", this.getUser())%></TD>
						<TD class="propertyContent">
		                <input type="text" name="formHeight" value="<%=umbraco.helper.Request("formHeight")%>" size="6" class="guiInputText" disabled><br />
						</TD>
					</TR>
					<TR>
						<TD class="propertyHeader" id="formValueLabel" width="30%"><%=umbraco.ui.Text("value", this.getUser())%></TD>
						<TD class="propertyContent">
		                <input type="text" name="formValue" value="<%=umbraco.helper.Request("formValue")%>" size="20" class="guiInputText"><br />
						</TD>
					</TR>
					<TR>
						<TD class="propertyHeader" id="formSendToLabel" width="30%"><%=umbraco.ui.Text("reciept", this.getUser())%></TD>
						<TD class="propertyContent">
		                <input type="text" name="formSendTo" value="<%=umbraco.helper.Request("formSendTo")%>" size="20" class="guiInputText" disabled><br />
						</TD>
					</TR>
					<TR>
						<TD class="propertyHeader" id="formRedirectLabel" width="30%"><%=umbraco.ui.Text("showPageOnSend", this.getUser())%></TD>
						<TD class="propertyContent">
		                <input type="text" name="formRedirect" value="<%=umbraco.helper.Request("formRedirect")%>" size="20" class="guiInputText" disabled><br />
						</TD>
					</TR>
					<TR>
						<TD class="propertyHeader" width="30%"></TD>
						<TD class="propertyContent">
            <input type="button" value=" <%=umbraco.ui.Text("insert")%> " class="inpButton" onClick="insertFormField();"><br />
						</TD>
					</TR>
				</tbody>
			</TABLE>
		</form>
	</body>
</HTML>

