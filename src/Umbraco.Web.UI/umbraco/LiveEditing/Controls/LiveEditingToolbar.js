Type.registerNamespace("umbraco.presentation.LiveEditing.Controls");

/************************************ Toolbar class ***********************************/

// Constructor.
umbraco.presentation.LiveEditing.Controls.LiveEditingToolbar = function() {
    umbraco.presentation.LiveEditing.Controls.LiveEditingToolbar.initializeBase(this);
    this._inited = false;
    
    // init toolbar on application load
    var liveEditingToolbar = this;
    Sys.Application.add_load(function() { liveEditingToolbar._init(); });
}

umbraco.presentation.LiveEditing.Controls.LiveEditingToolbar.prototype = {
    // Initialize the toolbar.
    _init: function() {
        if (!this._inited) {
            var liveEditingToolbar = this;
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function(sender, args) { liveEditingToolbar._handleError(sender, args); });
            this._inited = true;
        }
    },

    // Fires the Save event.
    _save: function() {
        var handler = this.get_events().getHandler("save");
        var args = new Sys.EventArgs();
        args.cancel = false;
        if (handler)
            handler(this, args);

        if (!args.cancel) {
            this.setDirty(false);
            UmbSpeechBubble.ShowMessage("Info", "Saving", "Save in progress...");
        }
        return !args.cancel;

    },

    // Adds a listener for the Save event.
    add_save: function(handler) {
        this.get_events().addHandler("save", handler);
    },

    // Removes a listener for the Save event.
    remove_save: function(handler) {
        this.get_events().removeHandler("save", handler);
    },

    // Fires the Save and Publish event.
    _saveAndPublish: function() {
        var handler = this.get_events().getHandler("saveAndPublish");
        var args = new Sys.EventArgs();
        args.cancel = false;
        if (handler)
            handler(this, args);

        if (!args.cancel) {
            this.setDirty(false);
            UmbSpeechBubble.ShowMessage("Info", "Publishing", "Save and publish in progress...");
        }
        return !args.cancel;
    },

    // Adds a listener for the Save and Publish event.
    add_saveAndPublish: function(handler) {
        this.get_events().addHandler("saveAndPublish", handler);
    },

    // Removes a listener for the Save and Publish event.
    remove_saveAndPublish: function(handler) {
        this.get_events().removeHandler("saveAndPublish", handler);
    },

    // Sets whether the pages has unsaved changes.
    setDirty: function(isDirty) {
        window.onbeforeunload = isDirty ? function() { return "You have unsaved changes."; } : null;
    },

    // Global error handler. Displays a tooltip with the error message.
    _handleError: function(sender, args) {
        if (args.get_error() != undefined) {
            var errorMessage;
            if (args.get_response().get_statusCode() == '200') {
                errorMessage = args.get_error().message;
            }
            else {
                errorMessage = "An unspecified error occurred.";
            }
            args.set_errorHandled(true);
            UmbSpeechBubble.ShowMessage("info", "Error", errorMessage);
        }
    }
}

// Register the class and create a global instance.
umbraco.presentation.LiveEditing.Controls.LiveEditingToolbar.registerClass("umbraco.presentation.LiveEditing.Controls.LiveEditingToolbar", Sys.Component);
var LiveEditingToolbar = new umbraco.presentation.LiveEditing.Controls.LiveEditingToolbar();