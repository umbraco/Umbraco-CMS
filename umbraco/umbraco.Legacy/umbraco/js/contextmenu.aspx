<%@ Page language="c#" Codebehind="contextmenu.aspx.cs" AutoEventWireup="True" Inherits="umbraco.js.contextmenu" %>

function showContextMenu(el) {
	menu.el = el;
	if (parent.nodeType != '') {
		var menuContent = parent.nodeMenu;
		var menuitems = new Array();
		if (menuContent) {
			for (var i=0;i<menuContent.length;i++) {
				if (menuContent.substring(i,i+1) != ",") {
					thisMethod = findMenuItems(menuContent.substring(i,i+1));
					if (thisMethod) {
						menuitems[i] = new MenuItem(thisMethod.text,thisMethod.method,thisMethod.icon,false);
					}
				} else
					menuitems[i] = new MenuItem("","","",true);
			}
		}
		return menu.Show(menuitems);
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

function notImplemented() {
	alert('not implemented');
	return false;
}

