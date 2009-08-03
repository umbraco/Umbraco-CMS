
var templateEditing = false;
var macroEditElement = null;

function openDialog(diaTitle, diaDoc, dwidth, dheight)
{
  window.status = "";
  strFeatures = "dialogWidth=" + dwidth + "px;dialogHeight=" + dheight + "px;scrollbars=no;"
              + "center=yes;border=thin;help=no;status=no"
  strTitle = window.showModalDialog(diaDoc, "MyDialog", strFeatures);
  return strTitle;

}

function setRichTextObject(id) {
	currentRichTextDocument = document.frames[id + "_holder"].document;
	currentRichTextObject =  document.frames[id + "_holder"].document.getElementById("holder");
}

function umbracoEditorCommand(id,commandKey,option) 
  {   
    setRichTextObject(id);
    currentRichTextDocument = document.frames[id + "_holder"].document;
    currentRichTextDocument.execCommand(commandKey,true,option);
	currentRichTextDocument.focus();
  }
  
  function umbracoInsertField(theObject, tagUrl, tagName, tagText, width, height,path,move) {
  	var	fieldName = parent.parent.openDialog("umbracoField", path + tagUrl + ".aspx?tagName=" + tagName + "&tagText=" + tagText + "&move=" + move + "&objectId=" + theObject.id, width, height);

  }
  
  function umbracoInsertFieldDo(theObjectId, Text, move) {
 
  
	var theObject = document.getElementById(theObjectId);
	   
	if (Text) {
		if (move != 'undefined')
			insertAtCaretAndMove(theObject, Text, move);
		else
			insertAtCaret(theObject, Text)
	}
	
  }
  


///////////////////////////////////////////
// UMBRACO MAKRO-FUNKTIONER
///////////////////////////////////////////

  function umbracoInsertMacro(id, path) {
	setRichTextObject(id);
	macroEditElement = null;
  	if (document.selection.createRange().parentElement().tagName != 'BODY')
  		var	fieldTag = nytVindue(path+"/dialogs/editMacro.aspx?editor=true&umbPageId=" + umbPageId + "&umbVersionId=" + umbVersionId , 600, 350);
  }
  
  function umbracoTemplateInsertMacro() {
	var	fieldTag = nytVindue("../dialogs/editMacro.aspx", 600, 350);
  }
  
  function umbracoInsertMacroDo(fieldTag) {
  	fieldTag = fieldTag.replace(/\'/gi,"&#039;");
  	
	if (!templateEditing) {
		currentRichTextDocument.selection.createRange().pasteHTML('<img src="images/macro.gif" alt=\'' + fieldTag + '\'>'); 
	} else
		insertAtCaret(document.forms[0].TemplateBody, fieldTag + "</?UMBRACO_MACRO>")
  }

	function umbracoEditMacroDo(fieldTag, macroName, renderedContent) {
		if (macroEditElement != null) {
			macroEditElement.outerHTML = renderedContent;
		} else {
			currentRichTextDocument.selection.createRange().pasteHTML(renderedContent); 
		}
		macroEditElement = null;
	}

	function nytVindue(side, bredde, hoejde) {
		window.open(side, 'nytVindue', 'width=' + bredde + ',height=' + hoejde + ',scrollbars=yes');
	}
  

/////////////////////////////////////////////////////////
// BILLEDE FUNKTIONER
/////////////////////////////////////////////////////////

  function umbracoImage(id) {
	if (id != undefined)
		setRichTextObject(id)  
  	if (document.selection.createRange().parentElement().tagName != 'BODY') {
  
      var imageToInsert = "";
      imageToInsert = "" + openDialog("insertImage", "dialogs/insertimage.aspx", 560, 540);
      
      
      if (imageToInsert.toString() != 'undefined' && imageToInsert != '') {

			var imageName;
			var imageSource;
			var imageWidth;
			var imageHeight;
			var imageWidthHeightString = '';
			var orgWidth;
			var orgHeight;
			imageToInsert  = imageToInsert.split('|||');
			imageName = imageToInsert[0];
			imageSource = imageToInsert[1];
			imageWidth = imageToInsert[2];
			imageHeight = imageToInsert[3];
			imageTitle = imageToInsert[4];
			orgWidth = imageToInsert[5];
			orgHeight = imageToInsert[6];
			
			if (imageWidth != '' && imageWidth != '0' && imageWidth.toString() != 'undefined' && imageHeight != '' && imageHeight != '0' && imageHeight.toString() != 'undefined')
				imageWidthHeightString = ' onResize="umbracoImageResizeUpdateSize()" onResizeEnd="defaultStatus = \'\'; umbracoImageResize(this);" onresizestart="umbracoImageResizeStart(this);" width="' + imageWidth + '" height="' + imageHeight + '" umbracoOrgWidth="' + orgWidth + '" umbracoOrgHeight="' + orgHeight + '"'
						
		  document.selection.createRange().pasteHTML('<img src="'+ imageSource +'" title="' + imageTitle + '" alt="' + imageName + '"'+imageWidthHeightString+'>'); 
      }
     }
  }
  

/////////////////////////////////////////////////////////
// RELATIONER FUNKTIONER
/////////////////////////////////////////////////////////

	function doRelation() {
		var	fieldTag = nytVindue(umbracoConstGuiFolderName + "/umbracoRelation.aspx?table=dataStructure&id=" + umbracoQuerystring, 600, 350);
	}

/////////////////////////////////////////////////////////
// TABEL FUNKTIONER
/////////////////////////////////////////////////////////

  function umbracoInsertTable(id) {
      setRichTextObject(id)
      var tableToInsert = "";
      tableToInsert = "" + openDialog("insertTable", "dialogs/insertTable.aspx", 470, 560);
      
      if (tableToInsert.toString() != 'undefined' && tableToInsert != '') {
	      var sel = currentRichTextDocument.selection;
	      if (sel!=null) {
	        var rng = sel.createRange();
	    	if (rng!=null) {
	    		if (rng.text != '' && sel.type == 'Text')
	    			rng.pasteHTML (tableToInsert + rng.html)	
	    		else
	    			rng.pasteHTML (tableToInsert);
	    	}
	      }

      }
	  currentRichTextDocument.focus();
  }
  


/////////////////////////////////////////////////////////
// LINK FUNKTIONER
/////////////////////////////////////////////////////////

function umbracoAnchor(id) {
    setRichTextObject(id)
	insertAnchor = "" + openDialog("insertAnchor", "dialogs/insertAnchor.aspx", 390, 260);
	
	if (insertAnchor != '' && insertAnchor != 'undefined') {
		var sel = currentRichTextDocument.selection;
		if (sel!=null) {
		    var rng = sel.createRange();
    		if (rng!=null) {
				var theHTML = currentRichTextDocument.selection.createRange().htmlText;
				currentRichTextDocument.selection.createRange().pasteHTML('<a name=\"' + insertAnchor + '\">'+ theHTML +'</a>');    			
    		}
		}
	}
}

  function umbracoLink(id) {
      setRichTextObject(id)
      var linkToInsert = "";
      var sel = currentRichTextDocument.selection;
	  var tagContent = '';
      if (sel!=null) {
        var rng = sel.createRange();
    	if (rng!=null) {
			// test for links in range!
    		if (rng.text == '') {
    			var currentElement = rng.parentElement();
    			while (currentElement.tagName != 'A' && currentElement.parentNode.tagName != 'BODY') {
    				currentElement = currentElement.parentNode;
    			}
    			if (currentElement.tagName == 'A') {
    				// use editing method in richtextfunctions.js which is
    				// located in the iframe of the editing document
					document.frames[id + "_holder"].umbracoEditA(currentElement);
					return "";
				}
    		}
    		if (rng.text != '' && sel.type == 'Text') {
    			tagContent = rng.htmlText;
	  			linkToInsert = "" + openDialog("insertLink", "dialogs/insertlink.aspx", 440, 480);
	  			if (linkToInsert.toString() != 'undefined' && linkToInsert != '') {
	  				if (linkToInsert.substr(0,4) == 'true') {
		    				currentRichTextDocument.execCommand('CreateLink',false,formatLink(linkToInsert.substr(linkToInsert.indexOf('|')+1, linkToInsert.length)));
						if (rng.parentElement().tagName == "A"){
							rng.parentElement().target="_blank";
						}
	    				}
	    			else {
	    				currentRichTextDocument.execCommand('CreateLink',false,formatLink(linkToInsert.substr(linkToInsert.indexOf('|')+1, linkToInsert.length)));
	    			}
	    		}
        	} else {
        		if (sel.type == 'Control') {
        			var tag = rng.item(0);
        			var tagContent = '<' + tag.tagName;
        			var tagAttr = tag.attributes;
        			for (var i=0;i<tagAttr.length;i++) {
						
						if (tagAttr[i].nodeName == 'border') {
	        				tagContent += ' ' + tagAttr[i].nodeName + '="0"'
						} else {
							
	        				if (tagAttr[i].nodeValue != null && tagAttr[i].nodeValue != '' && tagAttr[i].nodeValue != 'inherit' && tagAttr[i].nodeName != 'start')
	        					tagContent += ' ' + tagAttr[i].nodeName + '="' + tagAttr[i].nodeValue + '"'
	        			}
        			}
        			
        			tagContent += '>';


		  			linkToInsert = "" + openDialog("insertLink", "dialogs/insertlink.aspx", 440, 480);
		  			if (linkToInsert.toString() != 'undefined' && linkToInsert != '') {
		  				if (linkToInsert.substr(0,4) == 'true')
		    				tagContent = '<a href="' + formatLink(linkToInsert.substr(linkToInsert.indexOf('|')+1, linkToInsert.length)) + '" target="_blank">' + tagContent + '</a>';
		    			else
		    				tagContent = '<a href="' + formatLink(linkToInsert.substr(linkToInsert.indexOf('|')+1, linkToInsert.length)) + '">' + tagContent + '</a>';
		    		}

					rng.item(0).outerHTML = tagContent;	
        		}
        			
        	}

        }
      }
	  
	  currentRichTextObject.focus();
  }
  
  function formatLink(link) {
	return link.replace(/\%3f/g,'\?');
  }
  
  function umbracoScriptlet() {
  	var	fieldName = parent.parent.openDialog("umbracoScriptlet", "settings/umbracoScriptlet.aspx", 500, 250);
	var fieldImg = "<img src=\"" + umbracoConstGuiFolderName + "/images/umbracoScriptlet.gif\" ALT=\"" + fieldName + "\" UMBRACO=\"umbracoScriptlet\">";
	var sel = currentRichTextDocument.selection;
	if (sel!=null) {
	    var rng = sel.createRange();
    	if (rng!=null && sel.type == 'Text')
        	rng.pasteHTML(fieldImg);
	}
  }

	function doSubmitAndPublish() {
		document.contentForm.doPublish.value = "true"; 
		doSubmit();
	}

/*	function doSubmit()
	{
		// Hvis der er tændt for styles, skal vi lige slukke for dem!
		if (typeof(currentRichTextObject) != 'undefined')
			if (currentRichTextObject)
				if (currentRichTextObject.innerHTML.indexOf('styleMarkStart.gif') > 0)
					umbracoShowStyles();

//		invokeSaveHandlers();
		document.contentForm.doSave.value = "true";
		document.contentForm.submit();
	}
*/
	function viewHTML(id)
	{
    setRichTextObject(id)
	window.open("viewHTML.aspx?rnd="+top.returnRandom(), 'nytVindue', 'width=700,height=500,scrollbars=auto');
	}

function umbracoInsertForm(id) {
    setRichTextObject(id)


	var	formField = openDialog("umbracoForm", "dialogs/insertFormField.aspx?rnd="+top.returnRandom(), 450, 480);
	if (formField) {
				
		currentRichTextDocument.selection.createRange().pasteHTML(formField);		
	}
}

function umbracoTextGen(id) {
    setRichTextObject(id)
	var	textGen = openDialog("umbracoTextGen", "dialogs/inserttextGen.aspx?rnd="+top.returnRandom(), 450, 330);
	if (textGen) 
		currentRichTextDocument.selection.createRange().pasteHTML(textGen.replace(/&amp;/g,"&"));
}

function addStyle(stylePickerID, id) {
	setRichTextObject(id)
	var styleToApply = stylePickerID[stylePickerID.selectedIndex].value;
	var sel = currentRichTextDocument.selection;
	if (sel!=null) {
		var rng = sel.createRange();
    	if (rng!=null) {
    		if (styleToApply != '') {
    				setClass(styleToApply);
    		}
    	}
	}
		stylePickerID.selectedIndex = 0;
//		currentRichTextDocument.all["theContent"].contentEditable = true;
		currentRichTextObject.focus();
	}
	
function setClass(theClass) {
	if (currentRichTextDocument.selection.type == 'Text') {
		var oSel = currentRichTextDocument.selection;
		var theHTML = oSel.createRange().htmlText;
		oSel.clear();
	    theHTML = theHTML.replace(/<span CLASS=\/?.*?>/gi,"");
	    theHTML = theHTML.replace(/<\/span>/gi,"");
	    theHTML = theHTML.replace(/<h\/?.*?>/gi,"");
	    theHTML = theHTML.replace(/<\/h\/?.*?>/gi,"");
	    theHTML = theHTML.replace(/<p>/gi,"");
	    theHTML = theHTML.replace(/<\/p>/gi,"");
	    if (theClass.indexOf(".") > -1)
	       theClass = '<span class=\"' + theClass.replace(/[.]/gi, "") + '\">'+ theHTML +'</span>';
	    else {
	       theClass = '<' + theClass + '>'+ theHTML +'</' + theClass + '>';
	    }
	    
	    currentRichTextDocument.selection.createRange().pasteHTML(theClass);
	} else {
		alert(parent.uiKeys['errors_stylesMustMarkBeforeSelect']);
	} 
}

function umbracoShowStyles(id) {

    setRichTextObject(id)
	var theHTML = currentRichTextObject.innerHTML;
	if (theHTML.indexOf('styleMarkStart.gif') > 0) {
		//document.all.showStyles.className = 'editorIcon';
		theHTML = theHTML.replace(/<img alt=\"Style: ([^"]+)\" \/?.*?>/gi,"<span class=$1>")
		theHTML = theHTML.replace(/<img alt=\"Formatering slut\" \/?.*?>/gi,"</span>")
	} else {
		if (theHTML.indexOf('<span') > -1 || theHTML.indexOf('<SPAN') > -1) {
			//document.all.showStyles.className = 'editorIconOn';
			theHTML = theHTML.replace(/<span CLASS=(\/?.*?)>/gi,"<img src=\"images/editor/styleMarkStart.gif\" alt=\"Style: $1\">")
			theHTML = theHTML.replace(/<span>/gi,"")
			theHTML = theHTML.replace(/<\/span>/gi,"<img src=\"images/editor/styleMarkEnd.gif\" alt=\"Formatering slut\">")
		} else
			alert(parent.uiKeys['errors_stylesNoStylesOnPage']);
	}
	
	currentRichTextObject.innerHTML = theHTML;
}
