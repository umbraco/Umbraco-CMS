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
        function ($timeout, Upload, localizationService, umbRequestHelper, overlayService) {
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
                        angular.forEach(files,
                            function(file) {

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
                                files.map(function(file) {
                                    file.uploadStatus = "error";
                                    file.serverErrorMessage = "File type is not allowed here";
                                    scope.rejected.push(file);
                                });
                                scope.queue = [];
                            }
                            // One allowed type
                            if (scope.acceptedMediatypes && scope.acceptedMediatypes.length === 1) {
                                // Standard setup - set alias to auto select to let the server best decide which media type to use
                                if (scope.acceptedMediatypes[0].alias === 'Image') {
                                    scope.contentTypeAlias = "umbracoAutoSelect";
                                } else {
                                    scope.contentTypeAlias = scope.acceptedMediatypes[0].alias;
                                }

                                _processQueueItem();
                            }
                            // More than one, open dialog
                            if (scope.acceptedMediatypes && scope.acceptedMediatypes.length > 1) {
                                _chooseMediaType();
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

                    function _chooseMediaType() {

                        const dialog = {
                            view: "itempicker",
                            filter: scope.acceptedMediatypes.length > 15,
                            availableItems: scope.acceptedMediatypes,
                            submit: function (model) {
                                scope.contentTypeAlias = model.selectedItem.alias;
                                _processQueueItem();

                                overlayService.close();
                            },
                            close: function () {

                                scope.queue.map(function (file) {
                                    file.uploadStatus = "error";
                                    file.serverErrorMessage = "Cannot upload this file, no mediatype selected";
                                    scope.rejected.push(file);
                                });
                                scope.queue = [];

                                overlayService.close();
                            }
                        };

                        localizationService.localize("defaultdialogs_selectMediaType").then(value => {
                            dialog.title = value;
                            overlayService.open(dialog);
                        });
                    }

                    scope.handleFiles = function(files, event) {
                        if (scope.filesQueued) {
                            scope.filesQueued(files, event);
                        }
                        _filesQueued(files, event);
                    };
                }
            };
        });
