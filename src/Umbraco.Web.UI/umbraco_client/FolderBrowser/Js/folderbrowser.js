
Umbraco.Sys.registerNamespace("Umbraco.Controls");

(function ($, Base, window, document, undefined) {

    var itemMappingOptions = {
        'create': function (o) {
            var item = ko.mapping.fromJS(o.data);
            item.selected = ko.observable(false);
            item.toggleSelected = function (itm, e) {

                if (this.selected()) {
                    return;
                }

                if (!e.ctrlKey) {
                    for (var i = 0; i < o.parent().length; i++) {
                        o.parent()[i].selected(false);
                    }
                }

                this.selected(true);
            };
            item.edit = function () {
                //TODO: Could do with a better way of getting to the parent control
                $(".umbFolderBrowser").folderBrowserApi()._editItem(this.Id());
            };
            return item;
        }
    };

    Umbraco.Controls.FolderBrowser = Base.extend({
        // Private
        _el: null,
        _elId: null,
        _parentId: null,
        _nodePath: null,
        _opts: null,
        _viewModel: null,

        _getChildNodes: function () {
            var self = this;

            $.ajaxSetup({ cache: false });
            $.getJSON(self._opts.basePath + "/FolderBrowserService/GetChildren/" + self._parentId, function (data) {
                if (data != undefined && data.length > 0) {
                    ko.mapping.fromJS(data, itemMappingOptions, self._viewModel.items);
                } else {
                    self._viewModel.items([]);
                    $("img.throbber").hide();
                }
            });
        },

        _getItemById: function (id) {
            var self = this;

            var results = ko.utils.arrayFilter(self._viewModel.items(), function (item) {
                return item.Id() === id;
            });

            return results.length == 1 ? results[0] : undefined;
        },

        _editItem: function (id) {
            var self = this;

            var item = self._getItemById(id);
            if (item === undefined) {
                throw Error("No item found with the id: " + id);
            }

            window.location.href = "editMedia.aspx?id=" + item.Id();
        },

        _downloadItem: function (id) {
            var self = this;

            var item = self._getItemById(id);
            if (item === undefined) {
                throw Error("No item found with the id: " + id);
            }

            window.open(item.FileUrl(), "Download");
        },

        _deleteItems: function (ids) {
            var self = this;

            var msg = ids.length + " item" + ((ids.length > 1) ? "s" : "");

            if (confirm(window.top.uiKeys['defaultdialogs_confirmdelete'] + ' the selected ' + msg + '?\n\n')) {
                $(window.top).trigger("nodeDeleting", []);

                $.getJSON(self._opts.basePath + "/FolderBrowserService/Delete/" + ids.join(), function (data) {
                    if (data != undefined && data.success) {
                        //raise nodeDeleted event
                        $(window.top).trigger("nodeDeleted", []);

                        //TODO: Reload current open node in tree

                        // Reload nodes
                        self._getChildNodes();

                    } else {
                        throw Error("There was an error deleting the selected nodes: " + ids.join());
                    }
                });
            }
        },

        _initViewModel: function () {
            var self = this;

            // Setup the viewmode;
            self._viewModel = $.extend({}, {
                parent: self,
                thumbSize: ko.observable('large'),
                filterTerm: ko.observable(''),
                items: ko.observableArray([]),
                queued: ko.observableArray([])
            });

            self._viewModel.thumbSize.subscribe(function (newValue) {
                $(".umbFolderBrowser .items").removeClass("large").removeClass("medium").removeClass("small").addClass(newValue);
            });

            self._viewModel.filtered = ko.computed(function () {
                return ko.utils.arrayFilter(this.items(), function (item) {
                    return item.Name().toLowerCase().indexOf(self._viewModel.filterTerm().toLowerCase()) > -1 ||
                        item.Tags().toLowerCase().indexOf(self._viewModel.filterTerm().toLowerCase()) > -1;
                });
            }, self._viewModel);

            self._viewModel.selected = ko.computed(function () {
                return ko.utils.arrayFilter(this.items(), function (item) {
                    return item.selected();
                });
            }, self._viewModel);

            self._viewModel.selectedIds = ko.computed(function () {
                var ids = [];
                ko.utils.arrayForEach(this.selected(), function (item) {
                    ids.push(item.Id());
                });
                return ids;
            }, self._viewModel);

            self._viewModel.itemIds = ko.computed(function () {
                var ids = [];
                ko.utils.arrayForEach(this.items(), function (item) {
                    ids.push(item.Id());
                });
                return ids;
            }, self._viewModel);

        },

        _initToolbar: function () {
            var self = this;

            // Inject the upload button into the toolbar
            var button = $("<a href='#' class='btn' title='upload'><i class='icon-upload'></i></a>");
            button.click(function (e) {
                e.preventDefault();
                $(".upload-overlay").show();
            });
            $(".umb-panel-header .umb-btn-toolbar").append(button);
        },

        _initOverlay: function () {
            var self = this;

            // Inject the upload overlay
            var instructions = 'draggable' in document.createElement('span') ? "<h1>Drag files here to upload</h1> \<p>Or, click the button below to choose the items to upload</p>" : "<h1>Click the browse button below to choose the items to upload</h1>";

            var overlay = $("<div class='upload-overlay'>" +
                "<div class='upload-panel'>" +
                instructions +
                "<form action=\"" + self._opts.umbracoPath + "/webservices/MediaUploader.ashx?format=json&action=upload&parentNodeId=" + this._parentId + "\" method=\"post\" enctype=\"multipart/form-data\">" +
                "<input id='fileupload' type='file' name='file' multiple>" +
                "<input type='hidden' name='__reqver' value='" + self._opts.reqver + "' />" +
                "<input type='hidden' name='name' />" +
                "<input type='hidden' name='replaceExisting' />" +
                "</form>" +
                "<ul class='queued' data-bind='foreach: queued'><li>" +
                "<input type='text' class='label' data-bind=\"value: name, valueUpdate: 'afterkeydown', enable: progress() == 0\" />" +
                "<span class='progress' data-bind='attr: { title : message }'><span data-bind=\"style: { width: progress() + '%' }, attr: { class: status() }\"></span></span>" +
                "<a href='' data-bind='click: cancel'><img src='images/delete.png' /></a>" +
                "</li></ul>" +
                "<div class='buttons'>" +
                "<span>" +
                "<button class='button upload' data-bind='enable: queued().length > 0'>Upload</button>" +
                "<input type='checkbox' id='replaceExisting' data-bind='enable: queued().length > 0' />" +
                "<label for='replaceExisting'>Overwrite existing?</label> " +
                "</span>" +
                "<a href='#' class='cancel'>Cancel All</a> &nbsp; " +
                "<a href='#' class='close'>Close</a>" +
                "</div>" +
                "</div>" +
                "</div>");

            $("body").prepend(overlay);

            // Create uploader
            jQuery("#fileupload").fileUploader({
                allowedExtension: '',
                dropTarget: ".upload-overlay",
                onAdd: function (data) {

                    // Create a bindable version of the data object
                    var file = {
                        uploaderId: data.uploaderId,
                        itemId: data.itemId,
                        name: ko.observable(data.name),
                        size: data.size,
                        progress: ko.observable(data.progress),
                        status: ko.observable(''),
                        message: ko.observable(''),
                        cancel: function () {
                            if (this.progress() < 100) {
                                jQuery("#fileupload").fileUploader("cancelItem", this.itemId);
                            } else {
                                self._viewModel.queued.remove(this);
                            }
                        }
                    };

                    file.name.subscribe(function (newValue) {
                        $("#fu-item-" + file.uploaderId + "-" + file.itemId + " input[name=name]").val(newValue);
                    });

                    // Store item back in context for easy access later
                    data.context = file;

                    // Push bindable item into queue
                    self._viewModel.queued.push(file);
                },
                onDone: function (data) {
                    switch (data.status) {
                        case 'success':
                            self._viewModel.queued.remove(data.context);
                            break;
                        case 'error':
                            data.context.message(data.message);
                            data.context.status(data.status);
                            //self._viewModel.queued.remove(data.context);
                            break;
                        case 'canceled':
                            data.context.message("Canceled by user");
                            data.context.progress(100);
                            data.context.status(data.status);
                            //self._viewModel.queued.remove(data.context);
                            break;
                        default:
                            break;
                    }
                },
                onProgress: function (data) {
                    data.context.progress(data.progress);
                },
                onDoneAll: function () {
                    self._getChildNodes();
                    $(".upload-overlay").hide();
                    UmbClientMgr.mainTree().syncTree(self._nodePath.toString(), true, false);
                }
            });

            // Hook up uploader buttons
            $(".upload-overlay .upload").click(function (e) {
                e.preventDefault();
                jQuery("#fileupload").fileUploader("uploadAll");
            });

            $(".upload-overlay #replaceExisting").click(function () {
                $("input[name=replaceExisting]").val($(this).is(":checked"));
            });

            $(".upload-overlay .cancel").click(function (e) {
                e.preventDefault();
                $("#fileupload").fileUploader("cancelAll");
            });

            $(".upload-overlay .close").click(function (e) {
                e.preventDefault();
                $(".upload-overlay").hide();
            });

            // Listen for drag events
            $(".umbFolderBrowser").on('dragenter dragover', function (e) {
                $(".upload-overlay").show();
            });

            $(".upload-overlay").on('dragleave dragexit', function (e) {
                $(this).hide();
            }).click(function () {
                $(this).hide();
            });

            $(".upload-panel").click(function (e) {
                e.stopPropagation();
            });
        },

        _initContextMenu: function () {
            var self = this;

            // Setup context menus
            $.contextMenu({
                selector: '.umbFolderBrowser .items li',
                callback: function (key, options) {
                    var id = options.$trigger.data("id");
                    switch (key) {
                        case "edit":
                            self._editItem(id);
                            break;
                        case "download":
                            self._downloadItem(id);
                            break;
                        case "delete":
                            self._deleteItems(self._viewModel.selectedIds());
                            break;
                        default:
                            break;
                    }
                },
                items: {
                    "edit": { name: "Edit", icon: "edit" },
                    "download": { name: "Download", icon: "download" },
                    "separator1": "-----",
                    "delete": { name: "Delete", icon: "delete" }
                },
                animation: { show: "fadeIn", hide: "fadeOut" }
            });
        },

        _initItems: function () {
            var self = this;

            $(".umbFolderBrowser .items").sortable({
                helper: "clone",
                opacity: 0.6,
                start: function (e, ui) {
                    // Add dragging class to container
                    $(".umbFolderBrowser .items").addClass("ui-sortable-dragging");
                },
                update: function (e, ui) {
                    // Can't sort when filtered so just return
                    if (self._viewModel.filterTerm().length > 0) {
                        return;
                    }

                    //var oldIndex = self._viewModel.items.indexOf(self._viewModel.tempItem());
                    var newIndex = ui.item.index();

                    $(".umbFolderBrowser .items .selected").sort(function (a, b) {
                        return parseInt($(a).data("order"), 10) > parseInt($(b).data("order"), 10) ? 1 : -1;
                    }).each(function (idx, itm) {
                        var id = $(itm).data("id");
                        var item = self._getItemById(id);
                        if (item !== undefined) {
                            var oldIndex = self._viewModel.items.indexOf(item);

                            // Update the index of the current item in the array
                            self._viewModel.items.splice((newIndex + idx), 0, self._viewModel.items.splice(oldIndex, 1)[0]);
                        }
                    });

                },
                stop: function (e, ui) {
                    // Remove dragging class from container
                    $(".umbFolderBrowser .items").removeClass("ui-sortable-dragging");

                    if (self._viewModel.filterTerm().length > 0) {
                        $(this).sortable("cancel");
                        alert("Can't sort items which have been filtered");
                    }
                    else {

                        $.ajax({
                            url: self._opts.umbracoPath + "/umbracoapi/media/postsort",
                            type: 'POST',
                            contentType: "application/json; charset=utf-8",
                            dataType: 'json',
                            data: JSON.stringify({
                                parentId: self._parentId,
                                idSortOrder: self._viewModel.itemIds()
                            }),
                            processData: false,
                            success: function (data, textStatus) {
                                if (textStatus == "error") {
                                    alert("Could not update sort order");
                                    self._getChildNodes();
                                }
                            },
                            error: function(data) {
                                alert("Could not update sort order. Err: " + data.statusText);
                                self._getChildNodes();
                            }
                        });
                    }
                }
            });
        },

        // Constructor
        constructor: function (el, opts) {
            var self = this;

            // Store el info
            self._el = el;
            self._elId = el.id;

            // Grab parent id from element
            self._parentId = $(el).data("parentid");
            
            // Grab node path from element
            self._nodePath = $(el).data("nodepath");

            // Merge options with default
            self._opts = $.extend({

                // Default options go here
            }, opts);

            self._initViewModel();
            self._initToolbar();
            self._initOverlay();
            self._initContextMenu();
            self._initItems();

            // Bind the viewmodel
            ko.applyBindings(self._viewModel, el);
            ko.applyBindings(self._viewModel, $(".upload-overlay").get(0));

            // Grab children media items
            self._getChildNodes();
        }

        // Public
    });

    $.fn.folderBrowser = function (o) {
        if ($(this).length != 1) {
            throw "Only one folder browser can exist on the page at any one time";
        }

        return $(this).each(function () {
            var folderBrowser = new Umbraco.Controls.FolderBrowser(this, o);
            $(this).data("api", folderBrowser);
        });
    };

    $.fn.folderBrowserApi = function () {
        //ensure there's only 1
        if ($(this).length != 1) {
            throw "Requesting the API can only match one element";
        }

        //ensure thsi is a collapse panel
        if ($(this).data("api") === null) {
            throw "The matching element had not been bound to a folderBrowser";
        }

        return $(this).data("api");
    };

})(jQuery, base2.Base, window, document);