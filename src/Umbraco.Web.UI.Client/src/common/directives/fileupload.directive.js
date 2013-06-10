/**
* @ngdoc directive 
* @name umbraco.directive:umbFileUpload
* @description A directive applied to a file input box so that outer scopes can listen for when a file is selected
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