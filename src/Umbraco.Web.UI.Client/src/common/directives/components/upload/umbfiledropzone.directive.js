/**
* @ngdoc directive
* @name umbraco.directives.directive:umbFileDropzone
* @restrict E
* @function
* @description Show a dropzone that allows the user to drag files and have them be uploaded to the media library
**/
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
          acceptedMediatypes: '<',

          filesQueued: '<',
          filesUploaded: '<'
        },
        link: function (scope, element, attrs) {
          scope.queue = [];
          scope.totalQueued = 0;
          scope.processing = [];
          scope.processed = [];
          scope.totalMessages = 0;
          // TODO - Make configurable in appsettings
          scope.batchSize = 10;
          scope.processingCount = 0;

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

          /**
           * Initial entrypoint to handle the queued files. It will determine if the files are acceptable
           * and determine if the user needs to pick a media type
           * @param files
           * @param event
           * @returns void
           * @private
           */
          function _filesQueued(files, event) {
            //Push into the queue
            Utilities.forEach(files, file => {
              if (_filterFile(file) === true) {
                file.messages = [];
                scope.queue.push(file);
              }
            });

            // Add all of the processing and processed files to account for uploading
            // files in stages (dragging files X at a time into the dropzone).
            scope.totalQueued = scope.queue.length + scope.processingCount + scope.processed.length;

            // Upload not allowed
            if (!scope.acceptedMediatypes || !scope.acceptedMediatypes.length) {
              files.map(file => {
                file.messages.push({ message: "File type is not allowed here", type: "Error" });
              });
            }

            // If we have Accepted Media Types, we will ask to choose Media Type
            if (scope.acceptedMediatypes) {

              // If the media type dialog returns a positive answer, it is safe to assume that the
              // contentTypeAlias has been chosen and we can return early because the dialog will start processing
              // the queue automatically
              if (_requestChooseMediaTypeDialog()) {
                return;
              }
            }

            // Start the processing of the queue here because the media type dialog was not shown and therefore
            // did not do it earlier
            _processQueueItems();
          }

          /**
           * Run through the queue and start processing files
           * @returns void
           * @private
           */
          function _processQueueItems() {

            if (scope.processingCount === scope.batchSize) {
              return;
            }

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
                $timeout(function () {
                  scope.processed.splice(0, currentLength);
                }, 3000);
              }
            } else if (scope.queue.length) {

              var file = scope.queue.shift();
              scope.processing.push(file);
              _upload(file);

              // If we still have items to process
              // do so right away for parallel uploads
              if (scope.queue.length > 0) {
                _processQueueItems();
              }
            }
          }

          /**
           * Upload a specific file and use the scope.contentTypeAlias for the type or fall back to letting
           * the backend auto select a type.
           * @param file
           * @returns void
           * @private
           */
          function _upload(file) {

            if (!file) {
              return;
            }

            if (file.$error) {
                file.done = true;
                scope.processed.push(file);
                file.messages.push({type: "Error", header: "Error"});
                return;
            }

            scope.propertyAlias = scope.propertyAlias ? scope.propertyAlias : "umbracoFile";
            scope.contentTypeAlias = scope.contentTypeAlias ? scope.contentTypeAlias : "umbracoAutoSelect";

            scope.processingCount++;

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
              .progress(function (evt) {
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
                file.done = true;
                scope.processed.push(file);
                scope.processingCount--;
                _processQueueItems();
              })
              .error(function (evt, status, headers, config) {
                //if the service returns a detailed error
                if (evt.InnerException) {
                  file.messages.push({ message: evt.InnerException.ExceptionMessage, type: "Error" });
                  //Check if its the common "too large file" exception
                  if (evt.InnerException.StackTrace &&
                    evt.InnerException.StackTrace.indexOf("ValidateRequestEntityLength") > 0) {
                    file.messages.push({ message: "File too large to upload", type: "Error", header: "Error" });
                  }
                } else if (status === 413) {
                  file.messages.push({ message: "File too large to upload", type: "Error", header: "Error" });
                } else if (evt.Message) {
                  file.messages.push({ message: evt.Message, type: "Error", header: "Error" });
                } else if (evt && typeof evt === "string") {
                  file.messages.push({ message: evt, type: "Error", header: "Error" });
                } else if (status === 404) {
                  // If file not found, server will return a 404 and display this message
                  file.messages.push({ message: "File not found", type: "Error" });
                } else if (status !== 200) {
                  file.messages.push({ message: "An unknown error occurred", type: "Error", header: "Error" });
                }

                file.done = true;
                scope.processed.push(file);
                scope.processingCount--;
                _processQueueItems();
              });
          }

          /**
           * Opens the media type dialog and lets the user choose a media type. If the queue is empty it will not show.
           * @returns {boolean}
           * @private
           */
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
                    file.messages.push({ message: "No files uploaded, no mediatype selected", type: "Error" });
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

          scope.handleFiles = function (files, event, invalidFiles) {
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
