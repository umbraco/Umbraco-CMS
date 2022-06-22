/**
* @ngdoc directive
* @name umbraco.directives.directive:umbFileDropzone
* @restrict E
* @function
* @description
**/

/*
TODO
.directive("umbFileDrop", function ($timeout, $upload, localizationService, umbRequestHelper){
    return{
        restrict: "A",
        link: function(scope, element, attrs){
            //load in the options model
        }
    }
})
*/

angular.module("umbraco.directives")
    .directive('umbFileDropzone',
        function ($timeout, Upload, localizationService, umbRequestHelper, overlayService, mediaHelper, mediaTypeHelper) {
            return {
                restrict: 'E',
                replace: true,
                templateUrl: 'views/components/upload/umb-file-dropzone.html',
                scope: {
                    parentId: '@',
                    contentTypeAlias: '@',
                    propertyAlias: '@',
                    accept: '@',
                    maxFileSize: '@',
 
                    compact: '@',
                    hideDropzone: '@',
                    acceptedMediatypes: '=',

                    filesQueued: '=',
                    handleFile: '=',
                    filesUploaded: '='
                },
                link: function(scope, element, attrs) {
                    scope.queue = [];
                    scope.totalQueued = 0;
                    scope.currentFile = undefined;
                    scope.processed = [];
                    scope.totalMessages = 0;

                    function _filterFile(file) {
                        var ignoreFileNames = ['Thumbs.db'];
                        var ignoreFileTypes = ['directory'];

                        // ignore files with names from the list
                        // ignore files with types from the list
                        // ignore files which starts with "."
                        if (ignoreFileNames.indexOf(file.name) === -1 &&
                            ignoreFileTypes.indexOf(file.type) === -1 &&
                            file.name.indexOf(".") !== 0) {
                            return true;
                        } else {
                            return false;
                        }
                    }

                    function _filesQueued(files, event) {
                        //Push into the queue
                        Utilities.forEach(files, file => {
                            if (_filterFile(file) === true) {
                                file.messages = [];
                                scope.queue.push(file);
                            }
                        });

                        // Upload not allowed
                        if (!scope.acceptedMediatypes || !scope.acceptedMediatypes.length) {
                            files.map(file => {
                                file.messages.push({message: "File type is not allowed here", type: "Error"});
                            });
                        }

                        // If we have Accepted Media Types, we will ask to choose Media Type, if Choose Media Type returns false, it only had one choice and therefor no reason to
                        if (scope.acceptedMediatypes && _requestChooseMediaTypeDialog() === false) {
                            scope.contentTypeAlias = "umbracoAutoSelect";
                        }

                        // Add the processed length, as we might be uploading in stages
                        scope.totalQueued = scope.queue.length + scope.processed.length;

                        _processQueueItems();                        
                    }

                    function _processQueueItems() {
                        // if we have processed all files, either by successful
                        // upload, or attending to all messages, we deem the
                        // action complete, else continue processing files
                        scope.totalMessages = scope.processed.filter(e => e.messages.length > 0).length;
                        if (scope.totalQueued === scope.processed.length) {
                          if (scope.totalMessages === 0) {
                                if (scope.filesUploaded) {
                                    //queue is empty, trigger the done action
                                    scope.filesUploaded(scope.done);
                                }
                                //auto-clear the done queue after 3 secs
                                var currentLength = scope.processed.length;
                                $timeout(function() {
                                    scope.processed.splice(0, currentLength);
                                }, 3000);
                            }
                        } else {
                            scope.currentFile = scope.queue.shift();
                            _upload(scope.currentFile);
                        }
                    }

                    function _upload(file) {

                        scope.propertyAlias = scope.propertyAlias ? scope.propertyAlias : "umbracoFile";
                        scope.contentTypeAlias = scope.contentTypeAlias ? scope.contentTypeAlias : "Image";

                        Upload.upload({
                                url: umbRequestHelper.getApiUrl("mediaApiBaseUrl", "PostAddFile"),
                                fields: {
                                    'currentFolder': scope.parentId,
                                    'contentTypeAlias': scope.contentTypeAlias,
                                    'propertyAlias': scope.propertyAlias,
                                    'path': file.path
                                },
                                file: file
                            })
                            .progress(function(evt) {
                                if (file.uploadStat !== "done" && file.uploadStat !== "error") {
                                  // calculate progress in percentage
                                  var progressPercentage = parseInt(100.0 * evt.loaded / evt.total, 10);
                                  // set percentage property on file
                                  file.uploadProgress = progressPercentage;
                                }
                            })
                            .success(function (data, status, headers, config) {
                                // Set server messages
                                file.messages = data.notifications;
                                scope.processed.push(file);
                                //after processing, test if everything is done
                                scope.currentFile = undefined;
                                _processQueueItems();
                            })
                            .error(function(evt, status, headers, config) {
                                //if the service returns a detailed error
                                if (evt.InnerException) {
                                    file.messages.push({ message: evt.InnerException.ExceptionMessage, type: "Error" });
                                    //Check if its the common "too large file" exception
                                    if (evt.InnerException.StackTrace &&
                                        evt.InnerException.StackTrace.indexOf("ValidateRequestEntityLength") > 0) {
                                        file.messages.push({ message: "File too large to upload", type: "Error" });
                                    }
                                } else if (evt.Message) {
                                    file.messages.push({message: evt.Message, type: "Error"});
                                } else if (evt && typeof evt === "string") {
                                    file.messages.push({message: evt, type: "Error"});
                                }
                                // If file not found, server will return a 404 and display this message
                                if (status === 404) {
                                    file.messages.push({message: "File not found", type: "Error"});
                                }
                                scope.currentFile = undefined;
                                _processQueueItems();
                            });
                    }

                    function _requestChooseMediaTypeDialog() {

                        if (scope.queue.length === 0) {
                            // if queue has no items so there is nothing to choose a type for
                            return false;
                        }

                        if (scope.acceptedMediatypes.length === 1) {
                            // if only one accepted type, then we wont ask to choose.
                            return false;
                        }

                        var uploadFileExtensions = scope.queue.map(file => mediaHelper.getFileExtension(file.name));

                        var filteredMediaTypes = mediaTypeHelper.getTypeAcceptingFileExtensions(scope.acceptedMediatypes, uploadFileExtensions);

                        var mediaTypesNotFile = filteredMediaTypes.filter(mediaType => mediaType.alias !== "File");

                        if (mediaTypesNotFile.length <= 1) {
                            // if only one  or less accepted types when we have filtered type 'file' out, then we wont ask to choose.
                            return false;
                        }


                        localizationService.localizeMany(["defaultdialogs_selectMediaType", "mediaType_autoPickMediaType"]).then(function (translations) {

                            filteredMediaTypes.push({
                                alias: "umbracoAutoSelect",
                                name: translations[1],
                                icon: "icon-wand"
                            });

                            const dialog = {
                                view: "itempicker",
                                filter: filteredMediaTypes.length > 8,
                                availableItems: filteredMediaTypes,
                                submit: function (model) {
                                    scope.contentTypeAlias = model.selectedItem.alias;
                                    _processQueueItems();

                                    overlayService.close();
                                },
                                close: function () {

                                    scope.queue.map(function (file) {
                                        file.messages.push({message:"No files uploaded, no mediatype selected", type: "Error"});
                                    });
                                    scope.queue = [];

                                    overlayService.close();
                                }
                            };

                            dialog.title = translations[0];
                            overlayService.open(dialog);
                        });

                        return true; // yes, we did open the choose-media dialog, therefore we return true.
                    }

                    scope.dismissMessages = function (file) {
                        file.messages = [];
                        _processQueueItems();
                    }

                    scope.dismissAllMessages = function () {
                      Utilities.forEach(scope.processed, file => {
                        file.messages = [];
                      });
                      _processQueueItems();
                    }

                    scope.handleFiles = function(files, event, invalidFiles) {
                        const allFiles = [...files, ...invalidFiles];

                        // add unique key for each files to use in ng-repeats
                        Utilities.forEach(allFiles, file => {
                            file.key = String.CreateGuid();
                        });

                        if (scope.filesQueued) {
                            scope.filesQueued(allFiles, event);
                        }
                        _filesQueued(allFiles, event);
                    };
                }
            };
        });
