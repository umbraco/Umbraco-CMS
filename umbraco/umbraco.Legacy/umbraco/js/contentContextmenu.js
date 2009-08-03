
// set css file to use for menus
cssFile = "css/officexp.css"
Menu.prototype.cssFile = cssFile;

var tmp;

// Build context menu


var oldOpenState = null;	// used to only change when needed
var lastKeyCode = 0;

function rememberKeyCode() {
	lastKeyCode = window.event.keyCode;
}

function showContextMenu() {
	if (parent.nodeID != '-1' && parent.nodeType != '') {
	
		var el = window.event.srcElement;
		
		var macroElement = null;
	
		// check for edit
		var showEditMenu = true //el != null &&
							// (el.tagName == "INPUT" || el.tagName == "TEXTAREA" || el.tagName == "DIV");
		

		var elList = ', ';
		var hasTable = false;
		// check for anchor	
		while ( el != null && el.tagName != "BODY") {
			if (el.tagName == "TABLE")
				hasTable = true;
				
			// New 2.1 macro checker
			if (el.tagName == 'IMG' || el.tagName == 'DIV') {
				var elementAttributeList = '';
				for (var temp=0; temp<el.attributes.length; temp++) {
					if (el.attributes[temp].value != '' && el.attributes[temp].value != 'null')
						elementAttributeList += el.attributes[temp].name + ': \'' + el.attributes[temp].value + '\'\n'
				}

				// vi tjekker i attribute-listen om der optraeder en makro
				if (elementAttributeList.indexOf('<?UMBRACO_MACRO') > -1 || elementAttributeList.indexOf('ismacro:') > -1) {
					elList = ', MACRO, ';
					macroElement = el;
					break;
				}
				else if (!hasTable && el.tagName != 'DIV')
					elList += 'IMG' + ', ';
			} else {
				if (!hasTable && el.tagName != "BODY") {
					elList += el.tagName + ', ';				
				}
			}

			el = el.parentNode;
		}
				
		//////////////////////////////////////////////////////////
		// Vi bygger menu'en udfra elementListen!
		//////////////////////////////////////////////////////////
		var eMenu = new Menu()

		// foerst standard funktioner
		if (elList.indexOf(', IMG,') == -1 && elList.indexOf(', MACRO,') == -1) {
			eMenu.add( resetItem = new MenuItem( "Reset paragraph", function () { umbracoAlign(el, "") }, "images/cMenu/editAlignReset.gif" ) );
			eMenu.add( leftItem = new MenuItem( "Align left", function () { umbracoAlign(el, "left") }, "images/cMenu/editAlignLeft.gif" ) );
			eMenu.add( centerItem = new MenuItem( "Center", function () { umbracoAlign(el, "center") }, "images/cMenu/editAlignCenter.gif" ) );
			eMenu.add( rightItem = new MenuItem( "Align right", function () { umbracoAlign(el, "right") }, "images/cMenu/editAlignRight.gif" ) );
		}

		// TAG: A - link
		if (elList.indexOf(', A,') > -1) {
			eMenu.add( new MenuSeparator() );
			eMenu.add( linkItem = new MenuItem( "Edit Link", function () { umbracoEditA(el) }, "images/cMenu/editA.gif" ) );
			linkItem.mnemonic = "l";
		}		
		
		// TAG: IMG - Billede
		if (elList.indexOf(', IMG,') > -1) {
			eMenu.add( new MenuSeparator() );
			eMenu.add( resetItem = new MenuItem( "Reset image", function () { umbracoAlign(el, "") }, "images/cMenu/editAlignReset.gif" ) );
			eMenu.add( leftItem = new MenuItem( "Align image left", function () { umbracoAlign(el, "left") }, "images/cMenu/editImageAlignLeft.gif" ) );
			eMenu.add( rightItem = new MenuItem( "Align image right", function () { umbracoAlign(el, "right") }, "images/cMenu/editImageAlignRight.gif" ) );
//			eMenu.add( new MenuSeparator() );
//			eMenu.add( imgItem = new MenuItem( "Rediger Billede", function () { umbracoEditImg(el) }, "images/cMenu/editImg.gif" ) );
//			imgItem.mnemonic = "b";
		}		
		
		// TAG: MACRO - Makro
		if (elList.indexOf(', MACRO,') > -1) {
			eMenu.add( new MenuSeparator() );
			eMenu.add( macroItem = new MenuItem( "Edit Macro", function () { umbracoEditMacro(macroElement) }, "images/cMenu/editMacro.gif" ) );
			macroItem.mnemonic = "m";
		}		
		
		// TAG: P, SPAN, DIV - Elementer hvor det er class/style som skal redigeres
		if (elList.indexOf(', P,') > -1 || elList.indexOf(', SPAN,') > -1 || elList.indexOf(', DIV,') > -1) {
//			eMenu.add( classItem = new MenuItem( "Rediger Formatering", function () { umbracoEditClass(el) }, "images/cMenu/editClass.gif" ) );
//			classItem.mnemonic = "f";
		}		
		
		// TAG: TD, TR, TABLE - Tabel-redigering
		if (elList.indexOf(', TD,') > -1 || elList.indexOf(', TR,') > -1 || elList.indexOf(', TABLE,') > -1) {
			eMenu.add( new MenuSeparator() );

/*			eMenu.add( tableItem = new MenuItem( "Rediger Tabel", function () { umbracoEditTable(el) }, "images/cMenu/editTable.gif" ) );
			tableItem.mnemonic = "t";
			
			// Evt. ogsaa cell-edit
			if (elList.indexOf(', TD,') > -1) {
				eMenu.add( cellItem = new MenuItem( "Rediger Celle", function () { umbracoEditCell(el) }, "images/cMenu/editcel.gif" ) );
				cellItem.mnemonic = "c";
			}
			eMenu.add( new MenuSeparator() );
*/

				eMenu.add( cellItem = new MenuItem( "Insert row", function () { umbracoAddRow(el) }, "images/cMenu/insrow.gif" ) );
				eMenu.add( cellItem = new MenuItem( "Insert column", function () { umbracoAddCol(el) }, "images/cMenu/inscol.gif" ) );
			eMenu.add( new MenuSeparator() );
				eMenu.add( cellItem = new MenuItem( "Remove row", function () { umbracoDeleteRow(el) }, "images/cMenu/delRow.gif" ) );
				eMenu.add( cellItem = new MenuItem( "Remove column", function () { umbracoDeleteCol(el) }, "images/cMenu/delcol.gif" ) );
			eMenu.add( new MenuSeparator() );
				eMenu.add( cellItem = new MenuItem( "Split cells", function () { umbracoSplitCell(el) }, "images/cMenu/spltCell.gif" ) );
				eMenu.add( cellItem = new MenuItem( "Collect cells", function () { umbracoGlueCell(el) }, "images/cMenu/mrgCell.gif" ) );

			// Evt. også cell-edit
			if (elList.indexOf(', TD,') > -1) {
				eMenu.add( cellItem = new MenuItem( "Edit Cell", function () { umbracoEditCell(el) }, "/umbraco/images/cMenu/editcel.gif" ) );
				cellItem.mnemonic = "c";
			}

		}		
		

		//////////////////////////////////////////////////////////
		// Slut paa at bygge menu
		//////////////////////////////////////////////////////////


		var showOpenItems = el != null && el.tagName == "A";
		
		if ( showOpenItems != oldOpenState ) {
			oldOpenState = showOpenItems;
		}
		
		if ( showOpenItems ) {
			openItem.action = openNewWinItem.action = el.href;
		}
		
		// find left and top
		var left, top;
		
		if ( showEditMenu )
			el = window.event.srcElement;
		else if ( !showOpenItems )
			el = document.documentElement;

		if ( lastKeyCode == 93 ) {	// context menu key
			left = posLib.getScreenLeft( el );
			top = posLib.getScreenTop( el );
		}
		else {
			left = window.event.screenX;
			top = window.event.screenY;
		}
		
		eMenu.invalidate();
		eMenu.show( left, top );
		
		event.returnValue = false;
		lastKeyCode = 0
	}
};

var functionsFrame = parent.parent.right;
var tabFrame = functionsFrame;
//tabFrame.initializeHasChanged();

document.attachEvent( "oncontextmenu", showContextMenu );
