/**
* @ngdoc directive
* @name umbraco.directives.directive:umbImageFolder
* @restrict E
* @function
**/
function umbImageFolder($rootScope, assetsService, $timeout, $log, umbRequestHelper, mediaResource, imageHelper) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: 'views/directives/imaging/umb-image-folder.html',
        scope: {
            options: '=',
            nodeId: '@',
            onUploadComplete: '='
        },
        link: function (scope, element, attrs) {
            //NOTE: Blueimp handlers are documented here: https://github.com/blueimp/jQuery-File-Upload/wiki/Options
            //NOTE: We are using a Blueimp version specifically ~9.4.0 because any higher than that and we get crazy errors with jquery, also note
            // that the jquery UI version 1.10.3 is required for this blueimp version! if we go higher to 1.10.4 it breaks! seriously! 
            // It's do to with the widget framework in jquery ui changes which must have broken a whole lot of stuff. So don't change it for now.

            if (scope.onUploadComplete && !angular.isFunction(scope.onUploadComplete)) {
                throw "onUploadComplete must be a function callback";
            }

            scope.uploading = false;
            scope.files = [];
            scope.progress = 0;

            var defaultOptions = {
                url: umbRequestHelper.getApiUrl("mediaApiBaseUrl", "PostAddFile") + "?origin=blueimp",
                //we'll start it manually to make sure the UI is all in order before processing
                autoUpload: true,
                disableImageResize: /Android(?!.*Chrome)|Opera/
                    .test(window.navigator.userAgent),
                previewMaxWidth: 150,
                previewMaxHeight: 150,
                previewCrop: true,
                dropZone: element.find(".drop-zone"),
                fileInput: element.find("input.uploader"),
                formData: {
                    currentFolder: scope.nodeId
                }
            };

            //merge options
            scope.blueimpOptions = angular.extend(defaultOptions, scope.options);

            function loadChildren(id) {
                mediaResource.getChildren(id)
                    .then(function(data) {
                        scope.images = data.items;
                    });
            }
            
            //when one is finished
            scope.$on('fileuploaddone', function(e, data) {
                scope.$apply(function() {
                    //remove the amount of files complete
                    //NOTE: function is here instead of in the loop otherwise jshint blows up
                    function findFile(file) { return file === data.files[i]; }
                    for (var i = 0; i < data.files.length; i++) {
                        var found = _.find(scope.files, findFile);
                        found.completed = true;
                    }

                    //when none are left resync everything
                    var remaining = _.filter(scope.files, function(file) { return file.completed !== true; });
                    if (remaining.length === 0) {

                        scope.progress = 100;

                        //just the ui transition isn't too abrupt, just wait a little here
                        $timeout(function() {
                            scope.progress = 0;
                            scope.files = [];
                            scope.uploading = false;

                            loadChildren(scope.nodeId);

                            //call the callback
                            scope.onUploadComplete.apply(this, [data]);


                        }, 200);


                    }
                });

            });

            //This handler gives us access to the file 'preview', this is the only handler that makes this available for whatever reason
            // so we'll use this to also perform the adding of files to our collection
            scope.$on('fileuploadprocessalways', function(e, data) {
                scope.$apply(function() {
                    scope.uploading = true;
                    scope.files.push(data.files[data.index]);
                });
            });

            //This executes prior to the whole processing which we can use to get the UI going faster,
            //this also gives us the start callback to invoke to kick of the whole thing
            scope.$on('fileuploadadd', function(e, data) {
                scope.$apply(function() {
                    scope.uploading = true;
                });
            });

            // All these sit-ups are to add dropzone area and make sure it gets removed if dragging is aborted! 
            scope.$on('fileuploaddragover', function(e, data) {
                if (!scope.dragClearTimeout) {
                    scope.$apply(function() {
                        scope.dropping = true;
                    });
                }
                else {
                    $timeout.cancel(scope.dragClearTimeout);
                }
                scope.dragClearTimeout = $timeout(function() {
                    scope.dropping = null;
                    scope.dragClearTimeout = null;
                }, 300);
            });

            //init load
            loadChildren(scope.nodeId);

        }
    };
}

angular.module("umbraco.directives")
    .directive("umbUploadPreview", function($parse) {
        return {
            link: function(scope, element, attr, ctrl) {
                var fn = $parse(attr.umbUploadPreview),
                    file = fn(scope);
                if (file.preview) {
                    element.append(file.preview);
                }
            }
        };
    })
    .directive('umbImageFolder', umbImageFolder);