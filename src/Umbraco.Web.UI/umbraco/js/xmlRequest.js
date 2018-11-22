var requestRunning = false;
var xmlHttp = null;
var xmlHttpDebug = false;

// Inspired by great work of Webfx in xloadtree
function umbracoStartXmlRequest(scriptUrl, postData, eventFunction) {

	// random hack for ie7
	day = new Date();
	z = day.getTime();
	y = (z - (parseInt(z/1000,10) * 1000))/10;
	scriptUrl += "&xmlRnd=" + y;

	if (xmlHttpDebug)
		alert(scriptUrl)		
		
	this.requestRunning = true;
	this.xmlHttpObject = XmlHttp.create();
	if (postData != "") {
		if (document.all) {
			this.xmlHttpObject.open("POST", scriptUrl, false);
			this.xmlHttpObject.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
		}
		else {
			eval(eventFunction);
		}
	} else 
		this.xmlHttpObject.open("GET", scriptUrl, true);
	
	
	this.xmlHttpObject.onreadystatechange = function () {
		if (xmlHttp.readyState == 4) {
		    // Removed the this from this.requestRunning = false; this was causing a bug in the find search box in cms backend.
			requestRunning = false;
			// debug
			if (xmlHttpDebug)
				alert(xmlHttp.responseText)
			eval(eventFunction);
			xmlHttp = null;
		}
	};
	
	// call in new thread to allow ui to update
	window.setTimeout(function () {
		this.xmlHttpObject.send(postData);
	}, 10);
	
	xmlHttp = this.xmlHttpObject;
	return this;
}

umbracoStartXmlRequest.prototype.ResultText = 
umbracoStartXmlRequest.prototype.ResultText = function () {
	return this.xmlHttpObject.responseText;
}

umbracoStartXmlRequest.prototype.ResultXml = 
umbracoStartXmlRequest.prototype.ResultXml = function () {
	return this.xmlHttpObject.responseXML;
}

function umbracoXmlRequestResult() {
	if (!requestRunning)
		return xmlHttp.responseXML
}

function umbracoXmlRequestResultTxt() {
	if (!requestRunning)
		return xmlHttp.responseText
}

function xmlReturnRandom() {
	day = new Date()
	z = day.getTime()
	y = (z - (parseInt(z/1000,10) * 1000))/10
	return y
}