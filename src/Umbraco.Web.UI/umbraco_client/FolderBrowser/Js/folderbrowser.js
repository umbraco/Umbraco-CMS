
Umbraco.Sys.registerNamespace("Umbraco.Controls");

(function ($, Base, window, document, undefined) {

    Umbraco.Controls.FolderBrowser = Base.extend({
        
        // Private
        _el: null,
        _elId: null,
        _parentId: null,
        _opts: null,
        _viewModel: null,
        
        _getChildNodes: function ()
        {
            var self = this;
            
            $.getJSON(self._opts.basePath + "/FolderBrowserService/GetChildNodes/" + self._parentId + "/" + self._viewModel.filterTerm(), function (data) {
                if (data != undefined && data.length > 0) {
                    ko.mapping.fromJS(data, {}, self._viewModel.items);
                } else {
                    self._viewModel.items([]);
                }
            });
        },
        
        _getItemById: function (id)
        {
            var self = this;
            
            var results = ko.utils.arrayFilter(self._viewModel.items(), function (item) {
                return item.Id() === id;
            });

            return results.length == 1 ? results[0] : null;
        },
        
        _deleteItem: function (id)
        {
            var self = this;

            var item = self._getItemById(id);
            if (item === null)
                throw Error("No item found with the id: " + id);

            if (confirm(window.top.uiKeys['defaultdialogs_confirmdelete'] + ' "' + item.Name() + '"?\n\n'))
            {
                $(window.top).trigger("nodeDeleting", []);

                var safePath = "," + item.Path() + ",";

                if (safePath.indexOf(",-20,") != -1 || safePath.indexOf(",-21,") != -1)
                {
                    window.top.umbraco.presentation.webservices.legacyAjaxCalls.DeleteContentPermanently(
                        item.Id(),
                        "media",
                        function () {
                            //raise nodeDeleted event
                            $(window.top).trigger("nodeDeleted", []);

                            //TODO: Reload current open node in tree

                            // Reload nodes
                            self._getChildNodes();
                        });
                }
                else
                {
                    window.top.umbraco.presentation.webservices.legacyAjaxCalls.Delete(
                        item.Id(), "",
                        "media",
                        function() {
                            //raise nodeDeleted event
                            $(window.top).trigger("nodeDeleted", []);

                            //TODO: Reload current open node in tree

                            // Reload nodes
                            self._getChildNodes();
                        },
                        function(error) {
                            //raise public error event
                            $(window.top).trigger("publicError", [error]);

                            //TODO: Reload current open node in tree

                            // Reload nodes
                            self._getChildNodes();
                        });
                }
            }
        },
        
        _initViewModel: function () 
        {
            var self = this;
            
            // Setup the viewmode;
            self._viewModel = $.extend({}, {
                parent: self,
                filterTerm: ko.observable(''),
                items: ko.observableArray([]),
                queued: ko.observableArray([])
            });

            self._viewModel.filterTerm.subscribe(function (newValue) {
                self._getChildNodes();
            });
        },
        
        _initToolbar: function () 
        {
            var self = this;
            
            // Inject the upload button into the toolbar
            var button = $("<input id='fbUploadToolbarButton' type='image' src='images/editor/upload.png' titl='Upload...' onmouseover=\"this.className='editorIconOver'\" onmouseout=\"this.className='editorIcon'\" onmouseup=\"this.className='editorIconOver'\" onmousedown=\"this.className='editorIconDown'\" />");
            button.click(function (e) {
                e.preventDefault();
                $(".upload-overlay").show();
            });

            $(".tabpage:first-child .menubar td[id$='tableContainerButtons'] .sl nobr").after(button);
        },
        
        _initOverlay: function ()
        {
            var self = this;
            
            // Inject the upload overlay
            var instructions = 'draggable' in document.createElement('span')
                ? "<h1>Drag files here to upload</h1> \
                   <p>Or, click the button below to chose the items to upload</p>"
                : "<h1>Click the browse button below to chose the items to upload</h1>";

            var overlay = $("<div class='upload-overlay'>" +
                "<div class='upload-panel'>" +
                instructions +
                "<form action=\"/base/FolderBrowserService/Upload/" + this._parentId + "\" method=\"post\" enctype=\"multipart/form-data\">" +
                "<input id='fileupload' type='file' name='file' multiple>" +
                "</form>" +
                "<ul class='queued' data-bind='foreach: queued'><li>" +
                "<span class='label' data-bind='text: name'></span>" +
                "<span class='progress'><span data-bind=\"style: { width: progress() + '%' }\"></span></span>" +
                "<a href='' data-bind='click: cancel'><img src='images/delete.png' /></a>" +
                "</li></ul>" +
                "<button class='button upload' data-bind='enable: queued().length > 0'>Upload</button>" +
                "<a href='#' class='cancel'>Cancel</a>" +
                "</div>" +
                "</div>");

            $("body").prepend(overlay);
            
            // Create uploader
            $("#fileupload").fileUploader({
                dropTarget: ".upload-overlay",
                onAdd: function (data) {

                    // Create a bindable version of the data object
                    var file = {
                        uploaderId: data.uploaderId,
                        itemId: data.itemId,
                        name: data.name,
                        size: data.size,
                        progress: ko.observable(data.progress),
                        cancel: function () {
                            if (this.progress() < 100)
                                $("#fileupload").fileUploader("cancelItem", this.itemId);
                            else
                                self._viewModel.queued.remove(this);
                        }
                    };

                    // Store item back in context for easy access later
                    data.context = file;

                    // Push bindable item into queue
                    self._viewModel.queued.push(file);
                },
                onDone: function (data) {
                    switch (data.status) {
                        case 'success':
                            //self._viewModel.queued.remove(data.context);
                            break;
                        case 'error':
                            self._viewModel.queued.remove(data.context);
                            break;
                        case 'canceled':
                            self._viewModel.queued.remove(data.context);
                            break;
                    }
                },
                onProgress: function (data) {
                    data.context.progress(data.progress);
                }
            });

            // Hook up uploader buttons
            $(".upload-overlay .upload").click(function (e) {
                e.preventDefault();
                $("#fileupload").fileUploader("uploadAll");
            });

            $(".upload-overlay .cancel").click(function (e) {
                e.preventDefault();
                $("#fileupload").fileUploader("cancelAll");
            });

            // Listen for drag events
            $(".umbFolderBrowser").live('dragenter dragover', function (e) {
                $(".upload-overlay").show();
            });

            $(".upload-overlay").live('dragleave dragexit', function (e) {
                $(this).hide();
            }).click(function () {
                $(this).hide();
            });

            $(".upload-panel").click(function (e) {
                e.stopPropagation();
            });
        },
        
        _initContextMenu: function () 
        {
            var self = this;

            // Setup context menus
            $.contextMenu({
                selector: '.umbFolderBrowser .items li',
                callback: function (key, options) {
                    var id = options.$trigger.data("id");
                    switch (key) {
                        case "delete":
                            self._deleteItem(id);
                            break;
                    }
                },
                items: {
                    "edit": { name: "Edit", icon: "edit" },
                    "delete": { name: "Delete", icon: "delete" }
                },
                animation: { show: "fadeIn", hide: "fadeOut" }
            });
        },

        // Constructor
        constructor: function (el, opts)
        {
            var self = this;

            // Store el info
            self._el = el;
            self._elId = el.id;

            // Grab parent id from element
            self._parentId = $(el).data("parentid");

            // Merge options with default
            self._opts = $.extend({
                // Default options go here
            }, opts);

            self._initViewModel();
            self._initToolbar();
            self._initOverlay();
            self._initContextMenu();

            // Bind the viewmodel
            ko.applyBindings(self._viewModel, el);
            ko.applyBindings(self._viewModel, $(".upload-overlay").get(0));
            
            // Grab children media items
            self._getChildNodes();
        }
        
        // Public
        
    });
    
    $.fn.folderBrowser = function (o)
    {
        if ($(this).length != 1) {
            throw "Only one folder browser can exist on the page at any one time";
        }

        return $(this).each(function () {
            var folderBrowser = new Umbraco.Controls.FolderBrowser(this, o);
            $(this).data("api", folderBrowser);
        });
    };
    
    $.fn.folderBrowserApi = function ()
    {
        //ensure there's only 1
        if ($(this).length != 1) {
            throw "Requesting the API can only match one element";
        }

        //ensure thsi is a collapse panel
        if ($(this).data("api") == null) {
            throw "The matching element had not been bound to a folderBrowser";
        }

        return $(this).data("api");
    };

})(jQuery, base2.Base, window, document)