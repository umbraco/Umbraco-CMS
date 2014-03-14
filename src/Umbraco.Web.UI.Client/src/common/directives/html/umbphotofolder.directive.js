/**
* @ngdoc directive
* @name umbraco.directives.directive:umbPhotoFolder
* @restrict E
**/
angular.module("umbraco.directives.html")
    .directive('umbPhotoFolder', function($compile, $log, $timeout, $filter, umbPhotoFolderHelper) {
        
        return {
            restrict: 'E',
            replace: true,
            require: '?ngModel',
            terminate: true,
            templateUrl: 'views/directives/html/umb-photo-folder.html',
            link: function(scope, element, attrs, ngModel) {

                ngModel.$render = function() {
                    if (ngModel.$modelValue) {

                        $timeout(function() {
                            var photos = ngModel.$modelValue;

                            scope.clickHandler = scope.$eval(element.attr('on-click'));

                            //todo: this doesn't do anything
                            var imagesOnly = element.attr('imagesOnly');

                            var margin = element.attr('border') ? parseInt(element.attr('border'), 10) : 5;
                            var startingIndex = element.attr('baseline') ? parseInt(element.attr('baseline'), 10) : 0;
                            var minWidth = element.attr('min-width') ? parseInt(element.attr('min-width'), 10) : 420;
                            var minHeight = element.attr('min-height') ? parseInt(element.attr('min-height'), 10) : 100;
                            var maxHeight = element.attr('max-height') ? parseInt(element.attr('max-height'), 10) : 300;
                            var idealImgPerRow = element.attr('ideal-items-per-row') ? parseInt(element.attr('ideal-items-per-row'), 10) : 5;
                            var fixedRowWidth = Math.max(element.width(), minWidth);

                            scope.containerStyle = { width: fixedRowWidth + "px" };
                            scope.rows = umbPhotoFolderHelper.buildGrid(photos, fixedRowWidth, maxHeight, startingIndex, minHeight, idealImgPerRow, margin);

                            if (attrs.filterBy) {
                                scope.$watch(attrs.filterBy, function(newVal, oldVal) {
                                    if (newVal && newVal !== oldVal) {
                                        var p = $filter('filter')(photos, newVal, false);
                                        scope.baseline = 0;
                                        var m = umbPhotoFolderHelper.buildGrid(p, fixedRowWidth, maxHeight, startingIndex, minHeight, idealImgPerRow, margin);
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
