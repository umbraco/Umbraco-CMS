/**
* @ngdoc directive
* @name umbraco.directives.directive:umbPhotoFolder
* @deprecated
* We plan to remove this directive in the next major version of umbraco (8.0). The directive is not recommended to use.
* @restrict E
**/

angular.module("umbraco.directives.html")
    .directive('umbPhotoFolder', function($compile, $log, $timeout, $filter, umbPhotoFolderHelper) {

        return {
            restrict: 'E',
            replace: true,
            require: '?ngModel',
            terminate: true,
            templateUrl: 'views/directives/_obsolete/umb-photo-folder.html',
            link: function(scope, element, attrs, ngModel) {

                var lastWatch = null;

                ngModel.$render = function() {
                    if (ngModel.$modelValue) {

                        $timeout(function() {
                            var photos = ngModel.$modelValue;

                            scope.clickHandler = scope.$eval(element.attr('on-click'));


                            var imagesOnly =  element.attr('images-only') === "true";


                            var margin = element.attr('border') ? parseInt(element.attr('border'), 10) : 5;
                            var startingIndex = element.attr('baseline') ? parseInt(element.attr('baseline'), 10) : 0;
                            var minWidth = element.attr('min-width') ? parseInt(element.attr('min-width'), 10) : 420;
                            var minHeight = element.attr('min-height') ? parseInt(element.attr('min-height'), 10) : 100;
                            var maxHeight = element.attr('max-height') ? parseInt(element.attr('max-height'), 10) : 300;
                            var idealImgPerRow = element.attr('ideal-items-per-row') ? parseInt(element.attr('ideal-items-per-row'), 10) : 5;
                            var fixedRowWidth = Math.max(element.width(), minWidth);

                            scope.containerStyle = { width: fixedRowWidth + "px" };
                            scope.rows = umbPhotoFolderHelper.buildGrid(photos, fixedRowWidth, maxHeight, startingIndex, minHeight, idealImgPerRow, margin, imagesOnly);

                            if (attrs.filterBy) {

                                //we track the watches that we create, we don't want to create multiple, so clear it
                                // if it already exists before creating another.
                                if (lastWatch) {
                                    lastWatch();
                                }

                                //TODO: Need to debounce this so it doesn't filter too often!
                                lastWatch = scope.$watch(attrs.filterBy, function (newVal, oldVal) {
                                    if (newVal && newVal !== oldVal) {
                                        var p = $filter('filter')(photos, newVal, false);
                                        scope.baseline = 0;
                                        var m = umbPhotoFolderHelper.buildGrid(p, fixedRowWidth, maxHeight, startingIndex, minHeight, idealImgPerRow, margin, imagesOnly);
                                        scope.rows = m;
                                    }
                                });
                            }

                        }, 500); //end timeout
                    } //end if modelValue

                }; //end $render
            }
        };
    });
