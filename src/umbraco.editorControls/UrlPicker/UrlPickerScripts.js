/// <reference path="http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.4.1-vsdoc.js" />
/* 
Description: Scripts for the uComponents URL Picker data type for Umbraco 4
Author: James Diacono (jd@thefarmdigital.com.au)
Codeplex username: diachedelic
Date created: 25th of November, 2010
Date modified: 15th of March, 2011 
*/

(function ($) {
    /* 
    * jQuery extension
    */
    $.fn.UrlPicker = function (options) {
        /* 
        * Private variables
        */

        // the most recently obtained state of the URL picker
        var state,
        // settings which define the restrictions and workings of this URL picker,
        // no matter what the state:
        settings,
        // functions in here will get called and passed 
        // the state whenever the change event is triggered:
        changeListeners = [],
        // public API for the URL picker:
        api,
        // the API which is exposed by the Content Picker datatype used 
        contentPickerApi,
        // the API which is exposed by the Media Picker datatype used
        mediaPickerApi;

        /* 
        * jQuery cache 
        */

        // overall container of the URL picker
        var $this,
        // collection of tab elements for different modes
        $tabs,
        // collection of views for different modes
        $views,
        // text box for the 'title' option
        $titleField,
        // checkbox for the 'new window' option
        $newWindowField,
        // text box for the raw URL mode
        $urlField,
        // file upload input
        $uploadUrl,
        // file upload container
        $uploadContainer;

        /* 
        * DOM Helper methods
        */

        // Sets the mode in the DOM
        var setMode = function (mode) {
            // Check is mode is allowed, if not default to the first available mode
            var isAllowed = false;
            for (var i = 0; i < settings.AllowedModes.length; i++) {
                if (parseInt(mode, 10) === settings.AllowedModes[i]) {
                    isAllowed = true;
                }
            }

            if (!isAllowed) {
                mode = settings.AllowedModes[0];
            }

            // Apply mode
            $tabs.removeClass("active");
            $tabs.filter("[data-mode=" + mode + "]").addClass("active");

            $views.removeClass("active");
            $views.filter("[data-mode=" + mode + "]").addClass("active");
        };

        // DOM -> state:
        // Returns and sets the "state" object, based on
        // the state of the DOM
        var retrieveState = function () {
            var oldNodeId = state.NodeId;

            state.Mode = parseInt($tabs.filter(".active").attr("data-mode"), 10);
            state.Title = $titleField.val();
            state.NewWindow = $newWindowField.is(":checked");

            switch (state.Mode) {
                case 1:
                    // URL
                    state.Url = $urlField.val();
                    state.NodeId = null;
                    break;

                case 2:
                    // Content
                    state.NodeId = contentPickerApi.GetValue();

                    // Check a node ID is specified
                    if (state.NodeId) {
                        // Check the node ID has changed (no point updating the
                        // URL for no reason - unless it's disappeared)
                        if (!state.Url || state.NodeId !== oldNodeId) {
                            updateContentUrl();
                        }
                    }
                    else {
                        // Remove the URL if no NodeId picked
                        state.Url = null;
                    }
                    break;

                case 3:
                    // Media
                    state.NodeId = mediaPickerApi.GetValue();

                    // Same as for 'content' above
                    if (state.NodeId) {
                        if (!state.Url || state.NodeId !== oldNodeId) {
                            updateMediaUrl();
                        }
                    }
                    else {
                        state.Url = null;
                    }
                    break;

                case 4:
                    // Upload
                    state.Url = $uploadUrl.attr("href");
                    state.NodeId = null;
                    break;

                default:
                    throw "Not implemented";
            }

            return state;
        };

        // state -> DOM:
        // Sets the DOM-state of the URL picker
        var applyState = function (newState) {
            if (newState) {
                state = newState;
            }

            setMode(state.Mode);

            $titleField.val(state.Title ? state.Title : "");
            $newWindowField.attr("checked", state.NewWindow);

            // Reset
            $urlField.val("");
            contentPickerApi.OriginalClearSelection();
            mediaPickerApi.OriginalClearSelection();
            $uploadUrl.val("");

            switch (state.Mode) {
                case 1:
                    // URL
                    $urlField.val(state.Url ? state.Url : "");
                    break;

                case 2:
                    // Content
                    if (state.NodeId) {
                        contentPickerApi.SaveSelection({
                            outVal: state.NodeId
                        });
                    }
                    break;

                case 3:
                    // Media
                    if (state.NodeId) {
                        mediaPickerApi.SaveSelection({
                            outVal: state.NodeId
                        });
                    }
                    break;

                case 4:
                    // Upload
                    var fileUrl = state.Url ? state.Url : "";

                    $uploadUrl.
                    attr("href", fileUrl).
                    text(fileUrl);

                    if (state.Url) {
                        // A file has been uploaded
                        $uploadContainer.addClass("done");
                    } else {
                        // A file has not been uploaded
                        $uploadContainer.removeClass("done");
                    }

                    break;

                default:
                    throw "Not implemented";
            }
        };

        // Sets up the DOM based on the settings
        var applySettings = function () {
            // Reset
            $this.
            find(".disabled").
            removeClass("disabled");

            // Disable disallowed tabs
            $tabs.each(function () {
                var tabMode = parseInt($(this).attr("data-mode"), 10);
                var isAllowed = false;
                for (var i = 0; i < settings.AllowedModes.length; i++) {
                    if (tabMode === settings.AllowedModes[i]) {
                        isAllowed = true;
                    }
                }
                if (!isAllowed) {
                    $(this).addClass("disabled");
                }
            });

            if (!settings.EnableTitle) {
                $this.children(".title").addClass("disabled");
            }

            if (!settings.EnableNewWindow) {
                $this.children(".new-window").addClass("disabled");
            }
        };

        // Indicates to the user that they should wait
        var showLoading = function () {
            $this.addClass("loading");
        };

        // Indicates to the user that waiting is over
        var hideLoading = function () {
            $this.removeClass("loading");
        };

        /* 
        * Triggers
        */

        // Call to trigger the change event, which refreshes the state from the DOM
        // and notifies listeners
        var change = function () {
            retrieveState();

            for (var i = 0; i < changeListeners.length; i++) {
                if (typeof changeListeners[i] === "function") {
                    changeListeners[i].apply(null, [state]);
                }
            }
        };

        /* 
        * Misc Helper methods
        */

        // Hacks the content/media pickers' APIs such that we are
        // essentially 'listening' for their events.
        var listenToPickerApi = function (api) {
            if (api.AreUrlPickerEventsAttached) {
                return;
            }
            
            api.OriginalSaveSelection = api.SaveSelection;
            api.SaveSelection = function (e) {
                api.OriginalSaveSelection.apply(api, [e]);
                change();
            };

            api.OriginalClearSelection = api.ClearSelection;
            api.ClearSelection = function (e) {
                api.OriginalClearSelection.apply(api);
                change();
            };

            api.AreUrlPickerEventsAttached = true;
        };

        // Updates the URL in state from the content node's ID
        var updateContentUrl = function () {
            showLoading();
            $.ajax({
                type: "POST",
                url: settings.ContentNodeUrlMethod,
                data: JSON.stringify({
                    id: state.NodeId
                }),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (msg) {
                    // Update URL:
                    state.Url = msg.d;
                    change();
                    hideLoading();
                }
            });
        };

        // Updates the URL in state from the Media node's ID
        var updateMediaUrl = function () {
            showLoading();
            $.ajax({
                type: "POST",
                url: settings.MediaNodeUrlMethod,
                data: JSON.stringify({
                    id: state.NodeId
                }),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (msg) {
                    // Update URL:
                    state.Url = msg.d;
                    change();
                    hideLoading();
                }
            });
        };

        // Uploads the file selected by the file upload mode, via ajax
        // Takes a callback which is given the URL of the uploaded file
        var uploadFile = function (callback) {
            // Settings checks
            if (!settings.UniquePropertyId) {
                throw "A unique property identifier must be specified in the settings";
            } else if (!settings.AjaxUploadHandlerGuid) {
                throw "You must include the AjaxUploadHandler's GUID in the settings";
            } else if (!settings.AjaxUploadHandlerUrl) {
                throw "You must include the AjaxUploadHandler's URL in the settings";
            }

            var $tempForm = $('<form enctype="multipart/form-data" method="post" />').
                hide().
                appendTo("body");

            // Reset and mark for loading
            $uploadContainer.
            removeClass("error").
            find(".error-message").
            remove();

            // 1. Set container to loading state
            // 2. Move all children to a temporary form
            // 3. Set the names of the file input fields
            var fileInputIndex = 0;
            showLoading();
            $uploadContainer.
            css("height", $uploadContainer.height()).
            find("input[type=file]").
            each(function () {
                $(this).attr("name", settings.AjaxUploadHandlerGuid + "_" + fileInputIndex);
                fileInputIndex++;
            }).
            end().
            children().
            appendTo($tempForm);

            $tempForm.
            ajaxSubmit({
                url: settings.AjaxUploadHandlerUrl + "?" + settings.AjaxUploadHandlerGuid + "_Id=" + settings.UniquePropertyId,
                success: function (responseText) {
                    // Strip any <pre> tags and parse the JSON
                    var response = jQuery.parseJSON(responseText.replace(/<(\/)?pre[^>]*>/gi, ""));

                    // Check for errors, if there are none do a report on files saved
                    if (response.statusCode !== 200) {
                        $uploadContainer.
                            addClass("error");

                        $uploadContainer.
                            append("<p class='error-message'>Error! " + response.statusDescription + "</p>");
                    } else {
                        // Remove the leading tilda and tell the callback
                        callback(response.filesSaved[0]);

                        // Reset file uploads
                        $tempForm[0].reset();
                    }


                    // Move inputs back
                    $uploadContainer.
                        css("height", "auto").
                        append($tempForm.children());

                    // Remove the temp form from the DOM
                    $tempForm.remove();

                    // Fresh and ready to go again
                    hideLoading();
                }
            });
        };

        /*
        * Constructor
        */

        options = $.extend({
            state: null,
            settings: null,
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
            throw "URL Picker plugin only takes single elements at a time";
        } else if (typeof change !== "function") {
            throw "The 'change' option is an event handler and must be a function";
        } else if (!state || typeof state !== "object") {
            throw "The 'state' option is null or of the wrong type";
        } else if (!settings || typeof settings !== "object") {
            throw "Settings are required in the options";
        }

        // Cache DOM elements
        $tabs = $this.find(".mode-chooser>li");
        $views = $this.find(".mode-views>li");
        $newWindowField = $this.children(".new-window").children("input[type=checkbox]");
        $titleField = $this.children(".title").children("input[type=text]");
        $urlField = $views.filter(".url").children("input[type=text]");
        $uploadContainer = $views.filter(".upload");
        $uploadUrl = $uploadContainer.find("a.file-url");

        // Cache picker APIs
        contentPickerApi = Umbraco.Controls.TreePicker.GetPickerById($views.filter(".content").attr("data-id"));
        mediaPickerApi = Umbraco.Controls.TreePicker.GetPickerById($views.filter(".media").attr("data-id"));

        // DOM event wire ups
        $tabs.click(function () {
            setMode($(this).attr("data-mode"));
            change();
        });

        $newWindowField.change(change);
        listenToPickerApi(contentPickerApi);
        listenToPickerApi(mediaPickerApi);
        $urlField.blur(change);
        $titleField.blur(change);

        // Initial setup:
        applySettings(settings);
        applyState(state);

        // File upload callback
        var fileUploaded = function (fileUrl) {
            state.Url = fileUrl;

            // Apply to the DOM
            applyState(state);

            // Update
            change();
        };

        // Sets up the ajax file upload listener
        $uploadContainer.
            delegate("input[type=file]", "change", function () {
                // Check if the user is supposed to be manually uploading
                var manualUpload = $uploadContainer.find(".upload-button").is(":visible");

                if ($(this).val() && !manualUpload) {
                    uploadFile(fileUploaded);
                }
            }).
            delegate(".upload-button", "click", function () {
                // Manual upload mode (For IE7 - doesn't respond to the file upload change event)
                var $fileInput = $uploadContainer.find("input[type=file]");

                if ($fileInput.val()) {
                    uploadFile(fileUploaded);
                }
            }).
            delegate(".delete-button", "click", function () {
                state.Url = null;

                // Apply to the DOM
                applyState(state);

                // Update
                change();
            });

        // Check for Umbraco edit page save handlers (we have to override 
        // this because it uses 'eval', which executes all handlers in the global
        // scope - bad!)
        var global = (function () { return this; })(),
            oldInvoke;
        if (typeof global.invokeSaveHandlers === 'function') {
            oldInvoke = global.invokeSaveHandlers;
            global.invokeSaveHandlers = function () {
                // Execute the handler in the global, with any arguments supplied
                oldInvoke.apply(global, arguments);

                // Run our own change handler
                change();
            };
        }

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