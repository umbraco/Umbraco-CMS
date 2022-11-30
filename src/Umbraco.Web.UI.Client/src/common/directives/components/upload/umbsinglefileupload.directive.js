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

    // cause we have the same template twice I choose to extract it to its own variable:
    var innerTemplate = "<input type='file' umb-file-upload accept='{{acceptFileExt}}' ng-click='handleClick($event)' />";

    return {
        restrict: "E",
        scope: {
            rebuild: "=",
            acceptFileExt: "<?"
        },
        replace: true,
        template: "<div class='umb-single-file-upload'>"+innerTemplate+"</div>",
        link: function (scope, el, attrs) {

            scope.readonly = false;

            scope.$watch("rebuild", function (newVal, oldVal) {
                if (newVal && newVal !== oldVal) {
                    //recompile it!
                    el.html(innerTemplate);
                    $compile(el.contents())(scope);
                }
            });

            scope.handleClick = function ($event) {
                if (scope.readonly) {
                    $event.preventDefault();
                    $event.stopPropagation();
                }
            };

            attrs.$observe('readonly', (value) => {
                scope.readonly = value !== undefined;
            });

        }
    };
}

angular.module('umbraco.directives').directive("umbSingleFileUpload", umbSingleFileUpload);
