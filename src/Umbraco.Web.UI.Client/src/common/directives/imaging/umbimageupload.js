/**
* @ngdoc directive
* @name umbraco.directives.directive:umbImageFileUpload
* @restrict E
* @function
* @description 
* This is a wrapper around the blueimp angular file-upload directive so that we can expose a proper API to other directives, the blueimp
* directive isn't actually made very well and only exposes an API/events on the $scope so we can't do things like require: "^fileUpload" and use
* it's instance.
**/
function umbImageUpload($compile) {
    return {
        restrict: 'A', 
        scope: true,
        link: function (scope, element, attrs) {
            //set inner scope variable to assign to file-upload directive in the template
            scope.innerOptions = scope.$eval(attrs.umbImageUpload);

            //compile an inner blueimp file-upload with our scope

            var x = angular.element('<div file-upload="innerOptions" />');
            element.append(x);
            $compile(x)(scope);
        },

        //Define a controller for this directive to expose APIs to other directives
        controller: function ($scope, $element, $attrs) {
            

            //create a method to allow binding to a blueimp event (which is based on it's directives scope)
            this.bindEvent = function (e, callback) {
                $scope.$on(e, callback);
            };

        }
    };
}

angular.module("umbraco.directives").directive('umbImageUpload', umbImageUpload);