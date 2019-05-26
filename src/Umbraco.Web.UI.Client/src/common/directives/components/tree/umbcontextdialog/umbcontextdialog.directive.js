(function() {
    'use strict';

    function UmbContextDialog(navigationService, keyboardService, editorService) {

        function link($scope) {
            
            $scope.outSideClick = function () {
                // Only close dialog if there's not an editor open (e.g. a picker has been launched).  Clicks within the edito
                // will trigger this functions, so we want to avoid closing the dialog that launched the editor.
                if (editorService.getNumberOfEditors() === 0) {
                    navigationService.hideDialog();
                }
            }

            keyboardService.bind("esc", function() {
                navigationService.hideDialog();
            });

            //ensure to unregister from all events!
            $scope.$on('$destroy', function () {
                keyboardService.unbind("esc");
            });

        }

        var directive = {
            restrict: 'E',
            transclude: true,
            templateUrl: "views/components/tree/umbcontextdialog/umb-context-dialog.html",
            scope: {
                dialogTitle: "<",
                currentNode: "<",
                view: "<"
            },
            link: link
        };
        return directive;
    }

    angular.module('umbraco.directives').directive('umbContextDialog', UmbContextDialog);

})();
