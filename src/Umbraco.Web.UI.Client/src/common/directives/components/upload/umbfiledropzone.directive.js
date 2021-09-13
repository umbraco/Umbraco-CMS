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
                    scope.done = [];
                    scope.rejected = [];
                    scope.currentFile = undefined;

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

                                if (file.$error) {
                                    scope.rejected.push(file);
                                } else {
                                    scope.queue.push(file);
                                }
                            }
                        });

                        //when queue is done, kick the uploader
                        if (!scope.working) {
                            // Upload not allowed
                            if (!scope.acceptedMediatypes || !scope.acceptedMediatypes.length) {
                                files.map(file => {
                                    file.uploadStatus = "error";
                                    file.serverErrorMessage = "File type is not allowed here";
                                    scope.rejected.push(file);
                                });
                                scope.queue = [];
                            }
                            // If we have Accepted Media Types, we will ask to choose Media Type, if Choose Media Type returns false, it only had one choice and therefor no reason to
                            if (scope.acceptedMediatypes && _requestChooseMediaTypeDialog() === false) {
                                scope.contentTypeAlias = "umbracoAutoSelect";

                                _processQueueItem();
                            }
                        }
                    }

                    function _processQueueItem() {
                        if (scope.queue.length > 0) {
                            scope.currentFile = scope.queue.shift();
                            _upload(scope.currentFile);
                        } else if (scope.done.length > 0) {
                            if (scope.filesUploaded) {
                                //queue is empty, trigger the done action
                                scope.filesUploaded(scope.done);
                            }

                            //auto-clear the done queue after 3 secs
                            var currentLength = scope.done.length;
                            $timeout(function() {
                                scope.done.splice(0, currentLength);
                            }, 3000);
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
                                  // set uploading status on file
                                  file.uploadStatus = "uploading";
                                }
                            })
                            .success(function(data, status, headers, config) {
                                if (data.notifications && data.notifications.length > 0) {
                                    // set error status on file
                                    file.uploadStatus = "error";
                                    // Throw message back to user with the cause of the error
                                    file.serverErrorMessage = data.notifications[0].message;
                                    // Put the file in the rejected pool
                                    scope.rejected.push(file);
                                } else {
                                    // set done status on file
                                    file.uploadStatus = "done";
                                    file.uploadProgress = 100;
                                    // set date/time for when done - used for sorting
                                    file.doneDate = new Date();
                                    // Put the file in the done pool
                                    scope.done.push(file);
                                }
                                scope.currentFile = undefined;
                                //after processing, test if everthing is done
                                _processQueueItem();
                            })
                            .error(function(evt, status, headers, config) {
                                // set status done
                                file.uploadStatus = "error";
                                //if the service returns a detailed error
                                if (evt.InnerException) {
                                    file.serverErrorMessage = evt.InnerException.ExceptionMessage;
                                    //Check if its the common "too large file" exception
                                    if (evt.InnerException.StackTrace &&
                                        evt.InnerException.StackTrace.indexOf("ValidateRequestEntityLength") > 0) {
                                        file.serverErrorMessage = "File too large to upload";
                                    }
                                } else if (evt.Message) {
                                    file.serverErrorMessage = evt.Message;
                                } else if (evt && typeof evt === 'string') {
                                    file.serverErrorMessage = evt;
                                }
                                // If file not found, server will return a 404 and display this message
                                if (status === 404) {
                                    file.serverErrorMessage = "File not found";
                                }
                                //after processing, test if everthing is done
                                scope.rejected.push(file);
                                scope.currentFile = undefined;
                                _processQueueItem();
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
                                    _processQueueItem();

                                    overlayService.close();
                                },
                                close: function () {

                                    scope.queue.map(function (file) {
                                        file.uploadStatus = "error";
                                        file.serverErrorMessage = "No files uploaded, no mediatype selected";
                                        scope.rejected.push(file);
                                    });
                                    scope.queue = [];

                                    overlayService.close();
                                }
                            };

                            dialog.title = translations[0];
                            overlayService.open(dialog);
                        });

                        return true;// yes, we did open the choose-media dialog, therefor we return true.
                    }

                    scope.handleFiles = function(files, event, invalidFiles) {
                        const allFiles = [...files, ...invalidFiles];

                        // add unique key for each files to use in ng-repeats
                        allFiles.forEach(file => {
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
