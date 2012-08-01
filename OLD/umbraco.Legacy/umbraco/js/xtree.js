/*----------------------------------------------------------------------------\
|                       Cross Browser Tree Widget 1.17                        |
|-----------------------------------------------------------------------------|
|                          Created by Emil A Eklund                           |
|                  (http://webfx.eae.net/contact.html#emil)                   |
|                      For WebFX (http://webfx.eae.net/)                      |
|-----------------------------------------------------------------------------|
| An object based tree widget,  emulating the one found in microsoft windows, |
| with persistence using cookies. Works in IE 5+, Mozilla and konqueror 3.    |
|-----------------------------------------------------------------------------|
|                   Copyright (c) 1999 - 2002 Emil A Eklund                   |
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
| Dependencies: xtree.css (To set up the CSS of the tree classes)             |
|-----------------------------------------------------------------------------|
| 2001-01-10 | Original Version Posted.                                       |
| 2001-03-18 | Added getSelected and get/setBehavior  that can make it behave |
|            | more like windows explorer, check usage for more information.  |
| 2001-09-23 | Version 1.1 - New features included  keyboard  navigation (ie) |
|            | and the ability  to add and  remove nodes dynamically and some |
|            | other small tweaks and fixes.                                  |
| 2002-01-27 | Version 1.11 - Bug fixes and improved mozilla support.         |
| 2002-06-11 | Version 1.12 - Fixed a bug that prevented the indentation line |
|            | from  updating correctly  under some  circumstances.  This bug |
|            | happened when removing the last item in a subtree and items in |
|            | siblings to the remove subtree where not correctly updated.    |
| 2002-06-13 | Fixed a few minor bugs cased by the 1.12 bug-fix.              |
| 2002-08-20 | Added usePersistence flag to allow disable of cookies.         |
| 2002-10-23 | (1.14) Fixed a plus icon issue                                 |
| 2002-10-29 | (1.15) Last changes broke more than they fixed. This version   |
|            | is based on 1.13 and fixes the bugs 1.14 fixed withou breaking |
|            | lots of other things.                                          |
| 2003-02-15 | The  selected node can now be made visible even when  the tree |
|            | control  loses focus.  It uses a new class  declaration in the |
|            | css file '.webfx-tree-item a.selected-inactive', by default it |
|            | puts a light-gray rectangle around the selected node.          |
| 2003-03-16 | Adding target support after lots of lobbying...                |
|-----------------------------------------------------------------------------|
| Created 2000-12-11 | All changes are in the log above. | Updated 2003-03-16 |
\----------------------------------------------------------------------------*/

// NH 12-08-2008, adding new tree sync methods
function SyncTree(path, isNew, newName, newPublishStatus, attempts, ref) {

    if (!attempts || attempts == "")
        attempts = 0;

    // prevent infinite loop
    if (attempts > 5) {
        // clear event bindings
        jQuery(document).unbind('xTreeNodeLoaded');
        jQuery(document).unbind('xTreeNodeExpanded');
        // throw error
        parent.UmbSpeechBubble.ShowMessage("error", "Can't expand tree", "There was an error loading children, please try again", false);
        return;
    }
    // crop tree till first startnode
    if (tree.nodeID != path[0]) {
        var newPath = "";
        var generatePath = false;
        for (var i = 0; i < path.length; i++) {
            if (!generatePath && tree.nodeID == path[i]) {
                generatePath = true;
            }

            if (generatePath) {
                newPath += path[i];
                if (i < path.length - 1) {
                    newPath += ",";
                }
            }
        }
        path = newPath.split(",");
    }

    var idToMark = path[path.length - 1];

    /*    if (isNew == 'true' && path.length == 2) {
    var docUrl = document.location.href;
    if (docUrl.indexOf('?') > 0) {
    document.location.href = docUrl + "&syncTree=" + path.toString();
    } else {
    document.location.href = docUrl + "?syncTree=" + path.toString();
    }
    }*/

    var nodeToMark = findNodeById(idToMark)
    // Test if id is loaded
    if (nodeToMark) {
        // make sure that all parents are expanded
        var nodeInPath = null;
        for (var i = 1; i < path.length - 1; i++) {
            var nodeInPath = findNodeById(path[i]);
            // depending on permissions, the node might not exist
            if (nodeInPath != '') {
                if (!nodeInPath.open) {
                    nodeInPath.expand();
                }
            }
        }

        if (newPublishStatus == '0') {
            nodeToMark.notPublished = 1;
            nodeToMark.iconClass = 'umbraco-tree-icon-grey';
            // add version overlay icon
            jQuery("#" + nodeToMark.id + " .overlayHolder").prepend(nodeToMark.getVersionOverlay());
            // force redraw
            if (newName == '')
                newName = nodeToMark.text;

        } else if (newPublishStatus == '1') {
            nodeToMark.notPublished = 0;
            nodeToMark.iconClass = '';
            // remove
            jQuery("#" + nodeToMark.id + " .newVersionOverlay").remove();
            // force redraw
            if (newName == '')
                newName = nodeToMark.text;
        }
        if (newName != '') {
            nodeToMark.text = newName;
            if (nodeToMark.notPublished) {
                jQuery("#" + nodeToMark.id + "-anchor").html("<span style=\"color: #999\">" + nodeToMark.text + "</span>");
            } else {
                jQuery("#" + nodeToMark.id + "-anchor").html(nodeToMark.text);
            }

        }

        nodeToMark.mark();

        // unbind all events
        jQuery(document).unbind('xTreeNodeLoaded');
        jQuery(document).unbind('xTreeNodeExpanded');
    } else {
        // TODO: This should be matched with start nodes!
        // make sure that all parents are expanded
        for (var i = 1; i < path.length - 1; i++) {
            var nodeInPath = findNodeById(path[i]);

            // depending on permissions, the node might not exist
            if (nodeInPath) {
                if (nodeInPath.hasChildren != 'false') {
                    if (!nodeInPath.open) {
                        if (nodeInPath.loaded != false) {
                            jQuery(document).one('xTreeNodeExpanded', function(event, msg) {
                                SyncTree(path, isNew, '', '', attempts + 1, 'expand');
                            });
                        } else {
                            jQuery(document).one('xTreeNodeLoaded', function(event, msg) {
                                SyncTree(path, isNew, '', '', attempts + 1, 'loaded 1');
                            });
                        }

                        // maybe reload
                        if (isNew == 'true' && i <= path.length - 2) {
                            nodeInPath.src = nodeInPath.src + "&reload=" + Math.random() * 10;
                        }
                        nodeInPath.expand();

                    } else if (isNew == 'true' && i == path.length - 2) {
                        // only expand if final node doesn't exist
                        if (!nodeInPath.loading) {
                            nodeInPath.src = nodeInPath.src + "&reload=" + Math.random() * 10;
                            jQuery(document).one('xTreeNodeLoaded', function(event, msg) {
                                SyncTree(path, isNew, '', '', attempts + 1, 'loaded 2');
                            });
                            nodeInPath.reload();
                        }
                    }
                }
            }
        }
    }

}

function findNodeById(id) {
    return findNodeByIdDo(tree, id);
}

function findNodeByIdDo(node, id) {
    var childNodes = node.childNodes;

    if (childNodes != null)
        for (var i = 0; i < childNodes.length; i++) {
        if (childNodes[i].nodeID == id)
            return childNodes[i];
        else if (childNodes[i].childNodes != null) {
            var tempNode = findNodeByIdDo(childNodes[i], id);
            if (tempNode != "")
                return tempNode;
        }
    }

    return "";
}
var webFXTreeConfig = {
    rootIcon: 'images/foldericon.png',
    openRootIcon: 'images/openfoldericon.png',
    folderIcon: 'images/foldericon.png',
    openFolderIcon: 'images/openfoldericon.png',
    fileIcon: 'images/file.png',
    iIcon: 'images/I.png',
    lIcon: 'images/L.png',
    lMinusIcon: 'images/Lminus.png',
    lPlusIcon: 'images/Lplus.png',
    tIcon: 'images/T.png',
    tMinusIcon: 'images/Tminus.png',
    tPlusIcon: 'images/Tplus.png',
    blankIcon: 'images/blank.png',
    defaultText: 'Tree Item',
    defaultAction: 'javascript:void(0);',
    defaultBehavior: 'classic',
    usePersistence: false
};

var webFXTreeHandler = {
    idCounter: 0,
    idPrefix: "webfx-tree-object-",
    all: {},
    behavior: null,
    selected: null,
    onSelect: null, /* should be part of tree, not handler */
    getId: function() { return this.idPrefix + this.idCounter++; },
    toggle: function(oItem) { this.all[oItem.id.replace('-plus', '')].toggle(); },
    select: function(oItem) { this.all[oItem.id.replace('-icon', '')].select(); },
    focus: function(oItem) { this.all[oItem.id.replace('-anchor', '')].focus(); },
    mark: function(oItem) { this.all[oItem.id.replace('-anchor', '').replace('-mainanchor', '')].mark(); },
    blur: function(oItem) { this.all[oItem.id.replace('-anchor', '')].blur(); },
    operaContextMenu: function(oItem) { if (window.opera) { return showContextMenu(oItem); } else { this.toggle(oItem); } },
    updateUmbracoData: function(oItem, postFix) { this.all[oItem.id.replace(postFix, '')].updateUmbracoData(); },
    keydown: function(oItem, e) { return this.all[oItem.id].keydown(e.keyCode); },
    cookies: new WebFXCookie(),
    insertHTMLBeforeEnd: function(oElement, sHTML) {
        if (oElement.insertAdjacentHTML != null) {
            oElement.insertAdjacentHTML("BeforeEnd", sHTML)
            return;
        }
        var df; // DocumentFragment
        var r = oElement.ownerDocument.createRange();
        r.selectNodeContents(oElement);
        r.collapse(false);
        df = r.createContextualFragment(sHTML);
        oElement.appendChild(df);
    }
};

/*
* WebFXCookie class
*/

function WebFXCookie() {
    if (document.cookie.length) { this.cookies = ' ' + document.cookie; }
}

WebFXCookie.prototype.setCookie = function(key, value) {
    document.cookie = key + "=" + escape(value);
}

WebFXCookie.prototype.getCookie = function(key) {
    if (this.cookies) {
        var start = this.cookies.indexOf(' ' + key + '=');
        if (start == -1) { return null; }
        var end = this.cookies.indexOf(";", start);
        if (end == -1) { end = this.cookies.length; }
        end -= start;
        var cookie = this.cookies.substr(start, end);
        return unescape(cookie.substr(cookie.indexOf('=') + 1, cookie.length - cookie.indexOf('=') + 1));
    }
    else { return null; }
}

/*
* WebFXTreeAbstractNode class
*/

function WebFXTreeAbstractNode(sText, sAction) {
    this.childNodes = [];
    this.id = webFXTreeHandler.getId();
    this.text = sText || webFXTreeConfig.defaultText;
    this.action = sAction || webFXTreeConfig.defaultAction;
    this._last = false;
    webFXTreeHandler.all[this.id] = this;
}

/*
* To speed thing up if you're adding multiple nodes at once (after load)
* use the bNoIdent parameter to prevent automatic re-indentation and call
* the obj.ident() method manually once all nodes has been added.
*/

WebFXTreeAbstractNode.prototype.add = function(node, bNoIdent) {
    node.parentNode = this;
    this.childNodes[this.childNodes.length] = node;
    var root = this;
    if (this.childNodes.length >= 2) {
        this.childNodes[this.childNodes.length - 2]._last = false;
    }
    while (root.parentNode) { root = root.parentNode; }
    if (root.rendered) {
        if (this.childNodes.length >= 2) {
            document.getElementById(this.childNodes[this.childNodes.length - 2].id + '-plus').src = ((this.childNodes[this.childNodes.length - 2].folder) ? ((this.childNodes[this.childNodes.length - 2].open) ? webFXTreeConfig.tMinusIcon : webFXTreeConfig.tPlusIcon) : webFXTreeConfig.tIcon);
            this.childNodes[this.childNodes.length - 2].plusIcon = webFXTreeConfig.tPlusIcon;
            this.childNodes[this.childNodes.length - 2].minusIcon = webFXTreeConfig.tMinusIcon;
            this.childNodes[this.childNodes.length - 2]._last = false;
        }
        this._last = true;
        var foo = this;
        while (foo.parentNode) {
            for (var i = 0; i < foo.parentNode.childNodes.length; i++) {
                if (foo.id == foo.parentNode.childNodes[i].id) { break; }
            }
            if (i == foo.parentNode.childNodes.length - 1) { foo.parentNode._last = true; }
            else { foo.parentNode._last = false; }
            foo = foo.parentNode;
        }
        webFXTreeHandler.insertHTMLBeforeEnd(document.getElementById(this.id + '-cont'), node.toString());
        if ((!this.folder) && (!this.openIcon)) {
            this.icon = webFXTreeConfig.folderIcon;
            this.openIcon = webFXTreeConfig.openFolderIcon;
        }
        if (!this.folder) { this.folder = true; this.collapse(true); }
        if (!bNoIdent) { this.indent(); }
    }
    return node;
}

WebFXTreeAbstractNode.prototype.toggle = function() {
    if (this.folder) {
        if (this.open) { this.collapse(); }
        else { this.expand(); }
    }
}

WebFXTreeAbstractNode.prototype.select = function() {
    document.getElementById(this.id + '-anchor').focus();
}

WebFXTreeAbstractNode.prototype.deSelect = function() {
    jQuery('#' + this.id + '-anchor').removeClass();
    webFXTreeHandler.selected = null;
}

WebFXTreeAbstractNode.prototype.focus = function() {
    /*
    // NH 12-09-2008, focus code moved to a new mark() function, so it's only when a tree item is clicked that it gets focus
    if ((webFXTreeHandler.selected) && (webFXTreeHandler.selected != this)) { webFXTreeHandler.selected.deSelect(); }
    webFXTreeHandler.selected = this;

if ((this.openIcon) && (webFXTreeHandler.behavior != 'classic')) { document.getElementById(this.id + '-icon').src = this.openIcon; }
    jQuery('#' + this.id + '-anchor').removeClass().addClass("selected");
    if (document.getElementById(this.id + '-anchor').visible)
    document.getElementById(this.id + '-anchor').focus();
    if (webFXTreeHandler.onSelect) { webFXTreeHandler.onSelect(this); }
    */
}

WebFXTreeAbstractNode.prototype.mark = function() {
    if ((webFXTreeHandler.selected) && (webFXTreeHandler.selected != this)) { webFXTreeHandler.selected.deSelect(); }
    webFXTreeHandler.selected = this;

    if ((this.openIcon) && (webFXTreeHandler.behavior != 'classic')) { document.getElementById(this.id + '-icon').src = this.openIcon; }
    jQuery('#' + this.id + '-anchor').removeClass().addClass("selected");

    if (document.getElementById(this.id + '-anchor').visible)
        document.getElementById(this.id + '-anchor').focus();

    if (webFXTreeHandler.onSelect) { webFXTreeHandler.onSelect(this); }
}


WebFXTreeAbstractNode.prototype.updateUmbracoData = function() {
    // umbraco Addition
    if (menuActive == '') {
        if (this.nodeID) {
            if (parent.nodeID != this.nodeID || parent.nodeType != this.nodeType) {
                parent.node = this;
                parent.nodeKey = this.id;
                parent.nodeID = this.nodeID;
                parent.nodeType = this.nodeType;
                parent.nodeName = this.text;
                parent.nodeMenu = this.menu;
            }
        } else {
            parent.node = null;
            parent.nodeID = -1;
            parent.nodeKey = '';
            parent.nodeType = '';
            parent.nodeName = '';
            parent.nodeMenu = '';
        }
    }
}

WebFXTreeAbstractNode.prototype.blur = function() {
    if ((webFXTreeHandler.selected) && (webFXTreeHandler.selected == this)) {
        if ((this.openIcon) && (webFXTreeHandler.behavior != 'classic')) { document.getElementById(this.id + '-icon').src = this.icon; }
        jQuery('#' + this.id + '-anchor').removeClass().addClass("selected-inactive");
    }
    // umbraco add on
    //	parent.nodeType = '';
    //	parent.nodeID = '-1';
}

WebFXTreeAbstractNode.prototype.doExpand = function() {

    //if (webFXTreeHandler.behavior == 'classic') { document.getElementById(this.id + '-icon').src = this.openIcon; }

    if (this.childNodes.length) { document.getElementById(this.id + '-cont').style.display = 'block'; }
    this.open = true;
    if (webFXTreeConfig.usePersistence) {
        webFXTreeHandler.cookies.setCookie(this.id.substr(18, this.id.length - 18), '1');
    }

    // trigger jquery event
    jQuery(document).trigger('xTreeNodeExpanded', this);
}

WebFXTreeAbstractNode.prototype.doCollapse = function() {
    //if (webFXTreeHandler.behavior == 'classic') { document.getElementById(this.id + '-icon').src = this.icon; }

    if (this.childNodes.length) { document.getElementById(this.id + '-cont').style.display = 'none'; }
    this.open = false;
    if (webFXTreeConfig.usePersistence) {
        webFXTreeHandler.cookies.setCookie(this.id.substr(18, this.id.length - 18), '0');
    }
}

WebFXTreeAbstractNode.prototype.expandAll = function() {
    this.expandChildren();
    if ((this.folder) && (!this.open)) { this.expand(); }
}

WebFXTreeAbstractNode.prototype.expandChildren = function() {
    for (var i = 0; i < this.childNodes.length; i++) {
        this.childNodes[i].expandAll();
    }
}

WebFXTreeAbstractNode.prototype.collapseAll = function() {
    this.collapseChildren();
    if ((this.folder) && (this.open)) { this.collapse(true); }
}

WebFXTreeAbstractNode.prototype.collapseChildren = function() {
    for (var i = 0; i < this.childNodes.length; i++) {
        this.childNodes[i].collapseAll();
    }
}

WebFXTreeAbstractNode.prototype.indent = function(lvl, del, last, level, nodesLeft) {
    /*
    * Since we only want to modify items one level below ourself,
    * and since the rightmost indentation position is occupied by
    * the plus icon we set this to -2
    */
    if (lvl == null) { lvl = -2; }
    var state = 0;
    for (var i = this.childNodes.length - 1; i >= 0; i--) {
        state = this.childNodes[i].indent(lvl + 1, del, last, level);
        if (state) { return; }
    }
    if (del) {
        if ((level >= this._level) && (document.getElementById(this.id + '-plus'))) {
            if (this.folder) {
                document.getElementById(this.id + '-plus').src = (this.open) ? webFXTreeConfig.lMinusIcon : webFXTreeConfig.lPlusIcon;
                this.plusIcon = webFXTreeConfig.lPlusIcon;
                this.minusIcon = webFXTreeConfig.lMinusIcon;
            }
            else if (nodesLeft) { document.getElementById(this.id + '-plus').src = webFXTreeConfig.lIcon; }
            return 1;
        }
    }
    var foo = document.getElementById(this.id + '-indent-' + lvl);
    if (foo) {
        if ((foo._last) || ((del) && (last))) { foo.src = webFXTreeConfig.blankIcon; }
        else { foo.src = webFXTreeConfig.iIcon; }
    }
    return 0;
}

/*
* WebFXTree class
*/

function WebFXTree(sText, sAction, sBehavior, sIcon, sOpenIcon, sNodeType, sNodeID, sMenu, sNotPublished, sIsProtected) {
    this.base = WebFXTreeAbstractNode;
    this.base(sText, sAction);

    //SD: Added this check here in order to render the proper images for root nodes
    var icon = (sIcon != null && sIcon != "" ? (sIcon.substring(0, 1) == "." && sIcon.substring(0, 2) != ".." ? sIcon.substring(1, sIcon.length) : 'images/umbraco/' + sIcon) : null);
    var openIcon = (sIcon != null && sIcon != "" ? (sOpenIcon.substring(0, 1) == "." && sOpenIcon.substring(0, 2) != ".." ? sOpenIcon.substring(1, sOpenIcon.length) : 'images/umbraco/' + sOpenIcon) : null);

    this.icon = icon || webFXTreeConfig.rootIcon;
    this.openIcon = openIcon || webFXTreeConfig.openRootIcon;

    this.nodeType = sNodeType;
    this.nodeID = sNodeID;
    if (sMenu) { this.menu = sMenu; }
    if (sNotPublished)
        this.notPublished = 1;
    else
        this.notPublished = 0;

    if (sIsProtected)
        this.isProtected = 1;
    else
        this.isProtected = 0;

    /* Defaults to open */
    if (webFXTreeConfig.usePersistence) {
        this.open = (webFXTreeHandler.cookies.getCookie(this.id.substr(18, this.id.length - 18)) == '0') ? false : true;
    } else { this.open = true; }
    this.folder = true;
    this.rendered = false;
    this.onSelect = null;
    if (!webFXTreeHandler.behavior) { webFXTreeHandler.behavior = sBehavior || webFXTreeConfig.defaultBehavior; }
}

WebFXTree.prototype = new WebFXTreeAbstractNode;

WebFXTree.prototype.setBehavior = function(sBehavior) {
    webFXTreeHandler.behavior = sBehavior;
};

WebFXTree.prototype.getBehavior = function(sBehavior) {
    return webFXTreeHandler.behavior;
};

WebFXTree.prototype.getSelected = function() {
    if (webFXTreeHandler.selected) { return webFXTreeHandler.selected; }
    else { return null; }
}

WebFXTree.prototype.remove = function() { }

WebFXTree.prototype.expand = function() {
    this.doExpand();
}

WebFXTree.prototype.collapse = function(b) {
    //    if (!b) { this.focus(); }
    this.doCollapse();
}

WebFXTree.prototype.getFirst = function() {
    return null;
}

WebFXTree.prototype.getLast = function() {
    return null;
}

WebFXTree.prototype.getNextSibling = function() {
    return null;
}

WebFXTree.prototype.getPreviousSibling = function() {
    return null;
}

WebFXTree.prototype.keydown = function(key) {
    if (key == 39) {
        if (!this.open) { this.expand(); }
        else if (this.childNodes.length) { this.childNodes[0].select(); }
        return false;
    }
    if (key == 37) { this.collapse(); return false; }
    if ((key == 40) && (this.open) && (this.childNodes.length)) { this.childNodes[0].select(); return false; }
    return true;
}

WebFXTree.prototype.toString = function() {
    /*var str = "<div onMouseOver=\"webFXTreeHandler.updateUmbracoData(this, '-icon');\" id=\"" + this.id + "\" ondblclick=\"webFXTreeHandler.toggle(this);\" class=\"webfx-root-item webfx-tree-item\" oncontextmenu=\"return showContextMenu(this);\" onkeydown=\"return webFXTreeHandler.keydown(this, event)\">" +
    "<img id=\"" + this.id + "-icon\" class=\"webfx-tree-icon\" src=\"" + ((webFXTreeHandler.behavior == 'classic' && this.open) ? this.openIcon : this.icon) + "\" onclick=\"webFXTreeHandler.select(this);\">" +
    "<a href=\"" + this.action + "\" id=\"" + this.id + "-anchor\" onfocus=\"webFXTreeHandler.focus(this);\" onblur=\"webFXTreeHandler.blur(this);\"" +
    (this.target ? " target=\"" + this.target + "\"" : "") +
    ">" + this.text + "</a></div>" +
    "<div id=\"" + this.id + "-cont\" class=\"webfx-tree-container\" style=\"display: " + ((this.open) ? 'block' : 'none') + ";\">";
    var sb = [];
    for (var i = 0; i < this.childNodes.length; i++) {
    sb[i] = this.childNodes[i].toString(i, this.childNodes.length);
    }
    this.rendered = true;
    */

    var ahrefStart = "<a href=\"" + this.action + "\" id=\"" + this.id + "-anchor\" onfocus=\"webFXTreeHandler.focus(this);\" onblur=\"webFXTreeHandler.blur(this);\">";
    var str = "<div onMouseOver=\"webFXTreeHandler.updateUmbracoData(this, '-icon');\" id=\"" + this.id + "\" ondblclick=\"webFXTreeHandler.toggle(this);\" class=\"webfx-root-item webfx-tree-item\" oncontextmenu=\"return showContextMenu(this);\" onkeydown=\"return webFXTreeHandler.keydown(this, event)\">";

    if (this.icon.substring(0, 6) != "images") {
        str += "<div class=\"sprTree " + this.icon + "\"><img id=\"" + this.id + "-icon\" class=\"webfx-tree-icon\" src=\"images/nada.gif\"></div>";
    } else {
        str += "<img id=\"" + this.id + "-icon\" class=\"webfx-tree-icon\" src=\"" + ((webFXTreeHandler.behavior == 'classic' && this.open) ? this.openIcon : this.icon) + "\" onclick=\"webFXTreeHandler.select(this);\"/>";
    }

    str += "<a href=\"" + this.action + "\" id=\"" + this.id + "-anchor\" onfocus=\"webFXTreeHandler.focus(this);\" onblur=\"webFXTreeHandler.blur(this);\"" +
    (this.target ? " target=\"" + this.target + "\"" : "") +
    ">" + this.text + "</a></div>" +
    "<div id=\"" + this.id + "-cont\" class=\"webfx-tree-container\" style=\"display: " + ((this.open) ? 'block' : 'none') + ";\">";
    var sb = [];
    for (var i = 0; i < this.childNodes.length; i++) {
        sb[i] = this.childNodes[i].toString(i, this.childNodes.length);
    }
    this.rendered = true;

    return str + sb.join("") + "</div>";

};

/*
* WebFXITreetem class
*/

function WebFXITreetem(sText, sAction, eParent, sIcon, sOpenIcon, sNodeType, sNodeID, sIconClass, sMenu, sNotPublished, sIsProtected) {
    this.base = WebFXTreeAbstractNode;
    this.base(sText, sAction);
    /* Defaults to close */
    if (webFXTreeConfig.usePersistence)
        this.open = (webFXTreeHandler.cookies.getCookie(this.id.substr(18, this.id.length - 18)) == '1') ? true : false;
    else
        this.open = false;
    if (sIcon) { this.icon = sIcon; }
    if (sText) { this.text = sText; }
    if (sNodeType) { this.nodeType = sNodeType; }
    if (sIconClass) { this.iconClass = sIconClass; }
    if (sNodeID) { this.nodeID = sNodeID; }
    if (sOpenIcon) { this.openIcon = sOpenIcon; }
    if (eParent) { eParent.add(this); }
    if (sMenu) { this.menu = sMenu; }
    if (sNotPublished)
        this.notPublished = 1;
    else
        this.notPublished = 0;
    if (sIsProtected)
        this.isProtected = 1;
    else
        this.isProtected = 0;
}

WebFXITreetem.prototype = new WebFXTreeAbstractNode;

WebFXITreetem.prototype.remove = function() {
    var iconSrc = document.getElementById(this.id + '-plus').src;
    var parentNode = this.parentNode;
    var prevSibling = this.getPreviousSibling(true);
    var nextSibling = this.getNextSibling(true);
    var folder = this.parentNode.folder;
    var last = ((nextSibling) && (nextSibling.parentNode) && (nextSibling.parentNode.id == parentNode.id)) ? false : true;
    this.getPreviousSibling().focus();
    this._remove();
    if (parentNode.childNodes.length == 0) {
        document.getElementById(parentNode.id + '-cont').style.display = 'none';
        parentNode.doCollapse();
        parentNode.folder = false;
        parentNode.open = false;
    }
    if (!nextSibling || last) { parentNode.indent(null, true, last, this._level, parentNode.childNodes.length); }
    if ((prevSibling == parentNode) && !(parentNode.childNodes.length)) {
        prevSibling.folder = false;
        prevSibling.open = false;
        iconSrc = document.getElementById(prevSibling.id + '-plus').src;
        iconSrc = iconSrc.replace('minus', '').replace('plus', '');
        document.getElementById(prevSibling.id + '-plus').src = iconSrc;
    }
    if (document.getElementById(prevSibling.id + '-plus')) {
        if (parentNode == prevSibling.parentNode) {
            iconSrc = iconSrc.replace('minus', '').replace('plus', '');
            document.getElementById(prevSibling.id + '-plus').src = iconSrc;
        }
    }
}

WebFXITreetem.prototype._remove = function() {
    for (var i = this.childNodes.length - 1; i >= 0; i--) {
        this.childNodes[i]._remove();
    }
    for (var i = 0; i < this.parentNode.childNodes.length; i++) {
        if (this == this.parentNode.childNodes[i]) {
            for (var j = i; j < this.parentNode.childNodes.length; j++) {
                this.parentNode.childNodes[j] = this.parentNode.childNodes[j + 1];
            }
            this.parentNode.childNodes.length -= 1;
            if (i + 1 == this.parentNode.childNodes.length) { this.parentNode._last = true; }
            break;
        }
    }
    webFXTreeHandler.all[this.id] = null;
    var tmp = document.getElementById(this.id);
    if (tmp) { tmp.parentNode.removeChild(tmp); }
    tmp = document.getElementById(this.id + '-cont');
    if (tmp) { tmp.parentNode.removeChild(tmp); }
}

WebFXITreetem.prototype.expand = function() {
    this.doExpand();
    document.getElementById(this.id + '-plus').src = this.minusIcon;
}

WebFXITreetem.prototype.collapse = function(b) {
    if (!b) { this.focus(); }
    this.doCollapse();
    document.getElementById(this.id + '-plus').src = this.plusIcon;
}

WebFXITreetem.prototype.getFirst = function() {
    return this.childNodes[0];
}

WebFXITreetem.prototype.getLast = function() {
    if (this.childNodes[this.childNodes.length - 1].open) { return this.childNodes[this.childNodes.length - 1].getLast(); }
    else { return this.childNodes[this.childNodes.length - 1]; }
}

WebFXITreetem.prototype.getNextSibling = function() {
    for (var i = 0; i < this.parentNode.childNodes.length; i++) {
        if (this == this.parentNode.childNodes[i]) { break; }
    }
    if (++i == this.parentNode.childNodes.length) { return this.parentNode.getNextSibling(); }
    else { return this.parentNode.childNodes[i]; }
}

WebFXITreetem.prototype.getPreviousSibling = function(b) {
    for (var i = 0; i < this.parentNode.childNodes.length; i++) {
        if (this == this.parentNode.childNodes[i]) { break; }
    }
    if (i == 0) { return this.parentNode; }
    else {
        if ((this.parentNode.childNodes[--i].open) || (b && this.parentNode.childNodes[i].folder)) { return this.parentNode.childNodes[i].getLast(); }
        else { return this.parentNode.childNodes[i]; }
    }
}

WebFXITreetem.prototype.keydown = function(key) {
    if ((key == 39) && (this.folder)) {
        if (!this.open) { this.expand(); }
        else { this.getFirst().select(); }
        return false;
    }
    else if (key == 37) {
        if (this.open) { this.collapse(); }
        else { this.parentNode.select(); }
        return false;
    }
    else if (key == 40) {
        if (this.open) { this.getFirst().select(); }
        else {
            var sib = this.getNextSibling();
            if (sib) { sib.select(); }
        }
        return false;
    }
    else if (key == 38) { this.getPreviousSibling().select(); return false; }
    return true;
}

WebFXITreetem.prototype.updateName = function(newName) {
    this.text = newName;
    if (this.notPublished) {
        jQuery("#" + this.id + "-anchor span").html(newName);
    } else {
        jQuery("#" + this.id + "-anchor").html(newName);
    }
}

WebFXITreetem.prototype.changePublishStatus = function(newNotPublishedStatus) {
    var oldStatus = this.notPublished;
    if (oldStatus != newNotPublishedStatus) {
        this.notPublished = oldStatus;
    }
}

WebFXITreetem.prototype.getVersionOverlay = function() {
    return "<div class=\"newVersionOverlay\"><img src=\"images/nada.gif\"/></div>";
}

WebFXITreetem.prototype.toString = function(nItem, nItemCount) {
    var foo = this.parentNode;
    var indent = '';
    if (nItem + 1 == nItemCount) { this.parentNode._last = true; }
    var indentWidth = 0;
    var i = 0;

    while (foo.parentNode) {
        foo = foo.parentNode;
        //indent = "<img id=\"" + this.id + "-indent-" + i + "\" src=\"" + ((foo._last) ? webFXTreeConfig.blankIcon : webFXTreeConfig.iIcon) + "\">" + indent;
        indentWidth = indentWidth + 20;
        i++;
    }

    this._level = i;
    if (this.childNodes.length) { this.folder = 1; }
    else { this.open = false; }
    if ((this.folder) || (webFXTreeHandler.behavior != 'classic')) {
        if (!this.icon) { this.icon = webFXTreeConfig.folderIcon; }
        if (!this.openIcon) { this.openIcon = webFXTreeConfig.openFolderIcon; }
    }
    else if (!this.icon) { this.icon = webFXTreeConfig.fileIcon; }
    var label = this.text.replace(/</g, '&lt;').replace(/>/g, '&gt;');

    if (parent.uiKeys != undefined)
        var uiKeysExists = true;
    else
        var uiKeysExists = false;

    if (uiKeysExists)
        var iconAlt = parent.uiKeys['content_clickToEdit'];

    var str = "<div style=\"margin-left: " + indentWidth + "px;\" onMouseOver=\"webFXTreeHandler.updateUmbracoData(this, '-icon');\" id=\"" + this.id + "\" ondblclick=\"webFXTreeHandler.operaContextMenu(this);\" class=\"webfx-tree-item\" oncontextmenu=\"return showContextMenu(this);\" onkeydown=\"return webFXTreeHandler.keydown(this, event)\">" +


    //indent +
		"<img id=\"" + this.id + "-plus\" src=\"" + ((this.folder) ? ((this.open) ? ((this.parentNode._last) ? webFXTreeConfig.lMinusIcon : webFXTreeConfig.tMinusIcon) : ((this.parentNode._last) ? webFXTreeConfig.lPlusIcon : webFXTreeConfig.tPlusIcon)) : ((this.parentNode._last) ? webFXTreeConfig.lIcon : webFXTreeConfig.tIcon)) + "\" onclick=\"webFXTreeHandler.toggle(this);\">" +
		"<div class=\"overlayHolder\">";

    if (this.notPublished == 1) {
        str += this.getVersionOverlay();
        if (uiKeysExists)
            iconAlt = parent.uiKeys['content_itemChanged'] + " (" + iconAlt + ")";
    }
    if (this.isProtected == 1) {
        str += "<div class=\"newProtectOverlay\"><img src=\"images/nada.gif\"/></div>"
    }

    str += "<a href=\"" + this.action + "\" id=\"" + this.id + "-mainanchor\" onclick=\"webFXTreeHandler.mark(this);\">"

    // NH: Adds support for css sprites
    if (this.icon.substring(0, 6) != "images") {
        if (this.iconClass) {
            if (uiKeysExists && this.nodeType == 'content')
                iconAlt = parent.uiKeys['content_itemNotPublished'] + " (" + iconAlt + ")";
            str += "<div class=\"sprTree " + this.icon + " " + this.iconClass + "\"><img alt=\"" + iconAlt + "\" id=\"" + this.id + "-icon\" src=\"images/nada.gif\"></a></div><a href=\"" + this.action + "\"  onclick=\"webFXTreeHandler.mark(this);\" id=\"" + this.id + "-anchor\"><span style=\"color: #999999\">" + label + "</span></a></div>";
        } else
            str += "<div class=\"sprTree " + this.icon + "\"><img id=\"" + this.id + "-icon\" alt=\"" + iconAlt + "\" class=\"webfx-tree-icon\" src=\"images/nada.gif\"></a></div><a href=\"" + this.action + "\"  onclick=\"webFXTreeHandler.mark(this);\" id=\"" + this.id + "-anchor\">" + label + "</a></div>";

    } else {
        if (this.iconClass) {
            if (uiKeysExists && this.nodeType == 'content')
                iconAlt = parent.uiKeys['content_itemNotPublished'] + " (" + iconAlt + ")";
            str += "<img alt=\"" + iconAlt + "\" id=\"" + this.id + "-icon\" class=\"" + this.iconClass + "\" src=\"" + ((webFXTreeHandler.behavior == 'classic' && this.open) ? this.openIcon : this.icon) + "\"></a><a href=\"" + this.action + "\" id=\"" + this.id + "-anchor\" onclick=\"webFXTreeHandler.mark(this);\"><span style=\"color: #999999\">" + label + "</span></a></div>";
        } else
            str += "<img id=\"" + this.id + "-icon\" alt=\"" + iconAlt + "\" class=\"webfx-tree-icon\" src=\"" + ((webFXTreeHandler.behavior == 'classic' && this.open) ? this.openIcon : this.icon) + "\"></a><a href=\"" + this.action + "\" id=\"" + this.id + "-anchor\" onclick=\"webFXTreeHandler.mark(this);\">" + label + "</a></div>";
    }

    str += "</div><div id=\"" + this.id + "-cont\" class=\"webfx-tree-container\" style=\"display: " + ((this.open) ? 'block' : 'none') + ";\">";
    var sb = [];
    for (var i = 0; i < this.childNodes.length; i++) {
        sb[i] = this.childNodes[i].toString(i, this.childNodes.length);
    }

    this.plusIcon = ((this.parentNode._last) ? webFXTreeConfig.lPlusIcon : webFXTreeConfig.tPlusIcon);
    this.minusIcon = ((this.parentNode._last) ? webFXTreeConfig.lMinusIcon : webFXTreeConfig.tMinusIcon);
    return str + sb.join("") + "</div>";
}

/* MOVED FROM treeGui.js */
webFXTreeConfig.rootIcon = "sprTreeFolder";
webFXTreeConfig.openRootIcon = "sprTreeFolder_o";
webFXTreeConfig.folderIcon = "sprTreeFolder";
webFXTreeConfig.openFolderIcon = "sprTreeFolder_o";

webFXTreeConfig.fileIcon = "images/tree/wait.gif";
webFXTreeConfig.lMinusIcon = "images/xp/Lminus.gif";
webFXTreeConfig.lPlusIcon = "images/xp/Lplus.gif";
webFXTreeConfig.tMinusIcon = "images/xp/Lminus.gif";
webFXTreeConfig.tPlusIcon = "images/xp/Lplus.gif";

webFXTreeConfig.iIcon = "images/xp/L.gif";
webFXTreeConfig.lIcon = "images/xp/L.gif";
webFXTreeConfig.tIcon = "images/xp/L.gif";

function umbracoXtreeUpdateNode(nodeID, nodeClass, nodeTitle) {

    for (var i = 0; i < parent.tree.webFXTreeHandler.idCounter; i++) {

        if (parent.tree.webFXTreeHandler.all["webfx-tree-object-" + i]) {
            if (parent.tree.webFXTreeHandler.all["webfx-tree-object-" + i].nodeID == nodeID) {

                if (webFXTreeHandler.all["webfx-tree-object-" + i].parentNode.parentNode) {
                    parent.tree.webFXTreeHandler.all["webfx-tree-object-" + i].parentNode.src =
						parent.tree.webFXTreeHandler.all["webfx-tree-object-" + i].parentNode.src + '&rnd=' + Math.random() * 10;
                    webFXTreeHandler.all["webfx-tree-object-" + i].parentNode.reload();

                    setTimeout("umbracoTreeDoFocus(" + nodeID + ")", 200);
                }
                else
                    document.location.href = document.location.href;

            }
        }
    }
}

function umbracoTreeDoFocus(nodeID) {

    var done = 0;

    if (webFXTreeHandler) {
        for (var i = 0; i < webFXTreeHandler.idCounter; i++) {
            if (webFXTreeHandler.all["webfx-tree-object-" + i]) {
                if (webFXTreeHandler.all["webfx-tree-object-" + i].nodeID == nodeID) {
                    webFXTreeHandler.all["webfx-tree-object-" + i].select();
                    done = 1;
                    webFXTreeHandler.all["webfx-tree-object-" + i].blur();
                }
            }
        }

    }

    if (done == 0) {
        setTimeout("umbracoTreeDoFocus(" + nodeID + ")", 200);
    }
}