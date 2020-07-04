var ctrlDown = false;
var shiftDown = false;
var keycode = 0

var currentRichTextDocument = null;
var currentRichTextObject = null;

function umbracoCheckKeysUp(e) {
	ctrlDown = e.ctrlKey;
	shiftDown = e.shiftKey;
}

function umbracoActivateKeys(ctrl, shift, key) {
	ctrlDown = ctrl;
	shiftDown = shift;
	keycode = key
	return runShortCuts();
}

function umbracoActivateKeysUp(ctrl, shift, key) {
	ctrlDown = ctrl;
	shiftDown = shift;
	keycode = key;
}

function umbracoCheckKeys(e) {
	ctrlDown = e.ctrlKey;
	shiftDown = e.shiftKey;
	keycode = e.keyCode;
	
	return runShortCuts();
}

function shortcutCheckKeysPressFirefox(e) {
	    if (ctrlDown && keycode == 83)
	        e.preventDefault();
}


function runShortCuts() {
	if (currentRichTextObject != undefined && currentRichTextObject != null) {
		if (ctrlDown) {
			if (!shiftDown && keycode == 9) 
				functionsFrame.tabSwitch(1);
			else
				if (shiftDown && keycode == 9) functionsFrame.tabSwitch(-1);

			if (keycode == 83) {doSubmit(); return false;}
			if (shiftDown && currentRichTextObject) {
				if (keycode == 70) {functionsFrame.umbracoInsertForm(myAlias); return false;}
				if (keycode == 76) {functionsFrame.umbracoLink(myAlias); return false;}
				if (keycode == 77) {functionsFrame.umbracoInsertMacro(myAlias, umbracoPath); return false;}
				if (keycode == 80) {functionsFrame.umbracoImage(myAlias); return false;}
				if (keycode == 84) {functionsFrame.umbracoInsertTable(myAlias); return false;}
				if (keycode == 86) {functionsFrame.umbracoShowStyles(myAlias); return false;}
				if (keycode == 85) {functionsFrame.document.getElementById('TabView1_tab01layer_publish').click(); return false;}
			}
		} 
			
	} else 
		if (isDialog) {
			if (keycode == 27) {window.close();} // ESC
			if (keycode == 13 && functionsFrame.submitOnEnter != undefined) {
				if (!functionsFrame.disableEnterSubmit) {
					if (functionsFrame.submitOnEnter) {
					 // firefox hack
					 if (window.addEventListener)
					    e.preventDefault();
					 doSubmit();
					}
				}
			}
			if (ctrlDown) {
				if (keycode == 83)
					doSubmit();
				else if (keycode == 85)
					document.getElementById('TabView1_tab01layer_publish').click();
				else if (!shiftDown && keycode == 9) {
					functionsFrame.tabSwitch(1);
					return false;
				}
				else
					if (shiftDown && keycode == 9) {
						functionsFrame.tabSwitch(-1);
						return false;
					}
					
			}
		}
	
		return true;
	
}

if (window.addEventListener) {
    document.addEventListener('keyup', umbracoCheckKeysUp, false);
    document.addEventListener('keydown', umbracoCheckKeys, false);
    document.addEventListener('keypress', shortcutCheckKeysPressFirefox, false);
} else {
    document.attachEvent( "onkeyup", umbracoCheckKeysUp);
    document.attachEvent("onkeydown", umbracoCheckKeys);            
}
