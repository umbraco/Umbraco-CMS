/**
* @ngdoc directive
* @name umbraco.directives.directive:umbImageUploadProgress
* @restrict E
* @function
**/
function umbImageUploadProgress($rootScope, assetsService, $timeout, $log, umbRequestHelper, mediaResource, imageHelper) {
    return {
        require: '^umbImageUpload',
        restrict: 'E',
        replace: true,
        template: '<div class="progress progress-striped active"><div class="bar" ng-style="{width: progress + \'%\'}"></div></div>',
        
        link: function (scope, element, attrs, umbImgUploadCtrl) {

            umbImgUploadCtrl.bindEvent('fileuploadprogressall', function (e, data) {
                scope.progress = parseInt(data.loaded / data.total * 100, 10);
            });
        }
    };
}

angular.module("umbraco.directives").directive('umbImageUploadProgress', umbImageUploadProgress);