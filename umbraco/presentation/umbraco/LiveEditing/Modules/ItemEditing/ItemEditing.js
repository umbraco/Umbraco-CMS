// Umbraco Live Editing - ItemEditing: Item Editing
var ItemEditing = null;

Type.registerNamespace("umbraco.presentation.LiveEditing");

/************************ ItemEditing class ************************/

// Creates a new instance of the ItemEditing class.
umbraco.presentation.LiveEditing.ItemEditing = function() {
    umbraco.presentation.LiveEditing.ItemEditing.initializeBase(this);
    this._inited = false;
    this._items = new Array();
    this._activeItem = null;
    this._editControl = null;
    this._submitControl = null;

    var itemEditingInternal = this;
    Sys.Debug.trace("Constructor, before init load");
    if (jQuery.browser.msie && !this._inited) {
        itemEditingInternal.init();
    }
    Sys.Application.add_load(function() { itemEditingInternal.init() });
    Sys.Debug.trace("Constructor, after init load");

}

umbraco.presentation.LiveEditing.ItemEditing.prototype = {
    // Initializes this instance.
    init: function() {
        Sys.Debug.trace("In init...");
        if (!this._inited) {
            this._inited = true;
            Sys.Debug.trace("Live Editing - ItemEditing: Initialization.");
            Sys.Debug.assert(typeof (jQuery) == 'function', "jQuery is not loaded.");

            this.itemsEnable();


            LiveEditingToolbar.add_save(function(sender, args) { ItemEditing.delaySaveWhenEditing(args, "save"); });
            LiveEditingToolbar.add_saveAndPublish(function(sender, args) { ItemEditing.delaySaveWhenEditing(args, "saveAndPublish"); });

            this._inited = true;
            Sys.Debug.trace("Live Editing - ItemEditing: Ready.");
        }
        else {
            this.updateItems();
            this.updateControls();
        }
    },

    // Starts Live Editing the specified item.
    // This method is triggered by the server.
    startEdit: function(itemId) {
        Sys.Debug.trace("Live Editing - ItemEditing: Start editing of Item " + itemId + ".");

        this._activeItem = this._items[itemId];
        Sys.Debug.assert(this._activeItem != null, "Live Editing - ItemEditing: Could not find item with ID " + itemId + ".");
        this._editControl = this.getElementsByTagName("umbraco:control")[0];
        Sys.Debug.assert(this._editControl != null, "Live Editing - ItemEditing: Could not find the editor control.");
        this._activeItem.fadeIn();
        this.moveChildControls(this._editControl, this._activeItem);

        // Only elements that are currently present, can cause item editing to stop.
        // This enables transparent use of dynamically created elements (such as context/dropdown menus)
        // as clicks on those elements will not trigger the stop edit signal.
        jQuery("*").each(function() { this._canStopEditing = true; });

        // raise event
        var handler = this.get_events().getHandler("startEdit");
        if (handler)
            handler(this, Sys.EventArgs.Empty);

        this.ignoreChildClicks(this._activeItem);

        LiveEditingToolbar.setDirty(true);
    },

    // Stops the editing of a specified item, and raises the stopEdit event.
    stopEdit: function() {
        if (this._activeItem != null) {
            Sys.Debug.trace("Live Editing - ItemEditing: Stop editing of " + this._activeItem.toString() + ".");

            // raise event
            var handler = this.get_events().getHandler("stopEdit");
            if (handler)
                handler(this, Sys.EventArgs.Empty);

            // submit changes
            Sys.Debug.assert(this._submitControl != null, "Live Editing - ItemEditing: Submit button not set.");
            this._submitControl.click();

            // hide control
            this._activeItem.fadeOut();
            this._activeItem = null;
            this._submitControl = null;
            this._editControl = null;
        }
    },

    // Adds an event handler to the startEdit event.
    add_startEdit: function(handler) {
        this.get_events().addHandler("startEdit", handler);
    },

    // Removes an event handler from the startEdit event.
    remove_startEdit: function(handler) {
        this.get_events().removeHandler("startEdit", handler);
    },

    // Adds an event handler to the stopEdit event.
    add_stopEdit: function(handler) {
        this.get_events().addHandler("stopEdit", handler);
    },

    // Removes an event handler from the stopEdit event.
    remove_stopEdit: function(handler) {
        this.get_events().removeHandler("stopEdit", handler);
    },

    // Cancels the save method when an item is active, and postpones it to the next postback.
    delaySaveWhenEditing: function(args, type) {
        if (this._activeItem != null) {
            this.stopEdit();
            args.cancel = true;
            (function() {
                var f = function() {
                    Sys.Application.remove_load(f);
                    setTimeout(function() {
                        UmbracoCommunicator.SendClientMessage(type, "");
                    }, 100);
                }
                Sys.Application.add_load(f);
            })();
        }
    },

    // Enables Live Editing of items.
    itemsEnable: function() {
        var items = this.getElementsByTagName("umbraco:iteminfo");
        Sys.Debug.trace("  Found " + items.length + " editable Umbraco item(s).");

        // enhance items with edit functionality
        for (var i = 0; i < items.length; i++) {
            var item = items[i];
            this._items[item.getAttribute("itemId")] = item;
            this.itemsAddFunctionality(item);
            Sys.Debug.trace("    " + (i + 1) + ". " + item.toString() + " is Live Editing enabled.");
        }

        // add "stop editing" handler when clicking outside the item

        jQuery(document).mousedown(function(event) {
            // the _canStopEditing property is set in startEdit
            if (ItemEditing._activeItem != null && event.target._canStopEditing) {
                if (!ItemEditing._activeItem.clicked)
                    ItemEditing.stopEdit();
                else
                    ItemEditing._activeItem.clicked = false;
            }
        });
        jQuery("#LiveEditingToolbar").mousedown(function() {
            if (ItemEditing._activeItem != null)
                ItemEditing._activeItem.clicked = true;
        });
    },

    // Adds Javascript functionality to an item.
    itemsAddFunctionality: function(item) {
        // add attributes to item
        item.itemId = item.getAttribute("itemId");
        item.nodeId = item.getAttribute("nodeId");
        item.fieldName = item.getAttribute("name");

        Sys.Debug.assert(item.itemId != null && item.nodeId != null && item.fieldName != null,
                                 "Live Editing - ItemEditing: Necessary umbraco:iteminfo attributes not present.");

        // add functions to item
        var itemPrototype = umbraco.presentation.LiveEditing.ItemFunctions.prototype;
        item.toString = itemPrototype.toString;
        item.activate = itemPrototype.activate;
        item.fadeIn = itemPrototype.fadeIn;
        item.fadeOut = itemPrototype.fadeOut;

        // register events
        // note: this doesn't work in IE, there it is set server-side as an attribute
        item.onclick = umbraco.presentation.LiveEditing.ItemFunctions.prototype.click;

        // disable hyperlinks to make them clickable for Live Editing
        this.disableHyperlinks(item);
    },

    // Update items that have changed.
    updateItems: function() {
        var itemUpdates = this.getElementsByTagName("umbraco:itemupdate");
        Sys.Debug.trace("Live Editing - ItemEditing: " + itemUpdates.length + " item update(s).");

        // find all item updates
        for (var i = 0; i < itemUpdates.length; i++) {
            var itemUpdate = itemUpdates[i];
            var itemId = itemUpdate.getAttribute("itemId");
            var item = this._items[itemId];

            if (item != null) {
                Sys.Debug.trace("  Updating " + item.toString() + ".");

                // remove old children and add updates ones
                while (item.firstChild != null)
                    item.removeChild(item.firstChild);
                while (itemUpdate.firstChild != null)
                    item.appendChild(itemUpdate.firstChild);
                item.fadeIn();

                // disable hyperlinks to make them clickable for Live Editing
                this.disableHyperlinks(item);
            }
            else {
                while (itemUpdate.firstChild != null)
                    itemUpdate.removeChild(itemUpdate.firstChild);
            }
        }
    },

    // Update controls that have changed.
    updateControls: function() {
        Sys.Debug.trace("Live Editing - ItemEditing: In updatecontrols");
        var controlUpdates = this.getElementsByTagName("umbraco:control");
        Sys.Debug.trace("Live Editing - ItemEditing: " + controlUpdates.length + " control update(s).");

        if (controlUpdates.length == 1) {
            if (this._activeItem != null && controlUpdates[0].firstChild != null) {
                Sys.Debug.trace("Live Editing - ItemEditing: updating edit control.");
                this.moveChildControls(controlUpdates[0], this._activeItem);
                this.ignoreChildClicks(this._activeItem);
            }

            this._submitControl = controlUpdates[0].nextSibling;
            Sys.Debug.assert(this._submitControl != null, "Live Editing - ItemEditing: Submit button not found.");
        }
    },

    // ignores clicks on child elements of the control
    ignoreChildClicks: function(control) {
        // all children set the clicked property to true on mousedown
        // to avoid editing being stopped because of the body mousedown trigger

        for (var i = 0; i < this._activeItem.childNodes.length; i++) {
            try {
                $addHandler(ItemEditing._activeItem.childNodes[i], "mousedown", function() {
                    if (ItemEditing._activeItem != null)
                        ItemEditing._activeItem.clicked = true;
                });
            }
            // some items don't support the onmousedown property
            catch (e) { }
        }
    },

    // Moves the child controls from source into destination, overwriting existing elements.
    moveChildControls: function(source, dest) {
        // save source childs to temp
        while (dest.firstChild != null)
            dest.removeChild(dest.firstChild);
        // add original source fields to dest
        while (source.firstChild != null)
            dest.appendChild(source.firstChild);
    },

    // Gets a list of elements with the specified tagname.
    // Works with custom namespaces in IE as well.
    getElementsByTagName: function(tagname) {
        var elements = document.getElementsByTagName(tagname);
        // if no elements are found, retry search without the namespace prefix
        if (elements.length == 0 && tagname.indexOf(":") > 0)
            elements = document.getElementsByTagName(tagname.substr(tagname.indexOf(":") + 1));
        return elements;
    },

    // Disables hyperlinks inside the specified element.
    disableHyperlinks: function(element) {
        var links = element.getElementsByTagName("a");
        for (var i = 0; i < links.length; i++)
            links[i].onclick = function() { return false; };
    }
}

umbraco.presentation.LiveEditing.ItemEditing.registerClass("umbraco.presentation.LiveEditing.ItemEditing", Sys.Component);



/********************* Live Editing item member functions *********************/

// The ItemFunctions class contains a set of functions that will be added to umbraco:item elements.
// This class will not be initialized.
umbraco.presentation.LiveEditing.ItemFunctions = function() { };

umbraco.presentation.LiveEditing.ItemFunctions.prototype = {

    // Returns a textual representation of an item.
    toString: function() {
        return "Item " + this.itemId + " (node " + this.nodeId + ": " + this.fieldName + ")";
    },

    // Activates an item for editing.
    activate: function() {
        ItemEditing._items[this.itemId] = this;
        if (this != ItemEditing._activeItem) {
            Sys.Debug.trace("Live Editing - ItemEditing: " + this.toString() + " was activated.");
            if (!Sys.WebForms.PageRequestManager.getInstance().get_isInAsyncPostBack()) {
                UmbracoCommunicator.SendClientMessage("edititem", this.itemId);
            }
            else {
                var itemId = this.itemId;
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function() {
                    if (itemId != 0) {
                        UmbracoCommunicator.SendClientMessage("edititem", itemId);
                        itemId = 0;
                    }
                });
            }
            this.fadeOut();
        }
    },

    // Item click handler.
    // Note: in IE, this is called through the onclick attribute.
    click: function(e) {
        if (ItemEditing._activeItem != null && ItemEditing._activeItem.itemId == this.itemId) {
            Sys.Debug.trace("Live Editing - ItemEditing: " + this.toString() + " click ignored because it is already active.");
        }
        else {
            Sys.Debug.trace("Live Editing - ItemEditing: " + this.toString() + " was clicked.");
            if (e) {
                e.stopPropagation(); // disable click event propagation to parent elements
                this.activate();
            }
            else {
                // Internet Explorer specific code
                window.event.cancelBubble = true; // disable click event propagation to parent elements
                // find the iteminfo self-or-ancestor,
                // and restore the functionality that magically disappeared
                var element = window.event.srcElement;
                while (element.tagName != 'iteminfo')
                    element = element.parentElement;
                ItemEditing.itemsAddFunctionality(element);
                element.activate();
            }
        }
    },

    // fades in the item
    fadeIn: function() {
        if (!jQuery.browser.msie)
            jQuery(this).stop().fadeTo(1, 0.0).fadeTo(500, 1);
    },

    // fades out the item
    fadeOut: function() {
        if (!jQuery.browser.msie)
            jQuery(this).stop().fadeTo(1, 1.0).fadeTo(500, 1);
    }
}

// global instance of the ItemEditing class
function initializeGlobalItemEditing() {
    ItemEditing = new umbraco.presentation.LiveEditing.ItemEditing();
}