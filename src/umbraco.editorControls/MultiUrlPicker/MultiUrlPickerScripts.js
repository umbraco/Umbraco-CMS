/// <reference path="http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.4.1-vsdoc.js" />
/* 
Description: Scripts for the uComponents Multi-URL Picker data type for Umbraco 4
Author: James Diacono (jd@thefarmdigital.com.au)
Codeplex username: diachedelic
Date created: 18th of March, 2011
Date modified: 21st of March, 2011
*/

(function ($) {
    /* 
    * jQuery extension
    */
    $.fn.MultiUrlPicker = function (options) {
        /* 
        * Private variables
        */

        // the most recently obtained state of the Multi-URL picker
        var state,
        // settings which define the restrictions and workings of this Multi-URL picker,
        // no matter what the state:
        settings,
        // functions in here will get called and passed the state whenever the change event is triggered:
        changeListeners = [],
        // public API
        api,
        // UrlPicker API
        urlPickerApi;

        /* 
        * jQuery cache 
        */

        // overall container of the Multi-URL picker
        var $this,
        $itemContainer,
        $urlPickerContainer,
        $addItemButton;

        /* 
        * DOM Helper methods
        */

        // Gets the DOM item's state
        var getItemState = function ($item) {
            return JSON.parse($item.attr("data-state"));
        };

        // Sets the DOM item's state
        var setItemState = function ($item, itemState) {
            $item.attr("data-state", JSON.stringify(itemState));
        };

        // Stores the URL picker safely away
        var shelveUrlPicker = function () {
            $urlPickerContainer.appendTo($this);
        };

        // Create new item and return it's jQuery
        var createNewItem = function (itemState) {
            var $item = $('<li class="item" />').
            append($('<div class="controls" />').
                append('<span class="drag-handle" />').
                append('<a href="#" title="Delete\'s the item" class="delete button">Delete</a>').
                append('<a href="#" title="Edit the item" class="edit button">Edit</a>').
                append('<a href="#" title="Save and stop editing this item" class="close button">Close</a>').
                append('<a href="#" title="Open URL in new window" class="url button" target="_blank" />'));

            setItemState($item, itemState);

            return $item;
        };

        // Enables editing for an item
        var editItem = function ($item) {
            // Catch any stray changes
            change();

            $itemContainer.children().removeClass("active");

            if ($item && $item.length) {
                $urlPickerContainer.appendTo($item);
                urlPickerApi.setState(getItemState($item));
                $item.addClass("active");
            } else {
                // Close editing
                shelveUrlPicker();
            }

        };

        // Removes editing functionality from the active item,
        // if any.
        var stopEditing = function () {
            // Catch any stray changes
            change();

            editItem(null);
        };

        // Delete item
        var deleteItem = function ($item) {
            // Catch any stray changes
            change();

            // Make sure the UrlPicker is safe
            if ($.contains($item[0], $urlPickerContainer[0])) {
                shelveUrlPicker();
            }
            $item.remove();
            change();
        };

        // DOM -> state:
        // Returns and sets the "state" object, based on
        // the state of the DOM
        var retrieveState = function () {
            var itemStates = [];

            $itemContainer.children().each(function () {
                if ($(this).is(".active")) {
                    // This item is currently being edited - get the latest
                    setItemState($(this), urlPickerApi.getState());
                }

                itemStates.push(getItemState($(this)));
            });

            // Save and return
            state.Items = itemStates;
            return state;
        };


        // state -> DOM:
        // Resets and sets the DOM-state of the URL picker
        var applyState = function (newState) {
            if (newState) {
                state = newState;
            }

            shelveUrlPicker();

            $itemContainer.empty();

            for (var i = 0; i < state.Items.length; i++) {
                var itemState = state.Items[i];

                $itemContainer.append(createNewItem(itemState));
            }
        };

        /* 
        * Triggers
        */

        // Call to trigger the change event, which refreshes the state from the DOM
        // and notifies listeners
        var change = function () {
            retrieveState();

            // Set reference to URL on the controls bar
            $itemContainer.children().each(function () {
                var $li = $(this),
                itemState = getItemState($li),
                url = itemState.Url || "";
                var displayText = itemState.Title || url || "";

                $li.
                    find(".url.button").
                    text(displayText).
                    attr("href", url);
            });

            for (var i = 0; i < changeListeners.length; i++) {
                if (typeof changeListeners[i] === "function") {
                    changeListeners[i].apply(null, [state]);
                }
            }
        };

        /*
        * Constructor
        */

        options = $.extend({
            state: null,
            settings: null,
            // The DOM ID of the shared Url Picker
            urlPickerId: null,
            change: function (state) {
                // Here is the state, it has changed, do something with it
                // if you like
            }
        }, options);

        $this = this;
        state = options.state;
        settings = options.settings;
        changeListeners.push(options.change);

        // Argument checks
        if ($this.length !== 1) {
            throw "The Multi-URL Picker plugin only takes single elements at a time";
        } else if (typeof change !== "function") {
            throw "The 'change' option is an event handler and must be a function";
        } else if (!state || typeof state !== "object") {
            throw "The 'state' option is null or of the wrong type";
        } else if (!settings || typeof settings !== "object") {
            throw "Settings are required in the options";
        }

        // Cache DOM elements
        $itemContainer = $this.children(".items");
        $addItemButton = $this.children(".add");
        $urlPickerContainer = $("#" + options.urlPickerId);

        // Set up URL picker
        if (!$urlPickerContainer.length) {
            throw "The shared URL Picker was not specified";
        }
        urlPickerApi = $urlPickerContainer.UrlPicker({
            state: settings.UrlPickerDefaultState,
            settings: settings.UrlPickerSettings,
            change: change
        });

        // Apply state
        applyState(state);
        change();

        // DOM Event wireups
        $addItemButton.click(function (e) {
            e.preventDefault();

            var $newItem = createNewItem(settings.UrlPickerDefaultState);

            $itemContainer.append($newItem);
            editItem($newItem);

            change();
        });
        $itemContainer.delegate(".controls > a.delete", "click", function (e) {
            e.preventDefault();
            deleteItem($(this).parents("li"));
        });
        $itemContainer.delegate(".controls > a.edit", "click", function (e) {
            e.preventDefault();
            editItem($(this).parents("li"));
        });
        $itemContainer.delegate(".controls > a.close", "click", function (e) {
            e.preventDefault();
            stopEditing();
        });

        $itemContainer.sortable({
            stop: function (event, ui) {
                change();
            },
            handle: ".controls",
            axis: "y"
        });

        // the methods exposed in the API which allow parent datatypes, among others, to
        // make use of this one
        api = {
            setState: function (newState) {
                applyState(newState);
            },
            getState: function () {
                return retrieveState();
            },
            addChangeListener: function (listener) {
                changeListeners.push(listener);
            }
        };

        // Store the api as jQuery data on the UrlPicker's container element
        $this.data("api", api);
        return api;
    };
})(jQuery);