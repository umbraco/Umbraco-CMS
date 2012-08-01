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

    var _this = this;
    Sys.Debug.trace("Constructor, before init load");
    if (!this._inited) {
        _this.init();
    }
    Sys.Application.add_load(function() {
        _this.init();
    });
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

            var _this = this;
            LiveEditingToolbar.add_save(function(sender, args) { _this.delaySaveWhenEditing(args, "save"); });
            LiveEditingToolbar.add_saveAndPublish(function(sender, args) { _this.delaySaveWhenEditing(args, "saveAndPublish"); });

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
        this._editControl = this.getElementsByTagName("umbraco:control");
        Sys.Debug.assert(this._editControl.length > 0, "Live Editing - ItemEditing: Could not find the editor control.");
        //this._activeItem.jItem.fadeIn();
        this.moveChildControls(this._editControl, this._activeItem.jItem);

        // Only elements that are currently present, can cause item editing to stop.
        // This enables transparent use of dynamically created elements (such as context/dropdown menus)
        // as clicks on those elements will not trigger the stop edit signal.
        jQuery("*").each(function () { jQuery(this).data("canStopEditing", true); });

        // raise event
        var handler = this.get_events().getHandler("startEdit");
        if (handler)
            handler(this, Sys.EventArgs.Empty);

        this.ignoreChildClicks(this._activeItem.jItem);

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
            //this._activeItem.jItem.fadeOut();
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
                        Sys.Debug.trace("Live Editing - Delayed Saving Changes to server");
                        UmbracoCommunicator.SendClientMessage(type, "");
                    }, 100);
                }
                Sys.Application.add_load(f);
            })();
        }
        else {
            Sys.Debug.trace("Live Editing - Saving Changes to server");
            if (!Sys.WebForms.PageRequestManager.getInstance().get_isInAsyncPostBack()) {
                UmbracoCommunicator.SendClientMessage(type, "");
            }
            else {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function() {
                    UmbracoCommunicator.SendClientMessage(type, "");
                });
            }
        }
    },

    // Enables Live Editing of items.
    itemsEnable: function() {
        var items = this.getElementsByTagName("umbraco:iteminfo");
        Sys.Debug.trace("  Found " + items.length + " editable Umbraco item(s).");

        // enhance items with edit functionality
        var _this = this;
        var i = 0;
        items.each(function() {
            var item = new umbraco.presentation.LiveEditing.activeItem(jQuery(this));
            _this._items[item.itemId] = item;
            Sys.Debug.trace("    " + (++i) + ". " + item.toString() + " is Live Editing enabled.");
        });
        // disable hyperlinks to make them clickable for Live Editing
        this.disableHyperlinks();

        // add "stop editing" handler when clicking outside the item
        var _this = this;
        jQuery(document).mousedown(function(event) {
            Sys.Debug.trace("DOCUMENT CLICKED");
            // the canStopEditing property is set in startEdit
            if (_this._activeItem != null && jQuery(event.target).data("canStopEditing")) {
                if (!_this._activeItem.clicked)
                    _this.stopEdit();
                else
                    _this._activeItem.clicked = false;
            }
        });
        jQuery("#LiveEditingToolbar").mousedown(function() {
            Sys.Debug.trace("TOOLBAR CLICKED");
            if (_this._activeItem != null)
                _this._activeItem.clicked = true;
        });
    },

    // Update items that have changed.
    updateItems: function() {
        var itemUpdates = this.getElementsByTagName("umbraco:itemupdate");
        Sys.Debug.trace("Live Editing - ItemEditing: " + itemUpdates.length + " item update(s).");

        var _this = this;
        itemUpdates.each(function() {
            var itemUpdate = jQuery(this);
            var itemId = itemUpdate.attr("itemId");
            var item = _this._items[itemId];

            if (item != null) {
                Sys.Debug.trace("  Updating " + item.toString() + ".");

                // remove old children and add updates ones
                _this.moveChildControls(itemUpdate, item.jItem);
                //item.jItem.fadeIn();

                // disable hyperlinks to make them clickable for Live Editing
                _this.disableHyperlinks();
            }
            else {
                itemUpdate.html("");
            }
        });
    },

    // Update controls that have changed.
    updateControls: function() {
        Sys.Debug.trace("Live Editing - ItemEditing: In updatecontrols");
        var controlUpdates = this.getElementsByTagName("umbraco:control");
        Sys.Debug.trace("Live Editing - ItemEditing: " + controlUpdates.length + " control update(s).");

        if (controlUpdates.length == 1) {
            if (this._activeItem != null && controlUpdates.children().length > 0) {
                Sys.Debug.trace("Live Editing - ItemEditing: updating edit control.");
                this.moveChildControls(controlUpdates, this._activeItem.jItem);
                this.ignoreChildClicks();
            }

            this._submitControl = controlUpdates.next();
            Sys.Debug.assert(this._submitControl.length > 0, "Live Editing - ItemEditing: Submit button not found.");
        }
    },

    // ignores clicks on child elements of the control
    ignoreChildClicks: function() {
        var _this = this;
        this._activeItem.jItem.children().mousedown(function(e) {
            _this._activeItem.clicked = true;
        });
    },

    // Moves the child controls from source into destination, overwriting existing elements.
    moveChildControls: function(source, dest) {
        Sys.Debug.trace("Live Editing - Moving Child Controls");

        //remove contents in the destination        
        dest.html("");

        //add the source to the destination
        dest.append(source.html());

        //remove teh contents from the source
        source.html("");

    },

    // Gets a list of elements with the specified tagname including namespaced ones
    getElementsByTagName: function(tagname) {
        var found = jQuery("body").find("*").filter(function(index) {
            if (this.nodeType != 3) {
                var nn = this.nodeName.toLowerCase();
                var ftn = tagname.toLowerCase();
                var ln = (ftn.indexOf(":") > 0 ? ftn.substr(ftn.indexOf(":") + 1) : ftn);
                return (nn == ftn
                    || (typeof this.scopeName != "undefined" && nn == ln && this.scopeName.toLowerCase() == ftn.substr(0, ftn.indexOf(":"))));
            }
            return false;
        });
        Sys.Debug.trace("found " + found.length + " elements with selector: " + tagname);
        return found;
    },

    // Disables hyperlinks inside the specified element.
    disableHyperlinks: function() {
        jQuery("a").click(function() {
            return false;
        });
    }
}

umbraco.presentation.LiveEditing.ItemEditing.registerClass("umbraco.presentation.LiveEditing.ItemEditing", Sys.Component);

//an object to store the information for the active item
umbraco.presentation.LiveEditing.activeItem = function(item) {
    //error checking
    if (item != null && item.length != 1) {
        return null;
    }
    //create the object with values, wire up events and return it
    var obj = {
        jItem: item,
        nodeId: item.attr("nodeId"),
        fieldName: item.attr("name"),
        itemId: item.attr("itemId"),
        clicked: false,
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
                //this.jItem.fadeOut();
            }
        },
        // Item click handler.
        onClick: function(e) {
            if (ItemEditing._activeItem != null && ItemEditing._activeItem.itemId == this.itemId) {
                Sys.Debug.trace("Live Editing - ItemEditing: " + this.toString() + " click ignored because it is already active.");
            }
            else {
                Sys.Debug.trace("Live Editing - ItemEditing: " + this.toString() + " was clicked.");
                e.stopPropagation(); // disable click event propagation to parent elements
                this.activate();
            }
        }
    }

    //keep the scope on the click event method call
    obj.jItem.click(function(e) {
        obj.onClick.call(obj, e);
    });

    return obj;
}


// global instance of the ItemEditing class
function initializeGlobalItemEditing() {
    ItemEditing = new umbraco.presentation.LiveEditing.ItemEditing();
}