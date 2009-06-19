var ultraSimpleMailerTotalMails;

function ultraSimpleMailer_doSend(id) {
	document.getElementById(id + "_doTest").value = "";
	document.getElementById(id + "_doSend").value = "true";
	document.forms[0].submit();
}

function ultraSimpleMailer_doSendTest(id) {
	document.getElementById(id + "_doTest").value = "true";
	document.getElementById(id + "_doSend").value = "";
	document.forms[0].submit();
}

function ultraSimpleMailerDoSend() {
	ultraSimpleMailerShowProgressBar();
	document.forms[0].submit();
}

function ultraSimpleMailerShowProgressBar() {
	setTimeout("ultraSimpleMailerShowStatus()", 100);
	setTimeout("ultraSimpleMailerUpdateStatus()", 200);
}


function ultraSimpleMailerShowStatus() {
	document.getElementById('ultraSimpleMailerFormDiv').style.display = 'none'; 
	document.getElementById('ultraSimpleMailerAnimDiv').style.display = 'block'; 
	document.getElementById('ultraSimpleMailerAnim').src = 'images/anims/publishPages.gif';
}

function ultraSimpleMailerUpdateStatus() {
  umbraco.presentation.webservices.legacyAjaxCalls.ProgressStatus(ultraSimpleMailerId + 'Done', ultraSimpleMailerUpdateStatusDo);
}


function ultraSimpleMailerUpdateStatusDo(retVal) {

	progressBarUpdateLabel('ultraSimpleMailerUpgradeStatus', "<b>" + retVal + " of " + ultraSimpleMailerTotalMails + "</b>");

	// progressbar
	retVal = parseInt(retVal);
	if (retVal > 0) {
		var percent = Math.round((retVal/ultraSimpleMailerTotalMails)*100);
		progressBarUpdate('ultraSimpleMailerUpgradeStatus', percent);
	}
	setTimeout("ultraSimpleMailerUpdateStatus()", 500);
}