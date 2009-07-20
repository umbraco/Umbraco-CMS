var isPasting = 0;
var isResizing = false;
var resizeEl;
var resizePosX, resizePosY

function openDialog(diaTitle, diaDoc, dwidth, dheight)
{
  window.status = "";
  strFeatures = "dialogWidth=" + dwidth + "px;dialogHeight=" + dheight + "px;scrollbars=no;"
              + "center=yes;border=thin;help=no;status=no"
  strTitle = window.showModalDialog(diaDoc, "MyDialog", strFeatures);
  return strTitle;

}

function handleBeforePaste( ) {
    event.returnValue = false;
}

function handlePaste() {
	document.selection.clear();
	event.returnValue = false;
	setTimeout("doPaste();", 5);
}

function doPaste() {
	var pasteContent=document.body.createTextRange();
	pasteContent.moveToElementText(document.getElementById("pasteHolder"));
	pasteContent.execCommand('Paste');
	pasteContent = document.getElementById("pasteHolder").innerHTML;
	if (pasteContent.indexOf('<FONT') > 0 || pasteContent.indexOf('MsoNormal') > 0 || pasteContent.indexOf('mso-') > 0 ) {
		action = openDialog("paste", "dialogs/paste.aspx", 520, 400);
		if (action == "removeSpecial")
			doPasteRemoveSpecial(pasteContent);
		else if (action == "removeAll")
			doPasteText(pasteContent)
		else
			copyPasteContent(pasteContent);
	}
	else
		copyPasteContent(pasteContent);
		
}

function doPasteRemoveSpecial(pasteContent) {
	pasteContent=pasteContent.replace(/<\/?SPAN[^>]*>/gi,"");
	pasteContent=pasteContent.replace(/<(\w[^>]*) class=([^ |>]*)([^>]*)/gi,"<$1$3");
	pasteContent=pasteContent.replace(/<(\w[^>]*) style="([^"]*)"([^>]*)/gi,"<$1$3");
	pasteContent=pasteContent.replace(/<(\w[^>]*) lang=([^ |>]*)([^>]*)/gi,"<$1$3");
	pasteContent=pasteContent.replace(/<\\?\?xml[^>]*>/gi,"");
	pasteContent=pasteContent.replace(/<\/?\w+:[^>]*>/gi,"");
	pasteContent=pasteContent.replace(/&nbsp;/," ");
	copyPasteContent(pasteContent)
}

function doPasteText(pasteContent) {
	copyPasteContent(clipboardData.getData("Text").replace(/\n/g,'<BR/>'));
}

function copyPasteContent(pasteContent) {
	      var sel = document.selection;
	      if (sel!=null) {
	        var rng = sel.createRange();
	    	if (rng!=null) {
    			rng.pasteHTML (pasteContent)	
	    	}
	      }
}

function umbracoImageResizeUpdateSize(e) {
	if (isResizing) {
		newWidth = (resizeEl.width < Math.abs(event.clientX-resizePosX)) ? 0 : resizeEl.width + (event.clientX-resizePosX)
		newHeight = (resizeEl.height < Math.abs(event.clientY-resizePosY)) ? 0 : resizeEl.height + (event.clientY-resizePosY)
		
		if (newWidth > resizeEl.umbracoOrgWidth || newHeight > resizeEl.umbracoOrgHeight)
			top.defaultStatus = 'Width: ' + newWidth + ', Height: ' + newHeight + '. ' + parent.parent.uiKeys['errors_pictureResizeBiggerThanOrg'];
		else
			top.defaultStatus = 'Width: ' + newWidth + ', Height: ' + newHeight;
	}
}

function umbracoImageResizeStart(el) {
	el.umbracoBeforeResizeWidth = el.width; 
	el.umbracoBeforeResizeHeight = el.height;
	isResizing = true;
	resizeEl = el;
	resizePosX = event.clientX
	resizePosY = event.clientY
	window.onmousemove = umbracoImageResizeUpdateSize();
}

function umbracoImageResize(el) {

	isResizing = false;
	document.onmousemove = "";

	var width = el.width;
	var height = el.height;
	var orgWidth = el.umbracoOrgWidth;
	var orgHeight = el.umbracoOrgHeight;
	var beforeResizeWidth = el.umbracoBeforeResizeWidth;
	var beforeResizeHeight = el.umbracoBeforeResizeHeight;

	var checkResize = true;
	var newHeight;
	var newWidth;

	if (width > orgWidth || height > orgHeight) 
		if (!confirm(parent.parent.uiKeys['errors_pictureResizeBiggerThanOrg'])) {
			newWidth = beforeResizeWidth;
			newHeight = beforeResizeHeight;
			checkResize = false;
		}

	if (checkResize) {		
		// Vi skal her ændre størrelse så det passer i forhold til ratio
		if (orgWidth > orgHeight)
			orgRatio = parseFloat(orgHeight/orgWidth).toFixed(2)
		else
			orgRatio = parseFloat(orgWidth/orgHeight).toFixed(2)
		
		// Vi skal bestemme om det er den nye højde eller bredde som skal være bestemmende for ny størrelse
		if (Math.abs(beforeResizeWidth-width) > Math.abs(beforeResizeHeight-height)) {
			newWidth = width;
			newHeight = Math.round(width * parseFloat(orgHeight/orgWidth).toFixed(2));
		} else {
			newWidth = Math.round(height * parseFloat(orgWidth/orgHeight).toFixed(2));
			newHeight = height;
		}
	}		
		
	el.removeAttribute("style");
	el.removeAttribute("width");
	el.removeAttribute("height");
	el.setAttribute("width", newWidth);
	el.setAttribute("height", newHeight);
	
	// Hvis der er et originalt filnavn, skal vi bruge det
	if (el.umbracoOrgFileName) {
		el.setAttribute("src", el.umbracoOrgFileName);
	}	

	// Hvis størrelsen er ændret fra originalen, skal umbraco generere et nyt billede!	
	if (newWidth != orgWidth || newHeight != orgHeight) {
		el.setAttribute("umbracoResizeImage", newWidth + "," + newHeight);
	}
}

function cleanHTML() {
	// Vi tjekker om det er dirty html der er sat ind
	if (document.body.innerHTML.indexOf('MsoNormal') > 0 || document.body.innerHTML.indexOf('mso-') > 0 ) {
   	for (var i = 0; i < document.all.length; i++) {
		document.all[i].removeAttribute("className",0);
		document.all[i].removeAttribute("style",0);
	}

	var tempContent = document.body.innerHTML
    tempContent = tempContent.replace(/<\?xml:.*?\/>/g, "");
    tempContent = tempContent.replace(/<o:p>&nbsp;<\/o:p>/g, "");
    tempContent = tempContent.replace(/o:/g, "");
    tempContent = tempContent.replace(/<st1:.*?>/g, "");
    tempContent = tempContent.replace(/<\/FONT>/g, "");
    tempContent = tempContent.replace(/<FONT\/?.*?>/g,"");
    tempContent = tempContent.replace(/<\/SPAN>/g, "");
    tempContent = tempContent.replace(/<SPAN\/?.*?>/g,"");
    tempContent = tempContent.replace(/<p><\/p>/gi, "");
   	document.body.innerHTML = tempContent;
	}
}



///////////////////////////////////////////////////////
// Context menu function
///////////////////////////////////////////////////////

/*  ___________________________________________________
	umbracoReturnElementTag(element, tagName)
	---------------------------------------------------
	Funktion som modtager et element og looper igennem
	dets forælder noder indtil tag'et som er bestemt i
	variablen "tagName" er fundet.
    ___________________________________________________
*/
function umbracoReturnElementTag(element, tagName) {
	// vi looper indtil vi finder tag'et
	while ( element.tagName != tagName && element.tagName != "BODY") {
		element = element.parentNode;
	}
	return element
}

/*  ___________________________________________________
	umbracoReturnAttributeNode(el, attributeName)
	---------------------------------------------------
	Returnerer en attributes værdi, hvis den eksisterer
    ___________________________________________________
*/
function umbracoReturnAttributeNode(el, attributeName) {
	if (el.getAttributeNode(attributeName))
		return el.getAttributeNode(attributeName).value;
	else
		return '';
}


/*  _________________________________________________________
	umbracoUpdateAttribute(el, attributeName, attributeValue)
	---------------------------------------------------------
	Opdaterer attributen "attributeName" i elementet "el",
	hvis der er sat en værdi i "attributeValue", ellers
	fjernes attributen.
    _________________________________________________________
*/
function umbracoUpdateAttribute(el, attributeName, attributeValue) {
	if (attributeValue != '\'\'' && attributeValue != '')
		el.setAttribute(attributeName, attributeValue)
	else
		el.removeAttribute(attributeName)
}


function umbracoIsArray() {

if (typeof arguments[0] == 'object') {
  var criterion =
    arguments[0].constructor.toString().match(/array/i);
   return (criterion != null);
  }
return false;
} 
/*  ___________________________________________________
	function umbracoEditIMG(el)
	---------------------------------------------------
	Funktion til at redigere et billede
    ___________________________________________________
*/
function umbracoEditImg(el) {
	alert("Endnu ikke klar :o)");
}


/*  ___________________________________________________
	function umbracoEditClass(el)
	---------------------------------------------------
	Funktion til at redigere style
    ___________________________________________________
*/
function umbracoEditClass(el) {
	alert("Endnu ikke klar :o)");
}


/*  ___________________________________________________
	function umbracoEditTable(el)
	---------------------------------------------------
	Funktion til at redigere tabel
    ___________________________________________________
*/
function umbracoEditTable(el) {
	var tempEL = umbracoReturnElementTag(el, "TABLE");
	
	// Indsaml properties i tabel
	var tableAlign = 
		umbracoReturnAttributeNode(tempEL, "align");
	var tableWidth = 
		umbracoReturnAttributeNode(tempEL, "width");
	var tableHeight = 
		umbracoReturnAttributeNode(tempEL, "height");
	var tablePadding = 
		umbracoReturnAttributeNode(tempEL, "cellPadding");
	var tableSpacing =
		umbracoReturnAttributeNode(tempEL, "cellSpacing");
	var tableBorder = 
		umbracoReturnAttributeNode(tempEL, "border");
	var tableClass = 
		umbracoReturnAttributeNode(tempEL, "class");
		
	var tableEditProp = parent.openDialog("editTable", 	"dialogs/insertTable.aspx?align=" + escape(tableAlign) +
													"&width=" + escape(tableWidth) +
													"&height=" + escape(tableHeight) +
													"&padding=" + escape(tablePadding) +
													"&spacing=" + escape(tableSpacing) +
													"&border=" + escape(tableBorder) +
													"&class=" + escape(tableClass) +
													"&edit=true", 470, 520);

	umbracoUpdateAttribute(tempEL, "align", tableEditProp[0]);
	umbracoUpdateAttribute(tempEL, "width", tableEditProp[1]);
	umbracoUpdateAttribute(tempEL, "height", tableEditProp[2]);
	umbracoUpdateAttribute(tempEL, "cellPadding", tableEditProp[3]);
	umbracoUpdateAttribute(tempEL, "cellSpacing", tableEditProp[4]);
	umbracoUpdateAttribute(tempEL, "border", tableEditProp[5]);
	umbracoUpdateAttribute(tempEL, "class", tableEditProp[6]);
		
}

/*  ___________________________________________________
	function umbracoAddRow(el)
	---------------------------------------------------
	Funktion til at indsætte en ny række i en tabel
    ___________________________________________________
*/
function umbracoAddRow(el) {
	var tempEL = umbracoReturnElementTag(el, "TABLE");
	var numberOfCols = 0;
	var maxNumberOfCols = 0;

	if (tempEL.childNodes.length < 2)
		tempEL = umbracoReturnElementTag(el, "TBODY");


	// Find antal kolonner i en række
	for (var i=0; i<tempEL.childNodes.length; i++) {
		
		for (var x=0; x<tempEL.childNodes[i].cells.length; x++)
			numberOfCols += parseInt(tempEL.childNodes[i].cells[x].getAttributeNode("colspan").value);
		
		if (numberOfCols > maxNumberOfCols) 
			maxNumberOfCols = numberOfCols
			
		numberOfCols = 0;
	}
	
	// Hvor skal vi indsætte rækken?
	while (el.tagName != "TR")
		el = el.parentElement;
	
	// Find ud af hvilken nummer række vi står i!
	var testTR = el
	var insertAtRow = 0;
	while (testTR.tagName == "TR") {
		testTR = testTR.previousSibling;
		
		if (!testTR)
			break;
			
		insertAtRow++;

		if (!testTR.previousSibling)
			break;
	}

	if (el.tagName == "TR") {
		newRow = el.parentElement.insertRow(insertAtRow);
		for (var i=0; i<maxNumberOfCols; i++) {
			newRow.insertCell(i);
		}
	}
	
}

/*  ___________________________________________________
	function umbracoDeleteRow(el)
	---------------------------------------------------
	Funktion til at slette en række i en tabel
    ___________________________________________________
*/
function umbracoDeleteRow(el) {
	var tempEL = umbracoReturnElementTag(el, "TABLE");

	if (tempEL.childNodes.length < 2)
		tempEL = umbracoReturnElementTag(el, "TBODY");


	// Hvor skal vi indsætte rækken?
	while (el.tagName != "TR")
		el = el.parentElement;
	
	// Find ud af hvilken nummer række vi står i!
	var testTR = el
	var deleteAtRow = 0;
	while (testTR.tagName == "TR") {
		testTR = testTR.previousSibling;
		
		if (!testTR)
			break;
			
		deleteAtRow++;

		if (!testTR.previousSibling)
			break;
	}

	if (el.tagName == "TR") {
		el.parentElement.deleteRow(deleteAtRow);
	}
	
}

/*  ___________________________________________________
	function umbracoDeleteCol(el)
	---------------------------------------------------
	Funktion til at slette en kolonne i en tabel
    ___________________________________________________
*/
function umbracoDeleteCol(el) {
	var tempEL = umbracoReturnElementTag(el, "TABLE");

	if (tempEL.childNodes.length < 2)
		tempEL = umbracoReturnElementTag(el, "TBODY");


	// Hvor skal vi slette kolonnen?
	while (el.tagName != "TD")
		el = el.parentElement;
	
	// Find ud af hvilken nummer kolonne vi står i!
	var testTD = el
	var deleteAtCol = 0;
	while (testTD.tagName == "TD") {
		testTD = testTD.previousSibling;
		
		if (!testTD)
			break;
			
		deleteAtCol++;

		if (!testTD.previousSibling)
			break;
	}


	// Loop igennem tabellen og slet kolonner
	for (var i=0; i<tempEL.childNodes.length; i++) {
		if (tempEL.childNodes[i].cells[deleteAtCol].getAttributeNode("colSpan").value > 1) {
			tempEL.childNodes[i].cells[deleteAtCol].setAttribute("colSpan", parseInt(tempEL.childNodes[i].cells[deleteAtCol].getAttributeNode("colSpan").value)-1);
		} else
			tempEL.childNodes[i].deleteCell(deleteAtCol);

	}
	
}
/*  ___________________________________________________
	function umbracoAddCol(el)
	---------------------------------------------------
	Funktion til at tilføje en kolonne i en tabel
    ___________________________________________________
*/
function umbracoAddCol(el) {
	var tempEL = umbracoReturnElementTag(el, "TABLE");

	if (tempEL.childNodes.length < 2)
		tempEL = umbracoReturnElementTag(el, "TBODY");


	// Hvor skal vi tilføje kolonnen?
	while (el.tagName != "TD")
		el = el.parentElement;
	
	// Find ud af hvilken nummer kolonne vi står i!
	var testTD = el
	var insertAtCol = 0;
	while (testTD.tagName == "TD") {
		testTD = testTD.previousSibling;
		
		if (!testTD)
			break;
			
		insertAtCol++;

		if (!testTD.previousSibling)
			break;
	}


	// Loop igennem tabellen og tilføj kolonner
	for (var i=0; i<tempEL.childNodes.length; i++) {
		var node = document.createElement("TD")
		tempEL.childNodes[i].insertBefore(node, tempEL.childNodes[i].childNodes[insertAtCol]);
	}
	
}


/*  ___________________________________________________
	function umbracoGlueCell(el)
	---------------------------------------------------
	Funktion til at opdele en celle
    ___________________________________________________
*/
function umbracoGlueCell(el) {
	var tempEL = umbracoReturnElementTag(el, "TD");
	
	if (tempEL.nextSibling) {
		var textFromOtherNode = tempEL.nextSibling.innerHTML;
		tempEL.parentElement.removeChild(tempEL.nextSibling);
		umbracoUpdateAttribute(tempEL, "colSpan", "2");
		tempEL.innerHTML = tempEL.innerHTML + textFromOtherNode;		
	}
	else
		alert(parent.uiKeys['tableColMergeLeft']);
}

/*  ___________________________________________________
	function umbracoSplitCell(el)
	---------------------------------------------------
	Funktion til at samle en celle
    ___________________________________________________
*/
function umbracoSplitCell(el) {
	var tempEL = umbracoReturnElementTag(el, "TD");
	var node = document.createElement("TD")
	if (parseInt(tempEL.getAttributeNode("colSpan").value) < 2)
		alert(parent.uiKeys['tableSplitNotSplittable']);
	else {
		umbracoUpdateAttribute(tempEL, "colSpan", "")
		tempEL.parentElement.insertBefore(node, tempEL);
	}
}
/*  ___________________________________________________
	function umbracoEditCell(el)
	---------------------------------------------------
	Funktion til at redigere en celle
    ___________________________________________________
*/
function umbracoEditCell(el) {
	var tempEl = umbracoReturnElementTag(el, "TD");
	
	if (el.getAttributeNode("width"))
		var cellWidth = el.getAttributeNode("width").value;
	else
		var cellWidth = '';
		
	if (!parseInt(cellWidth))
		cellWidth = '';
	
	if (el.getAttributeNode("align"))
		var cellAlign = el.getAttributeNode("align").value;
	else
		var cellAlign = '';
		
	if (el.getAttributeNode("vAlign"))
		var cellVAlign = el.getAttributeNode("vAlign").value;
	else
		var cellVAlign = '';
		
	var returnValues = parent.openDialog("editCell", 	"dialogs/editCell.aspx?width=" + cellWidth + 
														"&align=" + cellAlign +
														"&valign=" + cellVAlign, 480, 340);
	returnValues = returnValues.split(",");
	
	if (returnValues.length > 1) {
		
		var newCellWidth = returnValues[0];
		var newCellAlign = returnValues[1].replace(/\'/g,'');
		var newCellVAlign = returnValues[2].replace(/\'/g,'');
		var newCellColor = returnValues[3].replace(/\'/g,'');

		newCellAlign = newCellAlign.replace(/^\s+/g,'');
		newCellVAlign = newCellVAlign.replace(/^\s+/g,'');
		newCellColor = newCellColor.replace(/^\s+/g,'');
		
		if(newCellWidth) {
			if (parseInt(newCellWidth) > 0)
				tempEl.setAttribute("width", newCellWidth)
			else
				tempEl.removeAttribute("width")
		} else {
			if(newCellWidth == '')
				tempEl.removeAttribute("width");
		}

		if(newCellAlign) {
			if (newCellAlign != '\'\'' && newCellAlign != '')
				tempEl.setAttribute("align", newCellAlign)
			else
				tempEl.removeAttribute("align")
		} else {
			if(newCellAlign == '')
				tempEl.removeAttribute("align");
		}

		if(newCellVAlign) {
			if (newCellVAlign != '\'\'' && newCellVAlign != '')
				tempEl.setAttribute("vAlign", newCellVAlign)
			else
				tempEl.removeAttribute("vAlign")
		} else {
			if(newCellVAlign == '')
				tempEl.removeAttribute("vAlign");
		}

		if(newCellColor) {
			if (newCellColor!= '\'\'' && newCellColor != '') {
				el.style.cssText = 'background-color: #' + newCellColor;
			}else
				tempEl.removeAttribute("Style")
		} else {
			if(newCellColor == '')
				tempEl.removeAttribute("Style");
		}

	}
}


/*  ___________________________________________________
	function umbracoAlign(el, alignMode)
	---------------------------------------------------
	Funktion til at sætte alignment på et element via
	variablen alignMode
    ___________________________________________________
*/
function umbracoAlign(el, alignMode) {
	var tempEl;
	
	// først vil vi finde et img
	tempEl = umbracoReturnElementTag(el, "IMG")
	if (tempEl.tagName != 'IMG')
		tempEl = umbracoReturnElementTag(el, "P")
	if (tempEl.tagName != 'IMG' && tempEl.tagName != 'P')
		tempEl = umbracoReturnElementTag(el, "TABLE")
	
	el = tempEl;

	if (el.tagName == 'IMG' || el.tagName == 'P') {
		if (alignMode != '')
			el.setAttribute("align", alignMode);
		else
			el.removeAttribute("align");
	}
	
}


/*  ___________________________________________________
	function umbracoEditA(el)
	---------------------------------------------------
	Funktion til at redigere link. el er et element-
	objekt som er sendt via contextmenu
    ___________________________________________________
*/
function umbracoEditA(el) {
	
	el = umbracoReturnElementTag(el, "A")
	
	// vi henter attributter
	var currentLink = el.getAttributeNode("href").value;
	var currentLinkTarget = el.getAttributeNode("target").value;
	
	// åben vindue til at redigere link
	linkToInsert = "" + parent.openDialog("insertLink", "dialogs/insertlink.aspx?linkName=" + currentLink + "&linkNewWindow=" + currentLinkTarget, 440, 480);
	
	// Lav det returnerede til en streng
	linkToinsert = linkToInsert.toString();
	
	// vi tjekker om der er noget - hvis man lukker dialog, returneres der "undefined"
	if (linkToInsert != 'undefined') {
	
		// vi skal dele det returnerede op i target og link
		if (linkToInsert.substr(0,4) == 'true') {
			currentLink = linkToInsert.substr(linkToInsert.indexOf('|')+1, linkToInsert.length)
			currentLinkTarget = '_blank';
		} else {
			currentLink = linkToInsert.substr(linkToInsert.indexOf('|')+1, linkToInsert.length)
			currentLinkTarget = '';
		}
		
		// vi enten opdaterer eller fjerner link
		if (currentLink != '') {
			el.setAttribute("href", currentLink);
			el.setAttribute("target", currentLinkTarget);
		} else {
			el.removeNode(false);
		}
	}
}


/*  ___________________________________________________
	function umbracoEditMacro(el)
	---------------------------------------------------
	Funktion til at redigere macro. el er et element-
	objekt som er sendt via contextmenu
	Da en makro i editoren i virkeligheden blot er et
	billede (IMG-tag), findes de egentlige attributter
	på makroen i billedets ALT tag, så dette skal 
	findes, inden vi kalder selve makro-vinduet
    ___________________________________________________
*/
function umbracoEditMacro(el) {
	// da en makro i editoren er et IMG-tag, ønsker vi img-tag'et
//	el = umbracoReturnElementTag(el, "DIV")

	var attrString = "";
	var attrName = "";

	for (var i=0; i<el.attributes.length;i++) {
		attrName = el.attributes[i].nodeName.toLowerCase();
		if (el.attributes[i].nodeValue && (attrName != 'ismacro' && attrName != 'style' && attrName != 'contenteditable')) {
			attrString += el.attributes[i].nodeName + '=' + el.attributes[i].nodeValue.replace(/#/g, "%23").replace(/\</g, "%3C").replace(/\>/g, "%3E").replace(/\"/g, "%22") + '&';
		}
	}	
//	var attr = el.getAttributeNode("alt").value;
	
//	attr = attr.substr(attr.indexOf(" ")+1, attr.length);

/*

	// vi splitter attribute-stengen op
	var attrInside = false;
	var attrName = '';
	var attrValue = '';
	var attrString = '';
	for (var i=0;i<attr.length;i++) {
		
		if (attrInside) {
			if (attr.substr(i, 1) != '"') 
				attrValue += attr.substr(i, 1);
			else {
				attrInside = false;
				attrString += attrName + '=' + attrValue.replace(/#/g, "%23").replace(/\</g, "%3C").replace(/\>/g, "%3E").replace(/\"/g, "%22") + '&';

				attrName = '';
				attrValue = '';
				i++;
			}
		} else {
			if (attr.substr(i, 1) != '=') 
				attrName += attr.substr(i, 1);
			else {
				attrInside = true;
				i++;
			}
			
		}
	}
	*/
	
	// vi trunkerer strengen ved at fjerne et evt. overskydende amp;
	if (attrString.length > 0)
		attrString = attrString.substr(0, attrString.length-1);
		
	// vi opdaterer javascript variablen "macroEditElement" med vores element
	parent.macroEditElement = el;
	
	var	fieldTag = parent.nytVindue("dialogs/editMacro.aspx?mode=edit&" + attrString, 600, 350);

}