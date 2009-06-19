
// set css file to use for menus
cssFile = "css/officexp.css"
Menu.prototype.cssFile = cssFile;

var tmp;

// Build array of menu methods
var menuMethods = new Array(
	menuItem("C", "new.gif", "Create", "createNew()"),
	menuItem("D", "delete.small.png", "Delete", "deleteThis()"),
	menuItem("M", "cut.small.png", "Move", "moveThis()"),
	menuItem("O", "copy.small.png", "Copy", "copyThis()"),
	menuItem("S", "sort.png", "Sort", "sortThis()"),
	menuItem("R", "permission.png", "Rights", "permissionThis()"),
	menuItem("P", "protect.png", "Public access", "accessThis()"),
	menuItem("L", "refresh.png", "Reload", "refreshNode()")	);

// edit menu

var undoItem, cutItem, copyItem, pasteItem, deleteItem, selectAllItem;

var oldOpenState = null;	// used to only change when needed
var lastKeyCode = 0;

function rememberKeyCode() {
	lastKeyCode = window.event.keyCode;
}

function showContextMenu() {
	var eMenu = new Menu()
	if (parent.nodeID != '-1' && parent.nodeType != '') {
	
		var menuItems = parent.nodeMenu;
		
		for (var i=0;i<menuItems.length;i++) {
			if (menuItems.substring(i,i+1) != ",") {
				thisMethod = findMenuItems(menuItems.substring(i,i+1));
				if (thisMethod) {
					eMenu.add( temp = new MenuItem( thisMethod.text, thisMethod.method, "images/" + thisMethod.icon ) );
					temp.mnemonic = thisMethod.alias;
				}
			} else
				eMenu.add( new MenuSeparator() );
				
		}
		
		
		var el = window.event.srcElement;
		
		// check for edit
		var showEditMenu = true //el != null &&
							// (el.tagName == "INPUT" || el.tagName == "TEXTAREA" || el.tagName == "DIV");
		

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
		
		if ( showEditMenu ) {u
			// nogle skal muligvis disabled
			eMenu.invalidate();
			eMenu.show( left, top );
		}
		else {
			cMenu.invalidate();
			cMenu.show( left, top );
		}
		
		event.returnValue = false;
		lastKeyCode = 0
	}
};

function menuItem (alias, icon, text, method) {
	var _menuItem = new menuItemPrototype();
	_menuItem.alias = alias;
	_menuItem.icon = icon;
	_menuItem.text = text;
	_menuItem.method = method;
	return _menuItem;
}

function menuItemPrototype() {
	this.alias = "";
	this.icon = "";
	this.text = "";
	this.method = "";
}

function findMenuItems(alias) {
	for (var i=0;i<menuMethods.length;i++) {
		if (menuMethods[i].alias == alias) {
			return menuMethods[i];
			break;
		}
	}
}

document.attachEvent( "oncontextmenu", showContextMenu );
document.attachEvent( "onkeyup", rememberKeyCode );

