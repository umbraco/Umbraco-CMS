/**
* @ngdoc directive
* @name umbraco.directives.directive:umbSingleFileUpload
* @function
* @restrict A
* @scope
* @description
*  A single file upload field that will reset itself based on the object passed in for the rebuild parameter. This
*  is required because the only way to reset an upload control is to replace it's html.
**/
function umbSingleFileUpload($compile) {
    return {
        restrict: "E",
        scope: {
            rebuild: "="
        },
        replace: true,
        template: "<div><input type='file' umb-file-upload /></div>",
        link: function (scope, el, attrs) {

            scope.$watch("rebuild", function (newVal, oldVal) {
                if (newVal && newVal !== oldVal) {
                    //recompile it!
                    el.html("<input type='file' umb-file-upload />");
                    $compile(el.contents())(scope);
                }
            });

        }
    };
}

angular.module('umbraco.directives').directive("umbSingleFileUpload", umbSingleFileUpload);