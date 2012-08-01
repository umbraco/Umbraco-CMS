var step = 8;
var padding = 10;

function progressBarUpdate(id, percent) {
	var total = document.getElementById("progressBar" + id).style.width;
	total = Math.round(total.substring(0, total.length-2))-padding;
	var onePercent = total / 100;
	var progress = Math.round(onePercent*percent);	
	if (progress % step == 0 || percent > 99) {
		document.getElementById("progressBar" + id + "_indicator").style.width = progress;
	}
}

function progressBarUpdateLabel(id, text) {
	document.getElementById("progressBar" + id + "_text").innerHTML = text;
}

function progressBarTest(id, percent) {
	progressBarUpdate(id, percent);
	progressBarUpdateLabel(id, percent + '%');
	if (percent < 100) {
		percent++;
		setTimeout("progressBarTest('" + id + "', " + percent + ")", 100);
	}
}