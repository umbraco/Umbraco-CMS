/**
* @ngdoc directive
* @name umbraco.directives.directive:umbFileUpload
* @function
* @restrict A
* @scope
* @description
*  Listens for file input control changes and emits events when files are selected for use in other controllers.
**/
function umbFileUpload() {
    return {
        restrict: "A",
        scope: true, // create a new scope
        link: function (scope, el, attrs) {
            el.on('change', function (event) {
                var files = event.target.files;
                // emit event upward
                scope.$emit("filesSelected", { files: files });
                // clear the element value - this allows us to pick the same file again and again
                el.val('');
            })
            .on('drag dragstart dragend dragover dragenter dragleave drop', function (event) {
                event.preventDefault();
                event.stopPropagation();
            })
            .on('dragover dragenter', function () {
                scope.$emit("isDragover", { value: true });
            })
            .on('dragleave dragend drop', function () {
                scope.$emit("isDragover", { value: false });
            })
            .on('drop', function (event) {
                var files = event.originalEvent.dataTransfer.files;

                scope.$emit("filesSelected", { files: files });
            });
        }
    };
}

angular.module('umbraco.directives').directive("umbFileUpload", umbFileUpload);
