/*----------------------------------------------------------------------------\
|                               XLoadTree 1.1                                 |
|-----------------------------------------------------------------------------|
|                         Created by Erik Arvidsson                           |
|                  (http://webfx.eae.net/contact.html#erik)                   |
|                      For WebFX (http://webfx.eae.net/)                      |
|-----------------------------------------------------------------------------|
| An extension to xTree that allows sub trees to be loaded at runtime by      |
| reading XML files from the server. Works with IE5+ and Mozilla 1.0+         |
|-----------------------------------------------------------------------------|
|                   Copyright (c) 1999 - 2002 Erik Arvidsson                  |
|-----------------------------------------------------------------------------|
| This software is provided "as is", without warranty of any kind, express or |
| implied, including  but not limited  to the warranties of  merchantability, |
| fitness for a particular purpose and noninfringement. In no event shall the |
| authors or  copyright  holders be  liable for any claim,  damages or  other |
| liability, whether  in an  action of  contract, tort  or otherwise, arising |
| from,  out of  or in  connection with  the software or  the  use  or  other |
| dealings in the software.                                                   |
| - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - |
| This  software is  available under the  three different licenses  mentioned |
| below.  To use this software you must chose, and qualify, for one of those. |
| - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - |
| The WebFX Non-Commercial License          http://webfx.eae.net/license.html |
| Permits  anyone the right to use the  software in a  non-commercial context |
| free of charge.                                                             |
| - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - |
| The WebFX Commercial license           http://webfx.eae.net/commercial.html |
| Permits the  license holder the right to use  the software in a  commercial |
| context. Such license must be specifically obtained, however it's valid for |
| any number of  implementations of the licensed software.                    |
| - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - |
| GPL - The GNU General Public License    http://www.gnu.org/licenses/gpl.txt |
| Permits anyone the right to use and modify the software without limitations |
| as long as proper  credits are given  and the original  and modified source |
| code are included. Requires  that the final product, software derivate from |
| the original  source or any  software  utilizing a GPL  component, such  as |
| this, is also licensed under the GPL license.                               |
|-----------------------------------------------------------------------------|
| 2001-09-27 | Original Version Posted.                                       |
| 2002-01-19 | Added some simple error handling and string templates for      |
|            | reporting the errors.                                          |
| 2002-01-28 | Fixed loading issues in IE50 and IE55 that made the tree load  |
|            | twice.                                                         |
| 2002-10-10 | (1.1) Added reload method that reloads the XML file from the   |
|            | server.                                                        |
|-----------------------------------------------------------------------------|
| Dependencies: xtree.js - original xtree library                             |
|               xtree.css - simple css styling of xtree                       |
|               xmlextras.js - provides xml http objects and xml document     |
|                              objects                                        |
|-----------------------------------------------------------------------------|
| Created 2001-09-27 | All changes are in the log above. | Updated 2002-10-10 |
\----------------------------------------------------------------------------*/

if (parent.uiKeys != undefined)
	webFXTreeConfig.loadingText = parent.uiKeys['general_pleasewait'];
else
	webFXTreeConfig.loadingText = "...";
	
webFXTreeConfig.loadErrorTextTemplate = ""; //"Fejl \"%1%\"";
webFXTreeConfig.emptyErrorTextTemplate = ""; // "Fejl \"%1%\" er tomt";

/*
 * WebFXLoadTree class
 */

function WebFXLoadTree(sText, sXmlSrc, xXmlSrcRoot, sAction, sBehavior, sIcon, sOpenIcon, sNodeType, sNodeID, sMenu) {
	// call super
	this.WebFXTree = WebFXTree;
	this.WebFXTree(sText, sAction, sBehavior, sIcon, sOpenIcon, sNodeType, sNodeID, sMenu);
	
	// setup default property values
	this.src = sXmlSrc;
	this.srcRoot = xXmlSrcRoot;
	this.loading = false;
	this.loaded = false;
	this.errorText = "";
	
	if (this.src != "" && this.src != null) {
		// check start state and load if open
		if (this.open)
			_startLoadXmlTree(this.src, this);
		else {
			// and create loading item if not
			this._loadingItem = new WebFXITreetem(webFXTreeConfig.loadingText);
			this.add(this._loadingItem);
		}
	 }
}

WebFXLoadTree.prototype = new WebFXTree;

// override the expand method to load the xml file
WebFXLoadTree.prototype._webfxtree_expand = WebFXTree.prototype.expand;
WebFXLoadTree.prototype.expand = function() {
	if (!this.loaded && !this.loading) {
		// load
		_startLoadXmlTree(this.src, this);
	}
	this._webfxtree_expand();
};

/*
 * WebFXLoadITreetem class
 */

function WebFXLoadITreetem(sText, sXmlSrc, sXmlSrcRoot, sAction, eParent, sIcon, sOpenIcon, sNodeType, sNodeID, sIconClass, sMenu, sChanged, sIsProtected, hasChildren) {
	// call super
	this.WebFXITreetem = WebFXITreetem;
	this.WebFXITreetem(sText, sAction, eParent, sIcon, sOpenIcon, sNodeType, sNodeID, sIconClass, sMenu, sChanged, sIsProtected);

	// setup default property values
	this.src = sXmlSrc;
	this.srcRoot = sXmlSrcRoot;
	this.loading = false;
	this.loaded = false;
	this.errorText = "";
	this.hasChildren = hasChildren;

	// check start state and load if open
	if (this.src != null && this.src != "" && ((sNodeType != 'content' && sNodeType != 'media')|| hasChildren == 'true')) {
	    if (this.open)
			_startLoadXmlTree(this.src, this);
		else {
			// and create loading item if not
			this._loadingItem = new WebFXITreetem(webFXTreeConfig.loadingText);
			this.add(this._loadingItem);
		}
	}
}

WebFXLoadITreetem.prototype = new WebFXITreetem;

// override the expand method to load the xml file
WebFXLoadITreetem.prototype._webfxITreetem_expand = WebFXITreetem.prototype.expand;
WebFXLoadITreetem.prototype.expand = function() {
    if (!this.loaded && !this.loading) {
        // load
        _startLoadXmlTree(this.src, this);
    }
    this._webfxITreetem_expand();
};

// reloads the src file if already loaded
WebFXLoadTree.prototype.reload = 
WebFXLoadITreetem.prototype.reload = function () {
	// if loading do nothing
	if (this.loaded) {
		var open = this.open;
		// remove
		while (this.childNodes.length > 0)
			this.childNodes[this.childNodes.length - 1].remove();
		
		this.loaded = false;
		
		this._loadingItem = new WebFXITreetem(webFXTreeConfig.loadingText);
		this.add(this._loadingItem);
		
		if (open)
			this.expand();
	}
	else if (this.open && !this.loading) {
		_startLoadXmlTree(this.src, this);
	}
};

WebFXLoadTree.prototype.forceLoad = 
WebFXLoadITreetem.prototype.forceLoad = function () {
    _startLoadXmlTree(this.src, this);
}

/*
 * Helper functions
 */

// creates the xmlhttp object and starts the load of the xml document
function _startLoadXmlTree(sSrc, jsNode) {
	if (jsNode.loading || jsNode.loaded)
		return;
	jsNode.loading = true;
	var xmlHttp = XmlHttp.create();
	xmlHttp.open("GET", sSrc, true);	// async
	xmlHttp.onreadystatechange = function () {
		if (xmlHttp.readyState == 4) {
			_xmlFileLoaded(xmlHttp.responseXML, jsNode);
		}
	};
	// call in new thread to allow ui to update
	window.setTimeout(function () {
		xmlHttp.send(null);
	}, 10);
}


// Converts an xml tree to a js tree. See article about xml tree format
function _xmlTreeToJsTree(oNode) {
	// retreive attributes
	var text = oNode.getAttribute("text");
	var action = oNode.getAttribute("action");
	var iconClass = oNode.getAttribute("iconClass");
	var nodeType = oNode.getAttribute("nodeType");
	var nodeID = oNode.getAttribute("nodeID");
	var parent = null;
	var icon = oNode.getAttribute("icon").substring(0,1) == "." && oNode.getAttribute("icon").substring(0,2) != ".." ? oNode.getAttribute("icon").substring(1, oNode.getAttribute("icon").length) : 'images/umbraco/' + oNode.getAttribute("icon");
	var openIcon = oNode.getAttribute("openIcon").substring(0,1) == "." && oNode.getAttribute("openIcon").substring(0,2) != ".." ? oNode.getAttribute("openIcon").substring(1, oNode.getAttribute("openIcon").length) : 'images/umbraco/' + oNode.getAttribute("openIcon");
	var src = oNode.getAttribute("src");
	var hasChildren = oNode.getAttribute("hasChildren");
	
	if (oNode.getAttribute("rootSrc") != null) {
		var rootSrc = oNode.getAttribute("rootSrc");
	} else
		var rootSrc = null;
	if (oNode.getAttribute("menu") != null)
		var menu = oNode.getAttribute("menu");
	else
		var menu = null;
	
	//cast bools:	
	if (oNode.getAttribute("notPublished") != null)
		var notPublished = (oNode.getAttribute("notPublished").toLowerCase()=='true' || null);
	else
		var notPublished = null;
	if (oNode.getAttribute("isProtected") != null)
		var isProtected = (oNode.getAttribute("isProtected").toLowerCase()=='true' || null);
	else
		var isProtected = null;

	
	// create jsNode
	var jsNode;
	if ((src != null && src != "") || rootSrc != null && rootSrc != "")
	    jsNode = new WebFXLoadITreetem(text, src, rootSrc, action, parent, icon, openIcon, nodeType, nodeID, iconClass, menu, notPublished, isProtected, hasChildren);
	else
	    jsNode = new WebFXLoadITreetem(text, "", "", action, parent, icon, openIcon, nodeType, nodeID, iconClass, menu, notPublished, isProtected, hasChildren);
		
	// go through childNOdes
	var cs = oNode.childNodes;
	var l = cs.length;
	for (var i = 0; i < l; i++) {
		if (cs[i].tagName == "tree")
			jsNode.add( _xmlTreeToJsTree(cs[i]), true );
	}
	
	return jsNode;
}

// Inserts an xml document as a subtree to the provided node
function _xmlFileLoaded(oXmlDoc, jsParentNode) {
	if (jsParentNode.loaded)
		return;

	var bIndent = false;
	var bAnyChildren = false;
	jsParentNode.loaded = true;
	jsParentNode.loading = false;

	// check that the load of the xml file went well
	if( oXmlDoc == null || oXmlDoc.documentElement == null) {
		jsParentNode.errorText = parseTemplateString(webFXTreeConfig.loadErrorTextTemplate,
							jsParentNode.src);
	}
	else {
		// there is one extra level of tree elements
		var root = oXmlDoc.documentElement;

		// loop through all tree children
		var cs = root.childNodes;
		var l = cs.length;
		for (var i = 0; i < l; i++) {
			if (cs[i].tagName == "tree") {
				bAnyChildren = true;
				bIndent = true;
				jsParentNode.add( _xmlTreeToJsTree(cs[i]), true);
			}
		}

		// if no children we got an error
		if (!bAnyChildren)
			jsParentNode.errorText = parseTemplateString(webFXTreeConfig.emptyErrorTextTemplate,
										jsParentNode.src);
	}
	
	// remove dummy
	if (jsParentNode._loadingItem != null) {
		jsParentNode._loadingItem.remove();
		bIndent = true;
	}
	
	if (bIndent) {
		// indent now that all items are added
		jsParentNode.indent();
	}
	
	// show error in status bar
	if (jsParentNode.errorText != "")
	    window.status = jsParentNode.errorText;

	// trigger jquery event
	jQuery(document).trigger('xTreeNodeLoaded', this);

}

// parses a string and replaces %n% with argument nr n
function parseTemplateString(sTemplate) {
	var args = arguments;
	var s = sTemplate;
	
	s = s.replace(/\%\%/g, "%");
	
	for (var i = 1; i < args.length; i++)
		s = s.replace( new RegExp("\%" + i + "\%", "g"), args[i] )
	
	return s;
}