(function() {
    'use strict';

    function UmbContextDialog() {

        var directive = {
            restrict: 'E',
            transclude: true,
            templateUrl: "views/components/tree/umbcontextdialog/umb-context-dialog.html",
            scope: {
                title: "<",
                currentNode: "<",
                view: "<"
            }
        };
        return directive;
    }

    angular.module('umbraco.directives').directive('umbContextDialog', UmbContextDialog);

})();
