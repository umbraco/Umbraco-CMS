//**
// * POPUP WINDOW CODE v1.3
// * Used for displaying DHTML only popups instead of using buggy modal windows.
// *
// * By Seth Banks (webmaster at subimage dot com)
// * http://www.subimage.com/
// *
// * Contributions by:
// * 	Eric Angel - tab index code
// * 	Scott - hiding/showing selects for IE users
// *	Todd Huss - inserting modal dynamically and anchor classes
// *
// * Up to date code can be found at http://www.subimage.com/dhtml/subModal
// * 
// *
// * This code is free for you to use anywhere, just keep this comment block.
// */

// Popup code
var gPopupMask = null;
var gPopupContainer = null;
var gPopFrame = null;
var gReturnFunc;
var gPopupIsShown = false;

var gHideSelects = false;


var gTabIndexes = new Array();
// Pre-defined list of tags we want to disable/enable tabbing into
var gTabbableTags = new Array("A","BUTTON","TEXTAREA","INPUT","IFRAME");	

// If using Mozilla or Firefox, use Tab-key trap.
if (!document.all) {
	document.onkeypress = keyDownHandler;
}



//**
// * Initializes popup code on load.	
// */
function initPopUp() {
	// Add the HTML to the body
	theBody = document.getElementsByTagName('BODY')[0];
	
	popmask = document.createElement('div');
	popmask.id = 'popupMask';
	
	popcont = document.createElement('div');
	popcont.id = 'popupContainer';
	popcont.innerHTML = '' +
		'<div id="popupInner">' +
			'<div id="popupTitleBar">' +
				'<div id="popupTitle"></div>' +
				'<div id="popupControls">' +
					'<img src="images/close.png" onclick="hidePopWin(false);" />' +
				'</div>' +
			'</div>' +
			'<iframe src="" style="background-color:transparent;" scrolling="auto" frameborder="0" allowtransparency="true" id="popupFrame" name="popupFrame" width="100%" height="100%"></iframe>' +
		'</div>';
	theBody.appendChild(popmask);
	theBody.appendChild(popcont);
	
	gPopupMask = document.getElementById("popupMask");
	gPopupContainer = document.getElementById("popupContainer");
	gPopFrame = document.getElementById("popupFrame");	
	
	// check to see if this is IE version 6 or lower. hide select boxes if so
	// maybe they'll fix this in version 7?
	var brsVersion = parseInt(window.navigator.appVersion.charAt(0), 10);
	if (brsVersion <= 6 && window.navigator.userAgent.indexOf("MSIE") > -1) {
		gHideSelects = true;
	}
	
	
	
}
addEvent(window, "load", initPopUp);


// /**
//	* @argument width - int in pixels
//	* @argument height - int in pixels
//	* @argument url - url to display
//	* @argument returnFunc - function to call when returning true from the window.
//	*/

function showPopWin(url, width, height, returnFunc) {
	gPopupContainer.style.width = width + "px"; 
    gPopupContainer.style.height = height + "px";
        
	gPopupIsShown = true;
	disableTabIndexes();
	gPopupMask.style.display = "block";
	gPopupContainer.style.display = "block";
	// calculate where to place the window on screen
	centerPopWin(width, height);
	
	var titleBarHeight = parseInt(document.getElementById("popupTitleBar").offsetHeight, 10);

	setMaskSize();

	// need to set the width of the iframe to the title bar width because of the dropshadow
	// some oddness was occuring and causing the frame to poke outside the border in IE6
	//gPopFrame.style.width = parseInt(document.getElementById("popupTitleBar").offsetWidth, 10) + "px";
	gPopFrame.style.width = (width) + "px";
	gPopFrame.style.height = (height) + "px";
	
	// set the url
	gPopFrame.src = url;
	
	gReturnFunc = returnFunc;
	// for IE
	if (gHideSelects == true) {
		hideSelectBoxes();
	}
	
	gPopupMask.onclick = function(){hidePopWin(false);};
	
	window.setTimeout("setPopTitle();", 600);
}

//
var gi = 0;
function centerPopWin(width, height) {
	if (gPopupIsShown == true) {
		if (width == null || isNaN(width)) {
			width = gPopupContainer.offsetWidth;
		}
		if (height == null) {
			height = gPopupContainer.offsetHeight;
		}
		
		
		//var theBody = document.documentElement;
		var theBody = document.getElementsByTagName("BODY")[0];
		theBody.style.overflow = "hidden";
		
		var scTop = parseInt(theBody.scrollTop,10);
		var scLeft = parseInt(theBody.scrollLeft,10);
		
		gPopupMask.style.top = scTop + "px";
		gPopupMask.style.left = scLeft + "px";
	
		setMaskSize();
		
		//window.status = gPopupMask.style.top + " " + gPopupMask.style.left + " " + gi++;
		
		var titleBarHeight = parseInt(document.getElementById("popupTitleBar").offsetHeight, 10);
		
		var fullHeight = getViewportHeight();
		var fullWidth = getViewportWidth();
		
		gPopupContainer.style.top = (scTop + ((fullHeight - (height+titleBarHeight)) / 2)) + "px";
		gPopupContainer.style.left =  (scLeft + ((fullWidth - width) / 2)) + "px";
		//alert(fullWidth + " " + width + " " + gPopupContainer.style.left);
	}
}

addEvent(window, "resize", centerPopWin);
//addEvent(window, "scroll", centerPopWin);
window.onscroll = centerPopWin;

function setMaskSize() {
	var theBody = document.getElementsByTagName("BODY")[0];
			
	var fullHeight = getViewportHeight();
	var fullWidth = getViewportWidth();
	
	// Determine what's bigger, scrollHeight or fullHeight / width
	if (fullHeight > theBody.scrollHeight) {
		popHeight = fullHeight;
	} else {
		popHeight = theBody.scrollHeight
	}
	
	gPopupMask.style.height = popHeight + "px";
	gPopupMask.style.width = theBody.scrollWidth + "px";
}

// /**
// * @argument callReturnFunc - bool - determines if we call the return function specified
// * @argument returnVal - anything - return value 
// */
function hidePopWin(callReturnFunc, returnVal) {
    //alert(callReturnFunc);
    gPopupIsShown = false;
    restoreTabIndexes();
    if (gPopupMask == null) {
        return;
    }
    gPopupMask.style.display = "none";
    gPopupContainer.style.display = "none";
    if (callReturnFunc == true && gReturnFunc != null) {
        // edited by CDM -- gReturnFunc(window.frames["popupFrame"].returnVal);
        gReturnFunc( returnVal );
    }
    gPopFrame.src = "";
    // display all select boxes
    if (gHideSelects == true) {
        displaySelectBoxes();
    }

    var theBody = document.getElementsByTagName("BODY")[0];
    theBody.style.overflow = "";
}

// /**
// * Sets the popup title based on the title of the html document it contains.
// * Uses a timeout to keep checking until the title is valid.
// */
function setPopTitle() {
	return;
	if (window.frames["popupFrame"].document.title == null) {
		window.setTimeout("setPopTitle();", 10);
	} else {
		document.getElementById("popupTitle").innerHTML = window.frames["popupFrame"].document.title;
	}
}

// Tab key trap. iff popup is shown and key was [TAB], suppress it.
// @argument e - event - keyboard event that caused this function to be called.
function keyDownHandler(e) {
    if (gPopupIsShown && e.keyCode == 9)  return false;
}

// For IE.  Go through predefined tags and disable tabbing into them.
function disableTabIndexes() {
	if (document.all) {
		var i = 0;
		for (var j = 0; j < gTabbableTags.length; j++) {
			var tagElements = document.getElementsByTagName(gTabbableTags[j]);
			for (var k = 0 ; k < tagElements.length; k++) {
				gTabIndexes[i] = tagElements[k].tabIndex;
				tagElements[k].tabIndex="-1";
				i++;
			}
		}
	}
}

// For IE. Restore tab-indexes.
function restoreTabIndexes() {
	if (document.all) {
		var i = 0;
		for (var j = 0; j < gTabbableTags.length; j++) {
			var tagElements = document.getElementsByTagName(gTabbableTags[j]);
			for (var k = 0 ; k < tagElements.length; k++) {
				tagElements[k].tabIndex = gTabIndexes[i];
				tagElements[k].tabEnabled = true;
				i++;
			}
		}
	}
}


// /**
// * Hides all drop down form select boxes on the screen so they do not appear above the mask layer.
// * IE has a problem with wanted select form tags to always be the topmost z-index or layer
// *
// * Thanks for the code Scott!
// */
function hideSelectBoxes() {
	for(var i = 0; i < document.forms.length; i++) {
		for(var e = 0; e < document.forms[i].length; e++){
			if(document.forms[i].elements[e].tagName == "SELECT") {
				document.forms[i].elements[e].style.visibility="hidden";
			}
		}
	}
}

// /**
// * Makes all drop down form select boxes on the screen visible so they do not reappear after the dialog is closed.
// * IE has a problem with wanted select form tags to always be the topmost z-index or layer
// */
function displaySelectBoxes() {
	for(var i = 0; i < document.forms.length; i++) {
		for(var e = 0; e < document.forms[i].length; e++){
			if(document.forms[i].elements[e].tagName == "SELECT") {
			document.forms[i].elements[e].style.visibility="visible";
			}
		}
	}
}
