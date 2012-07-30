
Umbraco.Sys.registerNamespace("Umbraco.Controls");

(function ($, Base) {

    Umbraco.Controls.FolderBrowser = Base.extend({
        
        // Private
        _el: null,
        _elId: null,
        _parentId: null,
        _opts: null,
        _viewModel: null,
        
        _getChildNodes: function () {
            _this = this;
            
            $.getJSON(_this._opts.basePath + "/FolderBrowserService/GetChildNodes/" + _this._parentId + "/" + _this._viewModel.filterTerm(), function(data) {
                if (data != undefined && data.length > 0) {
                    ko.mapping.fromJS(data, {}, _this._viewModel.items);
                } else {
                    _this._viewModel.items([]);
                }
            });
        },
        
        // Constructor
        constructor: function (el, opts) {

            _this = this;

            // Store el info
            this._el = el;
            this._elId = el.id;

            // Grab parent id from element
            this._parentId = $(el).data("parentid");

            // Merge options with default
            this._opts = $.extend({
                // Default options go here
            }, opts);

            // Setup the viewmode;
            this._viewModel = $.extend({}, {
                parent: this,
                filterTerm: ko.observable(''),
                items: ko.observableArray([]),
                queued: ko.observableArray([])
            });

            this._viewModel.filterTerm.subscribe(function(newValue) {
                _this._getChildNodes();
            });

            // Inject the upload button into the toolbar
            var button = $("<input id='fbUploadToolbarButton' type='image' src='images/editor/upload.png' titl='Upload...' onmouseover=\"this.className='editorIconOver'\" onmouseout=\"this.className='editorIcon'\" onmouseup=\"this.className='editorIconOver'\" onmousedown=\"this.className='editorIconDown'\" />");
            button.click(function(e) {
                e.preventDefault();
                $(".upload-overlay").show();
            });
            
            $(".tabpage:first-child .menubar td[id$='tableContainerButtons'] .sl nobr").after(button);

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
                            if(this.progress() < 100)
                                $("#fileupload").fileUploader("cancelItem", this.itemId);
                            else
                                _this._viewModel.queued.remove(this);
                        }
                    };
                    
                    // Store item back in context for easy access later
                    data.context = file; 
                    
                    // Push bindable item into queue
                    _this._viewModel.queued.push(file);
                },
                onDone: function (data) {
                    switch(data.status) {
                        case 'success':
                            //_this._viewModel.queued.remove(data.context);
                            break;
                        case 'error':
                            _this._viewModel.queued.remove(data.context);
                            break;
                        case 'canceled':
                            _this._viewModel.queued.remove(data.context);
                            break;
                    }
                },
                onProgress: function(data) {
                    data.context.progress(data.progress);
                }
            });
            
            // Hook up uploader buttons
            $(".upload-overlay .upload").click(function(e) {
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
            }).click(function() {
                $(this).hide();
            });

            $(".upload-panel").click(function(e) {
                e.stopPropagation();
            });

            // Bind the viewmodel
            ko.applyBindings(this._viewModel, el);
            ko.applyBindings(this._viewModel, overlay.get(0));
            
            // Grab children media items
            this._getChildNodes();
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

})(jQuery, base2.Base)