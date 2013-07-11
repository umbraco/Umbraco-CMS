/**
* @ngdoc object
* @name umbraco.directives.directive:umbFileUpload
* @restrict AE 
* @element ANY
* @scope
**/
function umbFileUpload() {
    return {
        scope: true,        //create a new scope
        link: function (scope, el, attrs) {
            el.bind('change', function (event) {
                var files = event.target.files;
                //emit event upward
                scope.$emit("filesSelected", { files: files });                           
            });
        }
    };
}

angular.module('umbraco.directives').directive("umbFileUpload", umbFileUpload);