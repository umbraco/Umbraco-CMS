/*
*	Class: fileUploader
*	Use: Upload multiple files using jquery
*	Author: Matt Brailsford
*	Version: 1.0
*/

(function ($, window, document, undefined) {

    $.event.props.push('dataTransfer');

    var defaultOptions = {
        autoUpload: false,
        limit: false,
        allowedExtension: 'jpg|jpeg|gif|png|pdf',
        dropTarget: null,

        //Callbacks
        onValidationError: null,
        onAdd: function () { },
        onRemove: function () { },
        onUpload: function () { },
        onUploadAll: function () { },
        onProgress: function () { },
        onDone: function () { },
        onDoneAll: function () { }
    };

    var isHtml5 = (window.FormData) ? true : false;

    function FileUploader(el, options) {
        var self = this;
        var $el = $(el);

        // Setup instance
        self.input = el;
        self.form = $el.parents("form");
        self.uploaderId = ++$.fileUploader.count;

        self.opts = $.extend({}, defaultOptions, options);

        // Initialize
        self._init();

        // Return configures component
        return self;
    }

    FileUploader.prototype = {

        // Private methods
        _init: function () {
            var self = this;

            // Init vars
            self.wrapperId = 'fu-fileUploader-' + self.uploaderId;
            self.wrapper = '<div id="' + self.wrapperId + '" class="fu-fileUploader"><div class="fu-formContainer"></div><div class="fu-itemContainer"></div></div>';

            self.wrapperSelector = '#' + self.wrapperId;
            self.formContainerSelector = self.wrapperSelector + " .fu-formContainer";
            self.itemContainerSelector = self.wrapperSelector + " .fu-itemContainer";

            self.itemCount = 0;

            self.progressTimeout = null;
            self.inProgressJqXhr = null;
            self.inProgressItemId = 0;

            //prepend wrapper markup
            self.form.before(self.wrapper);

            //clear all form data
            $(":file", self.form).each(function () {
                $(this).val('');
            });

            // Hide the form (we'll just use it as a template)
            self.form.hide();

            // Create the actual form users will interact with
            self._createNewForm();

            // Hookup drag and drop support
            $(self.opts.dropTarget !== null ? self.opts.dropTarget : self.wrapperSelector).bind('dragenter dragover', false).bind('drop', function (e) {
                e.stopPropagation();
                e.preventDefault();
                self._addFileHtml5(e.dataTransfer);
            });
        },

        _getFileName: function (filePath) {
            var fileName = filePath;
            if (fileName.indexOf('/') > -1) {
                fileName = fileName.substring(fileName.lastIndexOf('/') + 1);
            } else if (fileName.indexOf('\\') > -1) {
                fileName = fileName.substring(fileName.lastIndexOf('\\') + 1);
            }
            return fileName;
        },

        _validateFileName: function (fileName) {
            var self = this;
            var extensions = new RegExp(self.opts.allowedExtension + '$', 'i');
            if (extensions.test(fileName)) {
                return fileName;
            } else {
                return -1;
            }
        },

        _getFileSize: function (file) {
            var fileSize = 0;
            if (file.size > 1024 * 1024) {
                fileSize = (Math.round(file.size * 100 / (1024 * 1024)) / 100).toString() + 'MB';
            } else {
                fileSize = (Math.round(file.size * 100 / 1024) / 100).toString() + 'KB';
            }
            return fileSize;
        },

        _createNewForm: function () {
            var self = this;
            var id = ++self.itemCount;

            var itemId = 'fu-item-' + self.uploaderId + '-' + id;
            var iframeId = 'fu-frame-' + self.uploaderId + '-' + id;
            var formId = 'fu-form-' + self.uploaderId + '-' + id;

            // Create item container
            var item = $('<div id="' + itemId + '" class="fu-item"></div>');

            // Create iFrame for iFrame based uploads
            $('<iframe name="' + iframeId + '"></iframe>').attr({
                id: iframeId,
                src: 'about:blank',
                style: 'display:none'
            }).prependTo(item);

            // Clone the form
            var newForm = self.form.clone().attr({
                id: formId,
                target: iframeId
            }).prependTo(item).show();

            // Remove any id's / css classes from file inputs
            $(":file", newForm).each(function (idx, itm) {
                $(itm).removeAttr("id").removeAttr("class");
            });

            // Hide everything in the form, other than file inputs
            newForm.children().each(function () {
                var isFile = $(this).is(':file');
                if (!isFile && $(this).find(':file').length === 0) {
                    $(this).hide();
                }
            });

            // Append everything to the form container
            item.prependTo(self.formContainerSelector);

            // Attatch event listener to file input
            $(":file", newForm).change(function () {
                if (isHtml5) {
                    self._addFileHtml5(this);
                } else {
                    self._addFile($(this));
                }
            });
        },

        _addFileHtml5: function (input) {
            var self = this;
            $.each(input.files, function (index, file) {
                self._addFile(file);
            });
            self._afterAddFile();
        },

        _addFile: function (input) {
            var self = this;
            var id = self.itemCount;
            var $item = $('#fu-item-' + self.uploaderId + '-' + id);

            // Validate file
            var filename = (isHtml5) ? input.name : self._getFileName($(input).val());
            if (self._validateFileName(filename) == -1) {
                if ($.isFunction(self.opts.onValidationError)) {
                    self.opts.onValidationError({ input: input, type: 'format' });
                } else {
                    alert('Invalid file!');
                }
                $item.find("form :file").val('');
                return false;
            }

            // Check to see if we have reached the limit
            if (self.opts.limit !== false &&
                self.opts.limit >= $(self.itemContainerSelector + " .fu-item").length) {
                if ($.isFunction(self.opts.onValidationError)) {
                    self.opts.onValidationError({ input: input, type: 'limit' });
                } else {
                    alert('Maximum limit reached!');
                }
                $item.find("form :file").val('');
                return false;
            }

            // Create context object
            var data = {
                uploaderId: self.uploaderId,
                itemId: id,
                name: filename,
                size: (isHtml5) ? self._getFileSize(input) : "Unkown",
                status: 'queued',
                progress: 0,
                input: input,
                context: null // Somewhere for users to store cutom data
            };

            //Store data in item
            $item.data('data', data);

            // Move item to queue, and hide it
            $item.appendTo(self.itemContainerSelector).hide();

            //Create a new form
            self._createNewForm();

            //Callback on file queued
            self.opts.onAdd(data);

            if (!isHtml5) {
                self._afterAddFile();
            }
        },

        _afterAddFile: function () {
            var self = this;
            if (self.opts.autoUpload) {
                self._processNextItemInQueue(true);
            }
        },

        _processNextItemInQueue: function (autoProceed) {
            var self = this;
            var totalItems = $(self.itemContainerSelector + " .fu-item");
            if (totalItems.length > 0) {
                var $pendingUpload = $(totalItems.get(0));
                var data = $pendingUpload.data('data');

                //before upload
                var response = self.opts.onUpload(data);
                if (response === false) {
                    return false; //TODO: Raise onDone event?
                }

                self.inProgressItemId = data.itemId;

                data.status = "inprogress";
                data.progress = 0;

                if (isHtml5) {
                    //Upload Using Html5 api
                    self._uploadHtml5($pendingUpload, autoProceed);
                } else {
                    //upload using iframe
                    self._uploadIFrame($pendingUpload, autoProceed);
                }
            }
        },

        _uploadHtml5: function ($item, autoProceed) {
            var self = this;
            var $form = $item.find("form");
            var data = $item.data('data');
            var file = data.input;
            if (file) {
                var fd = new FormData();
                fd.append($(":file", $form).first().attr('name'), file);

                //get other form input and append to formData
                $form.find(':input').each(function () {
                    if (!$(this).is(':file')) {
                        fd.append($(this).attr('name'), $(this).val());
                    }
                });

                //Upload using jQuery AJAX
                self.inProgressJqXhr = $.ajax({
                    url: $form.attr('action'),
                    data: fd,
                    cache: false,
                    contentType: false,
                    processData: false,
                    type: 'POST',
                    xhr: function () {
                        var req = $.ajaxSettings.xhr();
                        if (req) {
                            req.upload.addEventListener('progress', function (ev) {
                                data.progress = Math.round(ev.loaded * 100 / ev.total);
                                self.opts.onProgress(data);
                            }, false);
                        }
                        return req;
                    }
                }).success(function (d) {
                    data.status = 'success';
                    self.opts.onDone(data);
                }).error(function (jqXhr, textStatus, errorThrown) {
                    data.status = 'error';
                    data.message = errorThrown;
                    self.opts.onDone(data);
                }).complete(function (jqXhr, textStatus) {
                    self._afterUpload($item, autoProceed);
                });

            }
        },

        _uploadIFrame: function ($item, autoProceed) {
            var self = this;
            var $form = $item.find("form");
            var $iframe = $item.find("iframe");
            var data = $item.data('data');

            self._startDummyProgress(data);

            $form.submit();

            $iframe.load(function () {
                self._stopDummyProgress(data, true);

                // Read content returned
                var rawResponse;

                if (this.contentDocument) {
                    rawResponse = this.contentDocument.body.innerHTML;
                } else {
                    rawResponse = this.contentWindow.document.body.innerHTML;
                }

                var response = $.parseJSON(rawResponse);

                if (response.success) {
                    data.status = 'success';
                } else {
                    data.status = 'error';
                    data.message = 'An error occured whilst uploading.';
                }

                self.opts.onDone(data);

                self._afterUpload($item, autoProceed);
            });
        },

        _startDummyProgress: function (data, count) {
            var self = this;

            if (count === undefined) {
                count = 0;
            }

            if ($.fileUploader.percentageInterval[count]) {
                data.progress = $.fileUploader.percentageInterval[count] + Math.floor(Math.random() * 5 + 1);
                self.opts.onProgress(data);
            }

            if ($.fileUploader.timeInterval[count]) {
                self.progressTimeout = setTimeout(function () {
                    self._startDummyProgress(data, ++count);
                }, $.fileUploader.timeInterval[count] * 1000);
            }
        },

        _stopDummyProgress: function (data, success) {
            var self = this;

            clearTimeout(self.progressTimeout);

            if (success) {
                // Force a 100% onProgress event
                data.progress = 100;
                self.opts.onProgress(data);
            }
        },

        _afterUpload: function ($item, autoProceed) {
            var self = this;
            var data = $item.data('data');

            if (data.itemId == self.inProgressItemId) {
                self.inProgressItemId = 0;
                self.inProgressJqXhr = null;
            }

            $item.remove();

            if ($(self.itemContainerSelector + " .fu-item").length > 0) {
                if (autoProceed) {
                    self._processNextItemInQueue(autoProceed);
                }
            }
            else {
                self.opts.onDoneAll();
            }
        },

        // Public methods
        uploadAll: function () {
            var self = this;
            var result = self.opts.onUploadAll(self);
            if (result === false) {
                return false;
            }

            self._processNextItemInQueue(true);
        },

        cancelAll: function () {
            var self = this;

            // Remove current form
            $(self.formContainerSelector).empty();

            var queuedItems = $(self.itemContainerSelector + " .fu-item");
            for (var i = 0; i < queuedItems.length; i++) {
                var data = $(queuedItems.get(i)).data('data');
                self.cancelItem(data.itemId);
            }

            self.itemCount = 0;

            // Recreate the form
            self._createNewForm();
        },

        cancelItem: function (itemId) {
            var self = this;
            var $item = $("#fu-item-" + self.uploaderId + "-" + itemId);
            var data = $item.data('data');

            // If the item to cancel is in progress, abort the upload
            if (self.inProgressItemId == itemId && self.inProgressJqXhr) {
                //NB: The abort event handler will cleanup the aborted item from the queue
                self.inProgressJqXhr.abort();
            }
            else {
                // Update the status
                data.status = 'canceled';

                self.opts.onDone(data);

                self._afterUpload($item, false);
            }
        }
    };

    $.fileUploader = {
        version: '1.0',
        count: 0,
        timeInterval: [1, 2, 4, 2, 1, 5, 5, 5, 5],
        percentageInterval: [10, 20, 30, 40, 60, 80, 85, 90, 95]
    };

    $.fn.fileUploader = function (o) {
        var args = arguments;
        if (typeof o === 'string') {
            var api = this.fileUploaderApi();
            if (api[o]) {
                return api[o].apply(api, Array.prototype.slice.call(args, 1));
            } else {
                $.error('Method ' + o + ' does not exist on jQuery.fileUploader');
            }
        }
        else if (typeof o === 'object' || !o) {
            return this.each(function () {
                var fileUploader = new FileUploader(this, o);
                $(this).data("fileuploader_api", fileUploader);
            });
        }
    };

    $.fn.fileUploaderApi = function () {
        //ensure there's only 1
        if (this.length != 1) {
            throw "Requesting the API can only match one element";
        }

        //ensure thsi is a collapse panel
        if (this.data("fileuploader_api") === null) {
            throw "The matching element had not been bound to a fileUploader";
        }

        return this.data("fileuploader_api");
    };

})(jQuery, window, document);